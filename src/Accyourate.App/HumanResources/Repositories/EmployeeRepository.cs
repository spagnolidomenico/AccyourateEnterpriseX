using Microsoft.Data.Sqlite;
using Accyourate.App.HumanResources.Database;
using Accyourate.App.HumanResources.Models;

namespace Accyourate.App.HumanResources.Repositories;

public sealed class EmployeeRepository : HrRepositoryBase
{
    public EmployeeRepository(HrDatabase? database = null) : base(database) { }

    public IReadOnlyList<Employee> GetAll()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, EmployeeCode, FirstName, LastName, Email, Phone, RoleId, DepartmentId,
                   SiteId, ManagerId, EmploymentStatus, HireDate, TerminationDate, Notes, CreatedAt, UpdatedAt
            FROM Employees
            ORDER BY LastName, FirstName;
        """;

        using var reader = command.ExecuteReader();
        var result = new List<Employee>();
        while (reader.Read())
            result.Add(ReadEmployee(reader));
        return result;
    }

    public Employee? GetById(int id)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, EmployeeCode, FirstName, LastName, Email, Phone, RoleId, DepartmentId,
                   SiteId, ManagerId, EmploymentStatus, HireDate, TerminationDate, Notes, CreatedAt, UpdatedAt
            FROM Employees
            WHERE Id = $Id;
        """;
        command.Parameters.AddWithValue("$Id", id);

        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadEmployee(reader) : null;
    }

    public IReadOnlyList<Employee> Search(string query)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, EmployeeCode, FirstName, LastName, Email, Phone, RoleId, DepartmentId,
                   SiteId, ManagerId, EmploymentStatus, HireDate, TerminationDate, Notes, CreatedAt, UpdatedAt
            FROM Employees
            WHERE EmployeeCode LIKE $Query
               OR FirstName LIKE $Query
               OR LastName LIKE $Query
               OR Email LIKE $Query
               OR Phone LIKE $Query
               OR Notes LIKE $Query
            ORDER BY LastName, FirstName;
        """;
        command.Parameters.AddWithValue("$Query", $"%{query}%");

        using var reader = command.ExecuteReader();
        var result = new List<Employee>();
        while (reader.Read())
            result.Add(ReadEmployee(reader));
        return result;
    }

    public int Create(Employee employee)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();

        if (string.IsNullOrWhiteSpace(employee.EmployeeCode))
            employee.EmployeeCode = NextEmployeeCode(connection);

        var now = DateTime.Now.ToString("s");
        employee.CreatedAt = now;
        employee.UpdatedAt = now;

        command.CommandText = """
            INSERT INTO Employees (
                EmployeeCode, FirstName, LastName, Email, Phone, RoleId, DepartmentId, SiteId,
                ManagerId, EmploymentStatus, HireDate, TerminationDate, Notes, CreatedAt, UpdatedAt
            )
            VALUES (
                $EmployeeCode, $FirstName, $LastName, $Email, $Phone, $RoleId, $DepartmentId, $SiteId,
                $ManagerId, $EmploymentStatus, $HireDate, $TerminationDate, $Notes, $CreatedAt, $UpdatedAt
            );
            SELECT last_insert_rowid();
        """;
        AddEmployeeParameters(command, employee);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void Update(Employee employee)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();

        employee.UpdatedAt = DateTime.Now.ToString("s");

        command.CommandText = """
            UPDATE Employees
            SET EmployeeCode = $EmployeeCode,
                FirstName = $FirstName,
                LastName = $LastName,
                Email = $Email,
                Phone = $Phone,
                RoleId = $RoleId,
                DepartmentId = $DepartmentId,
                SiteId = $SiteId,
                ManagerId = $ManagerId,
                EmploymentStatus = $EmploymentStatus,
                HireDate = $HireDate,
                TerminationDate = $TerminationDate,
                Notes = $Notes,
                UpdatedAt = $UpdatedAt
            WHERE Id = $Id;
        """;
        command.Parameters.AddWithValue("$Id", employee.Id);
        AddEmployeeParameters(command, employee);
        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Employees WHERE Id = $Id;";
        command.Parameters.AddWithValue("$Id", id);
        command.ExecuteNonQuery();
    }

    private static string NextEmployeeCode(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT IFNULL(MAX(Id), 0) + 1 FROM Employees;";
        var next = Convert.ToInt32(command.ExecuteScalar());
        return $"EMP-{next:000000}";
    }

    private static void AddEmployeeParameters(SqliteCommand command, Employee employee)
    {
        command.Parameters.AddWithValue("$EmployeeCode", employee.EmployeeCode);
        command.Parameters.AddWithValue("$FirstName", employee.FirstName);
        command.Parameters.AddWithValue("$LastName", employee.LastName);
        command.Parameters.AddWithValue("$Email", employee.Email);
        command.Parameters.AddWithValue("$Phone", employee.Phone);
        command.Parameters.AddWithValue("$RoleId", employee.RoleId);
        command.Parameters.AddWithValue("$DepartmentId", employee.DepartmentId);
        command.Parameters.AddWithValue("$SiteId", employee.SiteId);
        command.Parameters.AddWithValue("$ManagerId", employee.ManagerId.HasValue ? employee.ManagerId.Value : DBNull.Value);
        command.Parameters.AddWithValue("$EmploymentStatus", employee.EmploymentStatus);
        command.Parameters.AddWithValue("$HireDate", employee.HireDate);
        command.Parameters.AddWithValue("$TerminationDate", employee.TerminationDate);
        command.Parameters.AddWithValue("$Notes", employee.Notes);
        command.Parameters.AddWithValue("$CreatedAt", employee.CreatedAt);
        command.Parameters.AddWithValue("$UpdatedAt", employee.UpdatedAt);
    }

    private static Employee ReadEmployee(SqliteDataReader reader)
    {
        return new Employee
        {
            Id = reader.GetInt32(0),
            EmployeeCode = reader.GetString(1),
            FirstName = reader.GetString(2),
            LastName = reader.GetString(3),
            Email = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
            Phone = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
            RoleId = reader.GetInt32(6),
            DepartmentId = reader.GetInt32(7),
            SiteId = reader.GetInt32(8),
            ManagerId = reader.IsDBNull(9) ? null : reader.GetInt32(9),
            EmploymentStatus = reader.GetString(10),
            HireDate = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
            TerminationDate = reader.IsDBNull(12) ? string.Empty : reader.GetString(12),
            Notes = reader.IsDBNull(13) ? string.Empty : reader.GetString(13),
            CreatedAt = reader.GetString(14),
            UpdatedAt = reader.GetString(15)
        };
    }
}
