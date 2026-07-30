namespace Accyourate.App.AssetManagement.Models;

public sealed class LocationStocktake
{
    public int Id { get; set; }
    public string SessionNumber { get; set; } = string.Empty;
    public int LocationId { get; set; }
    public string Status { get; set; } = StocktakeStatus.Open;
    public string OperatorName { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = DateTime.Now.ToString("s");
    public string ClosedAt { get; set; } = string.Empty;
    public List<LocationStocktakeLine> Lines { get; set; } = new();
    public int CountedLines => Lines.Count(x => x.CountedQuantity.HasValue);
    public int DifferenceLines => Lines.Count(x => x.CountedQuantity.HasValue && x.Difference != 0);
}

public sealed class LocationStocktakeLine
{
    public int Id { get; set; }
    public int StocktakeId { get; set; }
    public int InventoryItemId { get; set; }
    public string PartCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal ExpectedQuantity { get; set; }
    public decimal? CountedQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public string Notes { get; set; } = string.Empty;
    public decimal Difference => (CountedQuantity ?? ExpectedQuantity) - ExpectedQuantity;
    public decimal DifferenceValue => Difference * UnitCost;
}
