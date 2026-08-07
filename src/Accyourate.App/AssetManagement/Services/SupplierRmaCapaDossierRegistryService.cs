using Microsoft.Data.Sqlite;
using Accyourate.App.Platform.Notifications;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SupplierRmaCapaDossierRegistryRecord
{
    public int Id { get; init; }
    public int ActionId { get; init; }
    public string CaseNumber { get; init; } = "";
    public string ActionTitle { get; init; } = "";
    public string Operation { get; init; } = "";
    public string Outcome { get; init; } = "";
    public int FileCount { get; init; }
    public int AnomalyCount { get; init; }
    public string ArchivePath { get; init; } = "";
    public string ReportPath { get; init; } = "";
    public string CreatedAt { get; init; } = "";
    public string CreatedBy { get; init; } = "";
    public string DocumentStatus { get; init; } = "Attivo";
    public string Custodian { get; init; } = "";
    public string Revision { get; init; } = "1";
    public string RetentionUntil { get; init; } = "";
    public string ControlNotes { get; init; } = "";
    public string UpdatedAt { get; init; } = "";
    public string UpdatedBy { get; init; } = "";
    public string ReviewDueDate { get; init; } = "";
    public string LastReviewDate { get; init; } = "";
    public string ReviewOutcome { get; init; } = "Da eseguire";
    public string ReviewNotes { get; init; } = "";
    public string ReviewedBy { get; init; } = "";
    public string ApprovalStatus { get; init; } = "Bozza"; public string Approver { get; init; } = ""; public string ApprovalDate { get; init; } = ""; public string ApprovalNotes { get; init; } = ""; public bool IsLocked => ApprovalStatus=="Approvato";
    public bool ArchiveAvailable => File.Exists(ArchivePath);
    public bool ReportAvailable => File.Exists(ReportPath);
    public bool MissingDocuments => !ArchiveAvailable || (Operation=="Verifica"&&!ReportAvailable);
    public bool IsExpired => DateTime.TryParse(RetentionUntil,out var date)&&date.Date<DateTime.Today;
    public bool IsDueSoon => DateTime.TryParse(RetentionUntil,out var date)&&date.Date>=DateTime.Today&&date.Date<=DateTime.Today.AddDays(30);
    public bool IsReviewOverdue => DateTime.TryParse(ReviewDueDate,out var date)&&date.Date<DateTime.Today;
    public bool IsReviewDueSoon => DateTime.TryParse(ReviewDueDate,out var date)&&date.Date>=DateTime.Today&&date.Date<=DateTime.Today.AddDays(30);
}

public sealed class SupplierRmaCapaDossierRegistryEvent
{
    public int Id { get; init; } public int RegistryId { get; init; } public string EventType { get; init; }=""; public string OldValue { get; init; }=""; public string NewValue { get; init; }=""; public string Notes { get; init; }=""; public string CreatedAt { get; init; }=""; public string CreatedBy { get; init; }="";
}

