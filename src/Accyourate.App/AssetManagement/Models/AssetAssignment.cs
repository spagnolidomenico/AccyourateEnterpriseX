namespace Accyourate.App.AssetManagement.Models;

public sealed class AssetAssignment
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public int EmployeeId { get; set; }
    public string AssignedAt { get; set; } = DateTime.Now.ToString("s");
    public string ReturnedAt { get; set; } = string.Empty;
    public string AssignedBy { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string Status { get; set; } = "Attiva";
}
