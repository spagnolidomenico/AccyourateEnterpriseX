using Accyourate.App.Platform.Pdf;
using Accyourate.App.Platform.Settings;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SupplierRmaCapaGovernanceSnapshot
{
    public int Dossiers { get; init; }
    public int ActiveDossiers { get; init; }
    public int ArchivedDossiers { get; init; }
    public int ApprovedDossiers { get; init; }
    public int MissingDocuments { get; init; }
    public int ReviewsDue { get; init; }
    public int ReviewsOverdue { get; init; }
    public int Attestations { get; init; }
    public int ValidAttestations { get; init; }
    public int InvalidAttestations { get; init; }
    public int MissingAttestationArchives { get; init; }
    public int Exports { get; init; }
    public int ValidExports { get; init; }
    public int InvalidExports { get; init; }
    public int MissingExports { get; init; }
    public int RetentionDue { get; init; }
    public int RetentionOverdue { get; init; }
    public int PeriodicReviews { get; init; }
    public int PeriodicReviewsPendingApproval { get; init; }
    public int PeriodicReviewsApproved { get; init; }
    public int PeriodicReviewsOverdue { get; init; }
    public int PeriodicReviewAttestations { get; init; }
    public int ValidPeriodicReviewAttestations { get; init; }
    public int InvalidPeriodicReviewAttestations { get; init; }
    public int PeriodicReviewRetentions { get; init; }
    public int ValidPeriodicReviewRetentions { get; init; }
    public int PeriodicReviewRetentionsDue { get; init; }
    public int InvalidPeriodicReviewRetentions { get; init; }
    public int CriticalCount => MissingDocuments + ReviewsOverdue + InvalidAttestations + MissingAttestationArchives
        + InvalidExports + MissingExports + RetentionOverdue + PeriodicReviewsOverdue
        + InvalidPeriodicReviewAttestations + InvalidPeriodicReviewRetentions;
}

public sealed class SupplierRmaCapaGovernanceDashboardService
{
    private readonly SupplierRmaCapaDossierRegistryService _dossiers = new();
    private readonly SupplierRmaCapaAttestationService _attestations = new();
    private readonly SupplierRmaCapaAttestationExportService _exports = new();
    private readonly SupplierRmaCapaGovernanceReviewService _reviews = new();
    private readonly SupplierRmaCapaGovernanceReviewAttestationService _reviewAttestations = new();
    private readonly SupplierRmaCapaGovernanceReviewRetentionService _reviewRetention = new();
    private readonly SettingsService _settings = new();

    public SupplierRmaCapaGovernanceSnapshot Load()
    {
        var dossiers = _dossiers.GetAll();
        var attestations = _attestations.GetAll();
        var exports = _exports.GetExports();
        var reviews = _reviews.GetAll();
        var reviewAttestations = reviews.SelectMany(x => _reviewAttestations.GetAll(x.Id)).ToList();
        var retentions = reviews.SelectMany(x => _reviewRetention.GetAll(x.Id)).ToList();
        return new()
        {
            Dossiers = dossiers.Count,
            ActiveDossiers = dossiers.Count(x => x.DocumentStatus == "Attivo"),
            ArchivedDossiers = dossiers.Count(x => x.DocumentStatus == "Archiviato"),
            ApprovedDossiers = dossiers.Count(x => x.ApprovalStatus == "Approvato"),
            MissingDocuments = dossiers.Count(x => x.MissingDocuments),
            ReviewsDue = dossiers.Count(x => x.IsReviewDueSoon),
            ReviewsOverdue = dossiers.Count(x => x.IsReviewOverdue),
            Attestations = attestations.Count,
            ValidAttestations = attestations.Count(x => x.IsValid),
            InvalidAttestations = attestations.Count(x => !x.IsValid && x.ArchiveAvailable),
            MissingAttestationArchives = attestations.Count(x => !x.ArchiveAvailable),
            Exports = exports.Count,
            ValidExports = exports.Count(x => x.IsValid),
            InvalidExports = exports.Count(x => !x.IsValid && x.FileAvailable),
            MissingExports = exports.Count(x => !x.FileAvailable),
            RetentionDue = exports.Count(x => x.RetentionStatus == "In scadenza"),
            RetentionOverdue = exports.Count(x => x.RetentionStatus == "Scaduta"),
            PeriodicReviews = reviews.Count,
            PeriodicReviewsPendingApproval = reviews.Count(x => x.ApprovalStatus == "In approvazione"),
            PeriodicReviewsApproved = reviews.Count(x => x.ApprovalStatus == "Approvato"),
            PeriodicReviewsOverdue = reviews.Count(x => x.IsOverdue),
            PeriodicReviewAttestations = reviewAttestations.Count,
            ValidPeriodicReviewAttestations = reviewAttestations.Count(x => x.IsValid),
            InvalidPeriodicReviewAttestations = reviewAttestations.Count(x => !x.IsValid && x.IsCurrent),
            PeriodicReviewRetentions = retentions.Count,
            ValidPeriodicReviewRetentions = retentions.Count(x => x.IsValid && !x.IsDueSoon),
            PeriodicReviewRetentionsDue = retentions.Count(x => x.IsCurrent && (x.IsDueSoon || x.IsExpired)),
            InvalidPeriodicReviewRetentions = retentions.Count(x => x.IsCurrent && !x.HashMatches)
        };
    }

