namespace Accyourate.App.AssetManagement.Models;

public sealed class SparePartWarehouseLocation
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public string Aisle { get; set; } = string.Empty;
    public string Shelf { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string DisplayName => $"{Code} · {Name}";
}

public sealed class SparePartLocationBalance
{
    public int InventoryItemId { get; set; }
    public int LocationId { get; set; }
    public decimal Quantity { get; set; }
}

public sealed class SparePartLocationTransfer
{
    public int Id { get; set; }
    public int InventoryItemId { get; set; }
    public int FromLocationId { get; set; }
    public int ToLocationId { get; set; }
    public decimal Quantity { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string OperatorName { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = DateTime.Now.ToString("s");
}
