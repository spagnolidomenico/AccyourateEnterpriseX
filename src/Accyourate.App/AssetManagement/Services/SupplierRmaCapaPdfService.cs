using Accyourate.App.Platform.Pdf;
using Accyourate.App.Platform.Settings;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SupplierRmaCapaPdfService
{
    private readonly SettingsService _settings=new();

    public string Generate(IReadOnlyList<SupplierRmaCorrectiveAction> actions)
    {
        var settings=_settings.Load();var template=settings.DocumentTemplate??new DocumentTemplateSettings();
        var open=actions.Count(x=>x.Status is "Aperta" or "In corso");var overdue=actions.Count(x=>x.IsOverdue);var completed=actions.Count(x=>x.Status=="Completata");var awaiting=actions.Count(x=>x.Status=="Completata"&&x.EffectivenessStatus=="Da verificare");var effective=actions.Count(x=>x.EffectivenessStatus=="Efficace");var ineffective=actions.Count(x=>x.EffectivenessStatus=="Non efficace");
        var document=new SimplePdfDocument{Title="Registro CAPA RMA"};ApplyBranding(document,settings,template,$"CAPA-RMA-{DateTime.Now:yyyyMMdd-HHmm}");
        document.AddTitle("Registro azioni correttive e verifica efficacia RMA");
        document.AddKeyValue("Data elaborazione",DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        document.AddKeyValue("Periodo osservato",Period(actions));
        document.AddHeading("Indicatori CAPA");
        document.AddKeyValue("Azioni totali",actions.Count.ToString());document.AddKeyValue("Azioni aperte o in corso",open.ToString());document.AddKeyValue("Azioni scadute",overdue.ToString());document.AddKeyValue("Azioni completate",completed.ToString());document.AddKeyValue("Efficacia da verificare",awaiting.ToString());document.AddKeyValue("Azioni efficaci",effective.ToString());document.AddKeyValue("Azioni non efficaci",ineffective.ToString());
        document.AddStatus("Esito generale",overdue==0&&ineffective==0&&awaiting==0?"Conforme":"Richiede attenzione");
        document.AddHeading("Indicatori di efficacia");
        var verified=effective+ineffective;document.AddKeyValue("Tasso di efficacia",verified==0?"Non calcolabile":$"{effective*100d/verified:N1}%");document.AddKeyValue("Tasso di completamento",actions.Count==0?"Non calcolabile":$"{completed*100d/actions.Count:N1}%");
        AddSection(document,"Azioni aperte e in corso",actions.Where(x=>x.Status is "Aperta" or "In corso").OrderByDescending(x=>x.IsOverdue).ThenBy(x=>Parse(x.DueDate)));
        AddSection(document,"Azioni completate da verificare",actions.Where(x=>x.Status=="Completata"&&x.EffectivenessStatus=="Da verificare").OrderBy(x=>Parse(x.EffectivenessReviewDate)));
        AddSection(document,"Verifiche di efficacia registrate",actions.Where(x=>x.EffectivenessStatus is "Efficace" or "Non efficace").OrderByDescending(x=>Parse(x.EffectivenessVerifiedAt)));
        document.AddHeading("Nota metodologica");document.AddText("Il registro CAPA riepiloga le azioni correttive collegate alle verifiche di conformita RMA. Un'azione e considerata chiusa positivamente soltanto dopo la registrazione di un esito di efficacia supportato da evidenze.",9);
        document.AddQrCode("AXRMA:CAPA:REGISTER","Registro CAPA RMA");document.AddSignaturePair("Responsabile qualita","Responsabile processo");
        var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Accyourate Enterprise X","Report CAPA RMA");return new PdfExportService().Export(document,folder,$"Registro-CAPA-RMA-{DateTime.Now:yyyyMMdd-HHmmss}");
    }

    private static void AddSection(SimplePdfDocument document,string title,IEnumerable<SupplierRmaCorrectiveAction> source)
    {
        document.AddHeading(title);var values=source.ToList();if(values.Count==0){document.AddText("Nessuna voce presente.");return;}
        foreach(var value in values)
        {
            document.AddText($"{value.CaseNumber} - {value.Title}",11);
            document.AddText($"Stato: {value.Status} | Priorita: {value.Priority} | Responsabile: {Dash(value.Responsible)} | Scadenza: {Date(value.DueDate)}",9);
            document.AddText($"Efficacia: {Dash(value.EffectivenessStatus)} | Riesame: {Date(value.EffectivenessReviewDate)} | Verificata da: {Dash(value.EffectivenessVerifiedBy)}",9);
            if(!string.IsNullOrWhiteSpace(value.Description))document.AddText($"Azione: {value.Description}",9);
            if(!string.IsNullOrWhiteSpace(value.VerificationNotes))document.AddText($"Completamento: {value.VerificationNotes}",9);
            if(!string.IsNullOrWhiteSpace(value.EffectivenessNotes))document.AddText($"Evidenze efficacia: {value.EffectivenessNotes}",9);
        }
    }

    private static void ApplyBranding(SimplePdfDocument document,ApplicationSettings settings,DocumentTemplateSettings template,string code)
    {
        document.Branding.CompanyName=string.IsNullOrWhiteSpace(settings.Company.LegalName)?settings.Company.CompanyName:settings.Company.LegalName;
        document.Branding.CompanyDetailLines.AddRange(new[]{settings.Company.Address,string.Join(" ",new[]{settings.Company.City,settings.Company.Province}.Where(x=>!string.IsNullOrWhiteSpace(x))),string.Join(" - ",new[]{settings.Company.Phone,settings.Company.Email}.Where(x=>!string.IsNullOrWhiteSpace(x))),string.Join(" - ",new[]{settings.Company.VatNumber,settings.Company.FiscalCode}.Where(x=>!string.IsNullOrWhiteSpace(x)))}.Where(x=>!string.IsNullOrWhiteSpace(x)));
        document.Branding.HeaderLayout=template.HeaderLayout;document.Branding.LogoPath=settings.Company.LogoPath;document.Branding.LogoSize=template.LogoSize;document.Branding.LogoPosition=template.LogoPosition;document.Branding.PrimaryColor=template.PrimaryColor;document.Branding.DocumentLabel="REGISTRO CAPA RMA";document.Branding.DocumentCode=code;document.Branding.DocumentVersion=template.DocumentVersion;document.Branding.FooterText=template.FooterText;document.Branding.ConfidentialityText=template.ConfidentialityText;document.Branding.ShowLogo=template.ShowLogo;document.Branding.ShowCompanyDetails=template.ShowCompanyDetails;document.Branding.ShowDocumentMetadata=template.ShowDocumentMetadata;document.Branding.ShowFooter=template.ShowFooter;document.Branding.ShowPageNumber=template.ShowPageNumber;document.Branding.ShowPrintTimestamp=template.ShowPrintTimestamp;
    }
    private static DateTime Parse(string value)=>DateTime.TryParse(value,out var date)?date:DateTime.MinValue;private static string Date(string value)=>DateTime.TryParse(value,out var date)?date.ToString("dd/MM/yyyy"):"Non definita";private static string Dash(string value)=>string.IsNullOrWhiteSpace(value)?"Non specificato":value;private static string Period(IReadOnlyList<SupplierRmaCorrectiveAction> actions){var dates=actions.Select(x=>Parse(x.CreatedAt)).Where(x=>x>DateTime.MinValue).OrderBy(x=>x).ToList();return dates.Count==0?"Nessuna azione":$"{dates.First():dd/MM/yyyy} - {DateTime.Today:dd/MM/yyyy}";}
}
