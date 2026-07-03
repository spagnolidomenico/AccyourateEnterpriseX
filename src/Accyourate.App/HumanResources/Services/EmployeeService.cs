using Accyourate.App.HumanResources.Models;
using Accyourate.App.HumanResources.Repositories;
using Accyourate.App.Platform.Audit;
using Accyourate.App.Platform.Notifications;

namespace Accyourate.App.HumanResources.Services;

public sealed class EmployeeService
{
    private readonly EmployeeRepository _employees;
    private readonly AuditService _audit;
    private readonly NotificationService _notifications;

    public EmployeeService(EmployeeRepository? employees = null, AuditService? audit = null, NotificationService? notifications = null)
    {
        _employees = employees ?? new EmployeeRepository();
        _audit = audit ?? new AuditService();
        _notifications = notifications ?? new NotificationService();
    }

    public IReadOnlyList<Employee> GetAll() => _employees.GetAll();

    public Employee? GetById(int id) => _employees.GetById(id);

    public IReadOnlyList<Employee> Search(string query) => _employees.Search(query);

    public int Create(Employee employee, string userName = "System")
    {
        var id = _employees.Create(employee);

        _audit.Track(
            AuditAction.Created,
            $"Creato dipendente {employee.FullName}",
            "Employee",
            id.ToString(),
            employee.FullName,
            userName,
            AuditSeverity.Info,
            "HumanResources");

        _notifications.Publish(
            "Nuovo dipendente",
            $"{employee.FullName} è stato inserito in Human Resources.",
            NotificationCategory.MasterData,
            NotificationPriority.Info,
            userName,
            "open-employee",
            id.ToString());

        return id;
    }

    public void Update(Employee employee, string userName = "System")
    {
        _employees.Update(employee);

        _audit.Track(
            AuditAction.Updated,
            $"Aggiornato dipendente {employee.FullName}",
            "Employee",
            employee.Id.ToString(),
            employee.FullName,
            userName,
            AuditSeverity.Info,
            "HumanResources");
    }

    public void Delete(int id, string userName = "System")
    {
        var employee = _employees.GetById(id);
        _employees.Delete(id);

        _audit.Track(
            AuditAction.Deleted,
            $"Eliminato dipendente {employee?.FullName ?? id.ToString()}",
            "Employee",
            id.ToString(),
            employee?.FullName ?? string.Empty,
            userName,
            AuditSeverity.Warning,
            "HumanResources");
    }
}
