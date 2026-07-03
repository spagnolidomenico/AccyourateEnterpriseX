using Microsoft.Data.Sqlite;
using Accyourate.App.HumanResources.Database;
using Accyourate.App.HumanResources.Models;

namespace Accyourate.App.HumanResources.Repositories;

public sealed class RoleRepository : HrRepositoryBase
{
    public RoleRepository(HrDatabase? database = null) : base(database) { }

    public IReadOnlyList<HrRole> GetAll()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Code, Name, Area, IsActive, Notes FROM Roles ORDER BY Name;";
        using var reader = command.ExecuteReader();
        var result = new List<HrRole>();
        while (reader.Read())
        {
            result.Add(new HrRole
            {
                Id = reader.GetInt32(0),
                Code = reader.GetString(1),
                Name = reader.GetString(2),
                Area = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                IsActive = reader.GetInt32(4) == 1,
                Notes = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
            });
        }
        return result;
    }
}
