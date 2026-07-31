using Accyourate.App.AssetManagement.Models;
using Microsoft.Data.Sqlite;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SparePartReturnRepository
{
    private readonly string _connectionString;
    public SparePartReturnRepository(string? databasePath=null)
    {
        var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"AccyourateEnterpriseX");
        Directory.CreateDirectory(folder);_connectionString=$"Data Source={databasePath??Path.Combine(folder,"accyourate-assets.db")}";
        using var connection=Open();using var command=connection.CreateCommand();command.CommandText="""
            CREATE TABLE IF NOT EXISTS SparePartReturns(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ReturnNumber TEXT NOT NULL UNIQUE,
                PickRequestId INTEGER NOT NULL,
                InventoryItemId INTEGER NOT NULL,
                LocationId INTEGER NOT NULL DEFAULT 0,
                Quantity REAL NOT NULL,
                Condition TEXT NOT NULL,
                Reason TEXT,
                Notes TEXT,
                OperatorName TEXT,
                CreatedAt TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS IX_SparePartReturns_Request ON SparePartReturns(PickRequestId);
            """;
        command.ExecuteNonQuery();
    }

    public decimal ReturnedQuantity(int requestId)
    {
        using var connection=Open();using var command=connection.CreateCommand();
        command.CommandText="SELECT COALESCE(SUM(Quantity),0) FROM SparePartReturns WHERE PickRequestId=$request;";
        command.Parameters.AddWithValue("$request",requestId);return Convert.ToDecimal(command.ExecuteScalar());
    }

    public int Register(int requestId,decimal quantity,int locationId,string condition,string reason,string notes,string operatorName)
    {
        if(quantity<=0)throw new InvalidOperationException("La quantità restituita deve essere maggiore di zero.");
        using var connection=Open();using var transaction=connection.BeginTransaction();
        using var requestCommand=connection.CreateCommand();requestCommand.Transaction=transaction;requestCommand.CommandText="""
            SELECT RequestNumber,InventoryItemId,Quantity,Status
            FROM SparePartPickRequests WHERE Id=$id;
            """;
        requestCommand.Parameters.AddWithValue("$id",requestId);using var reader=requestCommand.ExecuteReader();
        if(!reader.Read())throw new InvalidOperationException("Richiesta di prelievo non trovata.");
        var requestNumber=reader.GetString(0);var itemId=reader.GetInt32(1);
        var delivered=Convert.ToDecimal(reader.GetDouble(2));var status=reader.GetString(3);reader.Close();
        if(status!=SparePartPickRequestStatus.Delivered)throw new InvalidOperationException("Il reso è consentito solo per richieste consegnate.");
        using var returnedCommand=connection.CreateCommand();returnedCommand.Transaction=transaction;
        returnedCommand.CommandText="SELECT COALESCE(SUM(Quantity),0) FROM SparePartReturns WHERE PickRequestId=$request;";
        returnedCommand.Parameters.AddWithValue("$request",requestId);var returned=Convert.ToDecimal(returnedCommand.ExecuteScalar());
        if(returned+quantity>delivered)throw new InvalidOperationException($"Quantità restituibile: {Math.Max(0,delivered-returned):N2}.");
        var reusable=condition==SparePartReturnCondition.Reusable;
        if(reusable&&locationId<=0)throw new InvalidOperationException("Seleziona l'ubicazione di reintegro.");
        decimal before=0,cost=0;
        using(var item=connection.CreateCommand())
        {
            item.Transaction=transaction;item.CommandText="SELECT Quantity,AverageUnitCost FROM SparePartsInventory WHERE Id=$id;";
            item.Parameters.AddWithValue("$id",itemId);using var itemReader=item.ExecuteReader();
            if(!itemReader.Read())throw new InvalidOperationException("Ricambio non trovato.");
            before=Convert.ToDecimal(itemReader.GetDouble(0));cost=Convert.ToDecimal(itemReader.GetDouble(1));
        }
        var number=$"RES-{DateTime.Today:yyyy}-{NextId(connection,transaction):D6}";
        if(reusable)
        {
            Execute(connection,transaction,"UPDATE SparePartsInventory SET Quantity=$quantity,UpdatedAt=$updated WHERE Id=$item;",
                ("$quantity",before+quantity),("$updated",DateTime.Now.ToString("s")),("$item",itemId));
            Execute(connection,transaction,"INSERT INTO SparePartLocationBalances(InventoryItemId,LocationId,Quantity) VALUES($item,$location,$quantity) ON CONFLICT(InventoryItemId,LocationId) DO UPDATE SET Quantity=Quantity+$quantity;",
                ("$item",itemId),("$location",locationId),("$quantity",quantity));
            Execute(connection,transaction,"INSERT INTO SparePartsInventoryMovements(InventoryItemId,MovementType,Quantity,UnitCost,Reference,Notes,CreatedAt,BalanceBefore,BalanceAfter) VALUES($item,'Carico - Reso',$quantity,$cost,$reference,$notes,$created,$before,$after);",
                ("$item",itemId),("$quantity",quantity),("$cost",cost),("$reference",number),("$notes",$"{requestNumber} - {notes}".Trim()),("$created",DateTime.Now.ToString("s")),("$before",before),("$after",before+quantity));
        }
        using var insert=connection.CreateCommand();insert.Transaction=transaction;insert.CommandText="""
            INSERT INTO SparePartReturns(ReturnNumber,PickRequestId,InventoryItemId,LocationId,Quantity,Condition,Reason,Notes,OperatorName,CreatedAt)
            VALUES($number,$request,$item,$location,$quantity,$condition,$reason,$notes,$operator,$created);
            SELECT last_insert_rowid();
            """;
        insert.Parameters.AddWithValue("$number",number);insert.Parameters.AddWithValue("$request",requestId);insert.Parameters.AddWithValue("$item",itemId);
        insert.Parameters.AddWithValue("$location",reusable?locationId:0);insert.Parameters.AddWithValue("$quantity",quantity);
        insert.Parameters.AddWithValue("$condition",condition);insert.Parameters.AddWithValue("$reason",reason);insert.Parameters.AddWithValue("$notes",notes);
        insert.Parameters.AddWithValue("$operator",operatorName);insert.Parameters.AddWithValue("$created",DateTime.Now.ToString("s"));
        var id=Convert.ToInt32(insert.ExecuteScalar());transaction.Commit();return id;
    }

    public IReadOnlyList<SparePartReturn> GetAll()
    {
        using var connection=Open();using var command=connection.CreateCommand();command.CommandText="""
            SELECT Id,ReturnNumber,PickRequestId,InventoryItemId,LocationId,Quantity,Condition,Reason,Notes,OperatorName,CreatedAt
            FROM SparePartReturns ORDER BY CreatedAt DESC,Id DESC;
            """;
        using var reader=command.ExecuteReader();var result=new List<SparePartReturn>();
        while(reader.Read())result.Add(new SparePartReturn{Id=reader.GetInt32(0),ReturnNumber=S(reader,1),PickRequestId=reader.GetInt32(2),
            InventoryItemId=reader.GetInt32(3),LocationId=reader.GetInt32(4),Quantity=D(reader,5),Condition=S(reader,6),
            Reason=S(reader,7),Notes=S(reader,8),OperatorName=S(reader,9),CreatedAt=S(reader,10)});
        return result;
    }

    private static int NextId(SqliteConnection c,SqliteTransaction t){using var command=c.CreateCommand();command.Transaction=t;command.CommandText="SELECT COALESCE(MAX(Id),0)+1 FROM SparePartReturns;";return Convert.ToInt32(command.ExecuteScalar());}
    private static void Execute(SqliteConnection c,SqliteTransaction t,string sql,params (string Name,object Value)[] values){using var command=c.CreateCommand();command.Transaction=t;command.CommandText=sql;foreach(var value in values)command.Parameters.AddWithValue(value.Name,value.Value);command.ExecuteNonQuery();}
    private SqliteConnection Open(){var c=new SqliteConnection(_connectionString);c.Open();return c;}
    private static decimal D(SqliteDataReader r,int i)=>r.IsDBNull(i)?0:Convert.ToDecimal(r.GetDouble(i));
    private static string S(SqliteDataReader r,int i)=>r.IsDBNull(i)?"":r.GetString(i);
}
