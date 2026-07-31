using Accyourate.App.AssetManagement.Models;
using Microsoft.Data.Sqlite;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SparePartPickRequestRepository
{
    private readonly string _connectionString;
    public SparePartPickRequestRepository(string? databasePath=null)
    {
        var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"AccyourateEnterpriseX");
        Directory.CreateDirectory(folder);_connectionString=$"Data Source={databasePath??Path.Combine(folder,"accyourate-assets.db")}";
        using var connection=Open();using var command=connection.CreateCommand();command.CommandText="""
            CREATE TABLE IF NOT EXISTS SparePartPickRequests(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                RequestNumber TEXT NOT NULL UNIQUE,
                InventoryItemId INTEGER NOT NULL,
                Quantity REAL NOT NULL,
                PreferredLocationId INTEGER NOT NULL DEFAULT 0,
                MaintenanceTicketId INTEGER NOT NULL DEFAULT 0,
                RequestedBy TEXT,
                Technician TEXT,
                Notes TEXT,
                Status TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                DeliveredAt TEXT
            );
            CREATE INDEX IF NOT EXISTS IX_SparePartPickRequests_Status
            ON SparePartPickRequests(Status,InventoryItemId);
            """;
        command.ExecuteNonQuery();
    }

    public int Create(SparePartPickRequest request)
    {
        if(request.Quantity<=0)throw new InvalidOperationException("La quantità richiesta deve essere maggiore di zero.");
        request.RequestNumber=string.IsNullOrWhiteSpace(request.RequestNumber)?NextNumber():request.RequestNumber.Trim();
        request.Status=SparePartPickRequestStatus.Draft;request.CreatedAt=request.UpdatedAt=DateTime.Now.ToString("s");
        using var connection=Open();using var command=connection.CreateCommand();command.CommandText="""
            INSERT INTO SparePartPickRequests
            (RequestNumber,InventoryItemId,Quantity,PreferredLocationId,MaintenanceTicketId,RequestedBy,Technician,Notes,Status,CreatedAt,UpdatedAt,DeliveredAt)
            VALUES($number,$item,$quantity,$location,$maintenance,$requested,$technician,$notes,$status,$created,$updated,'');
            SELECT last_insert_rowid();
            """;
        Add(command,request);request.Id=Convert.ToInt32(command.ExecuteScalar());return request.Id;
    }

    public IReadOnlyList<SparePartPickRequest> GetAll()
    {
        using var connection=Open();using var command=connection.CreateCommand();command.CommandText=Select+" ORDER BY CreatedAt DESC,Id DESC;";
        using var reader=command.ExecuteReader();var result=new List<SparePartPickRequest>();while(reader.Read())result.Add(Read(reader));return result;
    }

    public decimal ReservedQuantity(int itemId,int exceptRequestId=0)
    {
        using var connection=Open();using var command=connection.CreateCommand();command.CommandText="""
            SELECT COALESCE(SUM(Quantity),0) FROM SparePartPickRequests
            WHERE InventoryItemId=$item AND Id<>$except AND Status IN ($approved,$preparing);
            """;
        command.Parameters.AddWithValue("$item",itemId);command.Parameters.AddWithValue("$except",exceptRequestId);
        command.Parameters.AddWithValue("$approved",SparePartPickRequestStatus.Approved);command.Parameters.AddWithValue("$preparing",SparePartPickRequestStatus.Preparing);
        return Convert.ToDecimal(command.ExecuteScalar());
    }

    public void Approve(int id)
    {
        using var connection=Open();using var transaction=connection.BeginTransaction();var request=Get(connection,transaction,id);
        Require(request.Status,SparePartPickRequestStatus.Draft);
        var local=Scalar(connection,transaction,"SELECT COALESCE(SUM(Quantity),0) FROM SparePartLocationBalances WHERE InventoryItemId=$item;",request.InventoryItemId);
        var reserved=Scalar(connection,transaction,"SELECT COALESCE(SUM(Quantity),0) FROM SparePartPickRequests WHERE InventoryItemId=$item AND Id<>$id AND Status IN ('Approvata','In preparazione');",request.InventoryItemId,id);
        if(local-reserved<request.Quantity)throw new InvalidOperationException($"Disponibilità prenotabile insufficiente: {Math.Max(0,local-reserved):N2}.");
        SetStatus(connection,transaction,id,SparePartPickRequestStatus.Approved);transaction.Commit();
    }

    public void StartPreparation(int id)
    {
        using var connection=Open();using var transaction=connection.BeginTransaction();var request=Get(connection,transaction,id);
        Require(request.Status,SparePartPickRequestStatus.Approved);SetStatus(connection,transaction,id,SparePartPickRequestStatus.Preparing);transaction.Commit();
    }

    public void Cancel(int id)
    {
        using var connection=Open();using var transaction=connection.BeginTransaction();var request=Get(connection,transaction,id);
        if(request.Status==SparePartPickRequestStatus.Delivered)throw new InvalidOperationException("Una richiesta già consegnata non può essere annullata.");
        SetStatus(connection,transaction,id,SparePartPickRequestStatus.Cancelled);transaction.Commit();
    }

    public void Deliver(int id,string operatorName)
    {
        using var connection=Open();using var transaction=connection.BeginTransaction();var request=Get(connection,transaction,id);
        Require(request.Status,SparePartPickRequestStatus.Preparing);
        using var itemCommand=connection.CreateCommand();itemCommand.Transaction=transaction;
        itemCommand.CommandText="SELECT Quantity,AverageUnitCost FROM SparePartsInventory WHERE Id=$id;";itemCommand.Parameters.AddWithValue("$id",request.InventoryItemId);
        using var itemReader=itemCommand.ExecuteReader();if(!itemReader.Read())throw new InvalidOperationException("Ricambio non trovato.");
        var total=Convert.ToDecimal(itemReader.GetDouble(0));var cost=Convert.ToDecimal(itemReader.GetDouble(1));itemReader.Close();
        if(total<request.Quantity)throw new InvalidOperationException($"Giacenza totale insufficiente: {total:N2}.");
        using var balanceCommand=connection.CreateCommand();balanceCommand.Transaction=transaction;balanceCommand.CommandText="""
            SELECT LocationId,Quantity FROM SparePartLocationBalances
            WHERE InventoryItemId=$item AND Quantity>0
            ORDER BY CASE WHEN LocationId=$preferred THEN 0 ELSE 1 END,LocationId;
            """;
        balanceCommand.Parameters.AddWithValue("$item",request.InventoryItemId);balanceCommand.Parameters.AddWithValue("$preferred",request.PreferredLocationId);
        using var reader=balanceCommand.ExecuteReader();var balances=new List<(int Id,decimal Quantity)>();
        while(reader.Read())balances.Add((reader.GetInt32(0),Convert.ToDecimal(reader.GetDouble(1))));reader.Close();
        if(balances.Sum(x=>x.Quantity)<request.Quantity)throw new InvalidOperationException("Giacenza nelle ubicazioni insufficiente.");
        var remaining=request.Quantity;
        foreach(var balance in balances)
        {
            var picked=Math.Min(balance.Quantity,remaining);if(picked<=0)continue;
            Execute(connection,transaction,"UPDATE SparePartLocationBalances SET Quantity=$quantity WHERE InventoryItemId=$item AND LocationId=$location;",
                ("$quantity",balance.Quantity-picked),("$item",request.InventoryItemId),("$location",balance.Id));
            Execute(connection,transaction,"INSERT INTO SparePartLocationPicks(InventoryItemId,LocationId,Quantity,Reference,Notes,OperatorName,CreatedAt) VALUES($item,$location,$quantity,$reference,$notes,$operator,$created);",
                ("$item",request.InventoryItemId),("$location",balance.Id),("$quantity",picked),("$reference",request.RequestNumber),
                ("$notes",request.Notes),("$operator",operatorName),("$created",DateTime.Now.ToString("s")));
            remaining-=picked;if(remaining<=0)break;
        }
        var after=total-request.Quantity;
        Execute(connection,transaction,"UPDATE SparePartsInventory SET Quantity=$quantity,UpdatedAt=$updated WHERE Id=$item;",
            ("$quantity",after),("$updated",DateTime.Now.ToString("s")),("$item",request.InventoryItemId));
        Execute(connection,transaction,"INSERT INTO SparePartsInventoryMovements(InventoryItemId,MovementType,Quantity,UnitCost,Reference,Notes,CreatedAt,BalanceBefore,BalanceAfter) VALUES($item,'Scarico - Richiesta prelievo',$quantity,$cost,$reference,$notes,$created,$before,$after);",
            ("$item",request.InventoryItemId),("$quantity",-request.Quantity),("$cost",cost),("$reference",request.RequestNumber),
            ("$notes",request.Notes),("$created",DateTime.Now.ToString("s")),("$before",total),("$after",after));
        using var status=connection.CreateCommand();status.Transaction=transaction;status.CommandText="UPDATE SparePartPickRequests SET Status=$status,UpdatedAt=$updated,DeliveredAt=$delivered WHERE Id=$id;";
        status.Parameters.AddWithValue("$status",SparePartPickRequestStatus.Delivered);status.Parameters.AddWithValue("$updated",DateTime.Now.ToString("s"));
        status.Parameters.AddWithValue("$delivered",DateTime.Now.ToString("s"));status.Parameters.AddWithValue("$id",id);status.ExecuteNonQuery();
        transaction.Commit();
    }

    private string NextNumber()
    {
        using var connection=Open();using var command=connection.CreateCommand();
        var prefix=$"PRL-{DateTime.Today:yyyy}-";command.CommandText="SELECT COALESCE(MAX(Id),0)+1 FROM SparePartPickRequests;";
        return $"{prefix}{Convert.ToInt32(command.ExecuteScalar()):D6}";
    }
    private static void Require(string actual,string expected){if(actual!=expected)throw new InvalidOperationException($"Operazione consentita solo nello stato {expected}.");}
    private static void SetStatus(SqliteConnection c,SqliteTransaction t,int id,string value)=>Execute(c,t,"UPDATE SparePartPickRequests SET Status=$status,UpdatedAt=$updated WHERE Id=$id;",("$status",value),("$updated",DateTime.Now.ToString("s")),("$id",id));
    private static decimal Scalar(SqliteConnection c,SqliteTransaction t,string sql,int item,int id=0){using var command=c.CreateCommand();command.Transaction=t;command.CommandText=sql;command.Parameters.AddWithValue("$item",item);if(sql.Contains("$id"))command.Parameters.AddWithValue("$id",id);return Convert.ToDecimal(command.ExecuteScalar());}
    private static SparePartPickRequest Get(SqliteConnection c,SqliteTransaction t,int id){using var command=c.CreateCommand();command.Transaction=t;command.CommandText=Select+" WHERE Id=$id;";command.Parameters.AddWithValue("$id",id);using var reader=command.ExecuteReader();return reader.Read()?Read(reader):throw new InvalidOperationException("Richiesta non trovata.");}
    private static void Add(SqliteCommand c,SparePartPickRequest x){c.Parameters.AddWithValue("$number",x.RequestNumber);c.Parameters.AddWithValue("$item",x.InventoryItemId);c.Parameters.AddWithValue("$quantity",x.Quantity);c.Parameters.AddWithValue("$location",x.PreferredLocationId);c.Parameters.AddWithValue("$maintenance",x.MaintenanceTicketId);c.Parameters.AddWithValue("$requested",x.RequestedBy);c.Parameters.AddWithValue("$technician",x.Technician);c.Parameters.AddWithValue("$notes",x.Notes);c.Parameters.AddWithValue("$status",x.Status);c.Parameters.AddWithValue("$created",x.CreatedAt);c.Parameters.AddWithValue("$updated",x.UpdatedAt);}
    private static void Execute(SqliteConnection c,SqliteTransaction t,string sql,params (string Name,object Value)[] values){using var command=c.CreateCommand();command.Transaction=t;command.CommandText=sql;foreach(var value in values)command.Parameters.AddWithValue(value.Name,value.Value);command.ExecuteNonQuery();}
    private static SparePartPickRequest Read(SqliteDataReader r)=>new(){Id=r.GetInt32(0),RequestNumber=S(r,1),InventoryItemId=r.GetInt32(2),Quantity=D(r,3),PreferredLocationId=r.GetInt32(4),MaintenanceTicketId=r.GetInt32(5),RequestedBy=S(r,6),Technician=S(r,7),Notes=S(r,8),Status=S(r,9),CreatedAt=S(r,10),UpdatedAt=S(r,11),DeliveredAt=S(r,12)};
    private SqliteConnection Open(){var c=new SqliteConnection(_connectionString);c.Open();return c;}
    private static decimal D(SqliteDataReader r,int i)=>r.IsDBNull(i)?0:Convert.ToDecimal(r.GetDouble(i));
    private static string S(SqliteDataReader r,int i)=>r.IsDBNull(i)?"":r.GetString(i);
    private const string Select="SELECT Id,RequestNumber,InventoryItemId,Quantity,PreferredLocationId,MaintenanceTicketId,RequestedBy,Technician,Notes,Status,CreatedAt,UpdatedAt,DeliveredAt FROM SparePartPickRequests";
}
