using Accyourate.App.Platform.Audit;
using Accyourate.App.Platform.Notifications;
using Accyourate.App.Platform.Pdf;
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
        var d = new SimplePdfDocument { Title = $"Verbale di consegna {report.ReportNumber}" };
        d.AddTitle(string.IsNullOrWhiteSpace(settings.Company.LegalName) ? settings.Company.CompanyName : settings.Company.LegalName);
        d.AddHeading("Verbale di consegna beni aziendali");
        if (!string.IsNullOrWhiteSpace(settings.Company.Address))
            d.AddText(settings.Company.Address);
        if (!string.IsNullOrWhiteSpace(settings.Company.VatNumber))
            d.AddText($"P.IVA: {settings.Company.VatNumber}");
        if (!string.IsNullOrWhiteSpace(settings.Company.Email) || !string.IsNullOrWhiteSpace(settings.Company.Phone))
            d.AddText($"Email: {settings.Company.Email}  Tel: {settings.Company.Phone}");
        if (!string.IsNullOrWhiteSpace(settings.Company.Website))
            d.AddText(settings.Company.Website);
        d.AddBlank();
        d.AddText($"Numero verbale: {report.ReportNumber}");
        d.AddText($"Data emissione: {Fmt(report.ReportDate)}");
        d.AddText($"Stato: {report.Status}");
        d.AddBlank();
        d.AddHeading("Dipendente");
        d.AddText($"Nominativo: {report.EmployeeName}");
        d.AddText($"Asset principale: {report.AssetCode}");
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
        d.AddBlank(16);
        d.AddSignaturePair("Firma dipendente", "Firma azienda");
        return d;
    }

    private static string Fmt(string value) => DateTime.TryParse(value, out var date) ? date.ToString("dd/MM/yyyy HH:mm") : value;
}
