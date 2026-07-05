using Microsoft.Data.Sqlite;

namespace Accyourate.App.Platform.Dashboard;

public sealed class DashboardKpiService
{
    private readonly string _appFolder;

    public DashboardKpiService()
    {
        _appFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccyourateEnterpriseX");
    }

    public DashboardSnapshot Load()
    {
        return new DashboardSnapshot
        {
            Employees = Count(Path.Combine(_appFolder, "accyourate-hr.db"), "Employees"),
            ActiveEmployees = CountWhere(Path.Combine(_appFolder, "accyourate-hr.db"), "Employees", "EmploymentStatus = 'Active'"),
            Assets = Count(Path.Combine(_appFolder, "accyourate-assets.db"), "Assets"),
            AssignedAssets = CountWhere(Path.Combine(_appFolder, "accyourate-assets.db"), "Assets", "Status = 'Assegnato'"),
            DeliveryReports = Count(Path.Combine(_appFolder, "accyourate-assets.db"), "DeliveryReports"),
            GeneratedDeliveryReports = CountWhere(Path.Combine(_appFolder, "accyourate-assets.db"), "DeliveryReports", "Status = 'Generated'"),
            Documents = Count(Path.Combine(_appFolder, "accyourate-platform.db"), "Documents"),
            UnreadNotifications = CountWhere(Path.Combine(_appFolder, "accyourate-platform.db"), "Notifications", "IsRead = 0"),
            AuditEvents = Count(Path.Combine(_appFolder, "accyourate-platform.db"), "AuditRecords"),
            LastRefresh = DateTime.Now.ToString("s")
        };
    }

    private static int Count(string databasePath, string table)
    {
        return CountWhere(databasePath, table, "1 = 1");
    }

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
