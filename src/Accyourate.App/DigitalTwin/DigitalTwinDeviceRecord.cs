namespace Accyourate.App.DigitalTwin;

public sealed class DigitalTwinDeviceRecord
{
    public string Code { get; init; } = "";
    public string Type { get; init; } = "";
    public string Model { get; init; } = "";
    public string SerialNumber { get; init; } = "";
    public string Rfid { get; init; } = "";
    public string Status { get; init; } = "";
    public int BatteryLevel { get; init; }
    public int HeartRate { get; init; }
    public string EcgStatus { get; init; } = "";
    public string SignalQuality { get; init; } = "";
    public string Firmware { get; init; } = "";
    public string AssignedTo { get; init; } = "";
}
