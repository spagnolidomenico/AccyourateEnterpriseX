namespace Accyourate.App.Data;

public sealed class EmployeeRecord
{
    public long Id { get; set; }
    public string EmployeeCode { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Department { get; set; } = "";
    public string JobTitle { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string HireDate { get; set; } = "";
    public bool IsArchived { get; set; }
    public string CreatedAt { get; set; } = "";
    public string FullName => $"{FirstName} {LastName}".Trim();
}
