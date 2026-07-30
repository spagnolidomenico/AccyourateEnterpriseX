using Microsoft.Data.Sqlite;
using Accyourate.App.AssetManagement.Models;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SparePartsInventoryRepository
{
    private readonly string _connectionString;

    public SparePartsInventoryRepository(string? databasePath = null)
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AccyourateEnterpriseX");
        Directory.CreateDirectory(folder);
        _connectionString = $"Data Source={databasePath ?? Path.Combine(folder, "accyourate-assets.db")}";
        Initialize();
    }

    private void Initialize()
    {
        using var connection = Open();
        Execute(connection, """
            CREATE TABLE IF NOT EXISTS SparePartsInventory (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PartCode TEXT NOT NULL UNIQUE,
                Description TEXT NOT NULL,
                Supplier TEXT,
                Location TEXT,
                Quantity REAL NOT NULL DEFAULT 0,
                MinimumQuantity REAL NOT NULL DEFAULT 0,
                AverageUnitCost REAL NOT NULL DEFAULT 0,
                UpdatedAt TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS SparePartsInventoryMovements (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                InventoryItemId INTEGER NOT NULL,
                MovementType TEXT NOT NULL,
                Quantity REAL NOT NULL,
                UnitCost REAL NOT NULL DEFAULT 0,
                Reference TEXT,
                Notes TEXT,
                CreatedAt TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS IX_SparePartsMovements_Item
            ON SparePartsInventoryMovements(InventoryItemId);
            """);
    }

    public int SaveItem(SparePartInventoryItem item)
    {
        using var connection = Open();
        if (item.Id == 0)
        {
            using var existing = connection.CreateCommand();
            existing.CommandText = "SELECT Id FROM SparePartsInventory WHERE PartCode=$code COLLATE NOCASE LIMIT 1;";
            existing.Parameters.AddWithValue("$code", item.PartCode.Trim());
            var value = existing.ExecuteScalar();
            if (value is not null && value != DBNull.Value) item.Id = Convert.ToInt32(value);
        }
        item.UpdatedAt = DateTime.Now.ToString("s");
        using var command = connection.CreateCommand();
        command.CommandText = item.Id == 0
            ? """
              INSERT INTO SparePartsInventory
              (PartCode,Description,Supplier,Location,Quantity,MinimumQuantity,AverageUnitCost,UpdatedAt)
              VALUES($code,$description,$supplier,$location,$quantity,$minimum,$cost,$updated);
              SELECT last_insert_rowid();
              """
            : """
              UPDATE SparePartsInventory SET PartCode=$code,Description=$description,Supplier=$supplier,
                  Location=$location,MinimumQuantity=$minimum,AverageUnitCost=$cost,UpdatedAt=$updated
              WHERE Id=$id;
              SELECT $id;
              """;
        AddItem(command, item);
        item.Id = Convert.ToInt32(command.ExecuteScalar());
        return item.Id;
    }

    public void Receive(string code, string description, string supplier, decimal quantity, decimal unitCost, string reference)
    {
        if (quantity <= 0) return;
        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        var item = GetByCode(connection, transaction, code);
        if (item is null)
        {
            item = new SparePartInventoryItem
            {
                PartCode = code, Description = description, Supplier = supplier,
                Quantity = 0, AverageUnitCost = unitCost, UpdatedAt = DateTime.Now.ToString("s")
            };
            using var insert = connection.CreateCommand();
            insert.Transaction = transaction;
            insert.CommandText = """
                INSERT INTO SparePartsInventory
                (PartCode,Description,Supplier,Location,Quantity,MinimumQuantity,AverageUnitCost,UpdatedAt)
                VALUES($code,$description,$supplier,'',0,0,$cost,$updated);
                SELECT last_insert_rowid();
                """;
            AddItem(insert, item);
            item.Id = Convert.ToInt32(insert.ExecuteScalar());
        }
        var newQuantity = item.Quantity + quantity;
        var average = newQuantity <= 0 ? unitCost :
            ((item.Quantity * item.AverageUnitCost) + (quantity * unitCost)) / newQuantity;
        UpdateStock(connection, transaction, item.Id, newQuantity, average);
        InsertMovement(connection, transaction, item.Id, "Carico", quantity, unitCost, reference, "Ricezione ordine");
        transaction.Commit();
    }

    public bool Consume(string code, decimal quantity, string reference, string notes = "")
    {
        if (quantity <= 0) return false;
        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        var item = GetByCode(connection, transaction, code);
        if (item is null || item.Quantity < quantity) return false;
        UpdateStock(connection, transaction, item.Id, item.Quantity - quantity, item.AverageUnitCost);
        InsertMovement(connection, transaction, item.Id, "Scarico", -quantity, item.AverageUnitCost, reference, notes);
        transaction.Commit();
        return true;
    }

    public void Adjust(int itemId, decimal newQuantity, string notes)
    {
        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        var item = GetById(connection, transaction, itemId) ?? throw new InvalidOperationException("Ricambio non trovato.");
        var delta = newQuantity - item.Quantity;
        UpdateStock(connection, transaction, item.Id, Math.Max(0, newQuantity), item.AverageUnitCost);
        InsertMovement(connection, transaction, item.Id, "Rettifica", delta, item.AverageUnitCost, "Rettifica manuale", notes);
        transaction.Commit();
    }

    public IReadOnlyList<SparePartInventoryItem> GetItems()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = SelectItem + " ORDER BY PartCode;";
        using var reader = command.ExecuteReader();
        var result = new List<SparePartInventoryItem>();
        while (reader.Read()) result.Add(ReadItem(reader));
        return result;
    }

    public IReadOnlyList<SparePartInventoryMovement> GetMovements(int itemId, int limit = 100)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id,InventoryItemId,MovementType,Quantity,UnitCost,Reference,Notes,CreatedAt
            FROM SparePartsInventoryMovements WHERE InventoryItemId=$item
            ORDER BY CreatedAt DESC,Id DESC LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$item", itemId);
        command.Parameters.AddWithValue("$limit", Math.Max(1, limit));
        using var reader = command.ExecuteReader();
        var result = new List<SparePartInventoryMovement>();
        while (reader.Read())
            result.Add(new SparePartInventoryMovement
            {
                Id=reader.GetInt32(0),InventoryItemId=reader.GetInt32(1),MovementType=S(reader,2),
                Quantity=D(reader,3),UnitCost=D(reader,4),Reference=S(reader,5),Notes=S(reader,6),CreatedAt=S(reader,7)
            });
        return result;
    }

    private static SparePartInventoryItem? GetByCode(SqliteConnection c, SqliteTransaction t, string code)
    {
        using var cmd=c.CreateCommand();cmd.Transaction=t;cmd.CommandText=SelectItem+" WHERE PartCode=$code COLLATE NOCASE;";
        cmd.Parameters.AddWithValue("$code",code);using var r=cmd.ExecuteReader();return r.Read()?ReadItem(r):null;
    }
    private static SparePartInventoryItem? GetById(SqliteConnection c, SqliteTransaction t, int id)
    {
        using var cmd=c.CreateCommand();cmd.Transaction=t;cmd.CommandText=SelectItem+" WHERE Id=$id;";
        cmd.Parameters.AddWithValue("$id",id);using var r=cmd.ExecuteReader();return r.Read()?ReadItem(r):null;
    }
    private static void UpdateStock(SqliteConnection c,SqliteTransaction t,int id,decimal quantity,decimal cost)
    {
        using var cmd=c.CreateCommand();cmd.Transaction=t;cmd.CommandText="UPDATE SparePartsInventory SET Quantity=$quantity,AverageUnitCost=$cost,UpdatedAt=$updated WHERE Id=$id;";
        cmd.Parameters.AddWithValue("$quantity",quantity);cmd.Parameters.AddWithValue("$cost",cost);cmd.Parameters.AddWithValue("$updated",DateTime.Now.ToString("s"));cmd.Parameters.AddWithValue("$id",id);cmd.ExecuteNonQuery();
    }
    private static void InsertMovement(SqliteConnection c,SqliteTransaction t,int item,string type,decimal quantity,decimal cost,string reference,string notes)
    {
        using var cmd=c.CreateCommand();cmd.Transaction=t;cmd.CommandText="INSERT INTO SparePartsInventoryMovements(InventoryItemId,MovementType,Quantity,UnitCost,Reference,Notes,CreatedAt) VALUES($item,$type,$quantity,$cost,$reference,$notes,$created);";
        cmd.Parameters.AddWithValue("$item",item);cmd.Parameters.AddWithValue("$type",type);cmd.Parameters.AddWithValue("$quantity",quantity);cmd.Parameters.AddWithValue("$cost",cost);cmd.Parameters.AddWithValue("$reference",reference);cmd.Parameters.AddWithValue("$notes",notes);cmd.Parameters.AddWithValue("$created",DateTime.Now.ToString("s"));cmd.ExecuteNonQuery();
    }
    private static void AddItem(SqliteCommand cmd,SparePartInventoryItem item)
    {
        cmd.Parameters.AddWithValue("$id",item.Id);cmd.Parameters.AddWithValue("$code",item.PartCode);cmd.Parameters.AddWithValue("$description",item.Description);cmd.Parameters.AddWithValue("$supplier",item.Supplier);cmd.Parameters.AddWithValue("$location",item.Location);cmd.Parameters.AddWithValue("$quantity",item.Quantity);cmd.Parameters.AddWithValue("$minimum",item.MinimumQuantity);cmd.Parameters.AddWithValue("$cost",item.AverageUnitCost);cmd.Parameters.AddWithValue("$updated",item.UpdatedAt);
    }
    private static SparePartInventoryItem ReadItem(SqliteDataReader r)=>new(){Id=r.GetInt32(0),PartCode=S(r,1),Description=S(r,2),Supplier=S(r,3),Location=S(r,4),Quantity=D(r,5),MinimumQuantity=D(r,6),AverageUnitCost=D(r,7),UpdatedAt=S(r,8)};
    private SqliteConnection Open(){var c=new SqliteConnection(_connectionString);c.Open();return c;}
    private static decimal D(SqliteDataReader r,int i)=>r.IsDBNull(i)?0:Convert.ToDecimal(r.GetDouble(i));
    private static string S(SqliteDataReader r,int i)=>r.IsDBNull(i)?"":r.GetString(i);
    private static void Execute(SqliteConnection c,string sql){using var cmd=c.CreateCommand();cmd.CommandText=sql;cmd.ExecuteNonQuery();}
    private const string SelectItem="SELECT Id,PartCode,Description,Supplier,Location,Quantity,MinimumQuantity,AverageUnitCost,UpdatedAt FROM SparePartsInventory";
}
