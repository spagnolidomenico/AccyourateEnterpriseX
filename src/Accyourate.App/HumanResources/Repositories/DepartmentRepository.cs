using Microsoft.Data.Sqlite;
using Accyourate.App.HumanResources.Database;
using Accyourate.App.HumanResources.Models;

namespace Accyourate.App.HumanResources.Repositories;

public sealed class DepartmentRepository : HrRepositoryBase
{
    public DepartmentRepository(HrDatabase? database = null) : base(database) { }

    public IReadOnlyList<Department> GetAll()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Code, Name, SiteId, ManagerId, IsActive, Notes FROM Departments ORDER BY Name;";
        using var reader = command.ExecuteReader();
        var result = new List<Department>();
        while (reader.Read())
        {
            result.Add(new Department
            {
                Id = reader.GetInt32(0),
                Code = reader.GetString(1),
                Name = reader.GetString(2),
                SiteId = reader.GetInt32(3),
                ManagerId = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                IsActive = reader.GetInt32(5) == 1,
                Notes = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
            });
        }
        return result;
    }
}
