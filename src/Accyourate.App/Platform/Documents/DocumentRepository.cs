using Microsoft.Data.Sqlite;
using Accyourate.App.Platform.Settings;

namespace Accyourate.App.Platform.Documents;

public sealed class DocumentRepository
{
    private readonly string _databasePath;
    private readonly NumberGeneratorService _numberGenerator;

    public DocumentRepository(string? databasePath = null, NumberGeneratorService? numberGenerator = null)
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AccyourateEnterpriseX");
        Directory.CreateDirectory(folder);
        _databasePath = databasePath ?? Path.Combine(folder, "accyourate-platform.db");
        _numberGenerator = numberGenerator ?? new NumberGeneratorService();
        Initialize();
    }

    private string ConnectionString => $"Data Source={_databasePath}";

    public void Initialize()
    {
        using var connection = OpenConnection();
        Execute(connection, """
            CREATE TABLE IF NOT EXISTS Documents (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                DocumentNumber TEXT NOT NULL UNIQUE,
                Title TEXT NOT NULL,
                Category TEXT NOT NULL,
                FilePath TEXT NOT NULL,
                FileName TEXT,
                Extension TEXT,
                SizeBytes INTEGER NOT NULL DEFAULT 0,
                RelatedEntityType TEXT,
                RelatedEntityId TEXT,
                RelatedEntityLabel TEXT,
                CreatedAt TEXT NOT NULL,
                CreatedBy TEXT,
                Notes TEXT
            );
        """);
        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_Documents_DocumentNumber ON Documents(DocumentNumber);");
        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_Documents_Category ON Documents(Category);");
        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_Documents_RelatedEntity ON Documents(RelatedEntityType, RelatedEntityId);");
    }

    public int Register(DocumentRecord document)
    {
        using var connection = OpenConnection();
        if (string.IsNullOrWhiteSpace(document.DocumentNumber))
            document.DocumentNumber = NextDocumentNumber(connection);

        EnrichFileInfo(document);

        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Documents (
                DocumentNumber, Title, Category, FilePath, FileName, Extension, SizeBytes,
                RelatedEntityType, RelatedEntityId, RelatedEntityLabel, CreatedAt, CreatedBy, Notes
            )
            VALUES (
                $DocumentNumber, $Title, $Category, $FilePath, $FileName, $Extension, $SizeBytes,
                $RelatedEntityType, $RelatedEntityId, $RelatedEntityLabel, $CreatedAt, $CreatedBy, $Notes
            );
            SELECT last_insert_rowid();
        """;
        AddParameters(command, document);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public IReadOnlyList<DocumentRecord> GetLatest(int limit = 100)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, DocumentNumber, Title, Category, FilePath, FileName, Extension, SizeBytes,
                   RelatedEntityType, RelatedEntityId, RelatedEntityLabel, CreatedAt, CreatedBy, Notes
            FROM Documents
            ORDER BY CreatedAt DESC
            LIMIT $limit;
        """;
        command.Parameters.AddWithValue("$limit", Math.Max(1, limit));
        return ReadMany(command);
    }

    public IReadOnlyList<DocumentRecord> Search(string query, string category = "", int limit = 100)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        var hasCategory = !string.IsNullOrWhiteSpace(category) && category != "Tutti";
        command.CommandText = hasCategory
            ? """
                SELECT Id, DocumentNumber, Title, Category, FilePath, FileName, Extension, SizeBytes,
                       RelatedEntityType, RelatedEntityId, RelatedEntityLabel, CreatedAt, CreatedBy, Notes
                FROM Documents
                WHERE Category = $Category
                  AND (DocumentNumber LIKE $Query OR Title LIKE $Query OR FileName LIKE $Query OR RelatedEntityLabel LIKE $Query OR Notes LIKE $Query)
                ORDER BY CreatedAt DESC
                LIMIT $limit;
            """
            : """
                SELECT Id, DocumentNumber, Title, Category, FilePath, FileName, Extension, SizeBytes,
                       RelatedEntityType, RelatedEntityId, RelatedEntityLabel, CreatedAt, CreatedBy, Notes
                FROM Documents
                WHERE DocumentNumber LIKE $Query OR Title LIKE $Query OR FileName LIKE $Query OR RelatedEntityLabel LIKE $Query OR Notes LIKE $Query OR Category LIKE $Query
                ORDER BY CreatedAt DESC
                LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$Query", $"%{query}%");
        command.Parameters.AddWithValue("$limit", Math.Max(1, limit));
        if (hasCategory) command.Parameters.AddWithValue("$Category", category);
        return ReadMany(command);
    }

    private IReadOnlyList<DocumentRecord> ReadMany(SqliteCommand command)
    {
        using var reader = command.ExecuteReader();
        var result = new List<DocumentRecord>();
        while (reader.Read()) result.Add(ReadDocument(reader));
        return result;
    }

    private string NextDocumentNumber(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT IFNULL(MAX(Id), 0) + 1 FROM Documents;";
        return _numberGenerator.DocumentNumber(Convert.ToInt32(command.ExecuteScalar()));
    }

    private static void EnrichFileInfo(DocumentRecord document)
    {
        document.FileName = Path.GetFileName(document.FilePath);
        document.Extension = Path.GetExtension(document.FilePath);
        if (File.Exists(document.FilePath)) document.SizeBytes = new FileInfo(document.FilePath).Length;
    }

    private static void AddParameters(SqliteCommand command, DocumentRecord d)
    {
        command.Parameters.AddWithValue("$DocumentNumber", d.DocumentNumber);
        command.Parameters.AddWithValue("$Title", d.Title);
        command.Parameters.AddWithValue("$Category", d.Category);
        command.Parameters.AddWithValue("$FilePath", d.FilePath);
        command.Parameters.AddWithValue("$FileName", d.FileName);
        command.Parameters.AddWithValue("$Extension", d.Extension);
        command.Parameters.AddWithValue("$SizeBytes", d.SizeBytes);
        command.Parameters.AddWithValue("$RelatedEntityType", d.RelatedEntityType);
        command.Parameters.AddWithValue("$RelatedEntityId", d.RelatedEntityId);
        command.Parameters.AddWithValue("$RelatedEntityLabel", d.RelatedEntityLabel);
        command.Parameters.AddWithValue("$CreatedAt", d.CreatedAt);
        command.Parameters.AddWithValue("$CreatedBy", d.CreatedBy);
        command.Parameters.AddWithValue("$Notes", d.Notes);
    }

    private static DocumentRecord ReadDocument(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0),
        DocumentNumber = S(r, 1),
        Title = S(r, 2),
        Category = S(r, 3),
        FilePath = S(r, 4),
        FileName = S(r, 5),
        Extension = S(r, 6),
        SizeBytes = r.GetInt64(7),
        RelatedEntityType = S(r, 8),
        RelatedEntityId = S(r, 9),
        RelatedEntityLabel = S(r, 10),
        CreatedAt = S(r, 11),
        CreatedBy = S(r, 12),
        Notes = S(r, 13)
    };

    private SqliteConnection OpenConnection()
    {
        var c = new SqliteConnection(ConnectionString);
        c.Open();
        return c;
    }

    private static void Execute(SqliteConnection c, string sql)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    private static string S(SqliteDataReader r, int i) => r.IsDBNull(i) ? string.Empty : r.GetString(i);
}
