namespace Accyourate.App.Data;

public sealed class DatabaseDiagnostics
{
    public string DatabasePath { get; set; } = "";
    public bool Exists { get; set; }
    public long SizeBytes { get; set; }
    public int UsersCount { get; set; }
    public int ActiveUsersCount { get; set; }
    public int AuditCount { get; set; }
}
