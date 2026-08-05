using Accyourate.App.AssetManagement.Models;
using Accyourate.App.Platform.Pdf;
using Accyourate.App.Platform.Settings;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SupplierRmaAuditPdfService
{
    private readonly SettingsService _settings = new();

    public string Generate(IReadOnlyList<SparePartRmaCase> cases, IReadOnlyList<SupplierRmaDossierClosure> closures)
    {
        var validatedIds = closures.Select(x => x.RmaId).Distinct().ToHashSet();
        var complete = cases.Count(x => validatedIds.Contains(x.Id));
        var notValidated = cases.Count - complete;
        var overdue = cases.Where(IsOverdue).ToList();
        var closedWithoutValidation = cases.Where(x => x.Status == SparePartRmaStatus.Closed && !validatedIds.Contains(x.Id)).ToList();
        var latest = closures.GroupBy(x => x.RmaId).Select(x => x.OrderByDescending(y => Parse(y.ClosedAt)).First()).ToList();
        var durations = latest.Join(cases, x => x.RmaId, x => x.Id, (closure, rma) => (Start: Parse(rma.CreatedAt), End: Parse(closure.ClosedAt)))
            .Where(x => x.Start > DateTime.MinValue && x.End >= x.Start).Select(x => (x.End - x.Start).TotalDays).ToList();

        var settings = _settings.Load();
        var template = settings.DocumentTemplate ?? new DocumentTemplateSettings();
        var document = new SimplePdfDocument { Title = "Report audit RMA" };
        ApplyBranding(document, settings, template, $"AUD-RMA-{DateTime.Now:yyyyMMdd-HHmm}");
        document.AddTitle("Report audit pratiche RMA");
        document.AddKeyValue("Data elaborazione", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        document.AddKeyValue("Periodo osservato", Period(cases));

        document.AddHeading("Indicatori di controllo");
        document.AddKeyValue("Pratiche RMA totali", cases.Count.ToString());
        document.AddKeyValue("Fascicoli completi", complete.ToString());
        document.AddKeyValue("Pratiche non validate", notValidated.ToString());
        document.AddKeyValue("Pratiche scadute", overdue.Count.ToString());
        document.AddKeyValue("Pratiche chiuse non validate", closedWithoutValidation.Count.ToString());
        document.AddKeyValue("Tempo medio apertura-validazione", durations.Count == 0 ? "Non calcolabile" : $"{durations.Average():N1} giorni");
        document.AddStatus("Esito generale", notValidated == 0 && overdue.Count == 0 ? "Conforme" : "Richiede attenzione");

        document.AddHeading("Distribuzione per stato");
        foreach (var group in cases.GroupBy(x => x.Status).OrderBy(x => x.Key)) document.AddKeyValue(group.Key, group.Count().ToString());

        document.AddHeading("Pratiche che richiedono attenzione");
        var attention = cases.Where(x => IsOverdue(x) || !validatedIds.Contains(x.Id)).OrderByDescending(IsOverdue).ThenBy(x => Parse(x.DueDate)).Take(30).ToList();
        if (attention.Count == 0) document.AddText("Nessuna criticita rilevata.");
        foreach (var item in attention)
        {
            var reason = IsOverdue(item) ? "Scaduta" : item.Status == SparePartRmaStatus.Closed ? "Chiusa senza validazione" : "Fascicolo non validato";
            document.AddKeyValue(item.CaseNumber, $"{reason} - stato {item.Status} - scadenza {Date(item.DueDate)}");
        }

        document.AddHeading("Validazioni recenti");
        if (closures.Count == 0) document.AddText("Nessuna validazione registrata.");
        foreach (var item in closures.Take(30)) document.AddKeyValue(item.CaseNumber, $"{item.ValidationStatus} - {DateTimeValue(item.ClosedAt)} - {Dash(item.ClosedBy)}");

        document.AddHeading("Nota metodologica");
        document.AddText("Il report riepiloga lo stato delle pratiche RMA e dei relativi fascicoli alla data di elaborazione. Una pratica e considerata completa quando possiede almeno una validazione registrata.", 9);
        document.AddSignaturePair("Responsabile acquisti", "Responsabile qualita");
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Accyourate Enterprise X", "Report Audit RMA");
        return new PdfExportService().Export(document, folder, $"Report-Audit-RMA-{DateTime.Now:yyyyMMdd-HHmmss}");
    }

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
        document.Branding.HeaderLayout = template.HeaderLayout;
        document.Branding.LogoPath = settings.Company.LogoPath;
        document.Branding.LogoSize = template.LogoSize;
        document.Branding.LogoPosition = template.LogoPosition;
        document.Branding.PrimaryColor = template.PrimaryColor;
        document.Branding.DocumentLabel = "REPORT AUDIT RMA";
        document.Branding.DocumentCode = code;
        document.Branding.DocumentVersion = template.DocumentVersion;
        document.Branding.FooterText = template.FooterText;
        document.Branding.ConfidentialityText = template.ConfidentialityText;
        document.Branding.ShowLogo = template.ShowLogo;
        document.Branding.ShowCompanyDetails = template.ShowCompanyDetails;
        document.Branding.ShowDocumentMetadata = template.ShowDocumentMetadata;
        document.Branding.ShowFooter = template.ShowFooter;
        document.Branding.ShowPageNumber = template.ShowPageNumber;
        document.Branding.ShowPrintTimestamp = template.ShowPrintTimestamp;
    }

    private static bool IsOverdue(SparePartRmaCase item) => item.Status is not (SparePartRmaStatus.Closed or SparePartRmaStatus.Cancelled) && DateTime.TryParse(item.DueDate, out var due) && due.Date < DateTime.Today;
    private static string Period(IReadOnlyList<SparePartRmaCase> cases) { var dates = cases.Select(x => Parse(x.CreatedAt)).Where(x => x > DateTime.MinValue).OrderBy(x => x).ToList(); return dates.Count == 0 ? "Nessuna pratica" : $"{dates.First():dd/MM/yyyy} - {DateTime.Today:dd/MM/yyyy}"; }
    private static DateTime Parse(string value) => DateTime.TryParse(value, out var date) ? date : DateTime.MinValue;
    private static string Date(string value) => DateTime.TryParse(value, out var date) ? date.ToString("dd/MM/yyyy") : "Non definita";
    private static string DateTimeValue(string value) => DateTime.TryParse(value, out var date) ? date.ToString("dd/MM/yyyy HH:mm") : "Non definita";
    private static string Dash(string value) => string.IsNullOrWhiteSpace(value) ? "Non specificato" : value;
}
