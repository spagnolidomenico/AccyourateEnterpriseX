using Microsoft.Data.Sqlite;

namespace Accyourate.App.Platform.Notifications;

public sealed class NotificationService
{
    private readonly string _databasePath;

    public NotificationService(string? databasePath = null)
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccyourateEnterpriseX");

        Directory.CreateDirectory(folder);

        _databasePath = databasePath ?? Path.Combine(folder, "accyourate-platform.db");
        Initialize();
    }

    private string ConnectionString => $"Data Source={_databasePath}";

    public void Initialize()
    {
        using var connection = OpenConnection();

        Execute(connection, """
            CREATE TABLE IF NOT EXISTS Notifications (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Message TEXT NOT NULL,
                Category TEXT NOT NULL,
                Priority TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                CreatedBy TEXT,
                IsRead INTEGER NOT NULL DEFAULT 0,
                ReadAt TEXT,
                Action TEXT,
                Payload TEXT
            );
        """);

        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_Notifications_IsRead ON Notifications(IsRead);");
        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_Notifications_Category ON Notifications(Category);");
        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_Notifications_CreatedAt ON Notifications(CreatedAt);");

        SeedSystemWelcomeNotification();
    }

    public int Publish(
        string title,
        string message,
        string category = NotificationCategory.System,
        string priority = NotificationPriority.Info,
        string createdBy = "System",
        string action = "",
        string payload = "")
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO Notifications (
                Title, Message, Category, Priority, CreatedAt, CreatedBy, IsRead, ReadAt, Action, Payload
            )
            VALUES (
                $Title, $Message, $Category, $Priority, $CreatedAt, $CreatedBy, 0, '', $Action, $Payload
            );
            SELECT last_insert_rowid();
        """;

        command.Parameters.AddWithValue("$Title", title);
        command.Parameters.AddWithValue("$Message", message);
        command.Parameters.AddWithValue("$Category", category);
        command.Parameters.AddWithValue("$Priority", priority);
        command.Parameters.AddWithValue("$CreatedAt", DateTime.Now.ToString("s"));
        command.Parameters.AddWithValue("$CreatedBy", createdBy);
        command.Parameters.AddWithValue("$Action", action);
        command.Parameters.AddWithValue("$Payload", payload);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public IReadOnlyList<NotificationRecord> GetLatest(int limit = 20, bool unreadOnly = false)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = unreadOnly
            ? """
                SELECT Id, Title, Message, Category, Priority, CreatedAt, CreatedBy, IsRead, ReadAt, Action, Payload
                FROM Notifications
                WHERE IsRead = 0
                ORDER BY CreatedAt DESC
                LIMIT $limit;
            """
            : """
                SELECT Id, Title, Message, Category, Priority, CreatedAt, CreatedBy, IsRead, ReadAt, Action, Payload
                FROM Notifications
                ORDER BY CreatedAt DESC
                LIMIT $limit;
            """;

        command.Parameters.AddWithValue("$limit", Math.Max(1, limit));

        using var reader = command.ExecuteReader();
        var result = new List<NotificationRecord>();

        while (reader.Read())
            result.Add(ReadNotification(reader));

        return result;
    }

    public int CountUnread()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Notifications WHERE IsRead = 0;";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void MarkAsRead(int notificationId)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = """
            UPDATE Notifications
            SET IsRead = 1,
                ReadAt = $ReadAt
            WHERE Id = $Id;
        """;

        command.Parameters.AddWithValue("$Id", notificationId);
        command.Parameters.AddWithValue("$ReadAt", DateTime.Now.ToString("s"));
        command.ExecuteNonQuery();
    }

    public void MarkAllAsRead()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = """
            UPDATE Notifications
            SET IsRead = 1,
                ReadAt = $ReadAt
            WHERE IsRead = 0;
        """;

        command.Parameters.AddWithValue("$ReadAt", DateTime.Now.ToString("s"));
        command.ExecuteNonQuery();
    }

    public void Delete(int notificationId)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Notifications WHERE Id = $Id;";
        command.Parameters.AddWithValue("$Id", notificationId);
        command.ExecuteNonQuery();
    }

    private void SeedSystemWelcomeNotification()
    {
        using var connection = OpenConnection();
        using var count = connection.CreateCommand();
        count.CommandText = "SELECT COUNT(*) FROM Notifications;";

        if (Convert.ToInt32(count.ExecuteScalar()) > 0)
            return;

        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Notifications (
                Title, Message, Category, Priority, CreatedAt, CreatedBy, IsRead, ReadAt, Action, Payload
            )
            VALUES (
                'Notification Engine attivo',
                'Il motore notifiche di Accyourate Enterprise X è stato inizializzato.',
                'System',
                'Info',
                $CreatedAt,
                'System',
                0,
                '',
                '',
                ''
            );
        """;
        command.Parameters.AddWithValue("$CreatedAt", DateTime.Now.ToString("s"));
        command.ExecuteNonQuery();
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

    private static NotificationRecord ReadNotification(SqliteDataReader reader)
    {
        return new NotificationRecord
        {
            Id = reader.GetInt32(0),
            Title = reader.GetString(1),
            Message = reader.GetString(2),
            Category = reader.GetString(3),
            Priority = reader.GetString(4),
            CreatedAt = reader.GetString(5),
            CreatedBy = ReadString(reader, 6),
            IsRead = reader.GetInt32(7) == 1,
            ReadAt = ReadString(reader, 8),
            Action = ReadString(reader, 9),
            Payload = ReadString(reader, 10)
        };
    }

    private static string ReadString(SqliteDataReader reader, int index)
    {
        return reader.IsDBNull(index) ? string.Empty : reader.GetString(index);
    }

    private static void Execute(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
