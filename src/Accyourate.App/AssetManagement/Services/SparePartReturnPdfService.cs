using Accyourate.App.AssetManagement.Models;
using Accyourate.App.Platform.Pdf;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SparePartReturnPdfService
{
    public string Generate(SparePartReturn value,SparePartPickRequest request,SparePartInventoryItem item,SparePartWarehouseLocation? location)
    {
        var document=new SimplePdfDocument{Title="Verbale reso ricambi"};
        document.Branding.DocumentLabel="VERBALE RESO RICAMBI";document.Branding.DocumentCode=value.ReturnNumber;
        document.AddTitle("Verbale di restituzione ricambi");
        document.AddKeyValue("Numero reso",value.ReturnNumber);document.AddKeyValue("Richiesta originaria",request.RequestNumber);
        document.AddKeyValue("Data",Date(value.CreatedAt));document.AddKeyValue("Operatore",Dash(value.OperatorName));
        document.AddHeading("Materiale restituito");
        document.AddKeyValue("Ricambio",$"{item.PartCode} - {item.Description}");
        document.AddKeyValue("Quantità",value.Quantity.ToString("N2"));document.AddStatus("Condizione",value.Condition);
        document.AddKeyValue("Esito",value.Condition==SparePartReturnCondition.Reusable?"Reintegrato in magazzino":"Non reintegrato nella giacenza disponibile");
        document.AddKeyValue("Ubicazione",location?.DisplayName??"Non applicabile");
        document.AddKeyValue("Motivo",Dash(value.Reason));document.AddKeyValue("Note",Dash(value.Notes));
        document.AddQrCode($"AXRETURN:{value.ReturnNumber}","SCANSIONA RESO");
        document.AddSignaturePair("Restituito da","Verificato da");
        var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Accyourate Enterprise X","Resi Ricambi");
        return new PdfExportService().Export(document,folder,value.ReturnNumber);
    }
    private static string Date(string value)=>DateTime.TryParse(value,out var d)?d.ToString("dd/MM/yyyy HH:mm"):value;
    private static string Dash(string value)=>string.IsNullOrWhiteSpace(value)?"—":value;
}
