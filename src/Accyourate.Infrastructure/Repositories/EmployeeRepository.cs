using Microsoft.Data.Sqlite;
using Accyourate.Core.Repositories;
using Accyourate.Domain.Employees;
using Accyourate.Infrastructure.Data;

namespace Accyourate.Infrastructure.Repositories;

public sealed class EmployeeRepository : SqliteRepositoryBase, IEmployeeRepository
{
    public EmployeeRepository(AccyourateDatabaseContext context) : base(context)
    {
    }

    public IReadOnlyList<Employee> GetAll()
    {
        using var connection = CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, FullName, Email, Phone, Role, DepartmentId, SiteId, IsActive, Notes
            FROM Employees
            ORDER BY FullName;
        """;

        using var reader = command.ExecuteReader();
        var result = new List<Employee>();

        while (reader.Read())
            result.Add(ReadEmployee(reader));

        return result;
    }

    public Employee? GetById(int id)
    {
        using var connection = CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, FullName, Email, Phone, Role, DepartmentId, SiteId, IsActive, Notes
            FROM Employees
            WHERE Id = $id;
        """;
        command.Parameters.AddWithValue("$id", id);

        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadEmployee(reader) : null;
    }

    public IReadOnlyList<Employee> Search(string query)
    {
        using var connection = CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, FullName, Email, Phone, Role, DepartmentId, SiteId, IsActive, Notes
            FROM Employees
            WHERE FullName LIKE $query
               OR Email LIKE $query
               OR Role LIKE $query
            ORDER BY FullName;
        """;
        command.Parameters.AddWithValue("$query", $"%{query}%");

        using var reader = command.ExecuteReader();
        var result = new List<Employee>();

        while (reader.Read())
            result.Add(ReadEmployee(reader));

        return result;
    }

    public IReadOnlyList<Employee> GetActive()
    {
        using var connection = CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, FullName, Email, Phone, Role, DepartmentId, SiteId, IsActive, Notes
            FROM Employees
            WHERE IsActive = 1
            ORDER BY FullName;
        """;

        using var reader = command.ExecuteReader();
        var result = new List<Employee>();

        while (reader.Read())
            result.Add(ReadEmployee(reader));

        return result;
    }

    public int Create(Employee employee)
    {
        using var connection = CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Employees (FullName, Email, Phone, Role, DepartmentId, SiteId, IsActive, Notes)
            VALUES ($FullName, $Email, $Phone, $Role, $DepartmentId, $SiteId, $IsActive, $Notes);
            SELECT last_insert_rowid();
        """;
        AddParameters(command, employee);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void Update(Employee employee)
    {
        using var connection = CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Employees
            SET FullName = $FullName,
                Email = $Email,
                Phone = $Phone,
                Role = $Role,
                DepartmentId = $DepartmentId,
                SiteId = $SiteId,
                IsActive = $IsActive,
                Notes = $Notes
            WHERE Id = $Id;
        """;
        command.Parameters.AddWithValue("$Id", employee.Id);
        AddParameters(command, employee);
        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Employees WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    private static Employee ReadEmployee(SqliteDataReader reader)
    {
        return new Employee
        {
            Id = reader.GetInt32(0),
            FullName = reader.GetString(1),
            Email = ReadString(reader, 2),
            Phone = ReadString(reader, 3),
            Role = ReadString(reader, 4),
            DepartmentId = reader.GetInt32(5),
            SiteId = reader.GetInt32(6),
            IsActive = reader.GetInt32(7) == 1,
            Notes = ReadString(reader, 8)
        };
    }

    private static void AddParameters(SqliteCommand command, Employee employee)
    {
        command.Parameters.AddWithValue("$FullName", employee.FullName);
        command.Parameters.AddWithValue("$Email", employee.Email);
        command.Parameters.AddWithValue("$Phone", employee.Phone);
        command.Parameters.AddWithValue("$Role", employee.Role);
        command.Parameters.AddWithValue("$DepartmentId", employee.DepartmentId);
        command.Parameters.AddWithValue("$SiteId", employee.SiteId);
        command.Parameters.AddWithValue("$IsActive", employee.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("$Notes", employee.Notes);
    }
}
