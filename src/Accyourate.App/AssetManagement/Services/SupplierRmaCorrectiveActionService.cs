using Accyourate.App.Platform.Notifications;
using Microsoft.Data.Sqlite;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SupplierRmaCorrectiveAction
{
    public int Id { get; set; }
    public int ComplianceAuditId { get; set; }
    public int RmaId { get; set; }
    public string CaseNumber { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Responsible { get; set; } = "";
    public string DueDate { get; set; } = "";
    public string Priority { get; set; } = "Normale";
    public string Status { get; set; } = "Aperta";
    public string VerificationNotes { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public string CreatedBy { get; set; } = "";
    public string CompletedAt { get; set; } = "";
    public bool IsOverdue => Status is not ("Completata" or "Annullata") && DateTime.TryParse(DueDate, out var date) && date.Date < DateTime.Today;
}

public sealed class SupplierRmaCorrectiveActionService
{
    private readonly string _connectionString;
    private readonly NotificationService _notifications;

    public SupplierRmaCorrectiveActionService(string? databasePath = null, NotificationService? notifications = null)
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AccyourateEnterpriseX");
        Directory.CreateDirectory(folder);
        _connectionString = $"Data Source={databasePath ?? Path.Combine(folder, "accyourate-assets.db")}";
        _notifications = notifications ?? new NotificationService();
        using var connection = Open(); using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS SupplierRmaCorrectiveActions(
              Id INTEGER PRIMARY KEY AUTOINCREMENT,ComplianceAuditId INTEGER NOT NULL,RmaId INTEGER NOT NULL,
              CaseNumber TEXT NOT NULL,Title TEXT NOT NULL,Description TEXT,Responsible TEXT NOT NULL,DueDate TEXT NOT NULL,
              Priority TEXT NOT NULL,Status TEXT NOT NULL,VerificationNotes TEXT,CreatedAt TEXT NOT NULL,CreatedBy TEXT NOT NULL,CompletedAt TEXT);
            CREATE INDEX IF NOT EXISTS IX_SupplierRmaCorrectiveActions_StatusDue ON SupplierRmaCorrectiveActions(Status,DueDate);
            CREATE TABLE IF NOT EXISTS SupplierRmaCorrectiveActionNotifications(NotificationKey TEXT PRIMARY KEY,ActionId INTEGER NOT NULL,CreatedAt TEXT NOT NULL);
            """;
        command.ExecuteNonQuery();
    }

    public int Create(SupplierRmaCorrectiveAction value)
    {
        if (value.ComplianceAuditId <= 0 || value.RmaId <= 0) throw new InvalidOperationException("Verifica di conformita non valida.");
        if (string.IsNullOrWhiteSpace(value.Title) || string.IsNullOrWhiteSpace(value.Responsible)) throw new InvalidOperationException("Inserisci titolo e responsabile.");
        if (!DateTime.TryParse(value.DueDate, out var due)) throw new InvalidOperationException("Inserisci una scadenza valida.");
        using var connection = Open(); using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO SupplierRmaCorrectiveActions(ComplianceAuditId,RmaId,CaseNumber,Title,Description,Responsible,DueDate,Priority,Status,VerificationNotes,CreatedAt,CreatedBy,CompletedAt)
            VALUES($audit,$rma,$case,$title,$description,$responsible,$due,$priority,'Aperta','',$created,$user,''); SELECT last_insert_rowid();
            """;
        command.Parameters.AddWithValue("$audit",value.ComplianceAuditId); command.Parameters.AddWithValue("$rma",value.RmaId); command.Parameters.AddWithValue("$case",value.CaseNumber);
        command.Parameters.AddWithValue("$title",value.Title.Trim()); command.Parameters.AddWithValue("$description",value.Description.Trim()); command.Parameters.AddWithValue("$responsible",value.Responsible.Trim());
        command.Parameters.AddWithValue("$due",due.Date.ToString("yyyy-MM-dd")); command.Parameters.AddWithValue("$priority",value.Priority); command.Parameters.AddWithValue("$created",DateTime.Now.ToString("s")); command.Parameters.AddWithValue("$user",value.CreatedBy);
        var id=Convert.ToInt32(command.ExecuteScalar());
        _notifications.Publish("Azione correttiva RMA assegnata",$"{value.CaseNumber}: {value.Title} - scadenza {due:dd/MM/yyyy}",NotificationCategory.Asset,value.Priority=="Urgente"?NotificationPriority.High:NotificationPriority.Normal,value.CreatedBy,"open-rma-corrective-actions",id.ToString());
        return id;
    }

    public IReadOnlyList<SupplierRmaCorrectiveAction> GetAll()
    {
        using var connection=Open(); using var command=connection.CreateCommand();
        command.CommandText="SELECT Id,ComplianceAuditId,RmaId,CaseNumber,Title,Description,Responsible,DueDate,Priority,Status,VerificationNotes,CreatedAt,CreatedBy,CompletedAt FROM SupplierRmaCorrectiveActions ORDER BY CASE Status WHEN 'Aperta' THEN 0 WHEN 'In corso' THEN 1 ELSE 2 END,DueDate,Id DESC;";
        using var reader=command.ExecuteReader(); var values=new List<SupplierRmaCorrectiveAction>();
        while(reader.Read())values.Add(new(){Id=reader.GetInt32(0),ComplianceAuditId=reader.GetInt32(1),RmaId=reader.GetInt32(2),CaseNumber=S(reader,3),Title=S(reader,4),Description=S(reader,5),Responsible=S(reader,6),DueDate=S(reader,7),Priority=S(reader,8),Status=S(reader,9),VerificationNotes=S(reader,10),CreatedAt=S(reader,11),CreatedBy=S(reader,12),CompletedAt=S(reader,13)});
        return values;
    }

    public void ChangeStatus(int id,string status,string notes,string user)
    {
        if(status is not ("Aperta" or "In corso" or "Completata" or "Annullata"))throw new InvalidOperationException("Stato non valido.");
        using var connection=Open(); using var command=connection.CreateCommand(); command.CommandText="UPDATE SupplierRmaCorrectiveActions SET Status=$status,VerificationNotes=$notes,CompletedAt=$completed WHERE Id=$id;";
        command.Parameters.AddWithValue("$id",id);command.Parameters.AddWithValue("$status",status);command.Parameters.AddWithValue("$notes",notes.Trim());command.Parameters.AddWithValue("$completed",status=="Completata"?DateTime.Now.ToString("s"):"");
        if(command.ExecuteNonQuery()==0)throw new InvalidOperationException("Azione correttiva non trovata.");
        if(status=="Completata")_notifications.Publish("Azione correttiva RMA completata",$"Azione #{id} chiusa da {user}.",NotificationCategory.Asset,NotificationPriority.Info,user,"open-rma-corrective-actions",id.ToString());
    }

    public int PublishDueNotifications()
    {
        var count=0;
        foreach(var value in GetAll().Where(x=>x.Status is "Aperta" or "In corso"))
        {
            if(!DateTime.TryParse(value.DueDate,out var due))continue;var days=(due.Date-DateTime.Today).Days;if(days>7)continue;var type=days<0?"overdue":"due";var key=$"rma-corrective:{type}:{value.Id}:{DateTime.Today:yyyyMMdd}";
            using var connection=Open();using var check=connection.CreateCommand();check.CommandText="SELECT COUNT(*) FROM SupplierRmaCorrectiveActionNotifications WHERE NotificationKey=$key;";check.Parameters.AddWithValue("$key",key);if(Convert.ToInt32(check.ExecuteScalar())>0)continue;
            _notifications.Publish(days<0?"Azione correttiva RMA scaduta":"Azione correttiva RMA in scadenza",$"{value.CaseNumber}: {value.Title} - responsabile {value.Responsible}, scadenza {due:dd/MM/yyyy}.",NotificationCategory.Asset,days<0?NotificationPriority.Critical:NotificationPriority.High,"Controllo conformita RMA","open-rma-corrective-actions",value.Id.ToString());
            using var insert=connection.CreateCommand();insert.CommandText="INSERT INTO SupplierRmaCorrectiveActionNotifications(NotificationKey,ActionId,CreatedAt) VALUES($key,$id,$date);";insert.Parameters.AddWithValue("$key",key);insert.Parameters.AddWithValue("$id",value.Id);insert.Parameters.AddWithValue("$date",DateTime.Now.ToString("s"));insert.ExecuteNonQuery();count++;
        }
        return count;
    }

    private SqliteConnection Open(){var connection=new SqliteConnection(_connectionString);connection.Open();return connection;}
    private static string S(SqliteDataReader reader,int index)=>reader.IsDBNull(index)?"":reader.GetString(index);
}
