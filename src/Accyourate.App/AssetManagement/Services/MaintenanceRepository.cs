using Microsoft.Data.Sqlite;
using Accyourate.App.AssetManagement.Models;

namespace Accyourate.App.AssetManagement.Services;

public sealed class MaintenanceRepository
{
    private readonly string _connectionString;

    public MaintenanceRepository(string? databasePath = null)
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AccyourateEnterpriseX");
        Directory.CreateDirectory(folder);
        _connectionString = $"Data Source={databasePath ?? Path.Combine(folder, "accyourate-assets.db")}";
        Initialize();
    }

    private void Initialize()
    {
        using var connection = Open();
        Execute(connection, """
            CREATE TABLE IF NOT EXISTS MaintenanceTickets (
                Id INTEGER PRIMARY KEY AUTOINCREMENT, AssetId INTEGER NOT NULL,
                Title TEXT NOT NULL, Description TEXT, Priority TEXT NOT NULL,
                Status TEXT NOT NULL, OpenedAt TEXT NOT NULL, ClosedAt TEXT,
                Technician TEXT, ResolutionNotes TEXT, ScheduledAt TEXT,
                Cost REAL NOT NULL DEFAULT 0, PdfPath TEXT, UpdatedAt TEXT,
                OverdueNotifiedAt TEXT
            );
            """);
        EnsureColumn(connection, "ScheduledAt", "TEXT");
        EnsureColumn(connection, "Cost", "REAL NOT NULL DEFAULT 0");
        EnsureColumn(connection, "PdfPath", "TEXT");
        EnsureColumn(connection, "UpdatedAt", "TEXT");
        EnsureColumn(connection, "OverdueNotifiedAt", "TEXT");
    }

    public int Create(MaintenanceTicket ticket)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO MaintenanceTickets
            (AssetId,Title,Description,Priority,Status,OpenedAt,ClosedAt,Technician,
             ResolutionNotes,ScheduledAt,Cost,PdfPath,UpdatedAt)
            VALUES
            ($asset,$title,$description,$priority,$status,$opened,'',$technician,
             '',$scheduled,$cost,'',$updated);
            SELECT last_insert_rowid();
            """;
        ticket.OpenedAt = DateTime.Now.ToString("s");
        ticket.UpdatedAt = ticket.OpenedAt;
        Add(command, ticket);
        ticket.Id = Convert.ToInt32(command.ExecuteScalar());
        return ticket.Id;
    }

    public IReadOnlyList<MaintenanceTicket> GetByAsset(int assetId)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = Select + " WHERE AssetId=$asset ORDER BY OpenedAt DESC, Id DESC;";
        command.Parameters.AddWithValue("$asset", assetId);
        using var reader = command.ExecuteReader();
        var result = new List<MaintenanceTicket>();
        while (reader.Read()) result.Add(Read(reader));
        return result;
    }

    public IReadOnlyList<MaintenanceTicket> GetAll(int limit = 1000)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = Select + " ORDER BY OpenedAt DESC, Id DESC LIMIT $limit;";
        command.Parameters.AddWithValue("$limit", Math.Max(1, limit));
        using var reader = command.ExecuteReader();
        var result = new List<MaintenanceTicket>();
        while (reader.Read()) result.Add(Read(reader));
        return result;
    }

    public void MarkOverdueNotified(int id)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE MaintenanceTickets
            SET OverdueNotifiedAt=$now, UpdatedAt=$now
            WHERE Id=$id;
            """;
        command.Parameters.AddWithValue("$now", DateTime.Now.ToString("s"));
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    public void Start(int id)
    {
        UpdateStatus(id, "In lavorazione", string.Empty, 0, string.Empty);
    }

    public void Complete(int id, string resolution, decimal cost, string pdfPath)
    {
        UpdateStatus(id, "Completato", resolution, cost, pdfPath);
    }

    private void UpdateStatus(int id, string status, string resolution, decimal cost, string pdfPath)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE MaintenanceTickets SET Status=$status,
                ClosedAt=CASE WHEN $status='Completato' THEN $now ELSE ClosedAt END,
                ResolutionNotes=CASE WHEN $resolution<>'' THEN $resolution ELSE ResolutionNotes END,
                Cost=CASE WHEN $cost>0 THEN $cost ELSE Cost END,
                PdfPath=CASE WHEN $pdf<>'' THEN $pdf ELSE PdfPath END,
                UpdatedAt=$now WHERE Id=$id;
            """;
        command.Parameters.AddWithValue("$status", status);
        command.Parameters.AddWithValue("$resolution", resolution);
        command.Parameters.AddWithValue("$cost", cost);
        command.Parameters.AddWithValue("$pdf", pdfPath);
        command.Parameters.AddWithValue("$now", DateTime.Now.ToString("s"));
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    private static void Add(SqliteCommand command, MaintenanceTicket ticket)
    {
        command.Parameters.AddWithValue("$asset", ticket.AssetId);
        command.Parameters.AddWithValue("$title", ticket.Title);
        command.Parameters.AddWithValue("$description", ticket.Description);
        command.Parameters.AddWithValue("$priority", ticket.Priority);
        command.Parameters.AddWithValue("$status", ticket.Status);
        command.Parameters.AddWithValue("$opened", ticket.OpenedAt);
        command.Parameters.AddWithValue("$technician", ticket.Technician);
        command.Parameters.AddWithValue("$scheduled", ticket.ScheduledAt);
        command.Parameters.AddWithValue("$cost", ticket.Cost);
        command.Parameters.AddWithValue("$updated", ticket.UpdatedAt);
    }

    private static MaintenanceTicket Read(SqliteDataReader r) => new()
    {
        Id=r.GetInt32(0), AssetId=r.GetInt32(1), Title=S(r,2), Description=S(r,3),
        Priority=S(r,4), Status=S(r,5), OpenedAt=S(r,6), ClosedAt=S(r,7),
        Technician=S(r,8), ResolutionNotes=S(r,9), ScheduledAt=S(r,10),
        Cost=r.IsDBNull(11)?0:Convert.ToDecimal(r.GetDouble(11)), PdfPath=S(r,12),
        UpdatedAt=S(r,13), OverdueNotifiedAt=S(r,14)
    };

    private SqliteConnection Open() { var c=new SqliteConnection(_connectionString); c.Open(); return c; }
    private static string S(SqliteDataReader r,int i)=>r.IsDBNull(i)?string.Empty:r.GetString(i);
    private static void Execute(SqliteConnection c,string sql){using var cmd=c.CreateCommand();cmd.CommandText=sql;cmd.ExecuteNonQuery();}
    private static void EnsureColumn(SqliteConnection c,string name,string definition)
    {
        using var check=c.CreateCommand(); check.CommandText="PRAGMA table_info(MaintenanceTickets);";
        using var r=check.ExecuteReader(); while(r.Read()) if(string.Equals(r.GetString(1),name,StringComparison.OrdinalIgnoreCase)) return;
        using var alter=c.CreateCommand(); alter.CommandText=$"ALTER TABLE MaintenanceTickets ADD COLUMN {name} {definition};"; alter.ExecuteNonQuery();
    }
    private const string Select = """
        SELECT Id,AssetId,Title,Description,Priority,Status,OpenedAt,ClosedAt,
               Technician,ResolutionNotes,ScheduledAt,Cost,PdfPath,UpdatedAt,OverdueNotifiedAt
        FROM MaintenanceTickets
        """;
}
