namespace Accyourate.App.HumanResources.Models;

public sealed class EmploymentContract
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string ContractType { get; set; } = Models.ContractType.Permanent;
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public string Notes { get; set; } = string.Empty;
}
