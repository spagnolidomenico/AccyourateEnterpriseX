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
            ActiveEmployees = CountWhere(Path.Combine(_appFolder, "accyourate-hr.db"), "Employees", "EmploymentStatus = 'Active' OR EmploymentStatus = 'Attivo' OR EmploymentStatus = 'active'"),
            AssignedAssets = CountWhere(Path.Combine(_appFolder, "accyourate-assets.db"), "Assets", "Status = 'Assegnato' OR Status = 'Assigned' OR AssignedToEmployeeId IS NOT NULL"),
            Documents = Count(Path.Combine(_appFolder, "accyourate-platform.db"), "Documents")
        };
    }

    public IReadOnlyList<HumanResourcesEmployeeRow> LoadEmployees(string search = "")
    {
        var databasePath = Path.Combine(_appFolder, "accyourate-hr.db");
        var result = new List<HumanResourcesEmployeeRow>();

        try
        {
            if (!File.Exists(databasePath))
                return result;

            using var connection = new SqliteConnection($"Data Source={databasePath}");
            connection.Open();

            if (!TableExists(connection, "Employees"))
                return result;

            using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT Id, EmployeeCode, FirstName, LastName, Email, Department, Role, EmploymentStatus, Phone, CreatedAt
                FROM Employees
                WHERE $Search = ''
                   OR EmployeeCode LIKE $Like
                   OR FirstName LIKE $Like
                   OR LastName LIKE $Like
                   OR Email LIKE $Like
                   OR Department LIKE $Like
                   OR Role LIKE $Like
                ORDER BY LastName, FirstName
                LIMIT 300;
            """;
            command.Parameters.AddWithValue("$Search", search ?? string.Empty);
            command.Parameters.AddWithValue("$Like", $"%{search}%");

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new HumanResourcesEmployeeRow
                {
                    Id = I(reader, 0),
                    EmployeeCode = S(reader, 1),
                    FirstName = S(reader, 2),
                    LastName = S(reader, 3),
                    Email = S(reader, 4),
                    Department = S(reader, 5),
                    Role = S(reader, 6),
                    EmploymentStatus = S(reader, 7),
                    Phone = S(reader, 8),
                    CreatedAt = S(reader, 9)
                });
            }
        }
        catch
        {
            return result;
        }

        return result;
    }

    public int CountEmployeeAssets(HumanResourcesEmployeeRow employee)
    {
        if (employee.Id <= 0 && string.IsNullOrWhiteSpace(employee.FullName))
            return 0;

        var databasePath = Path.Combine(_appFolder, "accyourate-assets.db");

        try
        {
            if (!File.Exists(databasePath))
                return 0;

            using var connection = new SqliteConnection($"Data Source={databasePath}");
            connection.Open();

            if (!TableExists(connection, "Assets"))
                return 0;

            using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT COUNT(*)
                FROM Assets
                WHERE AssignedToEmployeeId = $Id
                   OR AssignedTo LIKE $Name
                   OR AssignedToEmployeeName LIKE $Name;
            """;
            command.Parameters.AddWithValue("$Id", employee.Id);
            command.Parameters.AddWithValue("$Name", $"%{employee.FullName}%");
            return Convert.ToInt32(command.ExecuteScalar());
        }
        catch
        {
            return 0;
        }
    }

    public int CountEmployeeDocuments(HumanResourcesEmployeeRow employee)
    {
        var databasePath = Path.Combine(_appFolder, "accyourate-platform.db");

        try
        {
            if (!File.Exists(databasePath))
                return 0;

            using var connection = new SqliteConnection($"Data Source={databasePath}");
            connection.Open();

            if (!TableExists(connection, "Documents"))
                return 0;

            using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT COUNT(*)
                FROM Documents
                WHERE RelatedEntityId = $Id
                   OR RelatedEntityLabel LIKE $Name
                   OR Title LIKE $Name;
            """;
            command.Parameters.AddWithValue("$Id", employee.Id.ToString());
            command.Parameters.AddWithValue("$Name", $"%{employee.FullName}%");
            return Convert.ToInt32(command.ExecuteScalar());
        }
        catch
        {
            return 0;
        }
    }

    public IReadOnlyList<string> EmployeeTimeline(HumanResourcesEmployeeRow? employee)
    {
        if (employee is null)
        {
            return new[]
            {
                "Seleziona un dipendente per visualizzare la timeline.",
                "La Entity Page mostrerà asset, documenti e storico collegati."
            };
        }

        var items = new List<string>
        {
            $"Fascicolo aperto: {employee.FullName}",
            string.IsNullOrWhiteSpace(employee.CreatedAt)
                ? "Record HR disponibile."
                : $"Dipendente creato · {FormatDate(employee.CreatedAt)}"
        };

        var assets = CountEmployeeAssets(employee);
        if (assets > 0)
            items.Add($"{assets} asset collegati al dipendente.");

        var documents = CountEmployeeDocuments(employee);
        if (documents > 0)
            items.Add($"{documents} documenti collegati al dipendente.");

        return items;
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

    private static string S(SqliteDataReader reader, int index) => reader.IsDBNull(index) ? string.Empty : reader.GetString(index);

    private static int I(SqliteDataReader reader, int index) => reader.IsDBNull(index) ? 0 : reader.GetInt32(index);

    private static string FormatDate(string value)
    {
        return DateTime.TryParse(value, out var date)
            ? date.ToString("dd/MM/yyyy")
            : value;
    }
}
