namespace Accyourate.App.Data;

public sealed class WarehouseLocationRecord
{
    public long Id { get; set; }
    public string LocationCode { get; set; } = "";
    public string Warehouse { get; set; } = "";
    public string Aisle { get; set; } = "";
    public string Shelf { get; set; } = "";
    public string Level { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
