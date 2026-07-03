namespace Accyourate.App.HumanResources.Models;

public sealed class EmployeeDocument
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ExpirationDate { get; set; } = string.Empty;
    public string UploadedAt { get; set; } = DateTime.Now.ToString("s");
    public string UploadedBy { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
