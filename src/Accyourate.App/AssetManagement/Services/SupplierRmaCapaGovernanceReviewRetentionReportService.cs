using Accyourate.App.Platform.Pdf;
using Accyourate.App.Platform.Settings;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SupplierRmaCapaGovernanceReviewRetentionReportService
{
    private readonly SupplierRmaCapaGovernanceReviewService _reviews = new();
    private readonly SupplierRmaCapaGovernanceReviewRetentionService _retention = new();
    private readonly SettingsService _settings = new();

    public string Export(string search, string status)
    {
        var term = (search ?? "").Trim();
        var selectedStatus = string.IsNullOrWhiteSpace(status) ? "Tutti gli stati" : status;
        var entries = _reviews.GetAll()
            .SelectMany(review => _retention.GetAll(review.Id).Select(record => new Entry(review, record)))
            .Where(x => MatchesSearch(x, term) && MatchesStatus(x.Record, selectedStatus))
            .OrderByDescending(x => x.Record.Id).ToList();

        var settings = _settings.Load();
        var template = settings.DocumentTemplate ?? new DocumentTemplateSettings();
        var document = new SimplePdfDocument { Title = "Registro audit conservazioni Governance CAPA" };
        ApplyBranding(document, settings, template, $"CAPA-RET-AUD-{DateTime.Now:yyyyMMdd-HHmm}");
        document.AddTitle("Registro audit conservazioni riesami Governance CAPA");
        document.AddKeyValue("Data elaborazione", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        document.AddKeyValue("Operatore", Environment.UserName);
        document.AddKeyValue("Ricerca", string.IsNullOrWhiteSpace(term) ? "Nessuna" : term);
        document.AddKeyValue("Filtro stato", selectedStatus);
        document.AddKeyValue("Conservazioni incluse", entries.Count.ToString());
        document.AddHeading("Riepilogo integrita");
        document.AddKeyValue("Valide", entries.Count(x => x.Record.ValidationStatus == "Valida").ToString());
        document.AddKeyValue("In scadenza", entries.Count(x => x.Record.ValidationStatus == "In scadenza").ToString());
        document.AddKeyValue("Scadute", entries.Count(x => x.Record.ValidationStatus == "Conservazione scaduta").ToString());
        document.AddKeyValue("Non valide", entries.Count(x => x.Record.ValidationStatus is "Archivio mancante" or "Archivio modificato").ToString());
        document.AddKeyValue("Superate", entries.Count(x => x.Record.ValidationStatus == "Conservazione superata").ToString());
        document.AddHeading("Dettaglio conservazioni");
        if (entries.Count == 0) document.AddText("Nessuna conservazione corrisponde ai filtri applicati.");
        foreach (var entry in entries)
        {
            var record = entry.Record;
            document.AddText($"Riesame #{entry.Review.Id:D6} - revisione {record.Revision} - {record.ValidationStatus}", 11);
            document.AddText($"Responsabile: {Dash(entry.Review.Reviewer)} | Custode: {Dash(record.Custodian)}", 9);
            document.AddText($"Archiviazione: {DateTimeValue(record.ArchivedAt)} | Conservazione fino al: {DateValue(record.RetentionUntil)}", 9);
            document.AddText($"Archivio: {record.ArchivePath}", 8);
            document.AddText($"SHA-256: {record.ArchiveHash}", 8);
        }
        document.AddSignaturePair("Responsabile qualita", "Responsabile conservazione");
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Accyourate Enterprise X", "Audit Conservazioni Governance CAPA");
        return new PdfExportService().Export(document, folder, $"Audit-Conservazioni-Governance-CAPA-{DateTime.Now:yyyyMMdd-HHmmss}");
    }

    private static bool MatchesSearch(Entry entry, string term) => string.IsNullOrWhiteSpace(term)
        || entry.Review.Id.ToString("D6").Contains(term, StringComparison.OrdinalIgnoreCase)
        || entry.Review.Reviewer.Contains(term, StringComparison.OrdinalIgnoreCase)
        || entry.Record.Custodian.Contains(term, StringComparison.OrdinalIgnoreCase)
        || entry.Review.Outcome.Contains(term, StringComparison.OrdinalIgnoreCase);

    private static bool MatchesStatus(SupplierRmaCapaGovernanceReviewRetentionRecord record, string selected) => selected switch
    {
        "Valida" => record.ValidationStatus == "Valida",
        "In scadenza" => record.ValidationStatus == "In scadenza",
        "Scaduta" => record.ValidationStatus == "Conservazione scaduta",
        "Non valida" => record.ValidationStatus is "Archivio mancante" or "Archivio modificato",
        "Superata" => record.ValidationStatus == "Conservazione superata",
        _ => true
    };

    private static void ApplyBranding(SimplePdfDocument document, ApplicationSettings settings, DocumentTemplateSettings template, string code)
    {
        document.Branding.CompanyName = string.IsNullOrWhiteSpace(settings.Company.LegalName) ? settings.Company.CompanyName : settings.Company.LegalName;
        document.Branding.CompanyDetailLines.AddRange(new[]
        {
            settings.Company.Address,
            string.Join(" ", new[] { settings.Company.City, settings.Company.Province }.Where(x => !string.IsNullOrWhiteSpace(x))),
            string.Join(" - ", new[] { settings.Company.Phone, settings.Company.Email }.Where(x => !string.IsNullOrWhiteSpace(x))),
            string.Join(" - ", new[] { settings.Company.VatNumber, settings.Company.FiscalCode }.Where(x => !string.IsNullOrWhiteSpace(x)))
        }.Where(x => !string.IsNullOrWhiteSpace(x)));
        document.Branding.HeaderLayout = template.HeaderLayout; document.Branding.LogoPath = settings.Company.LogoPath;
        document.Branding.LogoSize = template.LogoSize; document.Branding.LogoPosition = template.LogoPosition;
        document.Branding.PrimaryColor = template.PrimaryColor; document.Branding.DocumentLabel = "AUDIT CONSERVAZIONI GOVERNANCE CAPA";
        document.Branding.DocumentCode = code; document.Branding.DocumentVersion = template.DocumentVersion;
        document.Branding.FooterText = template.FooterText; document.Branding.ConfidentialityText = template.ConfidentialityText;
        document.Branding.ShowLogo = template.ShowLogo; document.Branding.ShowCompanyDetails = template.ShowCompanyDetails;
        document.Branding.ShowDocumentMetadata = template.ShowDocumentMetadata; document.Branding.ShowFooter = template.ShowFooter;
        document.Branding.ShowPageNumber = template.ShowPageNumber; document.Branding.ShowPrintTimestamp = template.ShowPrintTimestamp;
    }

    private static string Dash(string value) => string.IsNullOrWhiteSpace(value) ? "-" : value;
    private static string DateValue(string value) => DateTime.TryParse(value, out var date) ? date.ToString("dd/MM/yyyy") : value;
    private static string DateTimeValue(string value) => DateTime.TryParse(value, out var date) ? date.ToString("dd/MM/yyyy HH:mm") : value;
    private sealed record Entry(SupplierRmaCapaGovernanceReview Review, SupplierRmaCapaGovernanceReviewRetentionRecord Record);
}
