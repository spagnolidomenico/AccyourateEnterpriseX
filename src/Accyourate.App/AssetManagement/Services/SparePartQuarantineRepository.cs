using Accyourate.App.AssetManagement.Models;
using Microsoft.Data.Sqlite;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SparePartQuarantineRepository
{
    private readonly string _connectionString;
    public SparePartQuarantineRepository(string? databasePath=null)
    {
        var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"AccyourateEnterpriseX");
        Directory.CreateDirectory(folder);_connectionString=$"Data Source={databasePath??Path.Combine(folder,"accyourate-assets.db")}";
        using var connection=Open();using var command=connection.CreateCommand();command.CommandText="""
            CREATE TABLE IF NOT EXISTS SparePartQuarantine(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,CaseNumber TEXT NOT NULL UNIQUE,ReturnId INTEGER NOT NULL,
                InventoryItemId INTEGER NOT NULL,LocationId INTEGER NOT NULL,Quantity REAL NOT NULL,
                InitialCondition TEXT NOT NULL,Status TEXT NOT NULL,EstimatedCost REAL NOT NULL DEFAULT 0,
                EvaluationNotes TEXT,AuthorizedBy TEXT,CreatedAt TEXT NOT NULL,UpdatedAt TEXT NOT NULL,ClosedAt TEXT
            );
            """;
        command.ExecuteNonQuery();
    }
    public IReadOnlyList<SparePartQuarantineItem> GetAll()
    {
        using var connection=Open();using var command=connection.CreateCommand();command.CommandText=Select+" ORDER BY CreatedAt DESC,Id DESC;";
        using var reader=command.ExecuteReader();var result=new List<SparePartQuarantineItem>();while(reader.Read())result.Add(Read(reader));return result;
    }
    public void Evaluate(int id,string decision,decimal estimatedCost,string notes,string user)
    {
        if(decision is not (SparePartQuarantineStatus.Repairable or SparePartQuarantineStatus.SupplierReturn or SparePartQuarantineStatus.DisposalApproved))
            throw new InvalidOperationException("Valutazione non valida.");
        using var connection=Open();using var transaction=connection.BeginTransaction();var item=Get(connection,transaction,id);
        if(item.Status!=SparePartQuarantineStatus.Pending)throw new InvalidOperationException("Il caso è già stato valutato.");
        Update(connection,transaction,id,decision,Math.Max(0,estimatedCost),notes,user,string.Empty);transaction.Commit();
    }
    public void Reintegrate(int id,int locationId,string notes,string user)
    {
        using var connection=Open();using var transaction=connection.BeginTransaction();var value=Get(connection,transaction,id);
        if(value.Status!=SparePartQuarantineStatus.Repairable)throw new InvalidOperationException("Solo il materiale riparabile può essere reintegrato.");
        if(locationId<=0)throw new InvalidOperationException("Seleziona l'ubicazione di reintegro.");
        using var item=connection.CreateCommand();item.Transaction=transaction;item.CommandText="SELECT Quantity,AverageUnitCost FROM SparePartsInventory WHERE Id=$id;";item.Parameters.AddWithValue("$id",value.InventoryItemId);
        using var reader=item.ExecuteReader();if(!reader.Read())throw new InvalidOperationException("Ricambio non trovato.");var before=Convert.ToDecimal(reader.GetDouble(0));var cost=Convert.ToDecimal(reader.GetDouble(1));reader.Close();
        Execute(connection,transaction,"UPDATE SparePartsInventory SET Quantity=$quantity,UpdatedAt=$updated WHERE Id=$item;",("$quantity",before+value.Quantity),("$updated",DateTime.Now.ToString("s")),("$item",value.InventoryItemId));
        Execute(connection,transaction,"INSERT INTO SparePartLocationBalances(InventoryItemId,LocationId,Quantity) VALUES($item,$location,$quantity) ON CONFLICT(InventoryItemId,LocationId) DO UPDATE SET Quantity=Quantity+$quantity;",("$item",value.InventoryItemId),("$location",locationId),("$quantity",value.Quantity));
        Execute(connection,transaction,"INSERT INTO SparePartsInventoryMovements(InventoryItemId,MovementType,Quantity,UnitCost,Reference,Notes,CreatedAt,BalanceBefore,BalanceAfter) VALUES($item,'Carico - Riparazione',$quantity,$cost,$reference,$notes,$created,$before,$after);",
            ("$item",value.InventoryItemId),("$quantity",value.Quantity),("$cost",cost),("$reference",value.CaseNumber),("$notes",notes),("$created",DateTime.Now.ToString("s")),("$before",before),("$after",before+value.Quantity));
        Execute(connection,transaction,"UPDATE SparePartQuarantine SET LocationId=$location WHERE Id=$id;",("$location",locationId),("$id",id));
        Update(connection,transaction,id,SparePartQuarantineStatus.Reintegrated,value.EstimatedCost,notes,user,DateTime.Now.ToString("s"));transaction.Commit();
    }
    public void CloseSupplierReturn(int id,string notes,string user)=>Close(id,SparePartQuarantineStatus.SupplierReturn,SparePartQuarantineStatus.ReturnedToSupplier,notes,user);
    public void Dispose(int id,string notes,string user)=>Close(id,SparePartQuarantineStatus.DisposalApproved,SparePartQuarantineStatus.Disposed,notes,user);
    private void Close(int id,string expected,string status,string notes,string user){using var connection=Open();using var transaction=connection.BeginTransaction();var value=Get(connection,transaction,id);if(value.Status!=expected)throw new InvalidOperationException($"Operazione consentita solo nello stato {expected}.");Update(connection,transaction,id,status,value.EstimatedCost,notes,user,DateTime.Now.ToString("s"));transaction.Commit();}
    private static void Update(SqliteConnection c,SqliteTransaction t,int id,string status,decimal cost,string notes,string user,string closed){using var command=c.CreateCommand();command.Transaction=t;command.CommandText="UPDATE SparePartQuarantine SET Status=$status,EstimatedCost=$cost,EvaluationNotes=$notes,AuthorizedBy=$user,UpdatedAt=$updated,ClosedAt=$closed WHERE Id=$id;";command.Parameters.AddWithValue("$status",status);command.Parameters.AddWithValue("$cost",cost);command.Parameters.AddWithValue("$notes",notes);command.Parameters.AddWithValue("$user",user);command.Parameters.AddWithValue("$updated",DateTime.Now.ToString("s"));command.Parameters.AddWithValue("$closed",closed);command.Parameters.AddWithValue("$id",id);command.ExecuteNonQuery();}
    private static SparePartQuarantineItem Get(SqliteConnection c,SqliteTransaction t,int id){using var command=c.CreateCommand();command.Transaction=t;command.CommandText=Select+" WHERE Id=$id;";command.Parameters.AddWithValue("$id",id);using var reader=command.ExecuteReader();return reader.Read()?Read(reader):throw new InvalidOperationException("Caso di quarantena non trovato.");}
    private static void Execute(SqliteConnection c,SqliteTransaction t,string sql,params (string Name,object Value)[] values){using var command=c.CreateCommand();command.Transaction=t;command.CommandText=sql;foreach(var value in values)command.Parameters.AddWithValue(value.Name,value.Value);command.ExecuteNonQuery();}
    private SqliteConnection Open(){var c=new SqliteConnection(_connectionString);c.Open();return c;}
    private static SparePartQuarantineItem Read(SqliteDataReader r)=>new(){Id=r.GetInt32(0),CaseNumber=S(r,1),ReturnId=r.GetInt32(2),InventoryItemId=r.GetInt32(3),LocationId=r.GetInt32(4),Quantity=D(r,5),InitialCondition=S(r,6),Status=S(r,7),EstimatedCost=D(r,8),EvaluationNotes=S(r,9),AuthorizedBy=S(r,10),CreatedAt=S(r,11),UpdatedAt=S(r,12),ClosedAt=S(r,13)};
    private static decimal D(SqliteDataReader r,int i)=>r.IsDBNull(i)?0:Convert.ToDecimal(r.GetDouble(i));
    private static string S(SqliteDataReader r,int i)=>r.IsDBNull(i)?"":r.GetString(i);
    private const string Select="SELECT Id,CaseNumber,ReturnId,InventoryItemId,LocationId,Quantity,InitialCondition,Status,EstimatedCost,EvaluationNotes,AuthorizedBy,CreatedAt,UpdatedAt,ClosedAt FROM SparePartQuarantine";
}
