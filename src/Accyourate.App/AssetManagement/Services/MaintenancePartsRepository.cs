using Microsoft.Data.Sqlite;
using Accyourate.App.AssetManagement.Models;

namespace Accyourate.App.AssetManagement.Services;

public sealed class MaintenancePartsRepository
{
    private readonly string _connectionString;

    public MaintenancePartsRepository(string? databasePath = null)
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccyourateEnterpriseX");
        Directory.CreateDirectory(folder);
        _connectionString = $"Data Source={databasePath ?? Path.Combine(folder, "accyourate-assets.db")}";
        Initialize();
    }

    private void Initialize()
    {
        using var connection = Open();
        Execute(connection, """
            CREATE TABLE IF NOT EXISTS MaintenanceSuppliers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL UNIQUE,
                Email TEXT,
                Phone TEXT,
                Notes TEXT
            );
            CREATE TABLE IF NOT EXISTS MaintenanceParts (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                MaintenanceTicketId INTEGER NOT NULL,
                PartCode TEXT,
                Description TEXT NOT NULL,
                Supplier TEXT,
                Quantity REAL NOT NULL DEFAULT 1,
                UnitCost REAL NOT NULL DEFAULT 0,
                Notes TEXT,
                CreatedAt TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS IX_MaintenanceParts_Ticket
            ON MaintenanceParts(MaintenanceTicketId);
            """);
    }

    public int Add(MaintenancePart part)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO MaintenanceParts
            (MaintenanceTicketId,PartCode,Description,Supplier,Quantity,UnitCost,Notes,CreatedAt)
            VALUES ($ticket,$code,$description,$supplier,$quantity,$cost,$notes,$created);
            SELECT last_insert_rowid();
            """;
        part.CreatedAt = DateTime.Now.ToString("s");
        command.Parameters.AddWithValue("$ticket", part.MaintenanceTicketId);
        command.Parameters.AddWithValue("$code", part.PartCode);
        command.Parameters.AddWithValue("$description", part.Description);
        command.Parameters.AddWithValue("$supplier", part.Supplier);
        command.Parameters.AddWithValue("$quantity", part.Quantity);
        command.Parameters.AddWithValue("$cost", part.UnitCost);
        command.Parameters.AddWithValue("$notes", part.Notes);
        command.Parameters.AddWithValue("$created", part.CreatedAt);
        part.Id = Convert.ToInt32(command.ExecuteScalar());
        if (!string.IsNullOrWhiteSpace(part.Supplier))
            EnsureSupplier(part.Supplier);
        return part.Id;
    }

    public void Delete(int id)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM MaintenanceParts WHERE Id=$id;";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<MaintenancePart> GetByTicket(int ticketId)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = Select + " WHERE MaintenanceTicketId=$ticket ORDER BY Id;";
        command.Parameters.AddWithValue("$ticket", ticketId);
        using var reader = command.ExecuteReader();
        var result = new List<MaintenancePart>();
        while (reader.Read()) result.Add(Read(reader));
        return result;
    }

    public IReadOnlyDictionary<int, decimal> GetTotalsByTicket()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT MaintenanceTicketId, COALESCE(SUM(Quantity*UnitCost),0)
            FROM MaintenanceParts
            GROUP BY MaintenanceTicketId;
            """;
        using var reader = command.ExecuteReader();
        var result = new Dictionary<int, decimal>();
        while (reader.Read())
            result[reader.GetInt32(0)] = Convert.ToDecimal(reader.GetDouble(1));
        return result;
    }

    public IReadOnlyList<string> GetSupplierNames()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Name FROM MaintenanceSuppliers ORDER BY Name;";
        using var reader = command.ExecuteReader();
        var result = new List<string>();
        while (reader.Read()) result.Add(reader.GetString(0));
        return result;
    }

    private void EnsureSupplier(string name)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO MaintenanceSuppliers(Name,Email,Phone,Notes)
            VALUES($name,'','','')
            ON CONFLICT(Name) DO NOTHING;
            """;
        command.Parameters.AddWithValue("$name", name.Trim());
        command.ExecuteNonQuery();
    }

    private static MaintenancePart Read(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        MaintenanceTicketId = reader.GetInt32(1),
        PartCode = S(reader, 2),
        Description = S(reader, 3),
        Supplier = S(reader, 4),
        Quantity = reader.IsDBNull(5) ? 0 : Convert.ToDecimal(reader.GetDouble(5)),
        UnitCost = reader.IsDBNull(6) ? 0 : Convert.ToDecimal(reader.GetDouble(6)),
        Notes = S(reader, 7),
        CreatedAt = S(reader, 8)
    };

    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private static string S(SqliteDataReader reader, int index) =>
        reader.IsDBNull(index) ? string.Empty : reader.GetString(index);

    private static void Execute(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private const string Select = """
        SELECT Id,MaintenanceTicketId,PartCode,Description,Supplier,Quantity,UnitCost,Notes,CreatedAt
        FROM MaintenanceParts
        """;
}
