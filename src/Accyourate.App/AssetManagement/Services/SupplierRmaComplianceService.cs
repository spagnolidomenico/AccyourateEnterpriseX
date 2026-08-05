using Microsoft.Data.Sqlite;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SupplierRmaComplianceItem
{
    public int Id{get;set;} public string Code{get;set;}=""; public string Label{get;set;}=""; public bool IsRequired{get;set;}=true; public bool IsActive{get;set;}=true;
}
public sealed class SupplierRmaComplianceCheck
{
    public int ItemId{get;set;} public string Code{get;set;}=""; public string Label{get;set;}=""; public bool IsRequired{get;set;} public bool IsCompliant{get;set;} public string Notes{get;set;}="";
}
public sealed class SupplierRmaComplianceAudit
{
    public int Id{get;set;} public int RmaId{get;set;} public string CaseNumber{get;set;}=""; public string Responsible{get;set;}=""; public string VerifiedAt{get;set;}="";
    public string Status{get;set;}=""; public string Findings{get;set;}=""; public string CorrectiveActions{get;set;}=""; public string CreatedAt{get;set;}=""; public string CreatedBy{get;set;}="";
    public IReadOnlyList<SupplierRmaComplianceCheck> Checks{get;set;}=Array.Empty<SupplierRmaComplianceCheck>();
}

