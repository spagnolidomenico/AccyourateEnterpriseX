namespace Accyourate.App.DigitalTwin;

public sealed class DigitalTwinTelemetryRecord
{
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public string DeviceCode { get; init; } = "";
    public int HeartRate { get; init; }
    public int BatteryLevel { get; init; }
    public string EcgStatus { get; init; } = "";
    public string SignalQuality { get; init; } = "";
    public string EventType { get; init; } = "";
}
