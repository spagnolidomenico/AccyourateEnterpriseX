using System.Security.Cryptography;
using System.Text;
using Accyourate.App.Platform.Pdf;
using Accyourate.App.Platform.Settings;
using Microsoft.Data.Sqlite;
using Accyourate.App.Platform.Notifications;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SupplierRmaCapaAttestationExportRecord
{
    public int Id { get; init; }
    public string Format { get; init; } = "";
    public string FilterDescription { get; init; } = "";
    public int RecordCount { get; init; }
    public int ValidCount { get; init; }
    public int InvalidCount { get; init; }
    public int MissingCount { get; init; }
    public string FilePath { get; init; } = "";
    public string FileHash { get; init; } = "";
    public string ExportedBy { get; init; } = "";
    public string ExportedAt { get; init; } = "";
    public string RetainUntil { get; init; } = "";
    public string ArchivedAt { get; init; } = "";
    public string ArchiveCopyPath { get; init; } = "";
    public bool FileAvailable => File.Exists(FilePath);
    public bool IsValid => FileAvailable && string.Equals(CurrentHash(), FileHash, StringComparison.OrdinalIgnoreCase);
    public string IntegrityStatus => !FileAvailable ? "File mancante" : IsValid ? "Integro" : "Modificato";
    public int DaysRemaining => DateTime.TryParse(RetainUntil, out var date) ? (date.Date - DateTime.Today).Days : 0;
    public string RetentionStatus => !string.IsNullOrWhiteSpace(ArchivedAt) ? "Archiviata" : DaysRemaining < 0 ? "Scaduta" : DaysRemaining <= 30 ? "In scadenza" : "In conservazione";
    private string CurrentHash() => FileAvailable ? Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(FilePath))) : "";
}

public sealed class SupplierRmaCapaAttestationRetentionAuditRecord
{
    public int Id { get; init; }
    public int ExportId { get; init; }
    public string Format { get; init; } = "";
    public string Action { get; init; } = "";
    public string Detail { get; init; } = "";
    public string PerformedBy { get; init; } = "";
    public string PerformedAt { get; init; } = "";
}

public sealed class SupplierRmaCapaAttestationExportService
{
    private readonly string _connectionString;
    private readonly SettingsService _settings = new();

