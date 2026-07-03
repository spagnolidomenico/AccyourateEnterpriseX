using Microsoft.Data.Sqlite;
using Accyourate.App.HumanResources.Database;
using Accyourate.App.HumanResources.Models;

namespace Accyourate.App.HumanResources.Repositories;

public sealed class SiteRepository : HrRepositoryBase
{
    public SiteRepository(HrDatabase? database = null) : base(database) { }

    public IReadOnlyList<Site> GetAll()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Code, Name, Address, City, Province, Country, IsMain, IsActive, Notes FROM Sites ORDER BY Name;";
        using var reader = command.ExecuteReader();
        var result = new List<Site>();
        while (reader.Read())
            result.Add(ReadSite(reader));
        return result;
    }

    public int Create(Site site)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Sites (Code, Name, Address, City, Province, Country, IsMain, IsActive, Notes)
            VALUES ($Code, $Name, $Address, $City, $Province, $Country, $IsMain, $IsActive, $Notes);
            SELECT last_insert_rowid();
        """;
        AddParameters(command, site);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static void AddParameters(SqliteCommand command, Site site)
    {
        command.Parameters.AddWithValue("$Code", site.Code);
        command.Parameters.AddWithValue("$Name", site.Name);
        command.Parameters.AddWithValue("$Address", site.Address);
        command.Parameters.AddWithValue("$City", site.City);
        command.Parameters.AddWithValue("$Province", site.Province);
        command.Parameters.AddWithValue("$Country", site.Country);
        command.Parameters.AddWithValue("$IsMain", site.IsMain ? 1 : 0);
        command.Parameters.AddWithValue("$IsActive", site.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("$Notes", site.Notes);
    }

    private static Site ReadSite(SqliteDataReader reader)
    {
        return new Site
        {
            Id = reader.GetInt32(0),
            Code = reader.GetString(1),
            Name = reader.GetString(2),
            Address = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
            City = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
            Province = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
            Country = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
            IsMain = reader.GetInt32(7) == 1,
            IsActive = reader.GetInt32(8) == 1,
            Notes = reader.IsDBNull(9) ? string.Empty : reader.GetString(9)
        };
    }
}
