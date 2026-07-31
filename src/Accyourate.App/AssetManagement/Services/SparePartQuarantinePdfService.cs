using Accyourate.App.AssetManagement.Models;
using Accyourate.App.Platform.Pdf;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SparePartQuarantinePdfService
{
    public string Generate(SparePartQuarantineItem value,SparePartReturn source,SparePartInventoryItem item,SparePartWarehouseLocation? location)
    {
        var document=new SimplePdfDocument{Title="Verbale quarantena ricambi"};document.Branding.DocumentLabel="VERBALE QUARANTENA";document.Branding.DocumentCode=value.CaseNumber;
        document.AddTitle("Verbale gestione materiale in quarantena");
        document.AddKeyValue("Numero pratica",value.CaseNumber);document.AddKeyValue("Reso originario",source.ReturnNumber);document.AddKeyValue("Data apertura",Date(value.CreatedAt));document.AddStatus("Stato pratica",value.Status);
        document.AddHeading("Materiale");document.AddKeyValue("Ricambio",$"{item.PartCode} - {item.Description}");document.AddKeyValue("Quantità",value.Quantity.ToString("N2"));document.AddKeyValue("Condizione iniziale",value.InitialCondition);document.AddKeyValue("Ubicazione",location?.DisplayName??"Non specificata");
        document.AddHeading("Valutazione");document.AddKeyValue("Costo stimato",$"EUR {value.EstimatedCost:N2}");document.AddKeyValue("Note",Dash(value.EvaluationNotes));document.AddKeyValue("Autorizzato da",Dash(value.AuthorizedBy));document.AddKeyValue("Chiusura",string.IsNullOrWhiteSpace(value.ClosedAt)?"Pratica aperta":Date(value.ClosedAt));
        document.AddQrCode($"AXQUAR:{value.CaseNumber}",string.Empty);
        document.AddSignaturePair("Responsabile magazzino","Responsabile autorizzazione");
        var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Accyourate Enterprise X","Quarantena Ricambi");
        return new PdfExportService().Export(document,folder,value.CaseNumber);
    }
    private static string Date(string value)=>DateTime.TryParse(value,out var d)?d.ToString("dd/MM/yyyy HH:mm"):value;
    private static string Dash(string value)=>string.IsNullOrWhiteSpace(value)?"—":value;
}
