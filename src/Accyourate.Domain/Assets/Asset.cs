namespace Accyourate.Domain.Assets;

public sealed class Asset
{
    public int Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string AssetTag { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PurchaseDate { get; set; } = string.Empty;
    public string WarrantyEndDate { get; set; } = string.Empty;
    public string OperatingSystem { get; set; } = string.Empty;
    public bool BitLockerEnabled { get; set; }
    public string Notes { get; set; } = string.Empty;
}
