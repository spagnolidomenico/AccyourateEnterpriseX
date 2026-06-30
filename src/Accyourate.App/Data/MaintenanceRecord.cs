namespace Accyourate.App.Data;

public sealed class MaintenanceRecord
{
    public long Id { get; set; }
    public long MedicalDeviceId { get; set; }
    public string DeviceCode { get; set; } = "";
    public string MaintenanceCode { get; set; } = "";
    public string MaintenanceType { get; set; } = "";
    public string FaultDescription { get; set; } = "";
    public string ActionTaken { get; set; } = "";
    public string PartsReplaced { get; set; } = "";
    public string Result { get; set; } = "";
    public string OperatorName { get; set; } = "";
    public string MaintenanceDate { get; set; } = "";
    public string Notes { get; set; } = "";
    public string CreatedAt { get; set; } = "";
}
