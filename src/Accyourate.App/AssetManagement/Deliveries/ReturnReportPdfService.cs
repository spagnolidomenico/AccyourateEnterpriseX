using Accyourate.App.AssetManagement.Models;
using Accyourate.App.Platform.Pdf;
using Accyourate.App.Platform.Qr;
using Accyourate.App.Platform.Settings;

namespace Accyourate.App.AssetManagement.Deliveries;

public sealed class ReturnReportPdfService
{
    private readonly SettingsService _settings;
    private readonly PdfExportService _pdf;

    public ReturnReportPdfService(SettingsService? settings = null, PdfExportService? pdf = null)
    {
        _settings = settings ?? new SettingsService();
        _pdf = pdf ?? new PdfExportService();
    }

    public string Generate(
        DeliveryRecord delivery,
        Asset asset,
        string employeeName,
        string generatedBy = "System")
    {
        var settings = _settings.Load();
        var template = settings.DocumentTemplate ?? new DocumentTemplateSettings();
        var reportNumber = $"VRR-{DateTime.Now:yyyy}-{delivery.Id:D6}";
        var document = BuildDocument(
            delivery,
            asset,
            employeeName,
            reportNumber,
            generatedBy,
            settings,
            template);

        return _pdf.Export(
            document,
            _settings.GetReturnReportsFolder(),
            $"{reportNumber}_{asset.AssetCode}_{employeeName}");
    }

    internal static SimplePdfDocument BuildDocument(
        DeliveryRecord delivery,
        Asset asset,
        string employeeName,
        string reportNumber,
        string generatedBy,
        ApplicationSettings settings,
        DocumentTemplateSettings template)
    {
        var companyName = string.IsNullOrWhiteSpace(settings.Company.LegalName)
            ? settings.Company.CompanyName
            : settings.Company.LegalName;

        var document = new SimplePdfDocument
        {
            Title = $"Verbale di riconsegna {reportNumber}"
        };

        ApplyBranding(document, settings, template, companyName, reportNumber);
        document.AddHeading("Verbale di riconsegna bene aziendale");
        document.AddKeyValue("Numero verbale", reportNumber);
        document.AddKeyValue("Data riconsegna", FormatDate(delivery.ReturnDate));
        document.AddKeyValue("Operatore", generatedBy);

        document.AddHeading("Dipendente");
        document.AddKeyValue("Nominativo", employeeName);

        document.AddHeading("Bene riconsegnato");
        document.AddKeyValue("Codice asset", asset.AssetCode);
        document.AddKeyValue("Categoria", asset.Category);
        document.AddKeyValue("Produttore e modello", $"{asset.Manufacturer} {asset.Model}".Trim());
        document.AddKeyValue("Numero seriale", asset.SerialNumber);
        document.AddKeyValue("Data consegna", FormatDate(delivery.DeliveryDate));

        document.AddHeading("Condizioni e note");
        document.AddStatus("Condizioni", delivery.ReturnCondition);
        document.AddBlank(6);
        document.AddText(string.IsNullOrWhiteSpace(delivery.ReturnNotes)
            ? "Nessuna annotazione aggiuntiva."
            : delivery.ReturnNotes);

        if (template.ShowQrCodePlaceholder)
        {
            var payload = QrDestinationBuilder.Build(
                template,
                "return-reports",
                reportNumber,
                new[]
                {
                    "Accyourate Enterprise X",
                    "Verbale di riconsegna",
                    $"Numero verbale: {reportNumber}",
                    $"Dipendente: {employeeName}",
                    $"Asset: {asset.AssetCode}",
                    $"Condizioni: {delivery.ReturnCondition}",
                    $"Data: {FormatDate(delivery.ReturnDate)}"
                });
            document.AddQrCode(payload, $"QR {reportNumber}");
        }

        if (template.ShowSignatures)
        {
            document.AddSignaturePair(
                string.IsNullOrWhiteSpace(template.LeftSignatureLabel)
                    ? "Consegnato da"
                    : template.LeftSignatureLabel,
                string.IsNullOrWhiteSpace(template.RightSignatureLabel)
                    ? "Ricevuto da"
                    : template.RightSignatureLabel);
        }

        return document;
    }

    private static void ApplyBranding(
        SimplePdfDocument document,
        ApplicationSettings settings,
        DocumentTemplateSettings template,
        string companyName,
        string reportNumber)
    {
        document.Branding.CompanyName = companyName;
        document.Branding.CompanyDetailLines.AddRange(BuildCompanyDetailLines(settings.Company, template.HeaderLayout));
        document.Branding.HeaderLayout = template.HeaderLayout;
        document.Branding.LogoPath = settings.Company.LogoPath;
        document.Branding.LogoSize = template.LogoSize;
        document.Branding.LogoPosition = template.LogoPosition;
        document.Branding.PrimaryColor = template.PrimaryColor;
        document.Branding.DocumentLabel = "VERBALE DI RICONSEGNA";
        document.Branding.DocumentCode = reportNumber;
        document.Branding.DocumentVersion = template.DocumentVersion;
        document.Branding.ConfidentialityText = template.ConfidentialityText;
        document.Branding.FooterText = template.FooterText;
        document.Branding.ShowLogo = template.ShowLogo;
        document.Branding.ShowCompanyDetails = template.ShowCompanyDetails;
        document.Branding.ShowDocumentMetadata = template.ShowDocumentMetadata;
        document.Branding.ShowFooter = template.ShowFooter;
        document.Branding.ShowPageNumber = template.ShowPageNumber;
        document.Branding.ShowPrintTimestamp = template.ShowPrintTimestamp;
    }

    private static IReadOnlyList<string> BuildCompanyDetailLines(CompanySettings company, string layout)
    {
        var location = Join(company.Address, Join(company.City, company.Province), company.Country);
        var tax = Join(Prefix("P.IVA", company.VatNumber), Prefix("C.F.", company.FiscalCode));
        var contacts = Join(Prefix("Tel.", company.Phone), company.Email);
        var digital = Join(Prefix("PEC", company.Pec), company.Website);
        var lines = new[] { location, tax, contacts, digital }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToList();
        return string.Equals(layout, "Compatta", StringComparison.OrdinalIgnoreCase)
            ? lines.Take(1).ToList()
            : lines;
    }

    private static string Join(params string[] values) =>
        string.Join(" | ", values.Where(value => !string.IsNullOrWhiteSpace(value)));

    private static string Prefix(string label, string value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : $"{label}: {value.Trim()}";

    private static string FormatDate(string value) =>
        DateTime.TryParse(value, out var date)
            ? date.ToString("dd/MM/yyyy HH:mm")
            : value;
}
