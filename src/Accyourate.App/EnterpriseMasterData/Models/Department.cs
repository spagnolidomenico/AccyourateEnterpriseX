namespace Accyourate.App.EnterpriseMasterData.Models;

public sealed class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SiteId { get; set; }
    public int ManagerEmployeeId { get; set; }
}
