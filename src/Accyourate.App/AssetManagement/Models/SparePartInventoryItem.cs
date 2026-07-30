namespace Accyourate.App.AssetManagement.Models;

public sealed class SparePartInventoryItem
{
    public int Id { get; set; }
    public string PartCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Supplier { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal MinimumQuantity { get; set; }
    public decimal AverageUnitCost { get; set; }
    public string UpdatedAt { get; set; } = DateTime.Now.ToString("s");
    public decimal StockValue => Quantity * AverageUnitCost;
    public bool IsLowStock => MinimumQuantity > 0 && Quantity < MinimumQuantity;
}

public sealed class SparePartInventoryMovement
{
    public int Id { get; set; }
    public int InventoryItemId { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = DateTime.Now.ToString("s");
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
}
