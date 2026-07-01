namespace Accyourate.App.AssetManagement.Models;

public sealed class AssetDocument
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string UploadedAt { get; set; } = DateTime.Now.ToString("s");
    public string Notes { get; set; } = string.Empty;
}
