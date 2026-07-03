using Accyourate.App.HumanResources.Models;
using Accyourate.App.HumanResources.Repositories;

namespace Accyourate.App.HumanResources.Services;

public sealed class HrLookupService
{
    private readonly SiteRepository _sites = new();
    private readonly DepartmentRepository _departments = new();
    private readonly RoleRepository _roles = new();

    public IReadOnlyList<Site> GetSites() => _sites.GetAll();
    public IReadOnlyList<Department> GetDepartments() => _departments.GetAll();
    public IReadOnlyList<HrRole> GetRoles() => _roles.GetAll();
}
