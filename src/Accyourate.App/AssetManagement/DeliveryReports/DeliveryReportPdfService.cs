using Accyourate.App.Platform.Audit;
using Accyourate.App.Platform.Notifications;
using Accyourate.App.Platform.Pdf;
using Accyourate.App.Platform.Qr;
using Accyourate.App.Platform.Settings;
using Accyourate.App.Platform.Documents;

namespace Accyourate.App.AssetManagement.DeliveryReports;

public sealed class DeliveryReportPdfService
{
    private readonly DeliveryReportRepository _repository;
    private readonly PdfExportService _pdf;
    private readonly AuditService _audit;
    private readonly NotificationService _notifications;
    private readonly SettingsService _settings;
    private readonly DocumentService _documents;

    public DeliveryReportPdfService(
        DeliveryReportRepository? repository = null,
        PdfExportService? pdf = null,
        AuditService? audit = null,
        NotificationService? notifications = null,
        SettingsService? settings = null,
        DocumentService? documents = null)
    {
        _repository = repository ?? new DeliveryReportRepository();
        _pdf = pdf ?? new PdfExportService();
        _audit = audit ?? new AuditService();
        _notifications = notifications ?? new NotificationService();
        _settings = settings ?? new SettingsService();
        _documents = documents ?? new DocumentService();
    }

    public string GeneratePdf(int deliveryReportId, string generatedBy = "System")
    {
        var report = _repository.GetById(deliveryReportId) ?? throw new InvalidOperationException("Verbale di consegna non trovato.");
        var items = _repository.GetItems(deliveryReportId);
        var settings = _settings.Load();
        var document = BuildDocument(report, items, settings);
        var folder = _settings.GetDeliveryReportsFolder();
        var path = _pdf.Export(document, folder, $"{report.ReportNumber}_{report.EmployeeName}");
        _repository.UpdatePdfPath(deliveryReportId, path, DeliveryReportStatus.Generated);

        _documents.RegisterFile(
            path,
            $"Verbale di consegna {report.ReportNumber}",
            DocumentCategory.DeliveryReport,
            "DeliveryReport",
            deliveryReportId.ToString(),
            report.EmployeeName,
            generatedBy,
            $"Asset: {report.AssetCode}");

        _audit.Track(AuditAction.Exported, $"Generato PDF verbale {report.ReportNumber}", "DeliveryReport", deliveryReportId.ToString(), report.ReportNumber, generatedBy, AuditSeverity.Info, "AssetManagement");
        _notifications.Publish("PDF verbale generato", $"Generato PDF per il verbale {report.ReportNumber}.", NotificationCategory.Documents, NotificationPriority.Info, generatedBy, "open-delivery-report-pdf", path);
        return path;
    }

    private static SimplePdfDocument BuildDocument(DeliveryReport report, IReadOnlyList<DeliveryReportItem> items, ApplicationSettings settings)
    {
        var template = settings.DocumentTemplate ?? new DocumentTemplateSettings();
        var companyName = string.IsNullOrWhiteSpace(settings.Company.LegalName)
            ? settings.Company.CompanyName
            : settings.Company.LegalName;
        var d = new SimplePdfDocument { Title = $"Verbale di consegna {report.ReportNumber}" };
        ApplyBranding(d, settings, template, companyName, report);

        d.AddHeading("Verbale di consegna beni aziendali");
        d.AddKeyValue("Numero verbale", report.ReportNumber);
        d.AddKeyValue("Data emissione", Fmt(report.ReportDate));
        d.AddStatus("Stato", report.Status);
        d.AddBlank();
        d.AddHeading("Dipendente");
        d.AddKeyValue("Nominativo", report.EmployeeName);
        d.AddKeyValue("Asset principale", report.AssetCode);
        d.AddBlank();
        d.AddHeading("Beni consegnati");
        if (items.Count == 0) d.AddText("Nessun bene associato al verbale.");
        var i = 1;
        foreach (var item in items)
        {
            d.AddText($"{i}. {item.AssetCode} - {item.Description}");
            if (!string.IsNullOrWhiteSpace(item.SerialNumber)) d.AddText($"   S/N: {item.SerialNumber}");
            d.AddText($"   Stato bene: {item.Condition}");
            if (!string.IsNullOrWhiteSpace(item.Notes)) d.AddText($"   Note: {item.Notes}");
            i++;
        }
        d.AddBlank();
        d.AddHeading("Dichiarazione");
        d.AddText("Il dipendente dichiara di ricevere i beni sopra elencati in buono stato e si impegna a custodirli con diligenza, restituendoli su richiesta dell'azienda o al termine del rapporto/assegnazione.");
        if (!string.IsNullOrWhiteSpace(report.Notes)) { d.AddBlank(); d.AddHeading("Note"); d.AddText(report.Notes); }
        if (template.ShowQrCodePlaceholder)
            d.AddQrCode(BuildQrPayload(report, items, template), $"QR {report.ReportNumber}");
        d.AddBlank(16);
        if (template.ShowSignatures)
        {
            d.AddSignaturePair(
                string.IsNullOrWhiteSpace(template.LeftSignatureLabel) ? "Firma dipendente" : template.LeftSignatureLabel,
                string.IsNullOrWhiteSpace(template.RightSignatureLabel) ? "Firma azienda" : template.RightSignatureLabel);
        }
        return d;
    }

