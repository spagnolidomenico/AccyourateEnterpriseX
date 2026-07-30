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
        EnsureColumn(connection, "SparePartsInventoryMovements", "BalanceBefore", "REAL NOT NULL DEFAULT 0");
        EnsureColumn(connection, "SparePartsInventoryMovements", "BalanceAfter", "REAL NOT NULL DEFAULT 0");
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
        InsertMovement(connection, transaction, item.Id, "Carico - Acquisto", quantity, unitCost, reference, "Ricezione ordine", item.Quantity, newQuantity);
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
        InsertMovement(connection, transaction, item.Id, "Scarico - Consumo", -quantity, item.AverageUnitCost, reference, notes, item.Quantity, item.Quantity - quantity);
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
        InsertMovement(connection, transaction, item.Id, "Rettifica", delta, item.AverageUnitCost, "Rettifica manuale", notes, item.Quantity, Math.Max(0, newQuantity));
        transaction.Commit();
    }

    public void RegisterManualMovement(int itemId, bool inbound, decimal quantity, decimal unitCost, string reason, string reference, string notes)
    {
        if (quantity <= 0) throw new InvalidOperationException("La quantità deve essere maggiore di zero.");
        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        var item = GetById(connection, transaction, itemId) ?? throw new InvalidOperationException("Ricambio non trovato.");
        if (!inbound && item.Quantity < quantity)
            throw new InvalidOperationException($"Giacenza insufficiente. Disponibili: {item.Quantity:N2}.");

        var before = item.Quantity;
        var after = inbound ? before + quantity : before - quantity;
        var cost = item.AverageUnitCost;
        if (inbound)
        {
            var effectiveCost = unitCost > 0 ? unitCost : item.AverageUnitCost;
            cost = after <= 0 ? effectiveCost :
                ((before * item.AverageUnitCost) + (quantity * effectiveCost)) / after;
            unitCost = effectiveCost;
        }
        else unitCost = item.AverageUnitCost;

        UpdateStock(connection, transaction, item.Id, after, cost);
        var direction = inbound ? "Carico" : "Scarico";
        InsertMovement(connection, transaction, item.Id, $"{direction} - {reason}", inbound ? quantity : -quantity,
            unitCost, reference, notes, before, after);
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
        => GetMovementsCore(itemId, limit);

    public IReadOnlyList<SparePartInventoryMovement> GetAllMovements(int limit = 1000)
        => GetMovementsCore(null, limit);

    private IReadOnlyList<SparePartInventoryMovement> GetMovementsCore(int? itemId, int limit)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id,InventoryItemId,MovementType,Quantity,UnitCost,Reference,Notes,CreatedAt,
                   BalanceBefore,BalanceAfter
            FROM SparePartsInventoryMovements
            WHERE ($item IS NULL OR InventoryItemId=$item)
            ORDER BY CreatedAt DESC,Id DESC LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$item", itemId.HasValue ? itemId.Value : DBNull.Value);
        command.Parameters.AddWithValue("$limit", Math.Max(1, limit));
        using var reader = command.ExecuteReader();
        var result = new List<SparePartInventoryMovement>();
        while (reader.Read())
            result.Add(new SparePartInventoryMovement
            {
                Id=reader.GetInt32(0),InventoryItemId=reader.GetInt32(1),MovementType=S(reader,2),
                Quantity=D(reader,3),UnitCost=D(reader,4),Reference=S(reader,5),Notes=S(reader,6),CreatedAt=S(reader,7),
                BalanceBefore=D(reader,8),BalanceAfter=D(reader,9)
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
    private static void InsertMovement(SqliteConnection c,SqliteTransaction t,int item,string type,decimal quantity,decimal cost,string reference,string notes,decimal before,decimal after)
    {
        using var cmd=c.CreateCommand();cmd.Transaction=t;cmd.CommandText="INSERT INTO SparePartsInventoryMovements(InventoryItemId,MovementType,Quantity,UnitCost,Reference,Notes,CreatedAt,BalanceBefore,BalanceAfter) VALUES($item,$type,$quantity,$cost,$reference,$notes,$created,$before,$after);";
        cmd.Parameters.AddWithValue("$item",item);cmd.Parameters.AddWithValue("$type",type);cmd.Parameters.AddWithValue("$quantity",quantity);cmd.Parameters.AddWithValue("$cost",cost);cmd.Parameters.AddWithValue("$reference",reference);cmd.Parameters.AddWithValue("$notes",notes);cmd.Parameters.AddWithValue("$created",DateTime.Now.ToString("s"));cmd.Parameters.AddWithValue("$before",before);cmd.Parameters.AddWithValue("$after",after);cmd.ExecuteNonQuery();
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
    private static void EnsureColumn(SqliteConnection connection,string table,string column,string definition)
    {
        using var check=connection.CreateCommand();check.CommandText=$"PRAGMA table_info({table});";
        using var reader=check.ExecuteReader();var found=false;
        while(reader.Read())if(string.Equals(reader.GetString(1),column,StringComparison.OrdinalIgnoreCase)){found=true;break;}
        reader.Close();if(found)return;
        using var alter=connection.CreateCommand();alter.CommandText=$"ALTER TABLE {table} ADD COLUMN {column} {definition};";alter.ExecuteNonQuery();
    }
    private const string SelectItem="SELECT Id,PartCode,Description,Supplier,Location,Quantity,MinimumQuantity,AverageUnitCost,UpdatedAt FROM SparePartsInventory";
}
