using Microsoft.Data.Sqlite;

namespace Accyourate.App.AssetManagement.Deliveries;

/// <summary>
/// Gestisce la persistenza SQLite del registro consegne.
/// La tabella viene creata automaticamente alla prima istanza del repository.
/// </summary>
public sealed class DeliveryRecordRepository
{
    private readonly string _connectionString;

    public DeliveryRecordRepository(string? databasePath = null)
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccyourateEnterpriseX");

        Directory.CreateDirectory(folder);

        var path = databasePath ?? Path.Combine(folder, "accyourate-assets.db");
        _connectionString = $"Data Source={path}";
        Initialize();
    }

    public void Initialize()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS asset_delivery_records (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                asset_id INTEGER NOT NULL,
                employee_id INTEGER NOT NULL,
                delivery_date TEXT NOT NULL,
                return_date TEXT,
                notes TEXT,
                status TEXT NOT NULL,
                created_at TEXT NOT NULL,
                updated_at TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS ix_asset_delivery_records_asset
                ON asset_delivery_records(asset_id);

            CREATE INDEX IF NOT EXISTS ix_asset_delivery_records_employee
                ON asset_delivery_records(employee_id);

            CREATE INDEX IF NOT EXISTS ix_asset_delivery_records_status
                ON asset_delivery_records(status);

            CREATE UNIQUE INDEX IF NOT EXISTS ux_asset_delivery_records_active_asset
                ON asset_delivery_records(asset_id)
                WHERE status = 'Active' AND (return_date IS NULL OR return_date = '');
            """;
        command.ExecuteNonQuery();
    }

    public int Create(DeliveryRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        ValidateForCreate(record);

        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();

        if (HasActiveForAsset(connection, transaction, record.AssetId))
            throw new InvalidOperationException("L'asset risulta già associato a una consegna attiva.");

        var now = DateTime.Now.ToString("s");
        record.CreatedAt = now;
        record.UpdatedAt = now;

        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO asset_delivery_records (
                asset_id, employee_id, delivery_date, return_date,
                notes, status, created_at, updated_at
            )
            VALUES (
                $assetId, $employeeId, $deliveryDate, $returnDate,
                $notes, $status, $createdAt, $updatedAt
            );
            SELECT last_insert_rowid();
            """;

        AddParameters(command, record);
        var id = Convert.ToInt32(command.ExecuteScalar());
        transaction.Commit();

        record.Id = id;
        return id;
    }

    public DeliveryRecord? GetById(int id)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = SelectColumns + " WHERE id = $id LIMIT 1;";
        command.Parameters.AddWithValue("$id", id);

        using var reader = command.ExecuteReader();
        return reader.Read() ? Read(reader) : null;
    }

    public DeliveryRecord? GetActiveByAsset(int assetId)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = SelectColumns + """

            WHERE asset_id = $assetId
              AND status = $status
              AND (return_date IS NULL OR return_date = '')
            ORDER BY delivery_date DESC
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$assetId", assetId);
        command.Parameters.AddWithValue("$status", DeliveryRecordStatus.Active);

        using var reader = command.ExecuteReader();
        return reader.Read() ? Read(reader) : null;
    }

    public bool HasActiveForAsset(int assetId)
    {
        using var connection = OpenConnection();
        return HasActiveForAsset(connection, null, assetId);
    }

    public IReadOnlyList<DeliveryRecord> GetByAsset(int assetId)
    {
        return Query(
            " WHERE asset_id = $value ORDER BY delivery_date DESC, id DESC;",
            assetId);
    }

    public IReadOnlyList<DeliveryRecord> GetByEmployee(int employeeId)
    {
        return Query(
            " WHERE employee_id = $value ORDER BY delivery_date DESC, id DESC;",
            employeeId);
    }

    public IReadOnlyList<DeliveryRecord> GetLatest(int limit = 100)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = SelectColumns +
                              " ORDER BY delivery_date DESC, id DESC LIMIT $limit;";
        command.Parameters.AddWithValue("$limit", Math.Max(1, limit));

        return ReadAll(command);
    }

    public void MarkReturned(int id, DateTime? returnDate = null, string? notes = null)
    {
        var current = GetById(id)
            ?? throw new InvalidOperationException("Consegna non trovata.");

        if (!current.IsActive)
            throw new InvalidOperationException("La consegna non è attiva e non può essere riconsegnata.");

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE asset_delivery_records
            SET return_date = $returnDate,
                notes = $notes,
                status = $status,
                updated_at = $updatedAt
            WHERE id = $id;
            """;
        command.Parameters.AddWithValue("$returnDate", (returnDate ?? DateTime.Now).ToString("s"));
        command.Parameters.AddWithValue("$notes", MergeNotes(current.Notes, notes));
        command.Parameters.AddWithValue("$status", DeliveryRecordStatus.Returned);
        command.Parameters.AddWithValue("$updatedAt", DateTime.Now.ToString("s"));
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    public void Cancel(int id, string? notes = null)
    {
        var current = GetById(id)
            ?? throw new InvalidOperationException("Consegna non trovata.");

        if (!current.IsActive && current.Status != DeliveryRecordStatus.Planned)
            throw new InvalidOperationException("La consegna non può essere annullata nello stato attuale.");

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE asset_delivery_records
            SET notes = $notes,
                status = $status,
                updated_at = $updatedAt
            WHERE id = $id;
            """;
        command.Parameters.AddWithValue("$notes", MergeNotes(current.Notes, notes));
        command.Parameters.AddWithValue("$status", DeliveryRecordStatus.Cancelled);
        command.Parameters.AddWithValue("$updatedAt", DateTime.Now.ToString("s"));
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    private IReadOnlyList<DeliveryRecord> Query(string whereClause, int value)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = SelectColumns + whereClause;
        command.Parameters.AddWithValue("$value", value);
        return ReadAll(command);
    }

    private static IReadOnlyList<DeliveryRecord> ReadAll(SqliteCommand command)
    {
        using var reader = command.ExecuteReader();
        var records = new List<DeliveryRecord>();
        while (reader.Read())
            records.Add(Read(reader));
        return records;
    }

    private static bool HasActiveForAsset(
        SqliteConnection connection,
        SqliteTransaction? transaction,
        int assetId)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT EXISTS (
                SELECT 1
                FROM asset_delivery_records
                WHERE asset_id = $assetId
                  AND status = $status
                  AND (return_date IS NULL OR return_date = '')
            );
            """;
        command.Parameters.AddWithValue("$assetId", assetId);
        command.Parameters.AddWithValue("$status", DeliveryRecordStatus.Active);
        return Convert.ToInt32(command.ExecuteScalar()) == 1;
    }

    private static void ValidateForCreate(DeliveryRecord record)
    {
        if (record.AssetId <= 0)
            throw new ArgumentException("AssetId deve essere maggiore di zero.", nameof(record));

        if (record.EmployeeId <= 0)
            throw new ArgumentException("EmployeeId deve essere maggiore di zero.", nameof(record));

        if (!DeliveryRecordStatus.IsValid(record.Status))
            throw new ArgumentException("Stato della consegna non valido.", nameof(record));

        if (string.IsNullOrWhiteSpace(record.DeliveryDate))
            record.DeliveryDate = DateTime.Now.ToString("s");
    }

    private static void AddParameters(SqliteCommand command, DeliveryRecord record)
    {
        command.Parameters.AddWithValue("$assetId", record.AssetId);
        command.Parameters.AddWithValue("$employeeId", record.EmployeeId);
        command.Parameters.AddWithValue("$deliveryDate", record.DeliveryDate);
        command.Parameters.AddWithValue(
            "$returnDate",
            string.IsNullOrWhiteSpace(record.ReturnDate) ? DBNull.Value : record.ReturnDate);
        command.Parameters.AddWithValue("$notes", record.Notes ?? string.Empty);
        command.Parameters.AddWithValue("$status", record.Status);
        command.Parameters.AddWithValue("$createdAt", record.CreatedAt);
        command.Parameters.AddWithValue("$updatedAt", record.UpdatedAt);
    }

    private static DeliveryRecord Read(SqliteDataReader reader)
    {
        return new DeliveryRecord
        {
            Id = reader.GetInt32(0),
            AssetId = reader.GetInt32(1),
            EmployeeId = reader.GetInt32(2),
            DeliveryDate = ReadString(reader, 3),
            ReturnDate = ReadString(reader, 4),
            Notes = ReadString(reader, 5),
            Status = ReadString(reader, 6),
            CreatedAt = ReadString(reader, 7),
            UpdatedAt = ReadString(reader, 8)
        };
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private static string ReadString(SqliteDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }

    private static string MergeNotes(string existing, string? additional)
    {
        if (string.IsNullOrWhiteSpace(additional))
            return existing ?? string.Empty;

        if (string.IsNullOrWhiteSpace(existing))
            return additional.Trim();

        return $"{existing.Trim()}{Environment.NewLine}{additional.Trim()}";
    }

    private const string SelectColumns = """
        SELECT id, asset_id, employee_id, delivery_date, return_date,
               notes, status, created_at, updated_at
        FROM asset_delivery_records
        """;
}
