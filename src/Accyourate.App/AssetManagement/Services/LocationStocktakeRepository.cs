using Microsoft.Data.Sqlite;
using Accyourate.App.AssetManagement.Models;

namespace Accyourate.App.AssetManagement.Services;

public sealed class LocationStocktakeRepository
{
    private readonly string _connectionString;
    public LocationStocktakeRepository(string? databasePath=null)
    {
        var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"AccyourateEnterpriseX");
        Directory.CreateDirectory(folder);_connectionString=$"Data Source={databasePath??Path.Combine(folder,"accyourate-assets.db")}";
        using var connection=Open();using var command=connection.CreateCommand();command.CommandText="""
            CREATE TABLE IF NOT EXISTS LocationStocktakes(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SessionNumber TEXT NOT NULL UNIQUE,
                LocationId INTEGER NOT NULL,
                Status TEXT NOT NULL,
                OperatorName TEXT,
                CreatedAt TEXT NOT NULL,
                ClosedAt TEXT
            );
            CREATE TABLE IF NOT EXISTS LocationStocktakeLines(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                StocktakeId INTEGER NOT NULL,
                InventoryItemId INTEGER NOT NULL,
                PartCode TEXT NOT NULL,
                Description TEXT NOT NULL,
                ExpectedQuantity REAL NOT NULL,
                CountedQuantity REAL,
                UnitCost REAL NOT NULL DEFAULT 0,
                Notes TEXT
            );
            """;
        command.ExecuteNonQuery();
    }

    public int Create(int locationId,string operatorName,IReadOnlyList<SparePartLocationBalance> balances,IReadOnlyDictionary<int,SparePartInventoryItem> items)
    {
        var local=balances.Where(x=>x.LocationId==locationId&&x.Quantity!=0).ToList();
        if(local.Count==0)throw new InvalidOperationException("L'ubicazione non contiene ricambi da inventariare.");
        using var connection=Open();using var transaction=connection.BeginTransaction();
        using var open=connection.CreateCommand();open.Transaction=transaction;open.CommandText="SELECT COUNT(*) FROM LocationStocktakes WHERE LocationId=$location AND Status<>$closed;";
        open.Parameters.AddWithValue("$location",locationId);open.Parameters.AddWithValue("$closed",StocktakeStatus.Closed);
        if(Convert.ToInt32(open.ExecuteScalar())>0)throw new InvalidOperationException("Esiste già una sessione aperta per questa ubicazione.");
        var number=NextNumber(connection,transaction);var created=DateTime.Now.ToString("s");
        using var insert=connection.CreateCommand();insert.Transaction=transaction;insert.CommandText="""
            INSERT INTO LocationStocktakes(SessionNumber,LocationId,Status,OperatorName,CreatedAt,ClosedAt)
            VALUES($number,$location,$status,$operator,$created,'');SELECT last_insert_rowid();
            """;
        insert.Parameters.AddWithValue("$number",number);insert.Parameters.AddWithValue("$location",locationId);
        insert.Parameters.AddWithValue("$status",StocktakeStatus.Open);insert.Parameters.AddWithValue("$operator",operatorName);
        insert.Parameters.AddWithValue("$created",created);var id=Convert.ToInt32(insert.ExecuteScalar());
        foreach(var balance in local)
        {
            if(!items.TryGetValue(balance.InventoryItemId,out var item))continue;
            using var line=connection.CreateCommand();line.Transaction=transaction;line.CommandText="""
                INSERT INTO LocationStocktakeLines(StocktakeId,InventoryItemId,PartCode,Description,ExpectedQuantity,CountedQuantity,UnitCost,Notes)
                VALUES($session,$item,$code,$description,$expected,NULL,$cost,'');
                """;
            line.Parameters.AddWithValue("$session",id);line.Parameters.AddWithValue("$item",item.Id);line.Parameters.AddWithValue("$code",item.PartCode);
            line.Parameters.AddWithValue("$description",item.Description);line.Parameters.AddWithValue("$expected",balance.Quantity);
            line.Parameters.AddWithValue("$cost",item.AverageUnitCost);line.ExecuteNonQuery();
        }
        transaction.Commit();return id;
    }

    public IReadOnlyList<LocationStocktake> GetAll()
    {
        using var connection=Open();using var command=connection.CreateCommand();
        command.CommandText="SELECT Id,SessionNumber,LocationId,Status,OperatorName,CreatedAt,ClosedAt FROM LocationStocktakes ORDER BY CreatedAt DESC,Id DESC;";
        using var reader=command.ExecuteReader();var result=new List<LocationStocktake>();while(reader.Read())result.Add(Read(reader));reader.Close();
        foreach(var session in result)session.Lines=Lines(connection,session.Id);return result;
    }
    public LocationStocktake Get(int id)
    {
        using var connection=Open();using var command=connection.CreateCommand();command.CommandText="SELECT Id,SessionNumber,LocationId,Status,OperatorName,CreatedAt,ClosedAt FROM LocationStocktakes WHERE Id=$id;";
        command.Parameters.AddWithValue("$id",id);using var reader=command.ExecuteReader();if(!reader.Read())throw new InvalidOperationException("Sessione non trovata.");var result=Read(reader);reader.Close();result.Lines=Lines(connection,id);return result;
    }
    public void SaveCounts(int id,IEnumerable<LocationStocktakeLine> lines)
    {
        using var connection=Open();using var transaction=connection.BeginTransaction();
        foreach(var line in lines){using var command=connection.CreateCommand();command.Transaction=transaction;command.CommandText="UPDATE LocationStocktakeLines SET CountedQuantity=$counted,Notes=$notes WHERE Id=$id AND StocktakeId=$session;";command.Parameters.AddWithValue("$counted",line.CountedQuantity.HasValue?line.CountedQuantity.Value:DBNull.Value);command.Parameters.AddWithValue("$notes",line.Notes);command.Parameters.AddWithValue("$id",line.Id);command.Parameters.AddWithValue("$session",id);command.ExecuteNonQuery();}
        using var status=connection.CreateCommand();status.Transaction=transaction;status.CommandText="UPDATE LocationStocktakes SET Status=$review WHERE Id=$id AND Status<>$closed;";status.Parameters.AddWithValue("$review",StocktakeStatus.Review);status.Parameters.AddWithValue("$closed",StocktakeStatus.Closed);status.Parameters.AddWithValue("$id",id);status.ExecuteNonQuery();transaction.Commit();
    }
    public void Close(int id,SparePartLocationsRepository locations,SparePartsInventoryRepository inventory)
    {
        var session=Get(id);if(session.Status==StocktakeStatus.Closed)throw new InvalidOperationException("Sessione già chiusa.");
        if(session.Lines.Any(x=>!x.CountedQuantity.HasValue))throw new InvalidOperationException("Completa tutti i conteggi.");
        var itemMap=inventory.GetItems().ToDictionary(x=>x.Id);
        foreach(var line in session.Lines.Where(x=>x.Difference!=0))
        {
            locations.SetStocktakeBalance(line.InventoryItemId,session.LocationId,line.CountedQuantity!.Value);
            if(itemMap.TryGetValue(line.InventoryItemId,out var item))inventory.Adjust(item.Id,Math.Max(0,item.Quantity+line.Difference),$"Inventario ubicazione {session.SessionNumber}");
        }
        using var connection=Open();using var command=connection.CreateCommand();command.CommandText="UPDATE LocationStocktakes SET Status=$status,ClosedAt=$closed WHERE Id=$id;";
        command.Parameters.AddWithValue("$status",StocktakeStatus.Closed);command.Parameters.AddWithValue("$closed",DateTime.Now.ToString("s"));command.Parameters.AddWithValue("$id",id);command.ExecuteNonQuery();
    }
    private static List<LocationStocktakeLine> Lines(SqliteConnection c,int id){using var command=c.CreateCommand();command.CommandText="SELECT Id,StocktakeId,InventoryItemId,PartCode,Description,ExpectedQuantity,CountedQuantity,UnitCost,Notes FROM LocationStocktakeLines WHERE StocktakeId=$id ORDER BY PartCode;";command.Parameters.AddWithValue("$id",id);using var r=command.ExecuteReader();var list=new List<LocationStocktakeLine>();while(r.Read())list.Add(new LocationStocktakeLine{Id=r.GetInt32(0),StocktakeId=r.GetInt32(1),InventoryItemId=r.GetInt32(2),PartCode=S(r,3),Description=S(r,4),ExpectedQuantity=D(r,5),CountedQuantity=r.IsDBNull(6)?null:D(r,6),UnitCost=D(r,7),Notes=S(r,8)});return list;}
    private static LocationStocktake Read(SqliteDataReader r)=>new(){Id=r.GetInt32(0),SessionNumber=S(r,1),LocationId=r.GetInt32(2),Status=S(r,3),OperatorName=S(r,4),CreatedAt=S(r,5),ClosedAt=S(r,6)};
    private static string NextNumber(SqliteConnection c,SqliteTransaction t){using var command=c.CreateCommand();command.Transaction=t;command.CommandText="SELECT COALESCE(MAX(Id),0)+1 FROM LocationStocktakes;";return $"INVL-{DateTime.Today:yyyy}-{Convert.ToInt32(command.ExecuteScalar()):D6}";}
    private SqliteConnection Open(){var c=new SqliteConnection(_connectionString);c.Open();return c;}private static decimal D(SqliteDataReader r,int i)=>r.IsDBNull(i)?0:Convert.ToDecimal(r.GetDouble(i));private static string S(SqliteDataReader r,int i)=>r.IsDBNull(i)?"":r.GetString(i);
}
