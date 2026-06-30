namespace Accyourate.App.Data;

public sealed class LaundryCycleRecord
{
    public long Id { get; set; }
    public long MedicalDeviceId { get; set; }
    public string DeviceCode { get; set; } = "";
    public string CycleCode { get; set; } = "";
    public string ProgramName { get; set; } = "";
    public string Temperature { get; set; } = "";
    public string WashDate { get; set; } = "";
    public string OperatorName { get; set; } = "";
    public string Result { get; set; } = "";
    public string Notes { get; set; } = "";
    public int WashCountAfter { get; set; }
    public string CreatedAt { get; set; } = "";
}
