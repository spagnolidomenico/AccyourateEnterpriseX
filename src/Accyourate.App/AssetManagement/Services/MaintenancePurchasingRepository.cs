using Microsoft.Data.Sqlite;
using Accyourate.App.AssetManagement.Models;

namespace Accyourate.App.AssetManagement.Services;

public sealed class MaintenancePurchasingRepository
{
    private readonly string _connectionString;

    public MaintenancePurchasingRepository(string? databasePath = null)
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccyourateEnterpriseX");
        Directory.CreateDirectory(folder);
        _connectionString = $"Data Source={databasePath ?? Path.Combine(folder, "accyourate-assets.db")}";
        Initialize();
    }

    private void Initialize()
    {
        using var connection = Open();
        Execute(connection, """
            CREATE TABLE IF NOT EXISTS MaintenanceSuppliers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL UNIQUE,
                Email TEXT, Phone TEXT, Notes TEXT,
                VatNumber TEXT, Address TEXT, City TEXT, ContactPerson TEXT
            );
            CREATE TABLE IF NOT EXISTS MaintenancePurchaseOrders (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                OrderNumber TEXT NOT NULL UNIQUE,
                SupplierId INTEGER NOT NULL,
                MaintenanceTicketId INTEGER NOT NULL DEFAULT 0,
                Status TEXT NOT NULL,
                OrderDate TEXT NOT NULL,
                ExpectedDate TEXT,
                ReceivedDate TEXT,
                Notes TEXT,
                PdfPath TEXT,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS MaintenancePurchaseOrderLines (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PurchaseOrderId INTEGER NOT NULL,
                PartCode TEXT,
                Description TEXT NOT NULL,
                Quantity REAL NOT NULL DEFAULT 1,
                UnitCost REAL NOT NULL DEFAULT 0,
                ReceivedQuantity REAL NOT NULL DEFAULT 0
            );
            CREATE INDEX IF NOT EXISTS IX_MaintenancePurchaseOrders_Supplier
            ON MaintenancePurchaseOrders(SupplierId);
            CREATE INDEX IF NOT EXISTS IX_MaintenancePurchaseOrderLines_Order
            ON MaintenancePurchaseOrderLines(PurchaseOrderId);
            """);
        EnsureColumn(connection, "MaintenanceSuppliers", "VatNumber", "TEXT");
        EnsureColumn(connection, "MaintenanceSuppliers", "Address", "TEXT");
        EnsureColumn(connection, "MaintenanceSuppliers", "City", "TEXT");
        EnsureColumn(connection, "MaintenanceSuppliers", "ContactPerson", "TEXT");
    }

    public int SaveSupplier(MaintenanceSupplier supplier)
    {
        using var connection = Open();
        if (supplier.Id == 0)
        {
            using var existing = connection.CreateCommand();
            existing.CommandText = "SELECT Id FROM MaintenanceSuppliers WHERE Name=$name COLLATE NOCASE LIMIT 1;";
            existing.Parameters.AddWithValue("$name", supplier.Name.Trim());
            var existingId = existing.ExecuteScalar();
            if (existingId is not null && existingId != DBNull.Value)
                supplier.Id = Convert.ToInt32(existingId);
        }
        using var command = connection.CreateCommand();
        command.CommandText = supplier.Id == 0
            ? """
              INSERT INTO MaintenanceSuppliers(Name,Email,Phone,Notes,VatNumber,Address,City,ContactPerson)
              VALUES($name,$email,$phone,$notes,$vat,$address,$city,$contact);
              SELECT last_insert_rowid();
              """
            : """
              UPDATE MaintenanceSuppliers SET Name=$name,Email=$email,Phone=$phone,Notes=$notes,
                  VatNumber=$vat,Address=$address,City=$city,ContactPerson=$contact
              WHERE Id=$id;
              SELECT $id;
              """;
        command.Parameters.AddWithValue("$id", supplier.Id);
        command.Parameters.AddWithValue("$name", supplier.Name);
        command.Parameters.AddWithValue("$email", supplier.Email);
        command.Parameters.AddWithValue("$phone", supplier.Phone);
        command.Parameters.AddWithValue("$notes", supplier.Notes);
        command.Parameters.AddWithValue("$vat", supplier.VatNumber);
        command.Parameters.AddWithValue("$address", supplier.Address);
        command.Parameters.AddWithValue("$city", supplier.City);
        command.Parameters.AddWithValue("$contact", supplier.ContactPerson);
        supplier.Id = Convert.ToInt32(command.ExecuteScalar());
        return supplier.Id;
    }

    public IReadOnlyList<MaintenanceSupplier> GetSuppliers()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id,Name,Email,Phone,Notes,VatNumber,Address,City,ContactPerson
            FROM MaintenanceSuppliers ORDER BY Name;
            """;
        using var reader = command.ExecuteReader();
        var result = new List<MaintenanceSupplier>();
        while (reader.Read())
            result.Add(new MaintenanceSupplier
            {
                Id = reader.GetInt32(0), Name = S(reader, 1), Email = S(reader, 2),
                Phone = S(reader, 3), Notes = S(reader, 4), VatNumber = S(reader, 5),
                Address = S(reader, 6), City = S(reader, 7), ContactPerson = S(reader, 8)
            });
        return result;
    }

    public int CreateOrder(MaintenancePurchaseOrder order)
    {
        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        order.OrderNumber = NextOrderNumber(connection, transaction);
        order.CreatedAt = DateTime.Now.ToString("s");
        order.UpdatedAt = order.CreatedAt;
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO MaintenancePurchaseOrders
            (OrderNumber,SupplierId,MaintenanceTicketId,Status,OrderDate,ExpectedDate,
             ReceivedDate,Notes,PdfPath,CreatedAt,UpdatedAt)
            VALUES($number,$supplier,$ticket,$status,$date,$expected,'',$notes,'',$created,$updated);
            SELECT last_insert_rowid();
            """;
        AddOrderParameters(command, order);
        order.Id = Convert.ToInt32(command.ExecuteScalar());
        foreach (var line in order.Lines)
            InsertLine(connection, transaction, order.Id, line);
        transaction.Commit();
        return order.Id;
    }

    public IReadOnlyList<MaintenancePurchaseOrder> GetOrders(int limit = 500)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = SelectOrder + " ORDER BY OrderDate DESC, Id DESC LIMIT $limit;";
        command.Parameters.AddWithValue("$limit", Math.Max(1, limit));
        using var reader = command.ExecuteReader();
        var result = new List<MaintenancePurchaseOrder>();
        while (reader.Read()) result.Add(ReadOrder(reader));
        reader.Close();
        foreach (var order in result)
            order.Lines = GetLines(connection, order.Id);
        return result;
    }

    public MaintenancePurchaseOrder? GetOrder(int id)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = SelectOrder + " WHERE Id=$id;";
        command.Parameters.AddWithValue("$id", id);
        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;
        var order = ReadOrder(reader);
        reader.Close();
        order.Lines = GetLines(connection, order.Id);
        return order;
    }

    public void SetStatus(int id, string status, string pdfPath = "")
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE MaintenancePurchaseOrders
            SET Status=$status,
                ReceivedDate=CASE WHEN $status='Ricevuto' THEN $now ELSE ReceivedDate END,
                PdfPath=CASE WHEN $pdf<>'' THEN $pdf ELSE PdfPath END,
                UpdatedAt=$now
            WHERE Id=$id;
            """;
        command.Parameters.AddWithValue("$status", status);
        command.Parameters.AddWithValue("$pdf", pdfPath);
        command.Parameters.AddWithValue("$now", DateTime.Now.ToString("s"));
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    public void SetPdfPath(int id, string path)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE MaintenancePurchaseOrders SET PdfPath=$path,UpdatedAt=$now WHERE Id=$id;";
        command.Parameters.AddWithValue("$path", path);
        command.Parameters.AddWithValue("$now", DateTime.Now.ToString("s"));
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    private static string NextOrderNumber(SqliteConnection connection, SqliteTransaction transaction)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT COALESCE(MAX(Id),0)+1 FROM MaintenancePurchaseOrders;
            """;
        var sequence = Convert.ToInt32(command.ExecuteScalar());
        return $"ODA-{DateTime.Today:yyyy}-{sequence:D6}";
    }

    private static void InsertLine(
        SqliteConnection connection,
        SqliteTransaction transaction,
        int orderId,
        MaintenancePurchaseOrderLine line)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO MaintenancePurchaseOrderLines
            (PurchaseOrderId,PartCode,Description,Quantity,UnitCost,ReceivedQuantity)
            VALUES($order,$code,$description,$quantity,$cost,$received);
            """;
        command.Parameters.AddWithValue("$order", orderId);
        command.Parameters.AddWithValue("$code", line.PartCode);
        command.Parameters.AddWithValue("$description", line.Description);
        command.Parameters.AddWithValue("$quantity", line.Quantity);
        command.Parameters.AddWithValue("$cost", line.UnitCost);
        command.Parameters.AddWithValue("$received", line.ReceivedQuantity);
        command.ExecuteNonQuery();
    }

    private static List<MaintenancePurchaseOrderLine> GetLines(SqliteConnection connection, int orderId)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id,PurchaseOrderId,PartCode,Description,Quantity,UnitCost,ReceivedQuantity
            FROM MaintenancePurchaseOrderLines WHERE PurchaseOrderId=$order ORDER BY Id;
            """;
        command.Parameters.AddWithValue("$order", orderId);
        using var reader = command.ExecuteReader();
        var result = new List<MaintenancePurchaseOrderLine>();
        while (reader.Read())
            result.Add(new MaintenancePurchaseOrderLine
            {
                Id = reader.GetInt32(0), PurchaseOrderId = reader.GetInt32(1),
                PartCode = S(reader, 2), Description = S(reader, 3),
                Quantity = D(reader, 4), UnitCost = D(reader, 5), ReceivedQuantity = D(reader, 6)
            });
        return result;
    }

    private static MaintenancePurchaseOrder ReadOrder(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(0), OrderNumber = S(reader, 1), SupplierId = reader.GetInt32(2),
        MaintenanceTicketId = reader.GetInt32(3), Status = S(reader, 4), OrderDate = S(reader, 5),
        ExpectedDate = S(reader, 6), ReceivedDate = S(reader, 7), Notes = S(reader, 8),
        PdfPath = S(reader, 9), CreatedAt = S(reader, 10), UpdatedAt = S(reader, 11)
    };

    private static void AddOrderParameters(SqliteCommand command, MaintenancePurchaseOrder order)
    {
        command.Parameters.AddWithValue("$number", order.OrderNumber);
        command.Parameters.AddWithValue("$supplier", order.SupplierId);
        command.Parameters.AddWithValue("$ticket", order.MaintenanceTicketId);
        command.Parameters.AddWithValue("$status", order.Status);
        command.Parameters.AddWithValue("$date", order.OrderDate);
        command.Parameters.AddWithValue("$expected", order.ExpectedDate);
        command.Parameters.AddWithValue("$notes", order.Notes);
        command.Parameters.AddWithValue("$created", order.CreatedAt);
        command.Parameters.AddWithValue("$updated", order.UpdatedAt);
    }

    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private static decimal D(SqliteDataReader reader, int index) =>
        reader.IsDBNull(index) ? 0 : Convert.ToDecimal(reader.GetDouble(index));
    private static string S(SqliteDataReader reader, int index) =>
        reader.IsDBNull(index) ? string.Empty : reader.GetString(index);
    private static void Execute(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
    private static void EnsureColumn(SqliteConnection connection, string table, string name, string definition)
    {
        using var check = connection.CreateCommand();
        check.CommandText = $"PRAGMA table_info({table});";
        using var reader = check.ExecuteReader();
        while (reader.Read())
            if (string.Equals(reader.GetString(1), name, StringComparison.OrdinalIgnoreCase)) return;
        reader.Close();
        using var alter = connection.CreateCommand();
        alter.CommandText = $"ALTER TABLE {table} ADD COLUMN {name} {definition};";
        alter.ExecuteNonQuery();
    }
    private const string SelectOrder = """
        SELECT Id,OrderNumber,SupplierId,MaintenanceTicketId,Status,OrderDate,
               ExpectedDate,ReceivedDate,Notes,PdfPath,CreatedAt,UpdatedAt
        FROM MaintenancePurchaseOrders
        """;
}
