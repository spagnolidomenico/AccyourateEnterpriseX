using Microsoft.Data.Sqlite;
using Accyourate.App.EnterpriseMasterData.Models;
using Accyourate.App.EnterpriseMasterData.Services;

namespace Accyourate.App.AssetManagement.Services;

public sealed class AssetAssignmentEngine
{
    private readonly string _assetDatabasePath;
    private readonly MasterDataService _masterDataService;

    public AssetAssignmentEngine(string? assetDatabasePath = null, MasterDataService? masterDataService = null)
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccyourateEnterpriseX");

        Directory.CreateDirectory(folder);

        _assetDatabasePath = assetDatabasePath ?? Path.Combine(folder, "accyourate-assets.db");
        _masterDataService = masterDataService ?? new MasterDataService();

        EnsureSchema();
    }

    private string ConnectionString => $"Data Source={_assetDatabasePath}";

    public IReadOnlyList<AssignableEmployee> GetEmployees()
    {
        return _masterDataService.GetEmployees()
            .Where(x => x.IsActive)
            .OrderBy(x => x.FullName)
            .Select(x => new AssignableEmployee
            {
                MasterEmployeeId = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                Role = x.Role,
                DepartmentId = x.DepartmentId,
                SiteId = x.SiteId
            })
            .ToList();
    }

    public IReadOnlyList<AssignableAsset> GetAvailableAssets()
    {
        using var connection = OpenConnection();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, AssetCode, Category, Manufacturer, Model, SerialNumber, Status
            FROM Assets
            WHERE Id NOT IN (
                SELECT AssetId
                FROM AssetAssignments
                WHERE Status = 'Attiva'
            )
            ORDER BY AssetCode;
        """;

        using var reader = command.ExecuteReader();
        var result = new List<AssignableAsset>();

        while (reader.Read())
        {
            result.Add(new AssignableAsset
            {
                AssetId = reader.GetInt32(0),
                AssetCode = ReadString(reader, 1),
                Category = ReadString(reader, 2),
                Manufacturer = ReadString(reader, 3),
                Model = ReadString(reader, 4),
                SerialNumber = ReadString(reader, 5),
                Status = ReadString(reader, 6)
            });
        }

        return result;
    }

    public IReadOnlyList<AssetAssignmentSummary> GetActiveAssignmentsForEmployee(int masterEmployeeId)
    {
        using var connection = OpenConnection();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT aa.Id,
                   aa.AssetId,
                   aa.EmployeeId,
                   e.FullName,
                   a.AssetCode,
                   a.Manufacturer,
                   a.Model,
                   aa.AssignedAt,
                   aa.Status,
                   aa.Notes
            FROM AssetAssignments aa
            JOIN Employees e ON e.Id = aa.EmployeeId
            JOIN Assets a ON a.Id = aa.AssetId
            WHERE e.MasterEmployeeId = $masterEmployeeId
              AND aa.Status = 'Attiva'
            ORDER BY aa.AssignedAt DESC;
        """;
        command.Parameters.AddWithValue("$masterEmployeeId", masterEmployeeId);

        using var reader = command.ExecuteReader();
        var result = new List<AssetAssignmentSummary>();

        while (reader.Read())
            result.Add(ReadAssignmentSummary(reader));

        return result;
    }

    public AssetAssignmentSummary? GetActiveAssignmentForAsset(int assetId)
    {
        using var connection = OpenConnection();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT aa.Id,
                   aa.AssetId,
                   aa.EmployeeId,
                   e.FullName,
                   a.AssetCode,
                   a.Manufacturer,
                   a.Model,
                   aa.AssignedAt,
                   aa.Status,
                   aa.Notes
            FROM AssetAssignments aa
            JOIN Employees e ON e.Id = aa.EmployeeId
            JOIN Assets a ON a.Id = aa.AssetId
            WHERE aa.AssetId = $assetId
              AND aa.Status = 'Attiva'
            ORDER BY aa.AssignedAt DESC
            LIMIT 1;
        """;
        command.Parameters.AddWithValue("$assetId", assetId);

        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadAssignmentSummary(reader) : null;
    }

    public int AssignAsset(int assetId, int masterEmployeeId, string assignedBy = "System", string notes = "")
    {
        var employee = _masterDataService.GetEmployeeById(masterEmployeeId)
            ?? throw new InvalidOperationException("Dipendente Master Data non trovato.");

        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();

        var assetEmployeeId = EnsureAssetEmployeeMirror(connection, transaction, employee);
        EnsureAssetExists(connection, transaction, assetId);

        using (var closeExisting = connection.CreateCommand())
        {
            closeExisting.Transaction = transaction;
            closeExisting.CommandText = """
                UPDATE AssetAssignments
                SET Status = 'Restituita',
                    ReturnedAt = $returnedAt
                WHERE AssetId = $assetId
                  AND Status = 'Attiva';
            """;
            closeExisting.Parameters.AddWithValue("$assetId", assetId);
            closeExisting.Parameters.AddWithValue("$returnedAt", DateTime.Now.ToString("s"));
            closeExisting.ExecuteNonQuery();
        }

        long assignmentId;

        using (var insert = connection.CreateCommand())
        {
            insert.Transaction = transaction;
            insert.CommandText = """
                INSERT INTO AssetAssignments (AssetId, EmployeeId, AssignedAt, ReturnedAt, AssignedBy, Notes, Status)
                VALUES ($assetId, $employeeId, $assignedAt, '', $assignedBy, $notes, 'Attiva');
                SELECT last_insert_rowid();
            """;
            insert.Parameters.AddWithValue("$assetId", assetId);
            insert.Parameters.AddWithValue("$employeeId", assetEmployeeId);
            insert.Parameters.AddWithValue("$assignedAt", DateTime.Now.ToString("s"));
            insert.Parameters.AddWithValue("$assignedBy", assignedBy);
            insert.Parameters.AddWithValue("$notes", notes);
            assignmentId = (long)(insert.ExecuteScalar() ?? 0L);
        }

        using (var updateAsset = connection.CreateCommand())
        {
            updateAsset.Transaction = transaction;
            updateAsset.CommandText = """
                UPDATE Assets
                SET Status = 'Assegnato',
                    UpdatedAt = $updatedAt
                WHERE Id = $assetId;
            """;
            updateAsset.Parameters.AddWithValue("$assetId", assetId);
            updateAsset.Parameters.AddWithValue("$updatedAt", DateTime.Now.ToString("s"));
            updateAsset.ExecuteNonQuery();
        }

        transaction.Commit();
        return Convert.ToInt32(assignmentId);
    }

    public void ReturnAssignment(int assignmentId, string notes = "")
    {
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();

        int assetId;

        using (var read = connection.CreateCommand())
        {
            read.Transaction = transaction;
            read.CommandText = "SELECT AssetId FROM AssetAssignments WHERE Id = $assignmentId;";
            read.Parameters.AddWithValue("$assignmentId", assignmentId);

            var raw = read.ExecuteScalar();
            if (raw is null)
                throw new InvalidOperationException("Assegnazione non trovata.");

            assetId = Convert.ToInt32(raw);
        }

        using (var updateAssignment = connection.CreateCommand())
        {
            updateAssignment.Transaction = transaction;
            updateAssignment.CommandText = """
                UPDATE AssetAssignments
                SET Status = 'Restituita',
                    ReturnedAt = $returnedAt,
                    Notes = CASE
                        WHEN $notes = '' THEN Notes
                        WHEN Notes IS NULL OR Notes = '' THEN $notes
                        ELSE Notes || char(10) || $notes
                    END
                WHERE Id = $assignmentId;
            """;
            updateAssignment.Parameters.AddWithValue("$assignmentId", assignmentId);
            updateAssignment.Parameters.AddWithValue("$returnedAt", DateTime.Now.ToString("s"));
            updateAssignment.Parameters.AddWithValue("$notes", notes);
            updateAssignment.ExecuteNonQuery();
        }

        using (var updateAsset = connection.CreateCommand())
        {
            updateAsset.Transaction = transaction;
            updateAsset.CommandText = """
                UPDATE Assets
                SET Status = 'Disponibile',
                    UpdatedAt = $updatedAt
                WHERE Id = $assetId;
            """;
            updateAsset.Parameters.AddWithValue("$assetId", assetId);
            updateAsset.Parameters.AddWithValue("$updatedAt", DateTime.Now.ToString("s"));
            updateAsset.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    public int SyncEmployeesFromMasterData()
    {
        var employees = _masterDataService.GetEmployees();
        var count = 0;

        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();

        foreach (var employee in employees)
        {
            EnsureAssetEmployeeMirror(connection, transaction, employee);
            count++;
        }

        transaction.Commit();
        return count;
    }

    private void EnsureSchema()
    {
        using var connection = OpenConnection();

        EnsureColumn(connection, "Employees", "MasterEmployeeId", "INTEGER NOT NULL DEFAULT 0");
        EnsureColumn(connection, "Employees", "Phone", "TEXT");
        EnsureColumn(connection, "Employees", "Notes", "TEXT");

        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_Employees_MasterEmployeeId ON Employees(MasterEmployeeId);");
        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_AssetAssignments_Status ON AssetAssignments(Status);");
    }

    private int EnsureAssetEmployeeMirror(SqliteConnection connection, SqliteTransaction transaction, EmployeeMasterData employee)
    {
        var existingId = FindMirroredEmployee(connection, transaction, employee);

        if (existingId > 0)
        {
            using var update = connection.CreateCommand();
            update.Transaction = transaction;
            update.CommandText = """
                UPDATE Employees
                SET MasterEmployeeId = $MasterEmployeeId,
                    FullName = $FullName,
                    Email = $Email,
                    Phone = $Phone,
                    Department = $Department,
                    Role = $Role,
                    Site = $Site,
                    IsActive = $IsActive,
                    Notes = $Notes
                WHERE Id = $Id;
            """;
            update.Parameters.AddWithValue("$Id", existingId);
            AddEmployeeParameters(update, employee);
            update.ExecuteNonQuery();
            return existingId;
        }

        using var insert = connection.CreateCommand();
        insert.Transaction = transaction;
        insert.CommandText = """
            INSERT INTO Employees (MasterEmployeeId, FullName, Email, Phone, Department, Role, Site, IsActive, Notes)
            VALUES ($MasterEmployeeId, $FullName, $Email, $Phone, $Department, $Role, $Site, $IsActive, $Notes);
            SELECT last_insert_rowid();
        """;
        AddEmployeeParameters(insert, employee);
        return Convert.ToInt32(insert.ExecuteScalar());
    }

    private int FindMirroredEmployee(SqliteConnection connection, SqliteTransaction transaction, EmployeeMasterData employee)
    {
        using var byMasterId = connection.CreateCommand();
        byMasterId.Transaction = transaction;
        byMasterId.CommandText = "SELECT Id FROM Employees WHERE MasterEmployeeId = $MasterEmployeeId LIMIT 1;";
        byMasterId.Parameters.AddWithValue("$MasterEmployeeId", employee.Id);

        var raw = byMasterId.ExecuteScalar();
        if (raw is not null)
            return Convert.ToInt32(raw);

        if (!string.IsNullOrWhiteSpace(employee.Email))
        {
            using var byEmail = connection.CreateCommand();
            byEmail.Transaction = transaction;
            byEmail.CommandText = "SELECT Id FROM Employees WHERE Email = $Email LIMIT 1;";
            byEmail.Parameters.AddWithValue("$Email", employee.Email);

            raw = byEmail.ExecuteScalar();
            if (raw is not null)
                return Convert.ToInt32(raw);
        }

        using var byName = connection.CreateCommand();
        byName.Transaction = transaction;
        byName.CommandText = "SELECT Id FROM Employees WHERE FullName = $FullName LIMIT 1;";
        byName.Parameters.AddWithValue("$FullName", employee.FullName);

        raw = byName.ExecuteScalar();
        return raw is null ? 0 : Convert.ToInt32(raw);
    }

    private static void EnsureAssetExists(SqliteConnection connection, SqliteTransaction transaction, int assetId)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT COUNT(*) FROM Assets WHERE Id = $assetId;";
        command.Parameters.AddWithValue("$assetId", assetId);

        if (Convert.ToInt32(command.ExecuteScalar()) == 0)
            throw new InvalidOperationException("Asset non trovato.");
    }

    private static void AddEmployeeParameters(SqliteCommand command, EmployeeMasterData employee)
    {
        command.Parameters.AddWithValue("$MasterEmployeeId", employee.Id);
        command.Parameters.AddWithValue("$FullName", employee.FullName);
        command.Parameters.AddWithValue("$Email", employee.Email);
        command.Parameters.AddWithValue("$Phone", employee.Phone);
        command.Parameters.AddWithValue("$Department", employee.DepartmentId.ToString());
        command.Parameters.AddWithValue("$Role", employee.Role);
        command.Parameters.AddWithValue("$Site", employee.SiteId.ToString());
        command.Parameters.AddWithValue("$IsActive", employee.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("$Notes", employee.Notes);
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();

        return connection;
    }

    private static void EnsureColumn(SqliteConnection connection, string tableName, string columnName, string definition)
    {
        using var check = connection.CreateCommand();
        check.CommandText = $"PRAGMA table_info({tableName});";

        using var reader = check.ExecuteReader();
        while (reader.Read())
        {
            var existing = reader.GetString(1);
            if (string.Equals(existing, columnName, StringComparison.OrdinalIgnoreCase))
                return;
        }

        using var alter = connection.CreateCommand();
        alter.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {definition};";
        alter.ExecuteNonQuery();
    }

    private static void Execute(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static string ReadString(SqliteDataReader reader, int index)
    {
        return reader.IsDBNull(index) ? string.Empty : reader.GetString(index);
    }

    private static AssetAssignmentSummary ReadAssignmentSummary(SqliteDataReader reader)
    {
        return new AssetAssignmentSummary
        {
            AssignmentId = reader.GetInt32(0),
            AssetId = reader.GetInt32(1),
            AssetEmployeeId = reader.GetInt32(2),
            EmployeeName = ReadString(reader, 3),
            AssetCode = ReadString(reader, 4),
            Manufacturer = ReadString(reader, 5),
            Model = ReadString(reader, 6),
            AssignedAt = ReadString(reader, 7),
            Status = ReadString(reader, 8),
            Notes = ReadString(reader, 9)
        };
    }
}

public sealed class AssignableEmployee
{
    public int MasterEmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public int SiteId { get; set; }

    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(Role) ? FullName : $"{FullName} - {Role}";
    }
}

public sealed class AssignableAsset
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{AssetCode} - {Manufacturer} {Model}";
    }
}

public sealed class AssetAssignmentSummary
{
    public int AssignmentId { get; set; }
    public int AssetId { get; set; }
    public int AssetEmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string AssetCode { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string AssignedAt { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
