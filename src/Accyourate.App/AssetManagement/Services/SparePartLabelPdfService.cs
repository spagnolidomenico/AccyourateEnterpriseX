using Accyourate.App.AssetManagement.Models;
using Accyourate.App.Platform.Pdf;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SparePartLabelPdfService
{
    private readonly PdfExportService _export=new();
    public string Generate(IReadOnlyList<SparePartInventoryItem> items)
    {
        if(items.Count==0)throw new InvalidOperationException("Non sono presenti ricambi da etichettare.");
        var document=new SimplePdfDocument{Title="Etichette QR ricambi"};
        document.Branding.DocumentLabel="ETICHETTE MAGAZZINO RICAMBI";
        document.Branding.DocumentCode=$"LBL-{DateTime.Today:yyyyMMdd}";
        document.AddTitle("Etichette QR ricambi");
        foreach(var item in items)
        {
            document.AddHeading($"{item.PartCode} - {item.Description}");
            document.AddKeyValue("Ubicazione",item.Location);
            document.AddKeyValue("Fornitore",item.Supplier);
            document.AddQrCode($"AXPART:{item.PartCode}","SCANSIONA PER CONTEGGIO");
            document.AddBlank(40);
        }
        var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Accyourate Enterprise X","Etichette Ricambi");
        return _export.Export(document,folder,$"etichette-ricambi-{DateTime.Now:yyyyMMdd-HHmmss}.pdf");
    }

    public string GenerateLocations(IReadOnlyList<SparePartWarehouseLocation> locations)
    {
        if(locations.Count==0)throw new InvalidOperationException("Non sono presenti ubicazioni da etichettare.");
        var document=new SimplePdfDocument{Title="Etichette QR ubicazioni"};
        document.Branding.DocumentLabel="ETICHETTE UBICAZIONI MAGAZZINO";
        document.Branding.DocumentCode=$"LOC-{DateTime.Today:yyyyMMdd}";
        document.AddTitle("Etichette QR ubicazioni");
        foreach(var location in locations)
        {
            document.AddHeading($"{location.Code} - {location.Name}");
            document.AddKeyValue("Magazzino",location.Warehouse);
            document.AddKeyValue("Posizione",$"{location.Aisle} {location.Shelf}".Trim());
            document.AddQrCode($"AXLOC:{location.Code}","SCANSIONA UBICAZIONE");
            document.AddBlank(40);
        }
        var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Accyourate Enterprise X","Etichette Ubicazioni");
        return _export.Export(document,folder,$"etichette-ubicazioni-{DateTime.Now:yyyyMMdd-HHmmss}.pdf");
    }
}
