using Accyourate.App.AssetManagement.Models;
using Accyourate.App.Platform.Pdf;
using Accyourate.App.Platform.Settings;

namespace Accyourate.App.AssetManagement.Services;

public sealed class MaintenancePurchaseOrderPdfService
{
    private readonly SettingsService _settings = new();

    public string Generate(MaintenancePurchaseOrder order, MaintenanceSupplier supplier, string generatedBy)
    {
        var settings = _settings.Load();
        var template = settings.DocumentTemplate ?? new DocumentTemplateSettings();
        var document = BuildDocument(order, supplier, generatedBy, settings, template);
        var folder = Path.Combine(
            Path.GetDirectoryName(_settings.GetDeliveryReportsFolder())!,
            "Ordini Ricambi");
        return new PdfExportService().Export(document, folder, order.OrderNumber);
    }

    internal static SimplePdfDocument BuildDocument(
        MaintenancePurchaseOrder order,
        MaintenanceSupplier supplier,
        string generatedBy,
        ApplicationSettings settings,
        DocumentTemplateSettings template)
    {
        var document = new SimplePdfDocument { Title = $"Ordine di acquisto {order.OrderNumber}" };
        document.Branding.CompanyName = string.IsNullOrWhiteSpace(settings.Company.LegalName)
            ? settings.Company.CompanyName : settings.Company.LegalName;
        document.Branding.CompanyDetailLines.AddRange(new[]
        {
            settings.Company.Address,
            string.Join(" ", new[] { settings.Company.City, settings.Company.Province }
                .Where(value => !string.IsNullOrWhiteSpace(value))),
            settings.Company.Email
        }.Where(value => !string.IsNullOrWhiteSpace(value)));
        document.Branding.HeaderLayout = template.HeaderLayout;
        document.Branding.LogoPath = settings.Company.LogoPath;
        document.Branding.PrimaryColor = template.PrimaryColor;
        document.Branding.DocumentLabel = "ORDINE DI ACQUISTO RICAMBI";
        document.Branding.DocumentCode = order.OrderNumber;
        document.Branding.DocumentVersion = template.DocumentVersion;
        document.Branding.FooterText = template.FooterText;
        document.Branding.ConfidentialityText = template.ConfidentialityText;
        document.Branding.ShowLogo = template.ShowLogo;
        document.Branding.ShowCompanyDetails = template.ShowCompanyDetails;
        document.Branding.ShowDocumentMetadata = template.ShowDocumentMetadata;
        document.Branding.ShowFooter = template.ShowFooter;
        document.Branding.ShowPageNumber = template.ShowPageNumber;
        document.Branding.ShowPrintTimestamp = template.ShowPrintTimestamp;

        document.AddHeading("Ordine");
        document.AddKeyValue("Numero", order.OrderNumber);
        document.AddKeyValue("Data ordine", Date(order.OrderDate));
        document.AddKeyValue("Consegna prevista", Date(order.ExpectedDate));
        document.AddKeyValue("Stato", order.Status);
        document.AddKeyValue("Generato da", generatedBy);
        document.AddHeading("Fornitore");
        document.AddKeyValue("Ragione sociale", supplier.Name);
        document.AddKeyValue("Partita IVA", supplier.VatNumber);
        document.AddKeyValue("Referente", supplier.ContactPerson);
        document.AddKeyValue("Indirizzo", $"{supplier.Address} {supplier.City}".Trim());
        document.AddKeyValue("Contatti", string.Join(" - ", new[] { supplier.Email, supplier.Phone }
            .Where(value => !string.IsNullOrWhiteSpace(value))));
        document.AddHeading("Articoli");
        foreach (var line in order.Lines)
            document.AddKeyValue(
                string.IsNullOrWhiteSpace(line.PartCode) ? line.Description : $"{line.PartCode} - {line.Description}",
                $"{line.Quantity:N2} x EUR {line.UnitCost:N2} = EUR {line.Total:N2}");
        document.AddStatus("Totale ordine", $"EUR {order.Total:N2}");
        if (!string.IsNullOrWhiteSpace(order.Notes))
        {
            document.AddHeading("Note");
            document.AddText(order.Notes);
        }
        if (template.ShowSignatures)
            document.AddSignaturePair("Responsabile acquisti", "Fornitore");
        return document;
    }

    private static string Date(string value) =>
        DateTime.TryParse(value, out var date) ? date.ToString("dd/MM/yyyy") : value;
}
