namespace Accyourate.App.AssetManagement.Models;

public static class StocktakeStatus
{
    public const string Open = "Aperta";
    public const string Review = "In verifica";
    public const string Closed = "Chiusa";
}

public sealed class SparePartStocktake
{
    public int Id { get; set; }
    public string SessionNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = StocktakeStatus.Open;
    public string OperatorName { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = DateTime.Now.ToString("s");
    public string ClosedAt { get; set; } = string.Empty;
    public List<SparePartStocktakeLine> Lines { get; set; } = new();
    public int CountedLines => Lines.Count(x => x.CountedQuantity.HasValue);
    public int DifferenceLines => Lines.Count(x => x.CountedQuantity.HasValue && x.Difference != 0);
    public decimal DifferenceValue => Lines.Where(x => x.CountedQuantity.HasValue).Sum(x => x.DifferenceValue);
}

public sealed class SparePartStocktakeLine
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
