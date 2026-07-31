using Accyourate.App.AssetManagement.Models;
using Accyourate.App.Platform.Pdf;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SparePartPickRequestPdfService
{
    public string Generate(SparePartPickRequest request,SparePartInventoryItem item,SparePartWarehouseLocation? location)
    {
        var document=new SimplePdfDocument{Title="Richiesta di prelievo ricambi"};
        document.Branding.DocumentLabel="RICHIESTA DI PRELIEVO";
        document.Branding.DocumentCode=request.RequestNumber;
        document.AddTitle("Richiesta di prelievo ricambi");
        document.AddKeyValue("Numero",request.RequestNumber);
        document.AddKeyValue("Stato",request.Status);
        document.AddKeyValue("Data richiesta",Date(request.CreatedAt));
        document.AddHeading("Materiale");
        document.AddKeyValue("Ricambio",$"{item.PartCode} - {item.Description}");
        document.AddKeyValue("Quantità",request.Quantity.ToString("N2"));
        document.AddKeyValue("Ubicazione preferita",location?.DisplayName??"Prelievo automatico");
        document.AddHeading("Destinazione");
        document.AddKeyValue("Richiedente",Dash(request.RequestedBy));
        document.AddKeyValue("Tecnico / destinatario",Dash(request.Technician));
        document.AddKeyValue("Manutenzione",request.MaintenanceTicketId>0?$"#{request.MaintenanceTicketId}":"Non collegata");
        document.AddKeyValue("Note",Dash(request.Notes));
        document.AddQrCode($"AXPICK:{request.RequestNumber}","SCANSIONA RICHIESTA");
        document.AddSignaturePair("Preparato da","Consegnato a");
        var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Accyourate Enterprise X","Richieste Prelievo");
        return new PdfExportService().Export(document,folder,request.RequestNumber);
    }
    private static string Date(string value)=>DateTime.TryParse(value,out var d)?d.ToString("dd/MM/yyyy HH:mm"):value;
    private static string Dash(string value)=>string.IsNullOrWhiteSpace(value)?"—":value;
}
