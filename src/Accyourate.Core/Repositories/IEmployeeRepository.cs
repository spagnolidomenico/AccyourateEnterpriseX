using Accyourate.Domain.Employees;

namespace Accyourate.Core.Repositories;

public interface IEmployeeRepository
{
    IReadOnlyList<Employee> GetAll();
    Employee? GetById(int id);
    int Create(Employee employee);
    void Update(Employee employee);
    void Delete(int id);
}
