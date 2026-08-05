using Accyourate.App.Platform.Pdf;
using Accyourate.App.Platform.Settings;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SupplierRmaCompliancePdfService
{
    private readonly SettingsService _settings=new();
    public string Generate(SupplierRmaComplianceAudit audit)
    {
        var settings=_settings.Load();var template=settings.DocumentTemplate??new DocumentTemplateSettings();var document=new SimplePdfDocument{Title=$"Verifica conformità {audit.CaseNumber}"};Apply(document,settings,template,$"CONF-{audit.CaseNumber}-{audit.Id:D5}");
        document.AddTitle("Verifica conformità fascicolo RMA");document.AddStatus("Esito",audit.Status);document.AddKeyValue("Pratica",audit.CaseNumber);document.AddKeyValue("Responsabile",audit.Responsible);document.AddKeyValue("Data verifica",Date(audit.VerifiedAt));document.AddKeyValue("Registrata da",audit.CreatedBy);
        document.AddHeading("Checklist delle evidenze");foreach(var x in audit.Checks){document.AddText($"{(x.IsCompliant?"CONFORME":"NON CONFORME")} - {x.Label} ({(x.IsRequired?"obbligatorio":"facoltativo")})",10);if(!string.IsNullOrWhiteSpace(x.Notes))document.AddText($"Note: {x.Notes}",9);}
        document.AddHeading("Rilievi e azioni");document.AddKeyValue("Rilievi / non conformità",Dash(audit.Findings));document.AddKeyValue("Azioni correttive",Dash(audit.CorrectiveActions));document.AddHeading("Collegamento al fascicolo");var dossier=SupplierRmaValidationService.DossierPath(audit.CaseNumber);document.AddKeyValue("Fascicolo",File.Exists(dossier)?dossier:"Non disponibile");document.AddQrCode($"AXRMA:{audit.CaseNumber}","Pratica RMA");document.AddSignaturePair("Responsabile verifica","Responsabile qualità");
        var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Accyourate Enterprise X","Conformità RMA");return new PdfExportService().Export(document,folder,$"Conformita-{audit.CaseNumber}-{audit.Id:D5}");
    }
    private static void Apply(SimplePdfDocument d,ApplicationSettings s,DocumentTemplateSettings t,string code){d.Branding.CompanyName=string.IsNullOrWhiteSpace(s.Company.LegalName)?s.Company.CompanyName:s.Company.LegalName;d.Branding.CompanyDetailLines.AddRange(new[]{s.Company.Address,string.Join(" ",new[]{s.Company.City,s.Company.Province}.Where(x=>!string.IsNullOrWhiteSpace(x))),string.Join(" - ",new[]{s.Company.Phone,s.Company.Email}.Where(x=>!string.IsNullOrWhiteSpace(x)))}.Where(x=>!string.IsNullOrWhiteSpace(x)));d.Branding.HeaderLayout=t.HeaderLayout;d.Branding.LogoPath=s.Company.LogoPath;d.Branding.LogoSize=t.LogoSize;d.Branding.LogoPosition=t.LogoPosition;d.Branding.PrimaryColor=t.PrimaryColor;d.Branding.DocumentLabel="VERIFICA CONFORMITÀ RMA";d.Branding.DocumentCode=code;d.Branding.DocumentVersion=t.DocumentVersion;d.Branding.FooterText=t.FooterText;d.Branding.ConfidentialityText=t.ConfidentialityText;d.Branding.ShowLogo=t.ShowLogo;d.Branding.ShowCompanyDetails=t.ShowCompanyDetails;d.Branding.ShowDocumentMetadata=t.ShowDocumentMetadata;d.Branding.ShowFooter=t.ShowFooter;d.Branding.ShowPageNumber=t.ShowPageNumber;d.Branding.ShowPrintTimestamp=t.ShowPrintTimestamp;}
    private static string Date(string value)=>DateTime.TryParse(value,out var d)?d.ToString("dd/MM/yyyy HH:mm"):"—";private static string Dash(string value)=>string.IsNullOrWhiteSpace(value)?"Nessuna indicazione":value;
}
