using Accyourate.Domain.Employees;

namespace Accyourate.Core.Repositories;

public interface IEmployeeRepository : IRepository<Employee>
{
    IReadOnlyList<Employee> Search(string query);
    IReadOnlyList<Employee> GetActive();
}
