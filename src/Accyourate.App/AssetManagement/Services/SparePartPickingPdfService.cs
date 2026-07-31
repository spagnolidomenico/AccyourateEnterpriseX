using Accyourate.App.AssetManagement.Models;
using Accyourate.App.Platform.Pdf;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SparePartPickingPdfService
{
    private readonly PdfExportService _export=new();

    public string Generate(
        IReadOnlyList<SparePartLocationPick> picks,
        IReadOnlyDictionary<int,SparePartInventoryItem> items,
        IReadOnlyDictionary<int,SparePartWarehouseLocation> locations,
        DateTimeOffset? from=null,DateTimeOffset? to=null)
    {
        if(picks.Count==0)throw new InvalidOperationException("Non sono presenti prelievi da stampare.");
        var number=$"PICK-{DateTime.Now:yyyyMMdd-HHmmss}";
        var document=new SimplePdfDocument{Title="Picking list ricambi"};
        document.Branding.DocumentLabel="PICKING LIST MAGAZZINO";
        document.Branding.DocumentCode=number;
        document.Branding.DocumentVersion="1.0";
        document.AddTitle("Picking list ricambi");
        document.AddKeyValue("Numero documento",number);
        document.AddKeyValue("Periodo",Period(from,to));
        document.AddKeyValue("Righe di prelievo",picks.Count.ToString());
        document.AddKeyValue("Quantità complessiva",picks.Sum(x=>x.Quantity).ToString("N2"));

        var groups=picks.GroupBy(x=>x.Reference??string.Empty)
            .OrderBy(x=>string.IsNullOrWhiteSpace(x.Key)?1:0).ThenBy(x=>x.Key);
        foreach(var group in groups)
        {
            document.AddHeading(string.IsNullOrWhiteSpace(group.Key)?"Prelievi senza riferimento":$"Riferimento {group.Key}");
            foreach(var pick in group.OrderBy(x=>x.CreatedAt).ThenBy(x=>x.LocationId))
            {
                items.TryGetValue(pick.InventoryItemId,out var item);locations.TryGetValue(pick.LocationId,out var location);
                document.AddKeyValue(
                    item is null?$"Ricambio #{pick.InventoryItemId}":$"{item.PartCode} - {item.Description}",
                    $"{pick.Quantity:N2} da {location?.Code??"#"+pick.LocationId}");
                document.AddText($"{Date(pick.CreatedAt)} | Operatore: {Dash(pick.OperatorName)} | Note: {Dash(pick.Notes)}",9);
            }
        }
        document.AddBlank(12);
        document.AddSignaturePair("Preparato da","Verificato da");
        var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Accyourate Enterprise X","Picking List");
        return _export.Export(document,folder,number);
    }

    private static string Period(DateTimeOffset? from,DateTimeOffset? to)
    {
        if(from.HasValue&&to.HasValue)return $"{from.Value:dd/MM/yyyy} - {to.Value:dd/MM/yyyy}";
        if(from.HasValue)return $"Dal {from.Value:dd/MM/yyyy}";
        if(to.HasValue)return $"Fino al {to.Value:dd/MM/yyyy}";
        return "Tutti i prelievi visualizzati";
    }
    private static string Date(string value)=>DateTime.TryParse(value,out var date)?date.ToString("dd/MM/yyyy HH:mm"):value;
    private static string Dash(string value)=>string.IsNullOrWhiteSpace(value)?"—":value;
}
