using Microsoft.Data.Sqlite;

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
    public bool ArchiveAvailable => File.Exists(ArchivePath);
    public bool ReportAvailable => File.Exists(ReportPath);
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
            """;command.ExecuteNonQuery();
    }

    public void RecordExport(SupplierRmaCorrectiveAction action,string archivePath,int fileCount,string user)=>Insert(action,"Esportazione","Creato",fileCount,0,archivePath,"",user);
    public void RecordVerification(SupplierRmaCorrectiveAction action,SupplierRmaCapaDossierVerificationResult result,string user)=>Insert(action,"Verifica",result.IsValid?"Integro":"Non conforme",result.Items.Count,result.Items.Count(x=>!x.IsValid),result.ArchivePath,result.ReportPath,user);

    public IReadOnlyList<SupplierRmaCapaDossierRegistryRecord> GetAll()
    {
        using var connection=Open();using var command=connection.CreateCommand();command.CommandText="SELECT Id,ActionId,CaseNumber,ActionTitle,Operation,Outcome,FileCount,AnomalyCount,ArchivePath,ReportPath,CreatedAt,CreatedBy FROM SupplierRmaCapaDossierRegistry ORDER BY CreatedAt DESC,Id DESC;";
        using var reader=command.ExecuteReader();var values=new List<SupplierRmaCapaDossierRegistryRecord>();while(reader.Read())values.Add(new(){Id=reader.GetInt32(0),ActionId=reader.GetInt32(1),CaseNumber=S(reader,2),ActionTitle=S(reader,3),Operation=S(reader,4),Outcome=S(reader,5),FileCount=reader.GetInt32(6),AnomalyCount=reader.GetInt32(7),ArchivePath=S(reader,8),ReportPath=S(reader,9),CreatedAt=S(reader,10),CreatedBy=S(reader,11)});return values;
    }

    private void Insert(SupplierRmaCorrectiveAction action,string operation,string outcome,int fileCount,int anomalies,string archive,string report,string user)
    {
        using var connection=Open();using var command=connection.CreateCommand();command.CommandText="INSERT INTO SupplierRmaCapaDossierRegistry(ActionId,CaseNumber,ActionTitle,Operation,Outcome,FileCount,AnomalyCount,ArchivePath,ReportPath,CreatedAt,CreatedBy) VALUES($action,$case,$title,$operation,$outcome,$files,$anomalies,$archive,$report,$date,$user);";
        command.Parameters.AddWithValue("$action",action.Id);command.Parameters.AddWithValue("$case",action.CaseNumber);command.Parameters.AddWithValue("$title",action.Title);command.Parameters.AddWithValue("$operation",operation);command.Parameters.AddWithValue("$outcome",outcome);command.Parameters.AddWithValue("$files",fileCount);command.Parameters.AddWithValue("$anomalies",anomalies);command.Parameters.AddWithValue("$archive",archive);command.Parameters.AddWithValue("$report",report);command.Parameters.AddWithValue("$date",DateTime.Now.ToString("s"));command.Parameters.AddWithValue("$user",string.IsNullOrWhiteSpace(user)?"Sistema":user);command.ExecuteNonQuery();
    }
    private SqliteConnection Open(){var connection=new SqliteConnection(_connectionString);connection.Open();return connection;}
    private static string S(SqliteDataReader reader,int index)=>reader.IsDBNull(index)?"":reader.GetString(index);
}
