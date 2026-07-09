using Microsoft.Data.Sqlite;

namespace Accyourate.App.HumanResources.Enterprise;

public sealed class HumanResourcesEnterpriseService
{
    private readonly string _appFolder;

    public HumanResourcesEnterpriseService()
    {
        _appFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccyourateEnterpriseX");
    }

    public HumanResourcesEnterpriseSnapshot Load()
    {
        return new HumanResourcesEnterpriseSnapshot
        {
            Employees = Count(Path.Combine(_appFolder, "accyourate-hr.db"), "Employees"),
            ActiveEmployees = CountWhere(Path.Combine(_appFolder, "accyourate-hr.db"), "Employees", "EmploymentStatus = 'Active' OR EmploymentStatus = 'Attivo'"),
            AssignedAssets = CountWhere(Path.Combine(_appFolder, "accyourate-assets.db"), "Assets", "Status = 'Assegnato' OR Status = 'Assigned'"),
            Documents = Count(Path.Combine(_appFolder, "accyourate-platform.db"), "Documents")
        };
    }

    public IReadOnlyList<string> EmployeeTimeline()
    {
        return new[]
        {
            "Fascicolo dipendente predisposto.",
            "Collegamenti Asset e Documenti in preparazione.",
            "Timeline HR pronta per integrazione eventi."
        };
    }

    private static int Count(string databasePath, string table) => CountWhere(databasePath, table, "1 = 1");

    private static int CountWhere(string databasePath, string table, string where)
    {
        try
        {
            if (!File.Exists(databasePath))
                return 0;

            using var connection = new SqliteConnection($"Data Source={databasePath}");
            connection.Open();

            if (!TableExists(connection, table))
                return 0;

            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM {table} WHERE {where};";
            return Convert.ToInt32(command.ExecuteScalar());
        }
        catch
        {
            return 0;
        }
    }

    private static bool TableExists(SqliteConnection connection, string table)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $table;";
        command.Parameters.AddWithValue("$table", table);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }
}
