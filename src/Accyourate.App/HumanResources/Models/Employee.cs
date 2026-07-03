namespace Accyourate.App.HumanResources.Models;

public sealed class Employee
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public int DepartmentId { get; set; }
    public int SiteId { get; set; }
    public int? ManagerId { get; set; }
    public string EmploymentStatus { get; set; } = Models.EmploymentStatus.Active;
    public string HireDate { get; set; } = string.Empty;
    public string TerminationDate { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = DateTime.Now.ToString("s");
    public string UpdatedAt { get; set; } = DateTime.Now.ToString("s");
}