public sealed class SupplierRmaCapaDossierRegistryService
{
    private readonly string _connectionString;
    public SupplierRmaCapaDossierRegistryService(string? databasePath=null)
    {
        var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"AccyourateEnterpriseX");Directory.CreateDirectory(folder);
        _connectionString=$"Data Source={databasePath??Path.Combine(folder,"accyourate-assets.db")}";
        using var connection=Open();using var command=connection.CreateCommand();command.CommandText="""
            CREATE TABLE IF NOT EXISTS SupplierRmaCapaDossierRegistry(
              Id INTEGER PRIMARY KEY AUTOINCREMENT,ActionId INTEGER NOT NULL,CaseNumber TEXT NOT NULL,ActionTitle TEXT NOT NULL,
              Operation TEXT NOT NULL,Outcome TEXT NOT NULL,FileCount INTEGER NOT NULL DEFAULT 0,AnomalyCount INTEGER NOT NULL DEFAULT 0,
              ArchivePath TEXT NOT NULL,ReportPath TEXT NOT NULL,CreatedAt TEXT NOT NULL,CreatedBy TEXT NOT NULL);
            CREATE INDEX IF NOT EXISTS IX_SupplierRmaCapaDossierRegistry_CaseDate ON SupplierRmaCapaDossierRegistry(CaseNumber,CreatedAt DESC);
            CREATE TABLE IF NOT EXISTS SupplierRmaCapaDossierRegistryEvents(Id INTEGER PRIMARY KEY AUTOINCREMENT,RegistryId INTEGER NOT NULL,EventType TEXT NOT NULL,OldValue TEXT,NewValue TEXT,Notes TEXT,CreatedAt TEXT NOT NULL,CreatedBy TEXT NOT NULL);
            CREATE INDEX IF NOT EXISTS IX_SupplierRmaCapaDossierRegistryEvents_Record ON SupplierRmaCapaDossierRegistryEvents(RegistryId,CreatedAt DESC);
            CREATE TABLE IF NOT EXISTS SupplierRmaCapaDossierRegistryNotifications(NotificationKey TEXT PRIMARY KEY,RegistryId INTEGER NOT NULL,CreatedAt TEXT NOT NULL);
            """;command.ExecuteNonQuery();
        EnsureColumn(connection,"DocumentStatus","TEXT NOT NULL DEFAULT 'Attivo'");EnsureColumn(connection,"Custodian","TEXT NOT NULL DEFAULT ''");EnsureColumn(connection,"Revision","TEXT NOT NULL DEFAULT '1'");EnsureColumn(connection,"RetentionUntil","TEXT NOT NULL DEFAULT ''");EnsureColumn(connection,"ControlNotes","TEXT NOT NULL DEFAULT ''");EnsureColumn(connection,"UpdatedAt","TEXT NOT NULL DEFAULT ''");EnsureColumn(connection,"UpdatedBy","TEXT NOT NULL DEFAULT ''");EnsureColumn(connection,"ReviewDueDate","TEXT NOT NULL DEFAULT ''");EnsureColumn(connection,"LastReviewDate","TEXT NOT NULL DEFAULT ''");EnsureColumn(connection,"ReviewOutcome","TEXT NOT NULL DEFAULT 'Da eseguire'");EnsureColumn(connection,"ReviewNotes","TEXT NOT NULL DEFAULT ''");EnsureColumn(connection,"ReviewedBy","TEXT NOT NULL DEFAULT ''");EnsureColumn(connection,"ApprovalStatus","TEXT NOT NULL DEFAULT 'Bozza'");EnsureColumn(connection,"Approver","TEXT NOT NULL DEFAULT ''");EnsureColumn(connection,"ApprovalDate","TEXT NOT NULL DEFAULT ''");EnsureColumn(connection,"ApprovalNotes","TEXT NOT NULL DEFAULT ''");
        using var migrate=connection.CreateCommand();migrate.CommandText="UPDATE SupplierRmaCapaDossierRegistry SET RetentionUntil=date(CreatedAt,'+10 years') WHERE RetentionUntil='';UPDATE SupplierRmaCapaDossierRegistry SET ReviewDueDate=date(CreatedAt,'+1 year') WHERE ReviewDueDate='';";migrate.ExecuteNonQuery();
    }

    public void RecordExport(SupplierRmaCorrectiveAction action,string archivePath,int fileCount,string user)=>Insert(action,"Esportazione","Creato",fileCount,0,archivePath,"",user);
    public void RecordVerification(SupplierRmaCorrectiveAction action,SupplierRmaCapaDossierVerificationResult result,string user)=>Insert(action,"Verifica",result.IsValid?"Integro":"Non conforme",result.Items.Count,result.Items.Count(x=>!x.IsValid),result.ArchivePath,result.ReportPath,user);

    public IReadOnlyList<SupplierRmaCapaDossierRegistryRecord> GetAll()
    {
        using var connection=Open();using var command=connection.CreateCommand();command.CommandText="SELECT Id,ActionId,CaseNumber,ActionTitle,Operation,Outcome,FileCount,AnomalyCount,ArchivePath,ReportPath,CreatedAt,CreatedBy,DocumentStatus,Custodian,Revision,RetentionUntil,ControlNotes,UpdatedAt,UpdatedBy,ReviewDueDate,LastReviewDate,ReviewOutcome,ReviewNotes,ReviewedBy,ApprovalStatus,Approver,ApprovalDate,ApprovalNotes FROM SupplierRmaCapaDossierRegistry ORDER BY CreatedAt DESC,Id DESC;";
        using var reader=command.ExecuteReader();var values=new List<SupplierRmaCapaDossierRegistryRecord>();while(reader.Read())values.Add(new(){Id=reader.GetInt32(0),ActionId=reader.GetInt32(1),CaseNumber=S(reader,2),ActionTitle=S(reader,3),Operation=S(reader,4),Outcome=S(reader,5),FileCount=reader.GetInt32(6),AnomalyCount=reader.GetInt32(7),ArchivePath=S(reader,8),ReportPath=S(reader,9),CreatedAt=S(reader,10),CreatedBy=S(reader,11),DocumentStatus=S(reader,12),Custodian=S(reader,13),Revision=S(reader,14),RetentionUntil=S(reader,15),ControlNotes=S(reader,16),UpdatedAt=S(reader,17),UpdatedBy=S(reader,18),ReviewDueDate=S(reader,19),LastReviewDate=S(reader,20),ReviewOutcome=S(reader,21),ReviewNotes=S(reader,22),ReviewedBy=S(reader,23),ApprovalStatus=S(reader,24),Approver=S(reader,25),ApprovalDate=S(reader,26),ApprovalNotes=S(reader,27)});return values;
    }

    public void UpdateControl(int id,string status,string custodian,string revision,string retentionUntil,string notes,string user)
    {
        if(GetAll().FirstOrDefault(x=>x.Id==id)?.IsLocked==true)throw new InvalidOperationException("Il riesame approvato e bloccato. Esegui una riapertura controllata.");
        if(status is not ("Attivo" or "Archiviato" or "Sospeso" or "Da revisionare"))throw new InvalidOperationException("Stato documentale non valido.");if(string.IsNullOrWhiteSpace(custodian))throw new InvalidOperationException("Indica il responsabile della conservazione.");if(string.IsNullOrWhiteSpace(revision))throw new InvalidOperationException("Indica il numero di revisione.");if(!DateTime.TryParse(retentionUntil,out var retention))throw new InvalidOperationException("Inserisci una scadenza di conservazione valida.");
        using var connection=Open();var old=ReadRecord(connection,id);using var command=connection.CreateCommand();command.CommandText="UPDATE SupplierRmaCapaDossierRegistry SET DocumentStatus=$status,Custodian=$custodian,Revision=$revision,RetentionUntil=$retention,ControlNotes=$notes,UpdatedAt=$date,UpdatedBy=$user WHERE Id=$id;";command.Parameters.AddWithValue("$id",id);command.Parameters.AddWithValue("$status",status);command.Parameters.AddWithValue("$custodian",custodian.Trim());command.Parameters.AddWithValue("$revision",revision.Trim());command.Parameters.AddWithValue("$retention",retention.ToString("yyyy-MM-dd"));command.Parameters.AddWithValue("$notes",notes.Trim());command.Parameters.AddWithValue("$date",DateTime.Now.ToString("s"));command.Parameters.AddWithValue("$user",string.IsNullOrWhiteSpace(user)?"Sistema":user);if(command.ExecuteNonQuery()==0)throw new InvalidOperationException("Registrazione non trovata.");AddEvent(connection,id,"Controllo documentale",$"{old.DocumentStatus};{old.Custodian};{old.Revision};{old.RetentionUntil}",$"{status};{custodian.Trim()};{revision.Trim()};{retention:yyyy-MM-dd}",notes,user);
    }

    public IReadOnlyList<SupplierRmaCapaDossierRegistryEvent> GetHistory(int registryId)
    {
        using var connection=Open();using var command=connection.CreateCommand();command.CommandText="SELECT Id,RegistryId,EventType,OldValue,NewValue,Notes,CreatedAt,CreatedBy FROM SupplierRmaCapaDossierRegistryEvents WHERE RegistryId=$id ORDER BY CreatedAt DESC,Id DESC;";command.Parameters.AddWithValue("$id",registryId);using var reader=command.ExecuteReader();var values=new List<SupplierRmaCapaDossierRegistryEvent>();while(reader.Read())values.Add(new(){Id=reader.GetInt32(0),RegistryId=reader.GetInt32(1),EventType=S(reader,2),OldValue=S(reader,3),NewValue=S(reader,4),Notes=S(reader,5),CreatedAt=S(reader,6),CreatedBy=S(reader,7)});return values;
    }

    public void CompleteReview(int id,string outcome,string custodian,string nextReviewDate,string retentionUntil,string notes,bool openRevision,string user)
    {
        if(GetAll().FirstOrDefault(x=>x.Id==id)?.IsLocked==true)throw new InvalidOperationException("Il riesame approvato e bloccato. Esegui una riapertura controllata.");
        if(outcome is not ("Confermato" or "Da aggiornare" or "Non conforme"))throw new InvalidOperationException("Esito del riesame non valido.");if(string.IsNullOrWhiteSpace(custodian))throw new InvalidOperationException("Indica il responsabile della conservazione.");if(!DateTime.TryParse(nextReviewDate,out var nextReview))throw new InvalidOperationException("Inserisci la data del prossimo riesame.");if(!DateTime.TryParse(retentionUntil,out var retention))throw new InvalidOperationException("Inserisci la nuova scadenza di conservazione.");if(string.IsNullOrWhiteSpace(notes))throw new InvalidOperationException("Inserisci le note del riesame.");
        using var connection=Open();var old=ReadRecord(connection,id);var revision=openRevision?NextRevision(old.Revision):old.Revision;var status=outcome=="Confermato"&&!openRevision?"Attivo":"Da revisionare";using var command=connection.CreateCommand();command.CommandText="UPDATE SupplierRmaCapaDossierRegistry SET DocumentStatus=$status,Custodian=$custodian,Revision=$revision,RetentionUntil=$retention,ReviewDueDate=$next,LastReviewDate=$today,ReviewOutcome=$outcome,ReviewNotes=$notes,ReviewedBy=$user,UpdatedAt=$date,UpdatedBy=$user WHERE Id=$id;";command.Parameters.AddWithValue("$id",id);command.Parameters.AddWithValue("$status",status);command.Parameters.AddWithValue("$custodian",custodian.Trim());command.Parameters.AddWithValue("$revision",revision);command.Parameters.AddWithValue("$retention",retention.ToString("yyyy-MM-dd"));command.Parameters.AddWithValue("$next",nextReview.ToString("yyyy-MM-dd"));command.Parameters.AddWithValue("$today",DateTime.Today.ToString("yyyy-MM-dd"));command.Parameters.AddWithValue("$outcome",outcome);command.Parameters.AddWithValue("$notes",notes.Trim());command.Parameters.AddWithValue("$date",DateTime.Now.ToString("s"));command.Parameters.AddWithValue("$user",string.IsNullOrWhiteSpace(user)?"Sistema":user);if(command.ExecuteNonQuery()==0)throw new InvalidOperationException("Registrazione non trovata.");AddEvent(connection,id,"Riesame periodico",old.ReviewOutcome,outcome,$"Responsabile: {custodian}. Revisione: {old.Revision} -> {revision}. Prossimo riesame: {nextReview:dd/MM/yyyy}. Conservazione: {retention:dd/MM/yyyy}. {notes}",user);
    }

    public void SubmitForApproval(int id,string approver,string user){if(string.IsNullOrWhiteSpace(approver))throw new InvalidOperationException("Indica l'approvatore.");using var c=Open();var old=ReadRecord(c,id);if(old.ApprovalStatus=="Approvato")throw new InvalidOperationException("Il riesame e gia approvato.");using var cmd=c.CreateCommand();cmd.CommandText="UPDATE SupplierRmaCapaDossierRegistry SET ApprovalStatus='In approvazione',Approver=$approver,ApprovalDate='',ApprovalNotes='',UpdatedAt=$date,UpdatedBy=$user WHERE Id=$id;";cmd.Parameters.AddWithValue("$id",id);cmd.Parameters.AddWithValue("$approver",approver.Trim());cmd.Parameters.AddWithValue("$date",DateTime.Now.ToString("s"));cmd.Parameters.AddWithValue("$user",user);cmd.ExecuteNonQuery();AddEvent(c,id,"Invio in approvazione",old.ApprovalStatus,"In approvazione",$"Approvatore: {approver}",user);new NotificationService().Publish("Riesame CAPA da approvare",$"{old.CaseNumber}: riesame inviato a {approver}.",NotificationCategory.Asset,NotificationPriority.High,user,"open-rma-corrective-actions",old.ActionId.ToString());}
    public void DecideApproval(int id,bool approved,string notes,string user){if(!approved&&string.IsNullOrWhiteSpace(notes))throw new InvalidOperationException("La motivazione del rifiuto e obbligatoria.");using var c=Open();var old=ReadRecord(c,id);if(old.ApprovalStatus!="In approvazione")throw new InvalidOperationException("Il riesame deve essere in approvazione.");var status=approved?"Approvato":"Rifiutato";using var cmd=c.CreateCommand();cmd.CommandText="UPDATE SupplierRmaCapaDossierRegistry SET ApprovalStatus=$status,ApprovalDate=$date,ApprovalNotes=$notes,Approver=$user,DocumentStatus=CASE WHEN $approved=1 THEN 'Archiviato' ELSE 'Da revisionare' END,UpdatedAt=$date,UpdatedBy=$user WHERE Id=$id;";cmd.Parameters.AddWithValue("$id",id);cmd.Parameters.AddWithValue("$status",status);cmd.Parameters.AddWithValue("$date",DateTime.Now.ToString("s"));cmd.Parameters.AddWithValue("$notes",notes.Trim());cmd.Parameters.AddWithValue("$user",user);cmd.Parameters.AddWithValue("$approved",approved?1:0);cmd.ExecuteNonQuery();AddEvent(c,id,"Decisione approvazione",old.ApprovalStatus,status,notes,user);}
    public void ReopenApproval(int id,string reason,string user){if(string.IsNullOrWhiteSpace(reason))throw new InvalidOperationException("Indica la motivazione della riapertura.");using var c=Open();var old=ReadRecord(c,id);if(old.ApprovalStatus!="Approvato")throw new InvalidOperationException("Solo un riesame approvato puo essere riaperto.");using var cmd=c.CreateCommand();cmd.CommandText="UPDATE SupplierRmaCapaDossierRegistry SET ApprovalStatus='Riaperto',DocumentStatus='Da revisionare',Revision=$revision,ApprovalNotes=$notes,UpdatedAt=$date,UpdatedBy=$user WHERE Id=$id;";cmd.Parameters.AddWithValue("$id",id);cmd.Parameters.AddWithValue("$revision",NextRevision(old.Revision));cmd.Parameters.AddWithValue("$notes",reason.Trim());cmd.Parameters.AddWithValue("$date",DateTime.Now.ToString("s"));cmd.Parameters.AddWithValue("$user",user);cmd.ExecuteNonQuery();AddEvent(c,id,"Riapertura controllata","Approvato","Riaperto",reason,user);}

    public int PublishAlerts(NotificationService? notifications=null)
    {
        notifications??=new NotificationService();var count=0;foreach(var item in GetAll())
        {
            if(item.MissingDocuments)count+=PublishAlert(notifications,item,"missing","Fascicolo CAPA incompleto",$"{item.CaseNumber}: archivio o verbale non disponibile.",NotificationPriority.Critical);
            else if(item.IsExpired)count+=PublishAlert(notifications,item,"expired","Conservazione fascicolo CAPA scaduta",$"{item.CaseNumber}: conservazione scaduta il {FormatDate(item.RetentionUntil)}.",NotificationPriority.High);
            else if(item.IsDueSoon)count+=PublishAlert(notifications,item,"due","Conservazione fascicolo CAPA in scadenza",$"{item.CaseNumber}: scadenza conservazione {FormatDate(item.RetentionUntil)}.",NotificationPriority.High);
            if(item.IsReviewOverdue)count+=PublishAlert(notifications,item,"review-overdue","Riesame fascicolo CAPA scaduto",$"{item.CaseNumber}: riesame previsto il {FormatDate(item.ReviewDueDate)}.",NotificationPriority.Critical);
            else if(item.IsReviewDueSoon)count+=PublishAlert(notifications,item,"review-due","Riesame fascicolo CAPA in scadenza",$"{item.CaseNumber}: prossimo riesame {FormatDate(item.ReviewDueDate)}.",NotificationPriority.High);
        }return count;
    }

    private int PublishAlert(NotificationService notifications,SupplierRmaCapaDossierRegistryRecord item,string type,string title,string message,string priority){var key=$"capa-dossier:{type}:{item.Id}:{DateTime.Today:yyyyMMdd}";using var connection=Open();using var check=connection.CreateCommand();check.CommandText="SELECT COUNT(*) FROM SupplierRmaCapaDossierRegistryNotifications WHERE NotificationKey=$key;";check.Parameters.AddWithValue("$key",key);if(Convert.ToInt32(check.ExecuteScalar())>0)return 0;notifications.Publish(title,message,NotificationCategory.Asset,priority,"Riesame fascicoli CAPA","open-rma-corrective-actions",item.ActionId.ToString());using var insert=connection.CreateCommand();insert.CommandText="INSERT INTO SupplierRmaCapaDossierRegistryNotifications(NotificationKey,RegistryId,CreatedAt) VALUES($key,$id,$date);";insert.Parameters.AddWithValue("$key",key);insert.Parameters.AddWithValue("$id",item.Id);insert.Parameters.AddWithValue("$date",DateTime.Now.ToString("s"));insert.ExecuteNonQuery();return 1;}

    private void Insert(SupplierRmaCorrectiveAction action,string operation,string outcome,int fileCount,int anomalies,string archive,string report,string user)
    {
        using var connection=Open();using var command=connection.CreateCommand();command.CommandText="INSERT INTO SupplierRmaCapaDossierRegistry(ActionId,CaseNumber,ActionTitle,Operation,Outcome,FileCount,AnomalyCount,ArchivePath,ReportPath,CreatedAt,CreatedBy) VALUES($action,$case,$title,$operation,$outcome,$files,$anomalies,$archive,$report,$date,$user);SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$action",action.Id);command.Parameters.AddWithValue("$case",action.CaseNumber);command.Parameters.AddWithValue("$title",action.Title);command.Parameters.AddWithValue("$operation",operation);command.Parameters.AddWithValue("$outcome",outcome);command.Parameters.AddWithValue("$files",fileCount);command.Parameters.AddWithValue("$anomalies",anomalies);command.Parameters.AddWithValue("$archive",archive);command.Parameters.AddWithValue("$report",report);command.Parameters.AddWithValue("$date",DateTime.Now.ToString("s"));command.Parameters.AddWithValue("$user",string.IsNullOrWhiteSpace(user)?"Sistema":user);var id=Convert.ToInt32(command.ExecuteScalar());using var defaults=connection.CreateCommand();defaults.CommandText="UPDATE SupplierRmaCapaDossierRegistry SET Custodian=$user,RetentionUntil=$retention,ReviewDueDate=$review WHERE Id=$id;";defaults.Parameters.AddWithValue("$id",id);defaults.Parameters.AddWithValue("$user",string.IsNullOrWhiteSpace(user)?"Sistema":user);defaults.Parameters.AddWithValue("$retention",DateTime.Today.AddYears(10).ToString("yyyy-MM-dd"));defaults.Parameters.AddWithValue("$review",DateTime.Today.AddYears(1).ToString("yyyy-MM-dd"));defaults.ExecuteNonQuery();AddEvent(connection,id,"Registrazione fascicolo","",outcome,$"{operation}: {Path.GetFileName(archive)}",user);
    }
    private SqliteConnection Open(){var connection=new SqliteConnection(_connectionString);connection.Open();return connection;}
    private static SupplierRmaCapaDossierRegistryRecord ReadRecord(SqliteConnection connection,int id){using var command=connection.CreateCommand();command.CommandText="SELECT DocumentStatus,Custodian,Revision,RetentionUntil,ReviewOutcome,ReviewDueDate,ApprovalStatus,ActionId,CaseNumber FROM SupplierRmaCapaDossierRegistry WHERE Id=$id;";command.Parameters.AddWithValue("$id",id);using var reader=command.ExecuteReader();if(!reader.Read())throw new InvalidOperationException("Registrazione non trovata.");return new(){DocumentStatus=S(reader,0),Custodian=S(reader,1),Revision=S(reader,2),RetentionUntil=S(reader,3),ReviewOutcome=S(reader,4),ReviewDueDate=S(reader,5),ApprovalStatus=S(reader,6),ActionId=reader.GetInt32(7),CaseNumber=S(reader,8)};}
    private static void AddEvent(SqliteConnection connection,int id,string type,string oldValue,string newValue,string notes,string user){using var command=connection.CreateCommand();command.CommandText="INSERT INTO SupplierRmaCapaDossierRegistryEvents(RegistryId,EventType,OldValue,NewValue,Notes,CreatedAt,CreatedBy) VALUES($id,$type,$old,$new,$notes,$date,$user);";command.Parameters.AddWithValue("$id",id);command.Parameters.AddWithValue("$type",type);command.Parameters.AddWithValue("$old",oldValue);command.Parameters.AddWithValue("$new",newValue);command.Parameters.AddWithValue("$notes",notes??"");command.Parameters.AddWithValue("$date",DateTime.Now.ToString("s"));command.Parameters.AddWithValue("$user",string.IsNullOrWhiteSpace(user)?"Sistema":user);command.ExecuteNonQuery();}
    private static void EnsureColumn(SqliteConnection connection,string name,string definition){using var check=connection.CreateCommand();check.CommandText="PRAGMA table_info(SupplierRmaCapaDossierRegistry);";using var reader=check.ExecuteReader();var exists=false;while(reader.Read())if(string.Equals(reader.GetString(1),name,StringComparison.OrdinalIgnoreCase)){exists=true;break;}reader.Close();if(exists)return;using var alter=connection.CreateCommand();alter.CommandText=$"ALTER TABLE SupplierRmaCapaDossierRegistry ADD COLUMN {name} {definition};";alter.ExecuteNonQuery();}
    private static string FormatDate(string value)=>DateTime.TryParse(value,out var date)?date.ToString("dd/MM/yyyy"):value;
    private static string NextRevision(string value)=>int.TryParse(value,out var number)?(number+1).ToString():$"{value}.1";
    private static string S(SqliteDataReader reader,int index)=>reader.IsDBNull(index)?"":reader.GetString(index);
}
