namespace Accyourate.App.Infrastructure.Database;

public sealed class DatabaseMigrationRecord
{
    public string Version { get; set; } = "";
    public string Name { get; set; } = "";
    public string ScriptFile { get; set; } = "";
}
