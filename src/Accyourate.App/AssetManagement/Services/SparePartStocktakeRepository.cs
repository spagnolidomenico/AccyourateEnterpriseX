using Microsoft.Data.Sqlite;
using Accyourate.App.AssetManagement.Models;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SparePartStocktakeRepository
{
    private readonly string _connectionString;
    public SparePartStocktakeRepository(string? databasePath=null)
    {
        var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"AccyourateEnterpriseX");
        Directory.CreateDirectory(folder);_connectionString=$"Data Source={databasePath??Path.Combine(folder,"accyourate-assets.db")}";
        using var connection=Open();using var command=connection.CreateCommand();
        command.CommandText="""
            CREATE TABLE IF NOT EXISTS SparePartStocktakes(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SessionNumber TEXT NOT NULL UNIQUE,
                Description TEXT,
                Status TEXT NOT NULL,
                OperatorName TEXT,
                CreatedAt TEXT NOT NULL,
                ClosedAt TEXT
            );
            CREATE TABLE IF NOT EXISTS SparePartStocktakeLines(
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
            CREATE INDEX IF NOT EXISTS IX_StocktakeLines_Session ON SparePartStocktakeLines(StocktakeId);
            """;
        command.ExecuteNonQuery();
    }

    public int Create(string description,string operatorName,IReadOnlyList<SparePartInventoryItem> items)
    {
        if(items.Count==0)throw new InvalidOperationException("Il magazzino non contiene ricambi da inventariare.");
        using var connection=Open();using var transaction=connection.BeginTransaction();
        var session=new SparePartStocktake{SessionNumber=NextNumber(connection,transaction),Description=description,OperatorName=operatorName,CreatedAt=DateTime.Now.ToString("s")};
        using(var command=connection.CreateCommand())
        {
            command.Transaction=transaction;command.CommandText="""
                INSERT INTO SparePartStocktakes(SessionNumber,Description,Status,OperatorName,CreatedAt,ClosedAt)
                VALUES($number,$description,$status,$operator,$created,'');SELECT last_insert_rowid();
                """;
            command.Parameters.AddWithValue("$number",session.SessionNumber);command.Parameters.AddWithValue("$description",session.Description);
            command.Parameters.AddWithValue("$status",session.Status);command.Parameters.AddWithValue("$operator",session.OperatorName);
            command.Parameters.AddWithValue("$created",session.CreatedAt);session.Id=Convert.ToInt32(command.ExecuteScalar());
        }
        foreach(var item in items)
        {
            using var line=connection.CreateCommand();line.Transaction=transaction;line.CommandText="""
                INSERT INTO SparePartStocktakeLines
                (StocktakeId,InventoryItemId,PartCode,Description,ExpectedQuantity,CountedQuantity,UnitCost,Notes)
                VALUES($session,$item,$code,$description,$expected,NULL,$cost,'');
                """;
            line.Parameters.AddWithValue("$session",session.Id);line.Parameters.AddWithValue("$item",item.Id);
            line.Parameters.AddWithValue("$code",item.PartCode);line.Parameters.AddWithValue("$description",item.Description);
            line.Parameters.AddWithValue("$expected",item.Quantity);line.Parameters.AddWithValue("$cost",item.AverageUnitCost);line.ExecuteNonQuery();
        }
        transaction.Commit();return session.Id;
    }

    public IReadOnlyList<SparePartStocktake> GetAll()
    {
        using var connection=Open();using var command=connection.CreateCommand();
        command.CommandText="SELECT Id,SessionNumber,Description,Status,OperatorName,CreatedAt,ClosedAt FROM SparePartStocktakes ORDER BY CreatedAt DESC,Id DESC;";
        using var reader=command.ExecuteReader();var result=new List<SparePartStocktake>();
        while(reader.Read())result.Add(Read(reader));reader.Close();
        foreach(var session in result)session.Lines=GetLines(connection,session.Id);
        return result;
    }

    public SparePartStocktake Get(int id)
    {
        using var connection=Open();using var command=connection.CreateCommand();
        command.CommandText="SELECT Id,SessionNumber,Description,Status,OperatorName,CreatedAt,ClosedAt FROM SparePartStocktakes WHERE Id=$id;";
        command.Parameters.AddWithValue("$id",id);using var reader=command.ExecuteReader();
        if(!reader.Read())throw new InvalidOperationException("Sessione inventariale non trovata.");
        var session=Read(reader);reader.Close();session.Lines=GetLines(connection,id);return session;
    }

    public void SaveCounts(int sessionId,IEnumerable<SparePartStocktakeLine> lines)
    {
        using var connection=Open();using var transaction=connection.BeginTransaction();
        foreach(var item in lines)
        {
            using var command=connection.CreateCommand();command.Transaction=transaction;
            command.CommandText="UPDATE SparePartStocktakeLines SET CountedQuantity=$counted,Notes=$notes WHERE Id=$id AND StocktakeId=$session;";
            command.Parameters.AddWithValue("$counted",item.CountedQuantity.HasValue?item.CountedQuantity.Value:DBNull.Value);
            command.Parameters.AddWithValue("$notes",item.Notes);command.Parameters.AddWithValue("$id",item.Id);
            command.Parameters.AddWithValue("$session",sessionId);command.ExecuteNonQuery();
        }
        using var status=connection.CreateCommand();status.Transaction=transaction;
        status.CommandText="UPDATE SparePartStocktakes SET Status=$status WHERE Id=$id AND Status<>$closed;";
        status.Parameters.AddWithValue("$status",StocktakeStatus.Review);status.Parameters.AddWithValue("$closed",StocktakeStatus.Closed);
        status.Parameters.AddWithValue("$id",sessionId);status.ExecuteNonQuery();transaction.Commit();
    }

    public void Close(int sessionId,SparePartsInventoryRepository inventory)
    {
        var session=Get(sessionId);
        if(session.Status==StocktakeStatus.Closed)throw new InvalidOperationException("La sessione è già chiusa.");
        if(session.Lines.Any(x=>!x.CountedQuantity.HasValue))throw new InvalidOperationException("Completa il conteggio di tutte le righe.");
        foreach(var line in session.Lines.Where(x=>x.Difference!=0))
            inventory.Adjust(line.InventoryItemId,line.CountedQuantity!.Value,$"Riconciliazione {session.SessionNumber}");
        using var connection=Open();using var command=connection.CreateCommand();
        command.CommandText="UPDATE SparePartStocktakes SET Status=$status,ClosedAt=$closed WHERE Id=$id;";
        command.Parameters.AddWithValue("$status",StocktakeStatus.Closed);command.Parameters.AddWithValue("$closed",DateTime.Now.ToString("s"));
        command.Parameters.AddWithValue("$id",sessionId);command.ExecuteNonQuery();
    }

    private static List<SparePartStocktakeLine> GetLines(SqliteConnection connection,int sessionId)
    {
        using var command=connection.CreateCommand();command.CommandText="""
            SELECT Id,StocktakeId,InventoryItemId,PartCode,Description,ExpectedQuantity,CountedQuantity,UnitCost,Notes
            FROM SparePartStocktakeLines WHERE StocktakeId=$session ORDER BY PartCode;
            """;
        command.Parameters.AddWithValue("$session",sessionId);using var reader=command.ExecuteReader();var result=new List<SparePartStocktakeLine>();
        while(reader.Read())result.Add(new SparePartStocktakeLine{Id=reader.GetInt32(0),StocktakeId=reader.GetInt32(1),InventoryItemId=reader.GetInt32(2),PartCode=S(reader,3),Description=S(reader,4),ExpectedQuantity=D(reader,5),CountedQuantity=reader.IsDBNull(6)?null:D(reader,6),UnitCost=D(reader,7),Notes=S(reader,8)});
        return result;
    }
    private static SparePartStocktake Read(SqliteDataReader r)=>new(){Id=r.GetInt32(0),SessionNumber=S(r,1),Description=S(r,2),Status=S(r,3),OperatorName=S(r,4),CreatedAt=S(r,5),ClosedAt=S(r,6)};
    private static string NextNumber(SqliteConnection c,SqliteTransaction t){using var command=c.CreateCommand();command.Transaction=t;command.CommandText="SELECT COALESCE(MAX(Id),0)+1 FROM SparePartStocktakes;";return $"INV-{DateTime.Today:yyyy}-{Convert.ToInt32(command.ExecuteScalar()):D6}";}
    private SqliteConnection Open(){var c=new SqliteConnection(_connectionString);c.Open();return c;}
    private static decimal D(SqliteDataReader r,int i)=>r.IsDBNull(i)?0:Convert.ToDecimal(r.GetDouble(i));
    private static string S(SqliteDataReader r,int i)=>r.IsDBNull(i)?"":r.GetString(i);
}
