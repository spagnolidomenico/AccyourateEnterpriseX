namespace Accyourate.App.Data;

public sealed class ControlUnitRecord
{
    public long Id { get; set; }
    public long MedicalDeviceId { get; set; }
    public string DeviceCode { get; set; } = "";
    public string FirmwareVersion { get; set; } = "";
    public string HardwareRevision { get; set; } = "";
    public string MacAddress { get; set; } = "";
    public string BatteryStatus { get; set; } = "";
    public string LastFunctionalTestDate { get; set; } = "";
    public string LastFunctionalTestResult { get; set; } = "";
    public string Notes { get; set; } = "";
}
