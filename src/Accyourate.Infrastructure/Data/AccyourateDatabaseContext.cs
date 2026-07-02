using Microsoft.Data.Sqlite;
using Accyourate.Core.Data;

namespace Accyourate.Infrastructure.Data;

public sealed class AccyourateDatabaseContext : IDatabaseInitializer
{
    private readonly AccyourateDatabaseOptions _options;

    public AccyourateDatabaseContext()
        : this(new AccyourateDatabaseOptions())
    {
    }

    public AccyourateDatabaseContext(AccyourateDatabaseOptions options)
    {
        _options = options;
        DatabasePath = ResolveDatabasePath(options);
    }

    public string DatabasePath { get; }

    public string ConnectionString => $"Data Source={DatabasePath}";

    public SqliteConnection CreateConnection()
    {
        var folder = Path.GetDirectoryName(DatabasePath);
        if (!string.IsNullOrWhiteSpace(folder))
            Directory.CreateDirectory(folder);

        var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        EnableForeignKeys(connection);
        return connection;
    }

    public void Initialize()
    {
        using var connection = CreateConnection();
    }

    public static void EnableForeignKeys(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON;";
        command.ExecuteNonQuery();
    }

    private static string ResolveDatabasePath(AccyourateDatabaseOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ExplicitDatabasePath))
            return options.ExplicitDatabasePath;

        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            options.AppFolderName);

        return Path.Combine(folder, options.DatabaseName);
    }
}
