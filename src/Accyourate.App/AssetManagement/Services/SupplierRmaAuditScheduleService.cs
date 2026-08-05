using Accyourate.App.Platform.Notifications;
using Microsoft.Data.Sqlite;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SupplierRmaAuditSchedule
{
    public int Id { get; set; }
    public string Name { get; set; } = "Audit RMA periodico";
    public string Frequency { get; set; } = "Mensile";
    public string NextRun { get; set; } = DateTime.Today.AddMonths(1).ToString("yyyy-MM-dd");
    public bool IsActive { get; set; } = true;
    public string LastRunAt { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public string UpdatedAt { get; set; } = "";
    public string DisplayStatus => !IsActive ? "Disattiva" : DateTime.TryParse(NextRun, out var date) && date.Date < DateTime.Today ? "Scaduta" : DateTime.TryParse(NextRun, out date) && date.Date <= DateTime.Today.AddDays(7) ? "In scadenza" : "Attiva";
}

public sealed class SupplierRmaAuditRun
{
    public int Id { get; set; }
    public int ScheduleId { get; set; }
    public string ScheduleName { get; set; } = "";
    public string ScheduledFor { get; set; } = "";
    public string ExecutedAt { get; set; } = "";
    public string Status { get; set; } = "";
    public string PdfPath { get; set; } = "";
    public string ExecutedBy { get; set; } = "";
    public string Notes { get; set; } = "";
}

public sealed class SupplierRmaAuditScheduleService
{
    private readonly string _connectionString;
    private readonly NotificationService _notifications;

    public SupplierRmaAuditScheduleService(string? databasePath = null, NotificationService? notifications = null)
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AccyourateEnterpriseX");
        Directory.CreateDirectory(folder); var path = databasePath ?? Path.Combine(folder, "accyourate-assets.db");
        _connectionString = $"Data Source={path}"; _notifications = notifications ?? new NotificationService();
        using var connection = Open(); using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS SupplierRmaAuditSchedules(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,Name TEXT NOT NULL,Frequency TEXT NOT NULL,NextRun TEXT NOT NULL,
                IsActive INTEGER NOT NULL DEFAULT 1,LastRunAt TEXT,CreatedAt TEXT NOT NULL,UpdatedAt TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS SupplierRmaAuditRuns(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,ScheduleId INTEGER NOT NULL,ScheduledFor TEXT,ExecutedAt TEXT NOT NULL,
                Status TEXT NOT NULL,PdfPath TEXT,ExecutedBy TEXT,Notes TEXT);
            CREATE TABLE IF NOT EXISTS SupplierRmaAuditScheduleNotifications(NotificationKey TEXT PRIMARY KEY,CreatedAt TEXT NOT NULL);
            CREATE INDEX IF NOT EXISTS IX_SupplierRmaAuditRuns_Schedule ON SupplierRmaAuditRuns(ScheduleId,ExecutedAt);
            """; command.ExecuteNonQuery();
    }

    public IReadOnlyList<SupplierRmaAuditSchedule> GetSchedules()
    {
        using var connection = Open(); using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id,Name,Frequency,NextRun,IsActive,LastRunAt,CreatedAt,UpdatedAt FROM SupplierRmaAuditSchedules ORDER BY IsActive DESC,NextRun,Id;";
        using var reader = command.ExecuteReader(); var result = new List<SupplierRmaAuditSchedule>();
        while (reader.Read()) result.Add(new() { Id=reader.GetInt32(0),Name=Text(reader,1),Frequency=Text(reader,2),NextRun=Text(reader,3),IsActive=reader.GetInt32(4)==1,LastRunAt=Text(reader,5),CreatedAt=Text(reader,6),UpdatedAt=Text(reader,7) });
        return result;
    }

    public IReadOnlyList<SupplierRmaAuditRun> GetRuns()
    {
        using var connection = Open(); using var command = connection.CreateCommand();
        command.CommandText = "SELECT r.Id,r.ScheduleId,COALESCE(s.Name,'Pianificazione rimossa'),r.ScheduledFor,r.ExecutedAt,r.Status,r.PdfPath,r.ExecutedBy,r.Notes FROM SupplierRmaAuditRuns r LEFT JOIN SupplierRmaAuditSchedules s ON s.Id=r.ScheduleId ORDER BY r.ExecutedAt DESC,r.Id DESC;";
        using var reader = command.ExecuteReader(); var result = new List<SupplierRmaAuditRun>();
        while(reader.Read()) result.Add(new(){Id=reader.GetInt32(0),ScheduleId=reader.GetInt32(1),ScheduleName=Text(reader,2),ScheduledFor=Text(reader,3),ExecutedAt=Text(reader,4),Status=Text(reader,5),PdfPath=Text(reader,6),ExecutedBy=Text(reader,7),Notes=Text(reader,8)});
        return result;
    }

    public int Save(SupplierRmaAuditSchedule value)
    {
        if(string.IsNullOrWhiteSpace(value.Name)) throw new InvalidOperationException("Inserisci il nome della pianificazione.");
        if(!DateTime.TryParse(value.NextRun,out _)) throw new InvalidOperationException("Inserisci una data valida.");
        var now=DateTime.Now.ToString("s"); using var connection=Open(); using var command=connection.CreateCommand();
        command.CommandText="INSERT INTO SupplierRmaAuditSchedules(Name,Frequency,NextRun,IsActive,LastRunAt,CreatedAt,UpdatedAt) VALUES($name,$frequency,$next,$active,'',$now,$now);SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$name",value.Name.Trim());command.Parameters.AddWithValue("$frequency",value.Frequency);command.Parameters.AddWithValue("$next",DateTime.Parse(value.NextRun).ToString("yyyy-MM-dd"));command.Parameters.AddWithValue("$active",value.IsActive?1:0);command.Parameters.AddWithValue("$now",now);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void Toggle(int id,bool active){using var connection=Open();using var command=connection.CreateCommand();command.CommandText="UPDATE SupplierRmaAuditSchedules SET IsActive=$active,UpdatedAt=$date WHERE Id=$id;";command.Parameters.AddWithValue("$id",id);command.Parameters.AddWithValue("$active",active?1:0);command.Parameters.AddWithValue("$date",DateTime.Now.ToString("s"));command.ExecuteNonQuery();}

    public string Run(SupplierRmaAuditSchedule schedule,string user,string notes="")
    {
        var cases=new SparePartRmaRepository().GetAll();var closures=new SupplierRmaValidationService().GetClosures();
        var path=new SupplierRmaAuditPdfService().Generate(cases,closures);var now=DateTime.Now;var next=Next(DateTime.TryParse(schedule.NextRun,out var planned)?planned:now,schedule.Frequency,now);
        using var connection=Open();using var transaction=connection.BeginTransaction();
        using(var insert=connection.CreateCommand()){insert.Transaction=transaction;insert.CommandText="INSERT INTO SupplierRmaAuditRuns(ScheduleId,ScheduledFor,ExecutedAt,Status,PdfPath,ExecutedBy,Notes) VALUES($schedule,$planned,$date,'Completata',$path,$user,$notes);";insert.Parameters.AddWithValue("$schedule",schedule.Id);insert.Parameters.AddWithValue("$planned",schedule.NextRun);insert.Parameters.AddWithValue("$date",now.ToString("s"));insert.Parameters.AddWithValue("$path",path);insert.Parameters.AddWithValue("$user",user);insert.Parameters.AddWithValue("$notes",notes.Trim());insert.ExecuteNonQuery();}
        using(var update=connection.CreateCommand()){update.Transaction=transaction;update.CommandText="UPDATE SupplierRmaAuditSchedules SET LastRunAt=$date,NextRun=$next,UpdatedAt=$date WHERE Id=$id;";update.Parameters.AddWithValue("$id",schedule.Id);update.Parameters.AddWithValue("$date",now.ToString("s"));update.Parameters.AddWithValue("$next",next.ToString("yyyy-MM-dd"));update.ExecuteNonQuery();}transaction.Commit();return path;
    }

    public int PublishDueNotifications()
    {
        var count=0;foreach(var item in GetSchedules().Where(x=>x.IsActive))
        {
            if(!DateTime.TryParse(item.NextRun,out var due)||due.Date>DateTime.Today.AddDays(7))continue;
            var key=$"audit-rma:{item.Id}:{due:yyyyMMdd}";using var connection=Open();using var check=connection.CreateCommand();check.CommandText="INSERT OR IGNORE INTO SupplierRmaAuditScheduleNotifications(NotificationKey,CreatedAt) VALUES($key,$date);SELECT changes();";check.Parameters.AddWithValue("$key",key);check.Parameters.AddWithValue("$date",DateTime.Now.ToString("s"));if(Convert.ToInt32(check.ExecuteScalar())==0)continue;
            var overdue=due.Date<DateTime.Today;_notifications.Publish(overdue?"Audit RMA scaduto":"Audit RMA da pianificare",$"{item.Name}: esecuzione prevista il {due:dd/MM/yyyy}.",NotificationCategory.Asset,overdue?NotificationPriority.Critical:NotificationPriority.High,"Audit RMA","open-rma-audit-schedules",item.Id.ToString());count++;
        }return count;
    }

    private static DateTime Next(DateTime current,string frequency,DateTime now){var next=frequency switch{"Trimestrale"=>current.AddMonths(3),"Annuale"=>current.AddYears(1),_=>current.AddMonths(1)};while(next.Date<=now.Date)next=frequency switch{"Trimestrale"=>next.AddMonths(3),"Annuale"=>next.AddYears(1),_=>next.AddMonths(1)};return next;}
    private SqliteConnection Open(){var connection=new SqliteConnection(_connectionString);connection.Open();return connection;}
    private static string Text(SqliteDataReader reader,int index)=>reader.IsDBNull(index)?"":reader.GetString(index);
}