    public SupplierRmaCapaAttestationExportService(string? databasePath = null)
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AccyourateEnterpriseX");
        Directory.CreateDirectory(folder);
        _connectionString = $"Data Source={databasePath ?? Path.Combine(folder, "accyourate-assets.db")}";
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS SupplierRmaCapaAttestationExports(Id INTEGER PRIMARY KEY AUTOINCREMENT,Format TEXT NOT NULL,FilterDescription TEXT NOT NULL,RecordCount INTEGER NOT NULL,ValidCount INTEGER NOT NULL,InvalidCount INTEGER NOT NULL,MissingCount INTEGER NOT NULL,FilePath TEXT NOT NULL,FileHash TEXT NOT NULL,ExportedBy TEXT NOT NULL,ExportedAt TEXT NOT NULL);";
        command.ExecuteNonQuery();
        EnsureColumn(connection, "SupplierRmaCapaAttestationExports", "RetainUntil", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "SupplierRmaCapaAttestationExports", "ArchivedAt", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "SupplierRmaCapaAttestationExports", "ArchiveCopyPath", "TEXT NOT NULL DEFAULT ''");
        using var setup = connection.CreateCommand(); setup.CommandText = "CREATE TABLE IF NOT EXISTS SupplierRmaCapaAttestationRetentionSettings(Id INTEGER PRIMARY KEY CHECK(Id=1),RetentionDays INTEGER NOT NULL);INSERT OR IGNORE INTO SupplierRmaCapaAttestationRetentionSettings(Id,RetentionDays) VALUES(1,365);CREATE TABLE IF NOT EXISTS SupplierRmaCapaAttestationRetentionAudit(Id INTEGER PRIMARY KEY AUTOINCREMENT,ExportId INTEGER NOT NULL,Action TEXT NOT NULL,Detail TEXT NOT NULL,PerformedBy TEXT NOT NULL,PerformedAt TEXT NOT NULL);CREATE TABLE IF NOT EXISTS SupplierRmaCapaAttestationRetentionNotifications(NotificationKey TEXT PRIMARY KEY,ExportId INTEGER NOT NULL,CreatedAt TEXT NOT NULL);"; setup.ExecuteNonQuery();
        using var migrate = connection.CreateCommand(); migrate.CommandText = "UPDATE SupplierRmaCapaAttestationExports SET RetainUntil=date(ExportedAt, '+' || (SELECT RetentionDays FROM SupplierRmaCapaAttestationRetentionSettings WHERE Id=1) || ' days') WHERE RetainUntil='';"; migrate.ExecuteNonQuery();
    }

    public string ExportCsv(IReadOnlyList<SupplierRmaCapaAttestation> rows, string filters)
    {
        var path = Path.Combine(ExportFolder(), $"Registro-attestazioni-CAPA-{DateTime.Now:yyyyMMdd-HHmmss}.csv");
        var text = new StringBuilder("Pratica;Revisione;Approvatore;Ruolo;Data attestazione;Stato;SHA-256;Archivio;Verbale\r\n");
        foreach (var row in rows)
            text.AppendLine(string.Join(";", new[] { Csv(row.CaseNumber), Csv(row.Revision), Csv(row.Approver), Csv(row.Role), Csv(Date(row.AttestedAt)), Csv(row.ValidationStatus), Csv(row.ArchiveHash), Csv(row.ArchivePath), Csv(row.ReportPath) }));
        File.WriteAllText(path, text.ToString(), new UTF8Encoding(true));
        Register("CSV", rows, filters, path);
        return path;
    }

    public string ExportPdf(IReadOnlyList<SupplierRmaCapaAttestation> rows, string filters)
    {
        var settings = _settings.Load();
        var template = settings.DocumentTemplate ?? new DocumentTemplateSettings();
        var document = new SimplePdfDocument { Title = "Registro attestazioni CAPA" };
        ApplyBranding(document, settings, template, $"CAPA-ATT-{DateTime.Now:yyyyMMdd-HHmm}");
        var valid = rows.Count(x => x.IsValid);
        var missing = rows.Count(x => !x.ArchiveAvailable);
        var invalid = rows.Count - valid - missing;
        document.AddTitle("Registro attestazioni fascicoli CAPA");
        document.AddKeyValue("Data elaborazione", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        document.AddKeyValue("Operatore", Environment.UserName);
        document.AddKeyValue("Filtri applicati", filters);
        document.AddHeading("Riepilogo esiti");
        document.AddKeyValue("Attestazioni esportate", rows.Count.ToString());
        document.AddKeyValue("Valide", valid.ToString());
        document.AddKeyValue("Non valide", invalid.ToString());
        document.AddKeyValue("Archivio mancante", missing.ToString());
        document.AddStatus("Esito del registro", invalid == 0 && missing == 0 ? "Conforme" : "Richiede attenzione");
        document.AddHeading("Dettaglio attestazioni");
        if (rows.Count == 0) document.AddText("Nessuna attestazione corrisponde ai filtri selezionati.");
        foreach (var row in rows)
        {
            document.AddText($"{row.CaseNumber} - revisione {Dash(row.Revision)}", 11);
            document.AddText($"Stato: {row.ValidationStatus} | Approvatore: {Dash(row.Approver)} | Ruolo: {Dash(row.Role)}", 9);
            document.AddText($"Data attestazione: {Date(row.AttestedAt)}", 9);
            document.AddText($"SHA-256: {row.ArchiveHash}", 8);
        }
        document.AddHeading("Tracciabilita del rapporto");
        document.AddText("Il rapporto rappresenta lo stato del registro al momento dell'esportazione e rispetta i filtri indicati nei metadati.", 9);
        document.AddSignaturePair("Responsabile qualita", "Responsabile processo");
        var path = new PdfExportService().Export(document, ExportFolder(), $"Registro-attestazioni-CAPA-{DateTime.Now:yyyyMMdd-HHmmss}");
        Register("PDF", rows, filters, path);
        return path;
    }

    public IReadOnlyList<SupplierRmaCapaAttestationExportRecord> GetExports()
    {
        using var connection = Open(); using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id,Format,FilterDescription,RecordCount,ValidCount,InvalidCount,MissingCount,FilePath,FileHash,ExportedBy,ExportedAt,COALESCE(RetainUntil,''),COALESCE(ArchivedAt,''),COALESCE(ArchiveCopyPath,'') FROM SupplierRmaCapaAttestationExports ORDER BY Id DESC;";
        using var reader = command.ExecuteReader(); var rows = new List<SupplierRmaCapaAttestationExportRecord>();
        while (reader.Read()) rows.Add(new SupplierRmaCapaAttestationExportRecord { Id=reader.GetInt32(0), Format=reader.GetString(1), FilterDescription=reader.GetString(2), RecordCount=reader.GetInt32(3), ValidCount=reader.GetInt32(4), InvalidCount=reader.GetInt32(5), MissingCount=reader.GetInt32(6), FilePath=reader.GetString(7), FileHash=reader.GetString(8), ExportedBy=reader.GetString(9), ExportedAt=reader.GetString(10), RetainUntil=reader.GetString(11), ArchivedAt=reader.GetString(12), ArchiveCopyPath=reader.GetString(13) });
        return rows;
    }

    public int GetRetentionDays(){using var c=Open();using var q=c.CreateCommand();q.CommandText="SELECT RetentionDays FROM SupplierRmaCapaAttestationRetentionSettings WHERE Id=1;";return Convert.ToInt32(q.ExecuteScalar());}
    public void SetRetentionDays(int days){if(days<1||days>36500)throw new InvalidOperationException("Indica un periodo tra 1 e 36500 giorni.");using var c=Open();using var q=c.CreateCommand();q.CommandText="UPDATE SupplierRmaCapaAttestationRetentionSettings SET RetentionDays=$d WHERE Id=1;";q.Parameters.AddWithValue("$d",days);q.ExecuteNonQuery();Audit(c,0,"Configurazione conservazione",$"Periodo impostato a {days} giorni");}
    public void Extend(int id,int days=365){using var c=Open();using var q=c.CreateCommand();q.CommandText="UPDATE SupplierRmaCapaAttestationExports SET RetainUntil=date(CASE WHEN date(RetainUntil)>date('now') THEN RetainUntil ELSE date('now') END,'+'||$d||' days') WHERE Id=$id;";q.Parameters.AddWithValue("$d",days);q.Parameters.AddWithValue("$id",id);q.ExecuteNonQuery();Audit(c,id,"Proroga conservazione",$"Proroga di {days} giorni");}
    public string Archive(int id)
    {
        var item=GetExports().FirstOrDefault(x=>x.Id==id)??throw new InvalidOperationException("Esportazione non trovata.");if(!item.FileAvailable)throw new InvalidOperationException("Il file originale non e disponibile.");
        var folder=Path.Combine(ExportFolder(),"Conservazione");Directory.CreateDirectory(folder);var target=Path.Combine(folder,$"{item.Id}-{Path.GetFileName(item.FilePath)}");File.Copy(item.FilePath,target,true);var hash=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(target)));if(!string.Equals(hash,item.FileHash,StringComparison.OrdinalIgnoreCase)){File.Delete(target);throw new InvalidOperationException("La copia non supera la verifica di integrita.");}
        using var c=Open();using var q=c.CreateCommand();q.CommandText="UPDATE SupplierRmaCapaAttestationExports SET ArchivedAt=$a,ArchiveCopyPath=$p WHERE Id=$id;";q.Parameters.AddWithValue("$a",DateTime.Now.ToString("s"));q.Parameters.AddWithValue("$p",target);q.Parameters.AddWithValue("$id",id);q.ExecuteNonQuery();Audit(c,id,"Archiviazione controllata",target);return target;
    }
    public int PublishRetentionNotifications(NotificationService? notifications=null)
    {
        notifications??=new NotificationService();var count=0;foreach(var x in GetExports().Where(x=>string.IsNullOrWhiteSpace(x.ArchivedAt)&&x.DaysRemaining<=30)){var key=$"capa-export-retention:{x.Id}:{DateTime.Today:yyyyMMdd}";using var c=Open();using var check=c.CreateCommand();check.CommandText="SELECT COUNT(*) FROM SupplierRmaCapaAttestationRetentionNotifications WHERE NotificationKey=$k;";check.Parameters.AddWithValue("$k",key);if(Convert.ToInt32(check.ExecuteScalar())>0)continue;var title=x.DaysRemaining<0?"Conservazione esportazione CAPA scaduta":"Conservazione esportazione CAPA in scadenza";notifications.Publish(title,$"Esportazione {x.Format} del {Date(x.ExportedAt)}: {x.RetentionStatus}.",NotificationCategory.Asset,x.DaysRemaining<0?NotificationPriority.Critical:NotificationPriority.High,"Conservazione attestazioni CAPA","open-rma-corrective-actions",x.Id.ToString());using var add=c.CreateCommand();add.CommandText="INSERT INTO SupplierRmaCapaAttestationRetentionNotifications(NotificationKey,ExportId,CreatedAt) VALUES($k,$id,$d);";add.Parameters.AddWithValue("$k",key);add.Parameters.AddWithValue("$id",x.Id);add.Parameters.AddWithValue("$d",DateTime.Now.ToString("s"));add.ExecuteNonQuery();count++;}return count;
    }
    public IReadOnlyList<SupplierRmaCapaAttestationRetentionAuditRecord> GetRetentionAudit()
    {
        using var c=Open();using var q=c.CreateCommand();q.CommandText="SELECT a.Id,a.ExportId,COALESCE(e.Format,''),a.Action,a.Detail,a.PerformedBy,a.PerformedAt FROM SupplierRmaCapaAttestationRetentionAudit a LEFT JOIN SupplierRmaCapaAttestationExports e ON e.Id=a.ExportId ORDER BY a.Id DESC;";using var r=q.ExecuteReader();var values=new List<SupplierRmaCapaAttestationRetentionAuditRecord>();while(r.Read())values.Add(new(){Id=r.GetInt32(0),ExportId=r.GetInt32(1),Format=r.GetString(2),Action=r.GetString(3),Detail=r.GetString(4),PerformedBy=r.GetString(5),PerformedAt=r.GetString(6)});return values;
    }
    public string ExportRetentionAuditPdf(IReadOnlyList<SupplierRmaCapaAttestationRetentionAuditRecord> rows,string filters)
    {
        var settings=_settings.Load();var template=settings.DocumentTemplate??new DocumentTemplateSettings();var document=new SimplePdfDocument{Title="Audit conservazione esportazioni CAPA"};ApplyBranding(document,settings,template,$"CAPA-AUD-{DateTime.Now:yyyyMMdd-HHmm}");document.Branding.DocumentLabel="AUDIT CONSERVAZIONE CAPA";document.AddTitle("Registro audit conservazione esportazioni CAPA");document.AddKeyValue("Data elaborazione",DateTime.Now.ToString("dd/MM/yyyy HH:mm"));document.AddKeyValue("Operatore",Environment.UserName);document.AddKeyValue("Filtri applicati",filters);document.AddKeyValue("Operazioni esportate",rows.Count.ToString());document.AddHeading("Cronologia operazioni");if(rows.Count==0)document.AddText("Nessuna operazione corrisponde ai filtri selezionati.");foreach(var row in rows){document.AddText($"{Date(row.PerformedAt)} - {row.Action}",11);document.AddText($"Esportazione: {(row.ExportId==0?"Configurazione generale":$"#{row.ExportId} {row.Format}")} | Operatore: {Dash(row.PerformedBy)}",9);document.AddText(row.Detail,9);}document.AddSignaturePair("Responsabile qualita","Responsabile processo");return new PdfExportService().Export(document,ExportFolder(),$"Audit-conservazione-CAPA-{DateTime.Now:yyyyMMdd-HHmmss}");
    }

    private void Register(string format, IReadOnlyList<SupplierRmaCapaAttestation> rows, string filters, string path)
    {
        var valid = rows.Count(x => x.IsValid); var missing = rows.Count(x => !x.ArchiveAvailable); var invalid = rows.Count - valid - missing;
        var hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
        File.WriteAllText(path + ".sha256", hash, Encoding.ASCII);
        using var connection = Open(); using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO SupplierRmaCapaAttestationExports(Format,FilterDescription,RecordCount,ValidCount,InvalidCount,MissingCount,FilePath,FileHash,ExportedBy,ExportedAt,RetainUntil) VALUES($f,$d,$n,$v,$i,$m,$p,$h,$u,$a,date($a,'+'||(SELECT RetentionDays FROM SupplierRmaCapaAttestationRetentionSettings WHERE Id=1)||' days'));";
        command.Parameters.AddWithValue("$f", format); command.Parameters.AddWithValue("$d", filters); command.Parameters.AddWithValue("$n", rows.Count); command.Parameters.AddWithValue("$v", valid); command.Parameters.AddWithValue("$i", invalid); command.Parameters.AddWithValue("$m", missing); command.Parameters.AddWithValue("$p", path); command.Parameters.AddWithValue("$h", hash); command.Parameters.AddWithValue("$u", Environment.UserName); command.Parameters.AddWithValue("$a", DateTime.Now.ToString("s")); command.ExecuteNonQuery();
    }

    private static void ApplyBranding(SimplePdfDocument d, ApplicationSettings s, DocumentTemplateSettings t, string code)
    {
        d.Branding.CompanyName = string.IsNullOrWhiteSpace(s.Company.LegalName) ? s.Company.CompanyName : s.Company.LegalName;
        d.Branding.CompanyDetailLines.AddRange(new[] { s.Company.Address, string.Join(" ", new[] { s.Company.City, s.Company.Province }.Where(x => !string.IsNullOrWhiteSpace(x))), string.Join(" - ", new[] { s.Company.Phone, s.Company.Email }.Where(x => !string.IsNullOrWhiteSpace(x))), string.Join(" - ", new[] { s.Company.VatNumber, s.Company.FiscalCode }.Where(x => !string.IsNullOrWhiteSpace(x))) }.Where(x => !string.IsNullOrWhiteSpace(x)));
        d.Branding.HeaderLayout=t.HeaderLayout; d.Branding.LogoPath=s.Company.LogoPath; d.Branding.LogoSize=t.LogoSize; d.Branding.LogoPosition=t.LogoPosition; d.Branding.PrimaryColor=t.PrimaryColor; d.Branding.DocumentLabel="REGISTRO ATTESTAZIONI CAPA"; d.Branding.DocumentCode=code; d.Branding.DocumentVersion=t.DocumentVersion; d.Branding.FooterText=t.FooterText; d.Branding.ConfidentialityText=t.ConfidentialityText; d.Branding.ShowLogo=t.ShowLogo; d.Branding.ShowCompanyDetails=t.ShowCompanyDetails; d.Branding.ShowDocumentMetadata=t.ShowDocumentMetadata; d.Branding.ShowFooter=t.ShowFooter; d.Branding.ShowPageNumber=t.ShowPageNumber; d.Branding.ShowPrintTimestamp=t.ShowPrintTimestamp;
    }

    private static string ExportFolder(){var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Accyourate Enterprise X","Registro attestazioni CAPA");Directory.CreateDirectory(folder);return folder;}
    private static string Csv(string value)=>$"\"{(value??string.Empty).Replace("\"","\"\"")}\"";
    private static string Date(string value)=>DateTime.TryParse(value,out var date)?date.ToString("dd/MM/yyyy HH:mm"):value;
    private static string Dash(string value)=>string.IsNullOrWhiteSpace(value)?"Non specificato":value;
    private SqliteConnection Open(){var connection=new SqliteConnection(_connectionString);connection.Open();return connection;}
    private static void EnsureColumn(SqliteConnection c,string table,string column,string definition){using var check=c.CreateCommand();check.CommandText=$"PRAGMA table_info({table});";using var r=check.ExecuteReader();while(r.Read())if(string.Equals(r.GetString(1),column,StringComparison.OrdinalIgnoreCase))return;r.Close();using var alter=c.CreateCommand();alter.CommandText=$"ALTER TABLE {table} ADD COLUMN {column} {definition};";alter.ExecuteNonQuery();}
    private static void Audit(SqliteConnection c,int id,string action,string detail){using var q=c.CreateCommand();q.CommandText="INSERT INTO SupplierRmaCapaAttestationRetentionAudit(ExportId,Action,Detail,PerformedBy,PerformedAt) VALUES($id,$a,$d,$u,$t);";q.Parameters.AddWithValue("$id",id);q.Parameters.AddWithValue("$a",action);q.Parameters.AddWithValue("$d",detail);q.Parameters.AddWithValue("$u",Environment.UserName);q.Parameters.AddWithValue("$t",DateTime.Now.ToString("s"));q.ExecuteNonQuery();}
}
