namespace Accyourate.App.Data;

public sealed class ProductionOrderRecord
{
    public long Id { get; set; }
    public string OrderCode { get; set; } = "";
    public long MedicalDeviceId { get; set; }
    public string DeviceCode { get; set; } = "";
    public string DeviceType { get; set; } = "";
    public string LotNumber { get; set; } = "";
    public string Status { get; set; } = "Pianificato";
    public string PlannedDate { get; set; } = "";
    public string OperatorName { get; set; } = "";
    public string Notes { get; set; } = "";
    public string CreatedAt { get; set; } = "";
}
