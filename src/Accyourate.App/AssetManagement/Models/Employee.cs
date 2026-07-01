namespace Accyourate.App.AssetManagement.Models;

public sealed class Employee
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Site { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
