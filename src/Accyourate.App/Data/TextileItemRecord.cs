namespace Accyourate.App.Data;

public sealed class TextileItemRecord
{
    public long Id { get; set; }
    public long MedicalDeviceId { get; set; }
    public string DeviceCode { get; set; } = "";
    public string TextileType { get; set; } = "";
    public string Size { get; set; } = "";
    public string Color { get; set; } = "";
    public string LotNumber { get; set; } = "";
    public string RfidCode { get; set; } = "";
    public int WashCount { get; set; }
    public string LastFunctionalTestDate { get; set; } = "";
    public string LastFunctionalTestResult { get; set; } = "";
    public string ConformityStatus { get; set; } = "";
    public string Notes { get; set; } = "";
}
