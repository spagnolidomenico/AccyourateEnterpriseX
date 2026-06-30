namespace Accyourate.App.Data;

public sealed class ShipmentRecord
{
    public long Id { get; set; }
    public string ShipmentCode { get; set; } = "";
    public long MedicalDeviceId { get; set; }
    public string DeviceCode { get; set; } = "";
    public string Destination { get; set; } = "";
    public string Status { get; set; } = "Preparazione";
    public string TrackingCode { get; set; } = "";
    public string OperatorName { get; set; } = "";
    public string ShipDate { get; set; } = "";
    public string ReturnDate { get; set; } = "";
    public string Notes { get; set; } = "";
    public string CreatedAt { get; set; } = "";
}
