namespace Accyourate.App.Data;

public sealed class QualityTestRecord
{
    public long Id { get; set; }
    public long MedicalDeviceId { get; set; }
    public string DeviceCode { get; set; } = "";
    public string TestCode { get; set; } = "";
    public string ChecklistName { get; set; } = "";
    public string FunctionalResult { get; set; } = "";
    public string ElectricalResult { get; set; } = "";
    public string ConformityResult { get; set; } = "";
    public string FinalResult { get; set; } = "";
    public string OperatorName { get; set; } = "";
    public string TestDate { get; set; } = "";
    public string Notes { get; set; } = "";
    public string CreatedAt { get; set; } = "";
}