    public string ExportPdf(SupplierRmaCapaGovernanceSnapshot value)
    {
        var settings = _settings.Load();
        var template = settings.DocumentTemplate ?? new DocumentTemplateSettings();
        var document = new SimplePdfDocument { Title = "Dashboard Governance CAPA" };
        Brand(document, settings, template, $"CAPA-GOV-{DateTime.Now:yyyyMMdd-HHmm}");
        document.AddTitle("Dashboard Governance CAPA");
        document.AddKeyValue("Data elaborazione", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        document.AddKeyValue("Operatore", Environment.UserName);
        document.AddStatus("Esito complessivo", value.CriticalCount == 0 ? "Conforme" : $"{value.CriticalCount} criticita da gestire");
        document.AddHeading("Fascicoli");
        document.AddKeyValue("Fascicoli totali", value.Dossiers.ToString());
        document.AddKeyValue("Attivi", value.ActiveDossiers.ToString());
        document.AddKeyValue("Archiviati", value.ArchivedDossiers.ToString());
        document.AddKeyValue("Approvati", value.ApprovedDossiers.ToString());
        document.AddKeyValue("Documenti mancanti", value.MissingDocuments.ToString());
        document.AddKeyValue("Riesami in scadenza", value.ReviewsDue.ToString());
        document.AddKeyValue("Riesami scaduti", value.ReviewsOverdue.ToString());
        document.AddHeading("Riesami periodici Governance");
        document.AddKeyValue("Riesami totali", value.PeriodicReviews.ToString());
        document.AddKeyValue("In approvazione", value.PeriodicReviewsPendingApproval.ToString());
        document.AddKeyValue("Approvati", value.PeriodicReviewsApproved.ToString());
        document.AddKeyValue("Scaduti", value.PeriodicReviewsOverdue.ToString());
        document.AddKeyValue("Attestazioni totali", value.PeriodicReviewAttestations.ToString());
        document.AddKeyValue("Attestazioni valide", value.ValidPeriodicReviewAttestations.ToString());
        document.AddKeyValue("Attestazioni non valide", value.InvalidPeriodicReviewAttestations.ToString());
        document.AddKeyValue("Conservazioni totali", value.PeriodicReviewRetentions.ToString());
        document.AddKeyValue("Conservazioni valide", value.ValidPeriodicReviewRetentions.ToString());
        document.AddKeyValue("Conservazioni in scadenza", value.PeriodicReviewRetentionsDue.ToString());
        document.AddKeyValue("Conservazioni non valide", value.InvalidPeriodicReviewRetentions.ToString());
        document.AddHeading("Attestazioni fascicoli");
        document.AddKeyValue("Totali", value.Attestations.ToString());
        document.AddKeyValue("Valide", value.ValidAttestations.ToString());
        document.AddKeyValue("Non valide", value.InvalidAttestations.ToString());
        document.AddKeyValue("Archivi mancanti", value.MissingAttestationArchives.ToString());
        document.AddHeading("Esportazioni e conservazione fascicoli");
        document.AddKeyValue("Esportazioni totali", value.Exports.ToString());
        document.AddKeyValue("Integre", value.ValidExports.ToString());
        document.AddKeyValue("Modificate", value.InvalidExports.ToString());
        document.AddKeyValue("File mancanti", value.MissingExports.ToString());
        document.AddKeyValue("Conservazioni in scadenza", value.RetentionDue.ToString());
        document.AddKeyValue("Conservazioni scadute", value.RetentionOverdue.ToString());
        document.AddSignaturePair("Responsabile qualita", "Responsabile processo");
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Accyourate Enterprise X", "Governance CAPA");
        return new PdfExportService().Export(document, folder, $"Dashboard-Governance-CAPA-{DateTime.Now:yyyyMMdd-HHmmss}");
    }

    private static void Brand(SimplePdfDocument document, ApplicationSettings settings, DocumentTemplateSettings template, string code)
    {
        document.Branding.CompanyName = string.IsNullOrWhiteSpace(settings.Company.LegalName) ? settings.Company.CompanyName : settings.Company.LegalName;
        document.Branding.CompanyDetailLines.AddRange(new[] { settings.Company.Address, string.Join(" ", new[] { settings.Company.City, settings.Company.Province }.Where(x => !string.IsNullOrWhiteSpace(x))), string.Join(" - ", new[] { settings.Company.Phone, settings.Company.Email }.Where(x => !string.IsNullOrWhiteSpace(x))), string.Join(" - ", new[] { settings.Company.VatNumber, settings.Company.FiscalCode }.Where(x => !string.IsNullOrWhiteSpace(x))) }.Where(x => !string.IsNullOrWhiteSpace(x)));
        document.Branding.HeaderLayout = template.HeaderLayout; document.Branding.LogoPath = settings.Company.LogoPath;
        document.Branding.LogoSize = template.LogoSize; document.Branding.LogoPosition = template.LogoPosition;
        document.Branding.PrimaryColor = template.PrimaryColor; document.Branding.DocumentLabel = "DASHBOARD GOVERNANCE CAPA";
        document.Branding.DocumentCode = code; document.Branding.DocumentVersion = template.DocumentVersion;
        document.Branding.FooterText = template.FooterText; document.Branding.ConfidentialityText = template.ConfidentialityText;
        document.Branding.ShowLogo = template.ShowLogo; document.Branding.ShowCompanyDetails = template.ShowCompanyDetails;
        document.Branding.ShowDocumentMetadata = template.ShowDocumentMetadata; document.Branding.ShowFooter = template.ShowFooter;
        document.Branding.ShowPageNumber = template.ShowPageNumber; document.Branding.ShowPrintTimestamp = template.ShowPrintTimestamp;
    }
}
