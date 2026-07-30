using Microsoft.Data.Sqlite;
using Accyourate.App.AssetManagement.Models;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SparePartReplenishmentRepository
{
    private readonly string _connectionString;

    public SparePartReplenishmentRepository(string? databasePath = null)
    {
        var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"AccyourateEnterpriseX");
        Directory.CreateDirectory(folder);
        _connectionString=$"Data Source={databasePath??Path.Combine(folder,"accyourate-assets.db")}";
        using var connection=Open();
        using var command=connection.CreateCommand();
        command.CommandText="""
            CREATE TABLE IF NOT EXISTS SparePartReplenishmentRequests (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                RequestNumber TEXT NOT NULL UNIQUE,
                InventoryItemId INTEGER NOT NULL,
                SupplierId INTEGER NOT NULL DEFAULT 0,
                Status TEXT NOT NULL,
                SuggestedQuantity REAL NOT NULL DEFAULT 0,
                RequestedQuantity REAL NOT NULL DEFAULT 0,
                Notes TEXT,
                PurchaseOrderId INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS IX_Replenishment_Item
            ON SparePartReplenishmentRequests(InventoryItemId);
            CREATE INDEX IF NOT EXISTS IX_Replenishment_Order
            ON SparePartReplenishmentRequests(PurchaseOrderId);
            """;
        command.ExecuteNonQuery();
    }

    public int Create(SparePartReplenishmentRequest request)
    {
        using var connection=Open();
        if(HasOpenRequest(connection,request.InventoryItemId))
            throw new InvalidOperationException("Esiste già una richiesta aperta per questo ricambio.");
        request.RequestNumber=NextNumber(connection);
        request.CreatedAt=DateTime.Now.ToString("s");request.UpdatedAt=request.CreatedAt;
        using var command=connection.CreateCommand();
        command.CommandText="""
            INSERT INTO SparePartReplenishmentRequests
            (RequestNumber,InventoryItemId,SupplierId,Status,SuggestedQuantity,RequestedQuantity,
             Notes,PurchaseOrderId,CreatedAt,UpdatedAt)
            VALUES($number,$item,$supplier,$status,$suggested,$requested,$notes,0,$created,$updated);
            SELECT last_insert_rowid();
            """;
        Add(command,request);
        request.Id=Convert.ToInt32(command.ExecuteScalar());
        return request.Id;
    }

    public IReadOnlyList<SparePartReplenishmentRequest> GetAll()
    {
        using var connection=Open();using var command=connection.CreateCommand();
        command.CommandText=Select+" ORDER BY CreatedAt DESC,Id DESC;";
        using var reader=command.ExecuteReader();var result=new List<SparePartReplenishmentRequest>();
        while(reader.Read())result.Add(Read(reader));return result;
    }

    public void UpdateDetails(int id,int supplierId,decimal quantity,string notes)
    {
        using var connection=Open();using var command=connection.CreateCommand();
        command.CommandText="""
            UPDATE SparePartReplenishmentRequests
            SET SupplierId=$supplier,RequestedQuantity=$quantity,Notes=$notes,UpdatedAt=$now
            WHERE Id=$id AND Status IN ('Bozza','Approvata');
            """;
        command.Parameters.AddWithValue("$supplier",supplierId);command.Parameters.AddWithValue("$quantity",quantity);
        command.Parameters.AddWithValue("$notes",notes);command.Parameters.AddWithValue("$now",DateTime.Now.ToString("s"));
        command.Parameters.AddWithValue("$id",id);command.ExecuteNonQuery();
    }

    public void SetStatus(int id,string status)
    {
        using var connection=Open();using var command=connection.CreateCommand();
        command.CommandText="UPDATE SparePartReplenishmentRequests SET Status=$status,UpdatedAt=$now WHERE Id=$id;";
        command.Parameters.AddWithValue("$status",status);command.Parameters.AddWithValue("$now",DateTime.Now.ToString("s"));
        command.Parameters.AddWithValue("$id",id);command.ExecuteNonQuery();
    }

    public void LinkOrder(int id,int purchaseOrderId)
    {
        using var connection=Open();using var command=connection.CreateCommand();
        command.CommandText="UPDATE SparePartReplenishmentRequests SET Status=$status,PurchaseOrderId=$order,UpdatedAt=$now WHERE Id=$id;";
        command.Parameters.AddWithValue("$status",ReplenishmentRequestStatus.Ordered);
        command.Parameters.AddWithValue("$order",purchaseOrderId);command.Parameters.AddWithValue("$now",DateTime.Now.ToString("s"));
        command.Parameters.AddWithValue("$id",id);command.ExecuteNonQuery();
    }

    public void CompleteByOrderId(int orderId)
    {
        using var connection=Open();using var command=connection.CreateCommand();
        command.CommandText="UPDATE SparePartReplenishmentRequests SET Status=$status,UpdatedAt=$now WHERE PurchaseOrderId=$order AND Status=$ordered;";
        command.Parameters.AddWithValue("$status",ReplenishmentRequestStatus.Completed);
        command.Parameters.AddWithValue("$ordered",ReplenishmentRequestStatus.Ordered);
        command.Parameters.AddWithValue("$now",DateTime.Now.ToString("s"));command.Parameters.AddWithValue("$order",orderId);
        command.ExecuteNonQuery();
    }

    private static bool HasOpenRequest(SqliteConnection connection,int itemId)
    {
        using var command=connection.CreateCommand();
        command.CommandText="SELECT COUNT(*) FROM SparePartReplenishmentRequests WHERE InventoryItemId=$item AND Status IN ('Bozza','Approvata','Ordinata');";
        command.Parameters.AddWithValue("$item",itemId);return Convert.ToInt32(command.ExecuteScalar())>0;
    }
    private static string NextNumber(SqliteConnection connection)
    {
        using var command=connection.CreateCommand();command.CommandText="SELECT COALESCE(MAX(Id),0)+1 FROM SparePartReplenishmentRequests;";
        return $"RDA-{DateTime.Today:yyyy}-{Convert.ToInt32(command.ExecuteScalar()):D6}";
    }
    private static void Add(SqliteCommand command,SparePartReplenishmentRequest r)
    {
        command.Parameters.AddWithValue("$number",r.RequestNumber);command.Parameters.AddWithValue("$item",r.InventoryItemId);
        command.Parameters.AddWithValue("$supplier",r.SupplierId);command.Parameters.AddWithValue("$status",r.Status);
        command.Parameters.AddWithValue("$suggested",r.SuggestedQuantity);command.Parameters.AddWithValue("$requested",r.RequestedQuantity);
        command.Parameters.AddWithValue("$notes",r.Notes);command.Parameters.AddWithValue("$created",r.CreatedAt);
        command.Parameters.AddWithValue("$updated",r.UpdatedAt);
    }
    private static SparePartReplenishmentRequest Read(SqliteDataReader r)=>new()
    {
        Id=r.GetInt32(0),RequestNumber=S(r,1),InventoryItemId=r.GetInt32(2),SupplierId=r.GetInt32(3),
        Status=S(r,4),SuggestedQuantity=D(r,5),RequestedQuantity=D(r,6),Notes=S(r,7),
        PurchaseOrderId=r.GetInt32(8),CreatedAt=S(r,9),UpdatedAt=S(r,10)
    };
    private SqliteConnection Open(){var c=new SqliteConnection(_connectionString);c.Open();return c;}
    private static decimal D(SqliteDataReader r,int i)=>r.IsDBNull(i)?0:Convert.ToDecimal(r.GetDouble(i));
    private static string S(SqliteDataReader r,int i)=>r.IsDBNull(i)?"":r.GetString(i);
    private const string Select="""
        SELECT Id,RequestNumber,InventoryItemId,SupplierId,Status,SuggestedQuantity,
               RequestedQuantity,Notes,PurchaseOrderId,CreatedAt,UpdatedAt
        FROM SparePartReplenishmentRequests
        """;
}
