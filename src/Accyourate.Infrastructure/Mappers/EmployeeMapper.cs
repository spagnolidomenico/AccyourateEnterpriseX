using Accyourate.Domain.Employees;

namespace Accyourate.Infrastructure.Mappers;

public static class EmployeeMapper
{
    public static Employee FromMasterData(
        int id,
        string fullName,
        string email,
        string phone,
        string role,
        int departmentId,
        int siteId,
        bool isActive,
        string notes)
    {
        return new Employee
        {
            Id = id,
            FullName = fullName,
            Email = email,
            Phone = phone,
            Role = role,
            DepartmentId = departmentId,
            SiteId = siteId,
            IsActive = isActive,
            Notes = notes
        };
    }
}
