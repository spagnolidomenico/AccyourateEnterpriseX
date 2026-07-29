using Accyourate.App.AssetManagement.Models;
using Accyourate.App.Platform.Pdf;
using Accyourate.App.Platform.Qr;
using Accyourate.App.Platform.Settings;

namespace Accyourate.App.AssetManagement.Services;

public sealed class MaintenancePdfService
{
    private readonly SettingsService _settings=new();
    public string Generate(MaintenanceTicket ticket,Asset asset,string generatedBy)
    {
        var settings=_settings.Load();var template=settings.DocumentTemplate??new DocumentTemplateSettings();
        var number=$"MNT-{DateTime.Now:yyyy}-{ticket.Id:D6}";
        var d=BuildDocument(ticket,asset,generatedBy,number,settings,template,
            new MaintenancePartsRepository().GetByTicket(ticket.Id));
        var folder=Path.Combine(Path.GetDirectoryName(_settings.GetDeliveryReportsFolder())!,"Verbali Manutenzione");
        return new PdfExportService().Export(d,folder,$"{number}_{asset.AssetCode}");
    }
    internal static SimplePdfDocument BuildDocument(MaintenanceTicket t,Asset a,string user,string number,ApplicationSettings s,DocumentTemplateSettings x,IReadOnlyList<MaintenancePart>? parts=null)
    {
        var d=new SimplePdfDocument{Title=$"Verbale manutenzione {number}"};
        d.Branding.CompanyName=string.IsNullOrWhiteSpace(s.Company.LegalName)?s.Company.CompanyName:s.Company.LegalName;
        d.Branding.CompanyDetailLines.AddRange(new[]{s.Company.Address,string.Join(" ",new[]{s.Company.City,s.Company.Province}.Where(v=>!string.IsNullOrWhiteSpace(v))),s.Company.Email}.Where(v=>!string.IsNullOrWhiteSpace(v)));
        d.Branding.HeaderLayout=x.HeaderLayout;d.Branding.LogoPath=s.Company.LogoPath;d.Branding.PrimaryColor=x.PrimaryColor;
        d.Branding.DocumentLabel="VERBALE DI MANUTENZIONE";d.Branding.DocumentCode=number;d.Branding.DocumentVersion=x.DocumentVersion;
        d.Branding.FooterText=x.FooterText;d.Branding.ConfidentialityText=x.ConfidentialityText;d.Branding.ShowLogo=x.ShowLogo;
        d.Branding.ShowCompanyDetails=x.ShowCompanyDetails;d.Branding.ShowDocumentMetadata=x.ShowDocumentMetadata;d.Branding.ShowFooter=x.ShowFooter;
        d.Branding.ShowPageNumber=x.ShowPageNumber;d.Branding.ShowPrintTimestamp=x.ShowPrintTimestamp;
        d.AddHeading("Verbale intervento di manutenzione");d.AddKeyValue("Numero",number);d.AddKeyValue("Operatore",user);
        d.AddHeading("Asset");d.AddKeyValue("Codice",a.AssetCode);d.AddKeyValue("Produttore e modello",$"{a.Manufacturer} {a.Model}".Trim());d.AddKeyValue("Seriale",a.SerialNumber);
        d.AddHeading("Intervento");d.AddKeyValue("Titolo",t.Title);d.AddKeyValue("Priorità e tecnico",$"{t.Priority} · {t.Technician}");d.AddKeyValue("Periodo",$"{Fmt(t.OpenedAt)} - {Fmt(t.ClosedAt)}");d.AddKeyValue("Costo",$"EUR {t.Cost:N2}");
        d.AddKeyValue("Scadenza SLA",Fmt(t.SlaDeadline));
        d.AddKeyValue("Esito SLA",SlaResult(t));
        d.AddKeyValue("Tempo di fermo",t.DowntimeMinutes>0?$"{t.DowntimeMinutes/60d:N1} ore":"Non registrato");
        d.AddHeading("Guasto segnalato");d.AddText(t.Description);d.AddHeading("Risoluzione");d.AddText(t.ResolutionNotes);
        if(parts is {Count:>0})
        {
            d.AddHeading("Ricambi e materiali");
            foreach(var part in parts)
                d.AddKeyValue(
                    string.IsNullOrWhiteSpace(part.PartCode)?part.Description:$"{part.PartCode} - {part.Description}",
                    $"{part.Quantity:N2} x EUR {part.UnitCost:N2} = EUR {part.TotalCost:N2} · {part.Supplier}");
            d.AddKeyValue("Totale ricambi",$"EUR {parts.Sum(part=>part.TotalCost):N2}");
        }
        if(x.ShowQrCodePlaceholder)d.AddQrCode(QrDestinationBuilder.Build(x,"maintenance",number,new[]{"Accyourate Enterprise X","Verbale manutenzione",$"Numero: {number}",$"Asset: {a.AssetCode}",$"Tecnico: {t.Technician}"}),$"QR {number}");
        if(x.ShowSignatures)d.AddSignaturePair("Tecnico","Responsabile");
        return d;
    }
    private static string Fmt(string v)=>DateTime.TryParse(v,out var d)?d.ToString("dd/MM/yyyy HH:mm"):v;
    private static string SlaResult(MaintenanceTicket ticket)
    {
        if(!DateTime.TryParse(ticket.SlaDeadline,out var deadline))return "Non definito";
        if(DateTime.TryParse(ticket.ClosedAt,out var closed))return closed<=deadline?"Rispettato":"Superato";
        return DateTime.Now<=deadline?"In corso":"Superato";
    }
}
