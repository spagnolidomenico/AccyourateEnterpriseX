namespace Accyourate.App.Data;

public sealed class WorkflowEventRecord
{
    public long Id { get; set; }
    public string EntityType { get; set; } = "";
    public long EntityId { get; set; }
    public string EntityCode { get; set; } = "";
    public string FromStatus { get; set; } = "";
    public string ToStatus { get; set; } = "";
    public string EventType { get; set; } = "";
    public string Notes { get; set; } = "";
    public string CreatedBy { get; set; } = "";
    public string CreatedAt { get; set; } = "";
}
