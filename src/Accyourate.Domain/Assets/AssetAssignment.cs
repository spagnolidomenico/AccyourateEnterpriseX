namespace Accyourate.Domain.Assets;

public sealed class AssetAssignment
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public int EmployeeId { get; set; }
    public string AssignedAt { get; set; } = string.Empty;
    public string ReturnedAt { get; set; } = string.Empty;
    public string Status { get; set; } = "Attiva";
    public string Notes { get; set; } = string.Empty;
}
