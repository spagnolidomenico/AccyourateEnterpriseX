namespace Accyourate.App.Platform.Audit;

public sealed class AuditRecord
{
    public int Id { get; set; }
    public string Action { get; set; } = AuditAction.System;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string EntityLabel { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Severity { get; set; } = AuditSeverity.Info;
    public string CreatedAt { get; set; } = DateTime.Now.ToString("s");
    public string SourceModule { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
}
