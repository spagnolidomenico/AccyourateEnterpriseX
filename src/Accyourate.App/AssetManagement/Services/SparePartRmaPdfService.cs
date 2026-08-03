using Accyourate.App.AssetManagement.Models;
using Accyourate.App.Platform.Pdf;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SparePartRmaPdfService
{
    public string Generate(SparePartRmaCase value, SparePartInventoryItem item, MaintenanceSupplier? supplier)
    {
        var document=new SimplePdfDocument{Title="Pratica RMA fornitore"};
        document.Branding.DocumentLabel="PRATICA RMA";document.Branding.DocumentCode=value.CaseNumber;
        document.AddTitle("Reso merce al fornitore");
        document.AddKeyValue("Numero pratica",value.CaseNumber);document.AddStatus("Stato",value.Status);
        document.AddKeyValue("Data apertura",Date(value.CreatedAt));document.AddKeyValue("Scadenza",Date(value.DueDate));
        document.AddHeading("Ricambio e fornitore");
        document.AddKeyValue("Ricambio",$"{item.PartCode} - {item.Description}");document.AddKeyValue("Quantità",value.Quantity.ToString("N2"));
        document.AddKeyValue("Fornitore",supplier?.Name??"Non specificato");document.AddKeyValue("Autorizzazione RMA",Dash(value.AuthorizationNumber));
        document.AddHeading("Spedizione");document.AddKeyValue("Corriere",Dash(value.Courier));document.AddKeyValue("Tracking",Dash(value.TrackingNumber));document.AddKeyValue("Data spedizione",Date(value.ShippedAt));
        document.AddHeading("Esito e costi");document.AddKeyValue("Esito",Dash(value.Outcome));document.AddKeyValue("Spese spedizione",$"EUR {value.ShippingCost:N2}");document.AddKeyValue("Costo risoluzione",$"EUR {value.ResolutionCost:N2}");document.AddKeyValue("Note",Dash(value.Notes));
        document.AddQrCode($"AXRMA:{value.CaseNumber}",string.Empty);document.AddSignaturePair("Responsabile magazzino","Fornitore");
        var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Accyourate Enterprise X","RMA Fornitori");
        return new PdfExportService().Export(document,folder,value.CaseNumber);
    }
    private static string Date(string value)=>DateTime.TryParse(value,out var d)?d.ToString("dd/MM/yyyy HH:mm"):"—";
    private static string Dash(string value)=>string.IsNullOrWhiteSpace(value)?"—":value;
}
