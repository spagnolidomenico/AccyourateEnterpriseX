namespace Accyourate.App.Data;

public sealed class MedicalDeviceRecord
{
    public long Id { get; set; }
    public string DeviceCode { get; set; } = "";
    public string DeviceType { get; set; } = "";
    public string Model { get; set; } = "";
    public string SerialNumber { get; set; } = "";
    public string LotNumber { get; set; } = "";
    public string RfidCode { get; set; } = "";
    public string QrCode { get; set; } = "";
    public string Status { get; set; } = "Disponibile";
    public string ProductionDate { get; set; } = "";
    public string TestDate { get; set; } = "";
    public string Notes { get; set; } = "";
    public bool IsArchived { get; set; }
    public string CreatedAt { get; set; } = "";
}
