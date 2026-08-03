using Accyourate.App.AssetManagement.Models;
using Microsoft.Data.Sqlite;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SupplierRmaEvaluationRepository
{
    private readonly string _connectionString;
    public SupplierRmaEvaluationRepository(string? databasePath=null){var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"AccyourateEnterpriseX");Directory.CreateDirectory(folder);_connectionString=$"Data Source={databasePath??Path.Combine(folder,"accyourate-assets.db")}";using var c=Open();using var cmd=c.CreateCommand();cmd.CommandText="""
        CREATE TABLE IF NOT EXISTS SupplierRmaEvaluations(
            SupplierId INTEGER PRIMARY KEY,
            Rating INTEGER NOT NULL,
            Notes TEXT,
            UpdatedBy TEXT,
            UpdatedAt TEXT NOT NULL
        );
        """;cmd.ExecuteNonQuery();}
    public void Save(int supplierId,int rating,string notes,string user){if(supplierId<=0)throw new InvalidOperationException("Fornitore non valido.");if(rating is <1 or >5)throw new InvalidOperationException("La valutazione deve essere compresa tra 1 e 5.");using var c=Open();using var cmd=c.CreateCommand();cmd.CommandText="""
        INSERT INTO SupplierRmaEvaluations(SupplierId,Rating,Notes,UpdatedBy,UpdatedAt)
        VALUES($supplier,$rating,$notes,$user,$date)
        ON CONFLICT(SupplierId) DO UPDATE SET Rating=$rating,Notes=$notes,UpdatedBy=$user,UpdatedAt=$date;
        """;cmd.Parameters.AddWithValue("$supplier",supplierId);cmd.Parameters.AddWithValue("$rating",rating);cmd.Parameters.AddWithValue("$notes",notes.Trim());cmd.Parameters.AddWithValue("$user",user);cmd.Parameters.AddWithValue("$date",DateTime.Now.ToString("s"));cmd.ExecuteNonQuery();}
    public IReadOnlyDictionary<int,SupplierRmaEvaluation> GetAll(){using var c=Open();using var cmd=c.CreateCommand();cmd.CommandText="SELECT SupplierId,Rating,Notes,UpdatedBy,UpdatedAt FROM SupplierRmaEvaluations;";using var r=cmd.ExecuteReader();var values=new Dictionary<int,SupplierRmaEvaluation>();while(r.Read()){var x=new SupplierRmaEvaluation{SupplierId=r.GetInt32(0),Rating=r.GetInt32(1),Notes=S(r,2),UpdatedBy=S(r,3),UpdatedAt=S(r,4)};values[x.SupplierId]=x;}return values;}
    private SqliteConnection Open(){var c=new SqliteConnection(_connectionString);c.Open();return c;}private static string S(SqliteDataReader r,int i)=>r.IsDBNull(i)?"":r.GetString(i);
}