    private static void ApplyBranding(
        SimplePdfDocument document,
        ApplicationSettings settings,
        DocumentTemplateSettings template,
        string companyName,
        DeliveryReport report)
    {
        document.Branding.CompanyName = companyName;
        document.Branding.CompanyDetailLines.AddRange(BuildCompanyDetailLines(settings.Company, template.HeaderLayout));
        document.Branding.HeaderLayout = template.HeaderLayout;
        document.Branding.LogoPath = settings.Company.LogoPath;
        document.Branding.LogoSize = template.LogoSize;
        document.Branding.LogoPosition = template.LogoPosition;
        document.Branding.PrimaryColor = template.PrimaryColor;
        document.Branding.DocumentLabel = "VERBALE DI CONSEGNA";
        document.Branding.DocumentCode = report.ReportNumber;
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
        var location = string.Join(" | ", new[]
        {
            company.Address,
            string.Join(" ", new[] { company.City, company.Province }
                .Where(value => !string.IsNullOrWhiteSpace(value))),
            company.Country
        }.Where(value => !string.IsNullOrWhiteSpace(value)));
        var tax = string.Join(" | ", new[]
        {
            Prefix("P.IVA", company.VatNumber),
            Prefix("C.F.", company.FiscalCode)
        }.Where(value => !string.IsNullOrWhiteSpace(value)));
        var contacts = string.Join(" | ", new[]
        {
            Prefix("Tel.", company.Phone),
            company.Email
        }.Where(value => !string.IsNullOrWhiteSpace(value)));
        var digital = string.Join(" | ", new[]
        {
            Prefix("PEC", company.Pec),
            company.Website
        }.Where(value => !string.IsNullOrWhiteSpace(value)));

        var lines = new[] { location, tax, contacts, digital }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToList();
        return string.Equals(layout, "Compatta", StringComparison.OrdinalIgnoreCase)
            ? lines.Take(1).ToList()
            : lines;
    }

    private static string Prefix(string label, string value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : $"{label}: {value.Trim()}";

    private static string BuildQrPayload(
        DeliveryReport report,
        IReadOnlyList<DeliveryReportItem> items,
        DocumentTemplateSettings template)
    {
        var assetCodes = items
            .Select(item => item.AssetCode)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        return QrDestinationBuilder.Build(
            template,
            "delivery-reports",
            report.ReportNumber,
            new[]
        {
            "Accyourate Enterprise X",
            "Verbale di consegna",
            $"Numero verbale: {report.ReportNumber}",
            $"Dipendente: {report.EmployeeName}",
            $"Asset principale: {report.AssetCode}",
            $"Beni: {string.Join(", ", assetCodes)}",
            $"Data: {Fmt(report.ReportDate)}"
        });
    }

    private static string Fmt(string value) => DateTime.TryParse(value, out var date) ? date.ToString("dd/MM/yyyy HH:mm") : value;
}
