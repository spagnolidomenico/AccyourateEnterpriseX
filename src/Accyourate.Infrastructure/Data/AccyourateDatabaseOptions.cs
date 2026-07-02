namespace Accyourate.Infrastructure.Data;

public sealed class AccyourateDatabaseOptions
{
    public string DatabaseName { get; set; } = "accyourate-enterprise.db";
    public string AppFolderName { get; set; } = "AccyourateEnterpriseX";
    public string? ExplicitDatabasePath { get; set; }
}
