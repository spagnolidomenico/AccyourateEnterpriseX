using Microsoft.Data.Sqlite;
using Accyourate.App.AssetManagement.Models;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SparePartLocationsRepository
{
    private readonly string _connectionString;
    public SparePartLocationsRepository(string? databasePath=null)
    {
        var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"AccyourateEnterpriseX");
        Directory.CreateDirectory(folder);_connectionString=$"Data Source={databasePath??Path.Combine(folder,"accyourate-assets.db")}";
        using var connection=Open();using var command=connection.CreateCommand();command.CommandText="""
            CREATE TABLE IF NOT EXISTS SparePartWarehouseLocations(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Code TEXT NOT NULL UNIQUE,
                Name TEXT NOT NULL,
                Warehouse TEXT,
                Aisle TEXT,
                Shelf TEXT,
                IsActive INTEGER NOT NULL DEFAULT 1
            );
            CREATE TABLE IF NOT EXISTS SparePartLocationBalances(
                InventoryItemId INTEGER NOT NULL,
                LocationId INTEGER NOT NULL,
                Quantity REAL NOT NULL DEFAULT 0,
                PRIMARY KEY(InventoryItemId,LocationId)
            );
            CREATE TABLE IF NOT EXISTS SparePartLocationTransfers(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                InventoryItemId INTEGER NOT NULL,
                FromLocationId INTEGER NOT NULL,
                ToLocationId INTEGER NOT NULL,
                Quantity REAL NOT NULL,
                Reference TEXT,
                Notes TEXT,
                OperatorName TEXT,
                CreatedAt TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS SparePartLocationPicks(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                InventoryItemId INTEGER NOT NULL,
                LocationId INTEGER NOT NULL,
                Quantity REAL NOT NULL,
                Reference TEXT,
                Notes TEXT,
                OperatorName TEXT,
                CreatedAt TEXT NOT NULL
            );
            """;
        command.ExecuteNonQuery();
    }

    public int SaveLocation(SparePartWarehouseLocation location)
    {
        using var connection=Open();
        if(location.Id==0)
        {
            using var existing=connection.CreateCommand();existing.CommandText="SELECT Id FROM SparePartWarehouseLocations WHERE Code=$code COLLATE NOCASE;";
            existing.Parameters.AddWithValue("$code",location.Code.Trim());var id=existing.ExecuteScalar();
            if(id is not null&&id!=DBNull.Value)location.Id=Convert.ToInt32(id);
        }
        using var command=connection.CreateCommand();command.CommandText=location.Id==0?"""
            INSERT INTO SparePartWarehouseLocations(Code,Name,Warehouse,Aisle,Shelf,IsActive)
            VALUES($code,$name,$warehouse,$aisle,$shelf,$active);SELECT last_insert_rowid();
            """:"""
            UPDATE SparePartWarehouseLocations SET Code=$code,Name=$name,Warehouse=$warehouse,Aisle=$aisle,Shelf=$shelf,IsActive=$active
            WHERE Id=$id;SELECT $id;
            """;
        command.Parameters.AddWithValue("$id",location.Id);command.Parameters.AddWithValue("$code",location.Code.Trim());
        command.Parameters.AddWithValue("$name",location.Name.Trim());command.Parameters.AddWithValue("$warehouse",location.Warehouse);
        command.Parameters.AddWithValue("$aisle",location.Aisle);command.Parameters.AddWithValue("$shelf",location.Shelf);
        command.Parameters.AddWithValue("$active",location.IsActive?1:0);location.Id=Convert.ToInt32(command.ExecuteScalar());return location.Id;
    }

    public IReadOnlyList<SparePartWarehouseLocation> GetLocations()
    {
        using var connection=Open();using var command=connection.CreateCommand();
        command.CommandText="SELECT Id,Code,Name,Warehouse,Aisle,Shelf,IsActive FROM SparePartWarehouseLocations ORDER BY Code;";
        using var reader=command.ExecuteReader();var result=new List<SparePartWarehouseLocation>();
        while(reader.Read())result.Add(new SparePartWarehouseLocation{Id=reader.GetInt32(0),Code=S(reader,1),Name=S(reader,2),Warehouse=S(reader,3),Aisle=S(reader,4),Shelf=S(reader,5),IsActive=reader.GetInt32(6)!=0});
        return result;
    }

    public IReadOnlyList<SparePartLocationBalance> GetBalances()
    {
        using var connection=Open();using var command=connection.CreateCommand();
        command.CommandText="SELECT InventoryItemId,LocationId,Quantity FROM SparePartLocationBalances ORDER BY InventoryItemId,LocationId;";
        using var reader=command.ExecuteReader();var result=new List<SparePartLocationBalance>();
        while(reader.Read())result.Add(new SparePartLocationBalance{InventoryItemId=reader.GetInt32(0),LocationId=reader.GetInt32(1),Quantity=D(reader,2)});
        return result;
    }

    public void SetStocktakeBalance(int itemId,int locationId,decimal quantity)
    {
        using var connection=Open();using var transaction=connection.BeginTransaction();
        SetBalance(connection,transaction,itemId,locationId,Math.Max(0,quantity));transaction.Commit();
    }

    public void ReceiveIntoLocation(int itemId,int locationId,decimal receivedQuantity,decimal currentTotal)
    {
        using var connection=Open();using var transaction=connection.BeginTransaction();
        using var count=connection.CreateCommand();count.Transaction=transaction;
        count.CommandText="SELECT COUNT(*) FROM SparePartLocationBalances WHERE InventoryItemId=$item;";
        count.Parameters.AddWithValue("$item",itemId);
        if(Convert.ToInt32(count.ExecuteScalar())==0)SetBalance(connection,transaction,itemId,locationId,currentTotal);
        else SetBalance(connection,transaction,itemId,locationId,Balance(connection,transaction,itemId,locationId)+receivedQuantity);
        transaction.Commit();
    }

    public decimal GetAvailableQuantity(int itemId)
    {
        using var connection=Open();using var command=connection.CreateCommand();
        command.CommandText="SELECT COALESCE(SUM(Quantity),0) FROM SparePartLocationBalances WHERE InventoryItemId=$item;";
        command.Parameters.AddWithValue("$item",itemId);
        return Convert.ToDecimal(command.ExecuteScalar());
    }

    public IReadOnlyList<SparePartPickAllocation> PickFromLocations(
        int itemId,decimal quantity,int? preferredLocationId,string reference,string notes,string operatorName)
    {
        if(quantity<=0)throw new InvalidOperationException("La quantità da prelevare deve essere maggiore di zero.");
        using var connection=Open();using var transaction=connection.BeginTransaction();
        using var command=connection.CreateCommand();command.Transaction=transaction;
        command.CommandText="""
            SELECT LocationId,Quantity FROM SparePartLocationBalances
            WHERE InventoryItemId=$item AND Quantity>0
            ORDER BY CASE WHEN LocationId=$preferred THEN 0 ELSE 1 END,LocationId;
            """;
        command.Parameters.AddWithValue("$item",itemId);command.Parameters.AddWithValue("$preferred",preferredLocationId??-1);
        using var reader=command.ExecuteReader();var balances=new List<(int LocationId,decimal Quantity)>();
        while(reader.Read())balances.Add((reader.GetInt32(0),Convert.ToDecimal(reader.GetDouble(1))));
        reader.Close();
        var available=balances.Sum(x=>x.Quantity);
        if(available<quantity)throw new InvalidOperationException($"Disponibilità insufficiente nelle ubicazioni: {available:N2} unità.");
        var remaining=quantity;var allocations=new List<SparePartPickAllocation>();
        foreach(var balance in balances)
        {
            var picked=Math.Min(balance.Quantity,remaining);if(picked<=0)continue;
            SetBalance(connection,transaction,itemId,balance.LocationId,balance.Quantity-picked);
            using var insert=connection.CreateCommand();insert.Transaction=transaction;insert.CommandText="""
                INSERT INTO SparePartLocationPicks
                (InventoryItemId,LocationId,Quantity,Reference,Notes,OperatorName,CreatedAt)
                VALUES($item,$location,$quantity,$reference,$notes,$operator,$created);
                """;
            insert.Parameters.AddWithValue("$item",itemId);insert.Parameters.AddWithValue("$location",balance.LocationId);
            insert.Parameters.AddWithValue("$quantity",picked);insert.Parameters.AddWithValue("$reference",reference);
            insert.Parameters.AddWithValue("$notes",notes);insert.Parameters.AddWithValue("$operator",operatorName);
            insert.Parameters.AddWithValue("$created",DateTime.Now.ToString("s"));insert.ExecuteNonQuery();
            allocations.Add(new SparePartPickAllocation{LocationId=balance.LocationId,Quantity=picked});
            remaining-=picked;if(remaining<=0)break;
        }
        transaction.Commit();return allocations;
    }

    public IReadOnlyList<SparePartLocationPick> GetPicks(int limit=500)
    {
        using var connection=Open();using var command=connection.CreateCommand();command.CommandText="""
            SELECT Id,InventoryItemId,LocationId,Quantity,Reference,Notes,OperatorName,CreatedAt
            FROM SparePartLocationPicks ORDER BY CreatedAt DESC,Id DESC LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$limit",Math.Max(1,limit));using var reader=command.ExecuteReader();
        var result=new List<SparePartLocationPick>();
        while(reader.Read())result.Add(new SparePartLocationPick{Id=reader.GetInt32(0),InventoryItemId=reader.GetInt32(1),
            LocationId=reader.GetInt32(2),Quantity=D(reader,3),Reference=S(reader,4),Notes=S(reader,5),
            OperatorName=S(reader,6),CreatedAt=S(reader,7)});
        return result;
    }

    public IReadOnlyList<SparePartLocationDiscrepancy> GetDiscrepancies(IReadOnlyList<SparePartInventoryItem> items)
    {
        var allocated=GetBalances().GroupBy(x=>x.InventoryItemId).ToDictionary(x=>x.Key,x=>x.Sum(y=>y.Quantity));
        return items.Select(item=>new SparePartLocationDiscrepancy
        {
            InventoryItemId=item.Id,PartCode=item.PartCode,Description=item.Description,
            TotalQuantity=item.Quantity,AllocatedQuantity=allocated.GetValueOrDefault(item.Id)
        }).Where(x=>x.Difference!=0).ToList();
    }

    public void ReconcileToLocation(IEnumerable<SparePartLocationDiscrepancy> differences,int locationId)
    {
        using var connection=Open();using var transaction=connection.BeginTransaction();
        foreach(var item in differences)
        {
            if(item.Difference>=0)
                SetBalance(connection,transaction,item.InventoryItemId,locationId,
                    Balance(connection,transaction,item.InventoryItemId,locationId)+item.Difference);
            else
            {
                var remaining=-item.Difference;
                using var command=connection.CreateCommand();command.Transaction=transaction;
                command.CommandText="SELECT LocationId,Quantity FROM SparePartLocationBalances WHERE InventoryItemId=$item AND Quantity>0 ORDER BY CASE WHEN LocationId=$preferred THEN 0 ELSE 1 END,Quantity DESC;";
                command.Parameters.AddWithValue("$item",item.InventoryItemId);command.Parameters.AddWithValue("$preferred",locationId);
                using var reader=command.ExecuteReader();var balances=new List<(int Id,decimal Quantity)>();
                while(reader.Read())balances.Add((reader.GetInt32(0),Convert.ToDecimal(reader.GetDouble(1))));reader.Close();
                foreach(var balance in balances){var reduction=Math.Min(balance.Quantity,remaining);SetBalance(connection,transaction,item.InventoryItemId,balance.Id,balance.Quantity-reduction);remaining-=reduction;if(remaining<=0)break;}
            }
        }
        transaction.Commit();
    }

    public void EnsureInitialAllocations(IReadOnlyList<SparePartInventoryItem> items)
    {
        using var connection=Open();using var transaction=connection.BeginTransaction();
        foreach(var item in items)
        {
            using var count=connection.CreateCommand();count.Transaction=transaction;
            count.CommandText="SELECT COUNT(*) FROM SparePartLocationBalances WHERE InventoryItemId=$item;";
            count.Parameters.AddWithValue("$item",item.Id);if(Convert.ToInt32(count.ExecuteScalar())>0)continue;
            var label=string.IsNullOrWhiteSpace(item.Location)?"Non assegnata":item.Location.Trim();
            var locationId=GetOrCreateLocation(connection,transaction,label);
            using var insert=connection.CreateCommand();insert.Transaction=transaction;
            insert.CommandText="INSERT INTO SparePartLocationBalances(InventoryItemId,LocationId,Quantity) VALUES($item,$location,$quantity);";
            insert.Parameters.AddWithValue("$item",item.Id);insert.Parameters.AddWithValue("$location",locationId);
            insert.Parameters.AddWithValue("$quantity",item.Quantity);insert.ExecuteNonQuery();
        }
        transaction.Commit();
    }

    public void Transfer(int itemId,int fromLocationId,int toLocationId,decimal quantity,string reference,string notes,string operatorName)
    {
        if(fromLocationId==toLocationId)throw new InvalidOperationException("Origine e destinazione devono essere diverse.");
        if(quantity<=0)throw new InvalidOperationException("La quantità deve essere maggiore di zero.");
        using var connection=Open();using var transaction=connection.BeginTransaction();
        var available=Balance(connection,transaction,itemId,fromLocationId);
        if(available<quantity)throw new InvalidOperationException($"Disponibilità insufficiente nell'ubicazione di origine: {available:N2}.");
        SetBalance(connection,transaction,itemId,fromLocationId,available-quantity);
        SetBalance(connection,transaction,itemId,toLocationId,Balance(connection,transaction,itemId,toLocationId)+quantity);
        using var command=connection.CreateCommand();command.Transaction=transaction;command.CommandText="""
            INSERT INTO SparePartLocationTransfers
            (InventoryItemId,FromLocationId,ToLocationId,Quantity,Reference,Notes,OperatorName,CreatedAt)
            VALUES($item,$from,$to,$quantity,$reference,$notes,$operator,$created);
            """;
        command.Parameters.AddWithValue("$item",itemId);command.Parameters.AddWithValue("$from",fromLocationId);
        command.Parameters.AddWithValue("$to",toLocationId);command.Parameters.AddWithValue("$quantity",quantity);
        command.Parameters.AddWithValue("$reference",reference);command.Parameters.AddWithValue("$notes",notes);
        command.Parameters.AddWithValue("$operator",operatorName);command.Parameters.AddWithValue("$created",DateTime.Now.ToString("s"));
        command.ExecuteNonQuery();transaction.Commit();
    }

    public IReadOnlyList<SparePartLocationTransfer> GetTransfers(int limit=500)
    {
        using var connection=Open();using var command=connection.CreateCommand();command.CommandText="""
            SELECT Id,InventoryItemId,FromLocationId,ToLocationId,Quantity,Reference,Notes,OperatorName,CreatedAt
            FROM SparePartLocationTransfers ORDER BY CreatedAt DESC,Id DESC LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$limit",Math.Max(1,limit));using var reader=command.ExecuteReader();var result=new List<SparePartLocationTransfer>();
        while(reader.Read())result.Add(new SparePartLocationTransfer{Id=reader.GetInt32(0),InventoryItemId=reader.GetInt32(1),FromLocationId=reader.GetInt32(2),ToLocationId=reader.GetInt32(3),Quantity=D(reader,4),Reference=S(reader,5),Notes=S(reader,6),OperatorName=S(reader,7),CreatedAt=S(reader,8)});
        return result;
    }

    private static int GetOrCreateLocation(SqliteConnection c,SqliteTransaction t,string label)
    {
        var code=MakeCode(label);using var find=c.CreateCommand();find.Transaction=t;find.CommandText="SELECT Id FROM SparePartWarehouseLocations WHERE Code=$code;";
        find.Parameters.AddWithValue("$code",code);var existing=find.ExecuteScalar();if(existing is not null&&existing!=DBNull.Value)return Convert.ToInt32(existing);
        using var insert=c.CreateCommand();insert.Transaction=t;insert.CommandText="INSERT INTO SparePartWarehouseLocations(Code,Name,Warehouse,Aisle,Shelf,IsActive) VALUES($code,$name,$name,'','',1);SELECT last_insert_rowid();";
        insert.Parameters.AddWithValue("$code",code);insert.Parameters.AddWithValue("$name",label);return Convert.ToInt32(insert.ExecuteScalar());
    }
    private static string MakeCode(string value){var cleaned=new string(value.ToUpperInvariant().Where(char.IsLetterOrDigit).Take(12).ToArray());return string.IsNullOrWhiteSpace(cleaned)?"NONASSEGNATA":cleaned;}
    private static decimal Balance(SqliteConnection c,SqliteTransaction t,int item,int location){using var command=c.CreateCommand();command.Transaction=t;command.CommandText="SELECT Quantity FROM SparePartLocationBalances WHERE InventoryItemId=$item AND LocationId=$location;";command.Parameters.AddWithValue("$item",item);command.Parameters.AddWithValue("$location",location);var value=command.ExecuteScalar();return value is null||value==DBNull.Value?0:Convert.ToDecimal(value);}
    private static void SetBalance(SqliteConnection c,SqliteTransaction t,int item,int location,decimal quantity){using var command=c.CreateCommand();command.Transaction=t;command.CommandText="INSERT INTO SparePartLocationBalances(InventoryItemId,LocationId,Quantity) VALUES($item,$location,$quantity) ON CONFLICT(InventoryItemId,LocationId) DO UPDATE SET Quantity=$quantity;";command.Parameters.AddWithValue("$item",item);command.Parameters.AddWithValue("$location",location);command.Parameters.AddWithValue("$quantity",quantity);command.ExecuteNonQuery();}
    private SqliteConnection Open(){var c=new SqliteConnection(_connectionString);c.Open();return c;}
    private static decimal D(SqliteDataReader r,int i)=>r.IsDBNull(i)?0:Convert.ToDecimal(r.GetDouble(i));
    private static string S(SqliteDataReader r,int i)=>r.IsDBNull(i)?"":r.GetString(i);
}
