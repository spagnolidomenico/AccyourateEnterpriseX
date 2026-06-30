namespace Accyourate.App.Data;

public sealed class DocumentRecord
{
    public long Id { get; set; }
    public string DocumentCode { get; set; } = "";
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public string EntityType { get; set; } = "";
    public long? EntityId { get; set; }
    public string EntityCode { get; set; } = "";
    public string FileName { get; set; } = "";
    public string FilePath { get; set; } = "";
    public string Version { get; set; } = "1.0";
    public string Status { get; set; } = "Attivo";
    public string CreatedBy { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public string Notes { get; set; } = "";
}
