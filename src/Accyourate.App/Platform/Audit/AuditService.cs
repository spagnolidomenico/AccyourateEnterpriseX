using Microsoft.Data.Sqlite;

namespace Accyourate.App.Platform.Audit;

public sealed class AuditService
{
    private readonly string _databasePath;

    public AuditService(string? databasePath = null)
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AccyourateEnterpriseX");
        Directory.CreateDirectory(folder);
        _databasePath = databasePath ?? Path.Combine(folder, "accyourate-platform.db");
        Initialize();
    }

    private string ConnectionString => $"Data Source={_databasePath}";

    public void Initialize()
    {
        using var connection = OpenConnection();
        Execute(connection, """
            CREATE TABLE IF NOT EXISTS AuditRecords (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Action TEXT NOT NULL,
                EntityType TEXT,
                EntityId TEXT,
                EntityLabel TEXT,
                Description TEXT NOT NULL,
                UserName TEXT,
                Severity TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                SourceModule TEXT,
                Payload TEXT
            );
        """);
        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_AuditRecords_CreatedAt ON AuditRecords(CreatedAt);");
        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_AuditRecords_Action ON AuditRecords(Action);");
        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_AuditRecords_EntityType ON AuditRecords(EntityType);");
        SeedSystemAuditRecord();
    }

    public int Track(string action, string description, string entityType = "", string entityId = "", string entityLabel = "", string userName = "System", string severity = AuditSeverity.Info, string sourceModule = "Platform", string payload = "")
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO AuditRecords (Action, EntityType, EntityId, EntityLabel, Description, UserName, Severity, CreatedAt, SourceModule, Payload)
            VALUES ($Action, $EntityType, $EntityId, $EntityLabel, $Description, $UserName, $Severity, $CreatedAt, $SourceModule, $Payload);
            SELECT last_insert_rowid();
        """;
        command.Parameters.AddWithValue("$Action", action);
        command.Parameters.AddWithValue("$EntityType", entityType);
        command.Parameters.AddWithValue("$EntityId", entityId);
        command.Parameters.AddWithValue("$EntityLabel", entityLabel);
        command.Parameters.AddWithValue("$Description", description);
        command.Parameters.AddWithValue("$UserName", userName);
        command.Parameters.AddWithValue("$Severity", severity);
        command.Parameters.AddWithValue("$CreatedAt", DateTime.Now.ToString("s"));
        command.Parameters.AddWithValue("$SourceModule", sourceModule);
        command.Parameters.AddWithValue("$Payload", payload);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public IReadOnlyList<AuditRecord> GetLatest(int limit = 50)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Action, EntityType, EntityId, EntityLabel, Description, UserName, Severity, CreatedAt, SourceModule, Payload
            FROM AuditRecords
            ORDER BY CreatedAt DESC
            LIMIT $limit;
        """;
        command.Parameters.AddWithValue("$limit", Math.Max(1, limit));
        using var reader = command.ExecuteReader();
        var result = new List<AuditRecord>();
        while (reader.Read())
            result.Add(ReadAuditRecord(reader));
        return result;
    }

    public IReadOnlyList<AuditRecord> Search(string query, int limit = 50)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Action, EntityType, EntityId, EntityLabel, Description, UserName, Severity, CreatedAt, SourceModule, Payload
            FROM AuditRecords
            WHERE Action LIKE $Query OR EntityType LIKE $Query OR EntityLabel LIKE $Query OR Description LIKE $Query OR UserName LIKE $Query OR SourceModule LIKE $Query
            ORDER BY CreatedAt DESC
            LIMIT $limit;
        """;
        command.Parameters.AddWithValue("$Query", $"%{query}%");
        command.Parameters.AddWithValue("$limit", Math.Max(1, limit));
        using var reader = command.ExecuteReader();
        var result = new List<AuditRecord>();
        while (reader.Read())
            result.Add(ReadAuditRecord(reader));
        return result;
    }

    private void SeedSystemAuditRecord()
    {
        using var connection = OpenConnection();
        using var count = connection.CreateCommand();
        count.CommandText = "SELECT COUNT(*) FROM AuditRecords;";
        if (Convert.ToInt32(count.ExecuteScalar()) > 0)
            return;
        Track(AuditAction.System, "Audit Engine inizializzato.", "Platform", "AuditEngine", "Audit Engine", "System", AuditSeverity.Info, "Platform");
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        return connection;
    }

    private static AuditRecord ReadAuditRecord(SqliteDataReader reader)
    {
        return new AuditRecord
        {
            Id = reader.GetInt32(0),
            Action = ReadString(reader, 1),
            EntityType = ReadString(reader, 2),
            EntityId = ReadString(reader, 3),
            EntityLabel = ReadString(reader, 4),
            Description = ReadString(reader, 5),
            UserName = ReadString(reader, 6),
            Severity = ReadString(reader, 7),
            CreatedAt = ReadString(reader, 8),
            SourceModule = ReadString(reader, 9),
            Payload = ReadString(reader, 10)
        };
    }

    private static string ReadString(SqliteDataReader reader, int index) => reader.IsDBNull(index) ? string.Empty : reader.GetString(index);

    private static void Execute(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
