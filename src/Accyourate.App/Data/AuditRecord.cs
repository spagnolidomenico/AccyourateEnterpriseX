namespace Accyourate.App.Data;

public sealed class AuditRecord
{
    public long Id { get; set; }
    public string Username { get; set; } = "";
    public string Action { get; set; } = "";
    public string Details { get; set; } = "";
    public string CreatedAt { get; set; } = "";
}
