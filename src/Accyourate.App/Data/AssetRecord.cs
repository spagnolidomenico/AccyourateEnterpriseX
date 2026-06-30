namespace Accyourate.App.Data;

public sealed class AssetRecord
{
    public long Id { get; set; }
    public string AssetCode { get; set; } = "";
    public string Category { get; set; } = "";
    public string Brand { get; set; } = "";
    public string Model { get; set; } = "";
    public string SerialNumber { get; set; } = "";
    public string OperatingSystem { get; set; } = "";
    public string Status { get; set; } = "Disponibile";
    public long? AssignedEmployeeId { get; set; }
    public string AssignedEmployeeName { get; set; } = "";
    public string PurchaseDate { get; set; } = "";
    public string WarrantyEnd { get; set; } = "";
    public string Notes { get; set; } = "";
    public bool IsArchived { get; set; }
    public string CreatedAt { get; set; } = "";
}
