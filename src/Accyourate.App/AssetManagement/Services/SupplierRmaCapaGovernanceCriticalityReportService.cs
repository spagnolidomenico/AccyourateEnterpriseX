using Accyourate.App.Platform.Pdf;
using Accyourate.App.Platform.Settings;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SupplierRmaCapaGovernanceCriticalityReportService
{
    private readonly SupplierRmaCapaGovernanceDashboardService _dashboard = new();
    private readonly SupplierRmaCapaGovernanceActionService _actions = new();
    private readonly SettingsService _settings = new();

    public string Export()
    {
        var snapshot = _dashboard.Load();
        var current = Current(snapshot);
        var actions = _actions.GetAll().Where(x => x.SourceType == "Criticita Governance CAPA").ToList();
        var active = actions.Where(x => x.Status != "Completata").ToList();
        var completed = actions.Where(x => x.Status == "Completata").ToList();
        var verified = completed.Where(x => !current.TryGetValue(x.SourceReference, out var count) || count == 0).ToList();
        var failedChecks = actions.Sum(x => _actions.History(x.Id).Count(e => e.EventType == "Verifica non superata"));

        var settings = _settings.Load();
        var template = settings.DocumentTemplate ?? new DocumentTemplateSettings();
        var document = new SimplePdfDocument { Title = "Registro audit criticita Governance CAPA" };
        Brand(document, settings, template, $"CAPA-CRIT-{DateTime.Now:yyyyMMdd-HHmm}");
        document.AddTitle("Registro audit criticita Governance CAPA");
        document.AddKeyValue("Data elaborazione", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        document.AddKeyValue("Operatore", Environment.UserName);
        document.AddStatus("Esito corrente", snapshot.CriticalCount == 0 ? "Conforme" : $"{snapshot.CriticalCount} criticita aperte");

        document.AddHeading("Riepilogo controlli");
        document.AddKeyValue("Criticita correnti", snapshot.CriticalCount.ToString());
        document.AddKeyValue("Azioni in carico", active.Count.ToString());
        document.AddKeyValue("Azioni scadute", active.Count(x => x.IsOverdue).ToString());
        document.AddKeyValue("Chiusure verificate", verified.Count.ToString());
        document.AddKeyValue("Verifiche non superate", failedChecks.ToString());

        document.AddHeading("Anomalie e scadenze correnti");
        var present = current.Where(x => x.Value > 0).ToList();
        if (present.Count == 0) document.AddText("Nessuna anomalia o scadenza corrente.");
        foreach (var item in present) document.AddKeyValue(item.Key, item.Value.ToString());

        document.AddHeading("Azioni collegate");
        if (actions.Count == 0) document.AddText("Nessuna azione collegata alle criticita.");
        foreach (var item in actions)
        {
            var verification = _actions.History(item.Id).Count(x => x.EventType == "Verifica non superata");
            document.AddHeading($"#{item.Id:D6} - {item.Title}");
            document.AddText($"Stato: {item.Status} | Responsabile: {item.Owner} | Priorita: {item.Priority} | Scadenza: {Date(item.DueDate)}", 9);
            if (verification > 0) document.AddText($"Verifiche non superate: {verification}", 9);
            if (item.Status == "Completata") document.AddText(current.TryGetValue(item.SourceReference, out var count) && count > 0 ? "Esito controllo: criticita ancora presente." : "Esito controllo: chiusura verificata.", 9);
        }

        document.AddSignaturePair("Responsabile qualita", "Responsabile processo");
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Accyourate Enterprise X", "Governance CAPA", "Audit criticita");
        return new PdfExportService().Export(document, folder, $"Audit-Criticita-Governance-CAPA-{DateTime.Now:yyyyMMdd-HHmmss}");
    }

    private static Dictionary<string, int> Current(SupplierRmaCapaGovernanceSnapshot x) => new(StringComparer.OrdinalIgnoreCase)
    {
        ["Documenti fascicolo mancanti"] = x.MissingDocuments,
        ["Riesami fascicolo scaduti"] = x.ReviewsOverdue,
        ["Attestazioni fascicolo non valide"] = x.InvalidAttestations,
        ["Archivi attestazione mancanti"] = x.MissingAttestationArchives,
        ["Esportazioni modificate"] = x.InvalidExports,
        ["File esportazione mancanti"] = x.MissingExports,
        ["Conservazioni fascicolo scadute"] = x.RetentionOverdue,
        ["Riesami Governance scaduti"] = x.PeriodicReviewsOverdue,
        ["Attestazioni riesame non valide"] = x.InvalidPeriodicReviewAttestations,
        ["Conservazioni riesame non valide"] = x.InvalidPeriodicReviewRetentions,
        ["Riesami fascicolo in scadenza"] = x.ReviewsDue,
        ["Conservazioni fascicolo in scadenza"] = x.RetentionDue,
        ["Conservazioni riesame da gestire"] = x.PeriodicReviewRetentionsDue
    };

    private static string Date(string value) => DateTime.TryParse(value, out var date) ? date.ToString("dd/MM/yyyy") : value;

    private static void Brand(SimplePdfDocument document, ApplicationSettings settings, DocumentTemplateSettings template, string code)
    {
        document.Branding.CompanyName = string.IsNullOrWhiteSpace(settings.Company.LegalName) ? settings.Company.CompanyName : settings.Company.LegalName;
        document.Branding.CompanyDetailLines.AddRange(new[] { settings.Company.Address, string.Join(" ", new[] { settings.Company.City, settings.Company.Province }.Where(x => !string.IsNullOrWhiteSpace(x))), string.Join(" - ", new[] { settings.Company.Phone, settings.Company.Email }.Where(x => !string.IsNullOrWhiteSpace(x))), string.Join(" - ", new[] { settings.Company.VatNumber, settings.Company.FiscalCode }.Where(x => !string.IsNullOrWhiteSpace(x))) }.Where(x => !string.IsNullOrWhiteSpace(x)));
        document.Branding.HeaderLayout = template.HeaderLayout; document.Branding.LogoPath = settings.Company.LogoPath; document.Branding.LogoSize = template.LogoSize; document.Branding.LogoPosition = template.LogoPosition;
        document.Branding.PrimaryColor = template.PrimaryColor; document.Branding.DocumentLabel = "AUDIT CRITICITA GOVERNANCE CAPA"; document.Branding.DocumentCode = code; document.Branding.DocumentVersion = template.DocumentVersion;
        document.Branding.FooterText = template.FooterText; document.Branding.ConfidentialityText = template.ConfidentialityText; document.Branding.ShowLogo = template.ShowLogo; document.Branding.ShowCompanyDetails = template.ShowCompanyDetails;
        document.Branding.ShowDocumentMetadata = template.ShowDocumentMetadata; document.Branding.ShowFooter = template.ShowFooter; document.Branding.ShowPageNumber = template.ShowPageNumber; document.Branding.ShowPrintTimestamp = template.ShowPrintTimestamp;
    }
}
