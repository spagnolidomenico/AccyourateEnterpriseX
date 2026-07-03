namespace Accyourate.App.AssetManagement.DeliveryReports;

public sealed class DeliveryReportItem
{
    public int Id { get; set; }
    public int DeliveryReportId { get; set; }
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Condition { get; set; } = "Buono";
    public string Notes { get; set; } = string.Empty;
}