public sealed class SupplierRmaComplianceService
{
    private readonly string _connectionString;
    public SupplierRmaComplianceService(string? databasePath=null){var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"AccyourateEnterpriseX");Directory.CreateDirectory(folder);_connectionString=$"Data Source={databasePath??Path.Combine(folder,"accyourate-assets.db")}";using var c=Open();using var cmd=c.CreateCommand();cmd.CommandText="""
        CREATE TABLE IF NOT EXISTS SupplierRmaComplianceItems(Id INTEGER PRIMARY KEY AUTOINCREMENT,Code TEXT NOT NULL UNIQUE,Label TEXT NOT NULL,IsRequired INTEGER NOT NULL DEFAULT 1,IsActive INTEGER NOT NULL DEFAULT 1,UpdatedAt TEXT NOT NULL);
        CREATE TABLE IF NOT EXISTS SupplierRmaComplianceAudits(Id INTEGER PRIMARY KEY AUTOINCREMENT,RmaId INTEGER NOT NULL,CaseNumber TEXT NOT NULL,Responsible TEXT NOT NULL,VerifiedAt TEXT NOT NULL,Status TEXT NOT NULL,Findings TEXT,CorrectiveActions TEXT,CreatedAt TEXT NOT NULL,CreatedBy TEXT NOT NULL);
        CREATE TABLE IF NOT EXISTS SupplierRmaComplianceChecks(Id INTEGER PRIMARY KEY AUTOINCREMENT,AuditId INTEGER NOT NULL,ItemId INTEGER NOT NULL,Code TEXT NOT NULL,Label TEXT NOT NULL,IsRequired INTEGER NOT NULL,IsCompliant INTEGER NOT NULL,Notes TEXT);
        CREATE INDEX IF NOT EXISTS IX_SupplierRmaComplianceAudits_Rma ON SupplierRmaComplianceAudits(RmaId,VerifiedAt);
        """;cmd.ExecuteNonQuery();Seed(c);}
    public IReadOnlyList<SupplierRmaComplianceItem> GetItems(bool activeOnly=true){using var c=Open();using var cmd=c.CreateCommand();cmd.CommandText="SELECT Id,Code,Label,IsRequired,IsActive FROM SupplierRmaComplianceItems"+(activeOnly?" WHERE IsActive=1":"")+" ORDER BY Id;";using var r=cmd.ExecuteReader();var list=new List<SupplierRmaComplianceItem>();while(r.Read())list.Add(new(){Id=r.GetInt32(0),Code=S(r,1),Label=S(r,2),IsRequired=r.GetInt32(3)==1,IsActive=r.GetInt32(4)==1});return list;}
    public int AddItem(string label,bool required){if(string.IsNullOrWhiteSpace(label))throw new InvalidOperationException("Inserisci la descrizione del controllo.");using var c=Open();using var cmd=c.CreateCommand();var code=$"CUS-{DateTime.Now:yyyyMMddHHmmssfff}";cmd.CommandText="INSERT INTO SupplierRmaComplianceItems(Code,Label,IsRequired,IsActive,UpdatedAt) VALUES($code,$label,$required,1,$date);SELECT last_insert_rowid();";cmd.Parameters.AddWithValue("$code",code);cmd.Parameters.AddWithValue("$label",label.Trim());cmd.Parameters.AddWithValue("$required",required?1:0);cmd.Parameters.AddWithValue("$date",DateTime.Now.ToString("s"));return Convert.ToInt32(cmd.ExecuteScalar());}
    public void ToggleItem(int id,bool active){using var c=Open();using var cmd=c.CreateCommand();cmd.CommandText="UPDATE SupplierRmaComplianceItems SET IsActive=$active,UpdatedAt=$date WHERE Id=$id;";cmd.Parameters.AddWithValue("$id",id);cmd.Parameters.AddWithValue("$active",active?1:0);cmd.Parameters.AddWithValue("$date",DateTime.Now.ToString("s"));cmd.ExecuteNonQuery();}
    public int Save(SupplierRmaComplianceAudit audit){if(audit.RmaId<=0||string.IsNullOrWhiteSpace(audit.Responsible))throw new InvalidOperationException("Seleziona la pratica e inserisci il responsabile.");audit.Status=audit.Checks.Where(x=>x.IsRequired).All(x=>x.IsCompliant)?"Conforme":"Non conforme";var now=DateTime.Now.ToString("s");using var c=Open();using var t=c.BeginTransaction();int id;using(var cmd=c.CreateCommand()){cmd.Transaction=t;cmd.CommandText="INSERT INTO SupplierRmaComplianceAudits(RmaId,CaseNumber,Responsible,VerifiedAt,Status,Findings,CorrectiveActions,CreatedAt,CreatedBy) VALUES($rma,$case,$responsible,$verified,$status,$findings,$actions,$created,$user);SELECT last_insert_rowid();";cmd.Parameters.AddWithValue("$rma",audit.RmaId);cmd.Parameters.AddWithValue("$case",audit.CaseNumber);cmd.Parameters.AddWithValue("$responsible",audit.Responsible.Trim());cmd.Parameters.AddWithValue("$verified",audit.VerifiedAt);cmd.Parameters.AddWithValue("$status",audit.Status);cmd.Parameters.AddWithValue("$findings",audit.Findings.Trim());cmd.Parameters.AddWithValue("$actions",audit.CorrectiveActions.Trim());cmd.Parameters.AddWithValue("$created",now);cmd.Parameters.AddWithValue("$user",audit.CreatedBy);id=Convert.ToInt32(cmd.ExecuteScalar());}foreach(var x in audit.Checks){using var cmd=c.CreateCommand();cmd.Transaction=t;cmd.CommandText="INSERT INTO SupplierRmaComplianceChecks(AuditId,ItemId,Code,Label,IsRequired,IsCompliant,Notes) VALUES($audit,$item,$code,$label,$required,$compliant,$notes);";cmd.Parameters.AddWithValue("$audit",id);cmd.Parameters.AddWithValue("$item",x.ItemId);cmd.Parameters.AddWithValue("$code",x.Code);cmd.Parameters.AddWithValue("$label",x.Label);cmd.Parameters.AddWithValue("$required",x.IsRequired?1:0);cmd.Parameters.AddWithValue("$compliant",x.IsCompliant?1:0);cmd.Parameters.AddWithValue("$notes",x.Notes);cmd.ExecuteNonQuery();}t.Commit();return id;}
    public IReadOnlyList<SupplierRmaComplianceAudit> GetAudits(){using var c=Open();using var cmd=c.CreateCommand();cmd.CommandText="SELECT Id,RmaId,CaseNumber,Responsible,VerifiedAt,Status,Findings,CorrectiveActions,CreatedAt,CreatedBy FROM SupplierRmaComplianceAudits ORDER BY VerifiedAt DESC,Id DESC;";using var r=cmd.ExecuteReader();var list=new List<SupplierRmaComplianceAudit>();while(r.Read())list.Add(new(){Id=r.GetInt32(0),RmaId=r.GetInt32(1),CaseNumber=S(r,2),Responsible=S(r,3),VerifiedAt=S(r,4),Status=S(r,5),Findings=S(r,6),CorrectiveActions=S(r,7),CreatedAt=S(r,8),CreatedBy=S(r,9)});r.Close();foreach(var audit in list)audit.Checks=GetChecks(c,audit.Id);return list;}
    private static IReadOnlyList<SupplierRmaComplianceCheck> GetChecks(SqliteConnection c,int auditId){using var cmd=c.CreateCommand();cmd.CommandText="SELECT ItemId,Code,Label,IsRequired,IsCompliant,Notes FROM SupplierRmaComplianceChecks WHERE AuditId=$audit ORDER BY Id;";cmd.Parameters.AddWithValue("$audit",auditId);using var r=cmd.ExecuteReader();var list=new List<SupplierRmaComplianceCheck>();while(r.Read())list.Add(new(){ItemId=r.GetInt32(0),Code=S(r,1),Label=S(r,2),IsRequired=r.GetInt32(3)==1,IsCompliant=r.GetInt32(4)==1,Notes=S(r,5)});return list;}
    private static void Seed(SqliteConnection c){using var cmd=c.CreateCommand();cmd.CommandText="""
        INSERT OR IGNORE INTO SupplierRmaComplianceItems(Code,Label,IsRequired,IsActive,UpdatedAt) VALUES
        ('RMA-AUTH','Autorizzazione RMA presente e valida',1,1,$date),('RMA-TRACK','Tracking e prova di spedizione disponibili',1,1,$date),
        ('RMA-COMM','Comunicazioni con il fornitore archiviate',1,1,$date),('RMA-EVID','Allegati ed evidenze disponibili',1,1,$date),
        ('RMA-FOLLOW','Solleciti conclusi o motivati',1,1,$date),('RMA-OUTCOME','Esito e costi della pratica registrati',1,1,$date),
        ('RMA-DOSSIER','Fascicolo definitivo generato',1,1,$date);
        """;cmd.Parameters.AddWithValue("$date",DateTime.Now.ToString("s"));cmd.ExecuteNonQuery();}
    private SqliteConnection Open(){var c=new SqliteConnection(_connectionString);c.Open();return c;}private static string S(SqliteDataReader r,int i)=>r.IsDBNull(i)?"":r.GetString(i);
}
