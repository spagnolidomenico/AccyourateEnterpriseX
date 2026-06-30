namespace Accyourate.App.Data;

public sealed class StockMovementRecord
{
    public long Id { get; set; }
    public long MedicalDeviceId { get; set; }
    public string DeviceCode { get; set; } = "";
    public string MovementType { get; set; } = "";
    public string FromLocationCode { get; set; } = "";
    public string ToLocationCode { get; set; } = "";
    public string Quantity { get; set; } = "1";
    public string Reason { get; set; } = "";
    public string OperatorName { get; set; } = "";
    public string CreatedAt { get; set; } = "";
}
