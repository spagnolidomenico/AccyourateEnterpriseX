using Microsoft.Data.Sqlite;

namespace Accyourate.App.Platform.Home;

public sealed class EnterpriseHomeService
{
    private readonly string _appFolder;

    public EnterpriseHomeService()
    {
        _appFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccyourateEnterpriseX");
    }

    public EnterpriseHomeSnapshot Load()
    {
        return new EnterpriseHomeSnapshot
        {
            Employees = Count(Path.Combine(_appFolder, "accyourate-hr.db"), "Employees"),
            Assets = Count(Path.Combine(_appFolder, "accyourate-assets.db"), "Assets"),
            Documents = Count(Path.Combine(_appFolder, "accyourate-platform.db"), "Documents"),
            DeliveryReports = Count(Path.Combine(_appFolder, "accyourate-assets.db"), "DeliveryReports"),
            UnreadNotifications = CountWhere(Path.Combine(_appFolder, "accyourate-platform.db"), "Notifications", "IsRead = 0"),
            BackupCount = Count(Path.Combine(_appFolder, "accyourate-platform.db"), "BackupHistory"),
            LastBackup = LastValue(Path.Combine(_appFolder, "accyourate-platform.db"), "BackupHistory", "CreatedAt", "CreatedAt DESC"),
            Version = ReadVersion(),
            UpdateStatus = "OK",
            DatabaseStatus = DatabaseStatus()
        };
    }

    public IReadOnlyList<string> RecentActivities()
    {
        var activities = new List<string>();

        AddLast(activities, Path.Combine(_appFolder, "accyourate-platform.db"), "BackupHistory", "Backup completato", "CreatedAt");
        AddLast(activities, Path.Combine(_appFolder, "accyourate-platform.db"), "Documents", "Documento registrato", "CreatedAt");
        AddLast(activities, Path.Combine(_appFolder, "accyourate-assets.db"), "DeliveryReports", "Verbale aggiornato", "ReportDate");
        AddLast(activities, Path.Combine(_appFolder, "accyourate-platform.db"), "Notifications", "Notifica generata", "CreatedAt");

        if (activities.Count == 0)
        {
            activities.Add("Nessuna attività recente disponibile.");
            activities.Add("Inizia creando un dipendente, un asset o un documento.");
        }

        return activities.Take(6).ToList();
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

    private static string LastValue(string databasePath, string table, string column, string orderBy)
    {
        try
        {
            if (!File.Exists(databasePath))
                return "Non disponibile";

            using var connection = new SqliteConnection($"Data Source={databasePath}");
            connection.Open();

            if (!TableExists(connection, table))
                return "Non disponibile";

            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT {column} FROM {table} ORDER BY {orderBy} LIMIT 1;";
            var value = command.ExecuteScalar()?.ToString();

            return DateTime.TryParse(value, out var date)
                ? date.ToString("dd/MM/yyyy HH:mm")
                : string.IsNullOrWhiteSpace(value) ? "Non disponibile" : value;
        }
        catch
        {
            return "Non disponibile";
        }
    }

    private static void AddLast(List<string> target, string databasePath, string table, string label, string dateColumn)
    {
        var last = LastValue(databasePath, table, dateColumn, $"{dateColumn} DESC");
        if (last != "Non disponibile")
            target.Add($"{label} · {last}");
    }

    private string DatabaseStatus()
    {
        var files = new[]
        {
            "accyourate-hr.db",
            "accyourate-assets.db",
            "accyourate-platform.db"
        };

        var present = files.Count(file => File.Exists(Path.Combine(_appFolder, file)));
        return $"{present}/{files.Length} database";
    }

    private static bool TableExists(SqliteConnection connection, string table)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $table;";
        command.Parameters.AddWithValue("$table", table);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private static string ReadVersion()
    {
        try
        {
            var current = Directory.GetCurrentDirectory();
            var versionPath = Path.Combine(current, "VERSION");
            return File.Exists(versionPath)
                ? File.ReadAllText(versionPath).Trim()
                : "0.9.0 RC1";
        }
        catch
        {
            return "0.9.0 RC1";
        }
    }
}
