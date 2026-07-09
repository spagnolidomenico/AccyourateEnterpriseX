namespace Accyourate.App.HumanResources.Enterprise;

public sealed class HumanResourcesEmployeeRow
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string EmploymentStatus { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}
