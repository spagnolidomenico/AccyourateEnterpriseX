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
                OverdueNotifiedAt TEXT, ReminderDays INTEGER NOT NULL DEFAULT 7,
                RecurrenceMonths INTEGER NOT NULL DEFAULT 0, NextTicketId INTEGER NOT NULL DEFAULT 0,
                ReminderNotifiedAt TEXT, WorkStartedAt TEXT, SlaDeadline TEXT,
                DowntimeMinutes INTEGER NOT NULL DEFAULT 0, SlaBreachedNotifiedAt TEXT
            );
            """);
        EnsureColumn(connection, "ScheduledAt", "TEXT");
        EnsureColumn(connection, "Cost", "REAL NOT NULL DEFAULT 0");
        EnsureColumn(connection, "PdfPath", "TEXT");
        EnsureColumn(connection, "UpdatedAt", "TEXT");
        EnsureColumn(connection, "OverdueNotifiedAt", "TEXT");
        EnsureColumn(connection, "ReminderDays", "INTEGER NOT NULL DEFAULT 7");
        EnsureColumn(connection, "RecurrenceMonths", "INTEGER NOT NULL DEFAULT 0");
        EnsureColumn(connection, "NextTicketId", "INTEGER NOT NULL DEFAULT 0");
        EnsureColumn(connection, "ReminderNotifiedAt", "TEXT");
        EnsureColumn(connection, "WorkStartedAt", "TEXT");
        EnsureColumn(connection, "SlaDeadline", "TEXT");
        EnsureColumn(connection, "DowntimeMinutes", "INTEGER NOT NULL DEFAULT 0");
        EnsureColumn(connection, "SlaBreachedNotifiedAt", "TEXT");
    }

    public int Create(MaintenanceTicket ticket)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO MaintenanceTickets
            (AssetId,Title,Description,Priority,Status,OpenedAt,ClosedAt,Technician,
             ResolutionNotes,ScheduledAt,Cost,PdfPath,UpdatedAt,ReminderDays,RecurrenceMonths,NextTicketId,
             WorkStartedAt,SlaDeadline,DowntimeMinutes,SlaBreachedNotifiedAt)
            VALUES
            ($asset,$title,$description,$priority,$status,$opened,'',$technician,
             '',$scheduled,$cost,'',$updated,$reminder,$recurrence,0,
             $started,$sla,$downtime,'');
            SELECT last_insert_rowid();
            """;
        ticket.OpenedAt = DateTime.Now.ToString("s");
        ticket.UpdatedAt = ticket.OpenedAt;
        if (ticket.Status != "Pianificato")
        {
            ticket.WorkStartedAt = ticket.OpenedAt;
            ticket.SlaDeadline = DateTime.Now.AddHours(SlaHours(ticket.Priority)).ToString("s");
        }
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

    public void MarkReminderNotified(int id)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE MaintenanceTickets
            SET ReminderNotifiedAt=$now, UpdatedAt=$now
            WHERE Id=$id;
            """;
        command.Parameters.AddWithValue("$now", DateTime.Now.ToString("s"));
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    public void Start(int id)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE MaintenanceTickets
            SET Status='In lavorazione', WorkStartedAt=$now,
                SlaDeadline=$sla, UpdatedAt=$now
            WHERE Id=$id;
            """;
        var ticket = GetById(id);
        var now = DateTime.Now;
        command.Parameters.AddWithValue("$now", now.ToString("s"));
        command.Parameters.AddWithValue("$sla", now.AddHours(SlaHours(ticket?.Priority ?? "Media")).ToString("s"));
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    public void Complete(int id, string resolution, decimal cost, string pdfPath)
    {
        UpdateStatus(id, "Completato", resolution, cost, pdfPath);
        UpdateDowntime(id);
        ScheduleNextOccurrence(id);
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
        command.Parameters.AddWithValue("$reminder", Math.Max(0, ticket.ReminderDays));
        command.Parameters.AddWithValue("$recurrence", Math.Max(0, ticket.RecurrenceMonths));
        command.Parameters.AddWithValue("$started", ticket.WorkStartedAt);
        command.Parameters.AddWithValue("$sla", ticket.SlaDeadline);
        command.Parameters.AddWithValue("$downtime", Math.Max(0, ticket.DowntimeMinutes));
    }

    private static MaintenanceTicket Read(SqliteDataReader r) => new()
    {
        Id=r.GetInt32(0), AssetId=r.GetInt32(1), Title=S(r,2), Description=S(r,3),
        Priority=S(r,4), Status=S(r,5), OpenedAt=S(r,6), ClosedAt=S(r,7),
        Technician=S(r,8), ResolutionNotes=S(r,9), ScheduledAt=S(r,10),
        Cost=r.IsDBNull(11)?0:Convert.ToDecimal(r.GetDouble(11)), PdfPath=S(r,12),
        UpdatedAt=S(r,13), OverdueNotifiedAt=S(r,14),
        ReminderDays=r.IsDBNull(15)?7:r.GetInt32(15),
        RecurrenceMonths=r.IsDBNull(16)?0:r.GetInt32(16),
        NextTicketId=r.IsDBNull(17)?0:r.GetInt32(17),
        ReminderNotifiedAt=S(r,18), WorkStartedAt=S(r,19), SlaDeadline=S(r,20),
        DowntimeMinutes=r.IsDBNull(21)?0:r.GetInt32(21), SlaBreachedNotifiedAt=S(r,22)
    };

    public MaintenanceTicket? GetById(int id)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = Select + " WHERE Id=$id;";
        command.Parameters.AddWithValue("$id", id);
        using var reader = command.ExecuteReader();
        return reader.Read() ? Read(reader) : null;
    }

    public void MarkSlaBreachedNotified(int id)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE MaintenanceTickets
            SET SlaBreachedNotifiedAt=$now, UpdatedAt=$now
            WHERE Id=$id;
            """;
        command.Parameters.AddWithValue("$now", DateTime.Now.ToString("s"));
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    private void UpdateDowntime(int id)
    {
        var ticket = GetById(id);
        var startText = string.IsNullOrWhiteSpace(ticket?.WorkStartedAt) ? ticket?.OpenedAt : ticket.WorkStartedAt;
        if (!DateTime.TryParse(startText, out var started)) return;
        var minutes = Math.Max(0, (int)Math.Round((DateTime.Now - started).TotalMinutes));
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE MaintenanceTickets SET DowntimeMinutes=$minutes WHERE Id=$id;";
        command.Parameters.AddWithValue("$minutes", minutes);
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    private void ScheduleNextOccurrence(int completedId)
    {
        using var connection = Open();
        using var select = connection.CreateCommand();
        select.CommandText = Select + " WHERE Id=$id;";
        select.Parameters.AddWithValue("$id", completedId);
        using var reader = select.ExecuteReader();
        if (!reader.Read()) return;
        var completed = Read(reader);
        reader.Close();
        if (completed.RecurrenceMonths <= 0 || completed.NextTicketId > 0 ||
            !DateTime.TryParse(completed.ScheduledAt, out var scheduled))
            return;

        var next = new MaintenanceTicket
        {
            AssetId = completed.AssetId,
            Title = completed.Title,
            Description = completed.Description,
            Priority = completed.Priority,
            Status = "Pianificato",
            Technician = completed.Technician,
            ScheduledAt = scheduled.AddMonths(completed.RecurrenceMonths).ToString("s"),
            ReminderDays = completed.ReminderDays,
            RecurrenceMonths = completed.RecurrenceMonths
        };
        var nextId = Create(next);
        using var update = connection.CreateCommand();
        update.CommandText = "UPDATE MaintenanceTickets SET NextTicketId=$next WHERE Id=$id;";
        update.Parameters.AddWithValue("$next", nextId);
        update.Parameters.AddWithValue("$id", completedId);
        update.ExecuteNonQuery();
    }

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
               Technician,ResolutionNotes,ScheduledAt,Cost,PdfPath,UpdatedAt,OverdueNotifiedAt,
               ReminderDays,RecurrenceMonths,NextTicketId,ReminderNotifiedAt,
               WorkStartedAt,SlaDeadline,DowntimeMinutes,SlaBreachedNotifiedAt
        FROM MaintenanceTickets
        """;

    public static int SlaHours(string priority) => priority switch
    {
        "Urgente" => 4,
        "Alta" => 8,
        "Bassa" => 72,
        _ => 24
    };
}
