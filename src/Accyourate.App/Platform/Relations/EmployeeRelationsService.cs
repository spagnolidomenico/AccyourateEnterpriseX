using Microsoft.Data.Sqlite;

namespace Accyourate.App.Platform.Relations;

public sealed class EmployeeRelationsService
{
    private readonly string _appFolder;

    public EmployeeRelationsService()
    {
        _appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AccyourateEnterpriseX");
    }

    public EmployeeRelationsSnapshot Load(string? employeeId, string? employeeName)
    {
        var safeEmployeeId = employeeId ?? string.Empty;
        var safeEmployeeName = employeeName ?? string.Empty;

        return new EmployeeRelationsSnapshot
        {
            EmployeeId = safeEmployeeId,
            EmployeeName = safeEmployeeName,
            Assets = LoadAssets(safeEmployeeId, safeEmployeeName),
            Documents = LoadDocuments(safeEmployeeId, safeEmployeeName),
            DeliveryReports = LoadDeliveryReports(safeEmployeeId, safeEmployeeName)
        };
    }

    private IReadOnlyList<EnterpriseRelationItem> LoadAssets(string employeeId, string employeeName)
    {
        var result = new List<EnterpriseRelationItem>();
        try
        {
            using var connection = OpenIfTableExists(Path.Combine(_appFolder, "accyourate-assets.db"), "Assets");
            if (connection is null) return result;
            var columns = Columns(connection, "Assets");
            var where = new List<string>();
            if (columns.Contains("AssignedToEmployeeId")) where.Add("CAST(AssignedToEmployeeId AS TEXT) = $EmployeeId");
            if (columns.Contains("EmployeeId")) where.Add("CAST(EmployeeId AS TEXT) = $EmployeeId");
            if (columns.Contains("AssignedTo")) where.Add("AssignedTo LIKE $EmployeeName");
            if (columns.Contains("AssignedToEmployeeName")) where.Add("AssignedToEmployeeName LIKE $EmployeeName");
            if (where.Count == 0) return result;
            using var command = connection.CreateCommand();
            command.CommandText = $"""
                SELECT {Col(columns,"Id")}, {Col(columns,"AssetCode")}, {Col(columns,"Category")}, {Col(columns,"Manufacturer")}, {Col(columns,"Model")}, {Col(columns,"SerialNumber")}, {Col(columns,"Status")}
                FROM Assets
                WHERE {string.Join(" OR ", where)}
                ORDER BY {OrderBy(columns,"AssetCode","Id")}
                LIMIT 20;
            """;
            command.Parameters.AddWithValue("$EmployeeId", employeeId ?? string.Empty);
            command.Parameters.AddWithValue("$EmployeeName", $"%{employeeName}%");
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new EnterpriseRelationItem
                {
                    Id = S(reader,0), EntityType = "Asset", Icon = "💻",
                    Title = string.IsNullOrWhiteSpace(S(reader,1)) ? $"{S(reader,3)} {S(reader,4)}".Trim() : S(reader,1),
                    Subtitle = $"{S(reader,2)} · {S(reader,3)} {S(reader,4)} · S/N {S(reader,5)}".Trim(' ', '·'),
                    Status = S(reader,6), OpenModuleId = "asset-management", OpenModuleTitle = "Asset Management"
                });
            }
        }
        catch { return result; }
        return result;
    }

    private IReadOnlyList<EnterpriseRelationItem> LoadDocuments(string employeeId, string employeeName)
    {
        var result = new List<EnterpriseRelationItem>();
        try
        {
            using var connection = OpenIfTableExists(Path.Combine(_appFolder, "accyourate-platform.db"), "Documents");
            if (connection is null) return result;
            var columns = Columns(connection, "Documents");
            var where = new List<string>();
            if (columns.Contains("RelatedEntityId")) where.Add("RelatedEntityId = $EmployeeId");
            if (columns.Contains("RelatedEntityLabel")) where.Add("RelatedEntityLabel LIKE $EmployeeName");
            if (columns.Contains("Title")) where.Add("Title LIKE $EmployeeName");
            if (where.Count == 0) return result;
            using var command = connection.CreateCommand();
            command.CommandText = $"""
                SELECT {Col(columns,"Id")}, {Col(columns,"DocumentNumber")}, {Col(columns,"Title")}, {Col(columns,"Category")}, {Col(columns,"FileName")}, {Col(columns,"CreatedAt")}
                FROM Documents
                WHERE {string.Join(" OR ", where)}
                ORDER BY {OrderBy(columns,"CreatedAt","Id")} DESC
                LIMIT 20;
            """;
            command.Parameters.AddWithValue("$EmployeeId", employeeId ?? string.Empty);
            command.Parameters.AddWithValue("$EmployeeName", $"%{employeeName}%");
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new EnterpriseRelationItem
                {
                    Id = S(reader,0), EntityType = "Document", Icon = "📄",
                    Title = string.IsNullOrWhiteSpace(S(reader,1)) ? S(reader,2) : $"{S(reader,1)} · {S(reader,2)}",
                    Subtitle = $"{S(reader,3)} · {S(reader,4)} · {FormatDate(S(reader,5))}".Trim(' ', '·'),
                    Status = S(reader,3), OpenModuleId = "document-center", OpenModuleTitle = "Centro Documenti"
                });
            }
        }
        catch { return result; }
        return result;
    }

    private IReadOnlyList<EnterpriseRelationItem> LoadDeliveryReports(string employeeId, string employeeName)
    {
        var result = new List<EnterpriseRelationItem>();
        try
        {
            using var connection = OpenIfTableExists(Path.Combine(_appFolder, "accyourate-assets.db"), "DeliveryReports");
            if (connection is null) return result;
            var columns = Columns(connection, "DeliveryReports");
            var where = new List<string>();
            if (columns.Contains("EmployeeId")) where.Add("CAST(EmployeeId AS TEXT) = $EmployeeId");
            if (columns.Contains("EmployeeName")) where.Add("EmployeeName LIKE $EmployeeName");
            if (where.Count == 0) return result;
            using var command = connection.CreateCommand();
            command.CommandText = $"""
                SELECT {Col(columns,"Id")}, {Col(columns,"ReportNumber")}, {Col(columns,"EmployeeName")}, {Col(columns,"AssetCode")}, {Col(columns,"Status")}, {Col(columns,"ReportDate")}
                FROM DeliveryReports
                WHERE {string.Join(" OR ", where)}
                ORDER BY {OrderBy(columns,"ReportDate","Id")} DESC
                LIMIT 20;
            """;
            command.Parameters.AddWithValue("$EmployeeId", employeeId ?? string.Empty);
            command.Parameters.AddWithValue("$EmployeeName", $"%{employeeName}%");
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new EnterpriseRelationItem
                {
                    Id = S(reader,0), EntityType = "DeliveryReport", Icon = "📦", Title = S(reader,1),
                    Subtitle = $"{S(reader,3)} · {FormatDate(S(reader,5))}".Trim(' ', '·'),
                    Status = S(reader,4), OpenModuleId = "delivery-reports", OpenModuleTitle = "Verbali consegna"
                });
            }
        }
        catch { return result; }
        return result;
    }

    private static SqliteConnection? OpenIfTableExists(string databasePath, string table)
    {
        if (!File.Exists(databasePath)) return null;
        var connection = new SqliteConnection($"Data Source={databasePath}");
        connection.Open();
        if (!TableExists(connection, table)) { connection.Dispose(); return null; }
        return connection;
    }

    private static bool TableExists(SqliteConnection connection, string table)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $table;";
        command.Parameters.AddWithValue("$table", table);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private static HashSet<string> Columns(SqliteConnection connection, string table)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info({table});";
        using var reader = command.ExecuteReader();
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (reader.Read()) columns.Add(reader.GetString(1));
        return columns;
    }

    private static string Col(HashSet<string> columns, string name) => columns.Contains(name) ? name : $"'' AS {name}";
    private static string OrderBy(HashSet<string> columns, params string[] preferred) => preferred.FirstOrDefault(columns.Contains) ?? "rowid";
    private static string S(SqliteDataReader reader, int index) => reader.IsDBNull(index) ? string.Empty : reader.GetValue(index)?.ToString() ?? string.Empty;
    private static string FormatDate(string value) => DateTime.TryParse(value, out var date) ? date.ToString("dd/MM/yyyy") : value;
}
