namespace Accyourate.App.Data;

public sealed class DatabaseVersionRecord
{
    public long Id { get; set; }
    public string Version { get; set; } = "";
    public string Description { get; set; } = "";
    public string AppliedAt { get; set; } = "";
}
