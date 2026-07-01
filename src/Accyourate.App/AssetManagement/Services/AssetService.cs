using Microsoft.Data.Sqlite;
using Accyourate.App.AssetManagement.Models;

namespace Accyourate.App.AssetManagement.Services;

public sealed class AssetService
{
    private readonly string _databasePath;

    public AssetService(string? databasePath = null)
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccyourateEnterpriseX");

        Directory.CreateDirectory(folder);

        _databasePath = databasePath ?? Path.Combine(folder, "accyourate-assets.db");

        Initialize();
        SeedDemoData();
    }

    private string ConnectionString => $"Data Source={_databasePath}";

    public void Initialize()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        Execute(connection, """
            CREATE TABLE IF NOT EXISTS Assets (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                AssetCode TEXT NOT NULL UNIQUE,
                Category TEXT NOT NULL,
                Manufacturer TEXT NOT NULL,
                Model TEXT NOT NULL,
                SerialNumber TEXT,
                AssetTag TEXT,
                Status TEXT NOT NULL,
                PurchaseDate TEXT,
                WarrantyEndDate TEXT,
                OperatingSystem TEXT,
                BitLockerEnabled INTEGER NOT NULL DEFAULT 0,
                Notes TEXT,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );
        """);

        Execute(connection, """
            CREATE TABLE IF NOT EXISTS Employees (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FullName TEXT NOT NULL,
                Email TEXT,
                Department TEXT,
                Role TEXT,
                Site TEXT,
                IsActive INTEGER NOT NULL DEFAULT 1
            );
        """);

        Execute(connection, """
            CREATE TABLE IF NOT EXISTS AssetAssignments (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                AssetId INTEGER NOT NULL,
                EmployeeId INTEGER NOT NULL,
                AssignedAt TEXT NOT NULL,
                ReturnedAt TEXT,
                AssignedBy TEXT,
                Notes TEXT,
                Status TEXT NOT NULL,
                FOREIGN KEY (AssetId) REFERENCES Assets(Id),
                FOREIGN KEY (EmployeeId) REFERENCES Employees(Id)
            );
        """);

        Execute(connection, """
            CREATE TABLE IF NOT EXISTS MaintenanceTickets (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                AssetId INTEGER NOT NULL,
                Title TEXT NOT NULL,
                Description TEXT,
                Priority TEXT NOT NULL,
                Status TEXT NOT NULL,
                OpenedAt TEXT NOT NULL,
                ClosedAt TEXT,
                Technician TEXT,
                ResolutionNotes TEXT,
                FOREIGN KEY (AssetId) REFERENCES Assets(Id)
            );
        """);

        Execute(connection, """
            CREATE TABLE IF NOT EXISTS AssetDocuments (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                AssetId INTEGER NOT NULL,
                DocumentType TEXT,
                FileName TEXT NOT NULL,
                FilePath TEXT,
                UploadedAt TEXT NOT NULL,
                Notes TEXT,
                FOREIGN KEY (AssetId) REFERENCES Assets(Id)
            );
        """);

        Execute(connection, """
            CREATE TABLE IF NOT EXISTS AssetCredentials (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                AssetId INTEGER NOT NULL,
                CredentialType TEXT,
                Username TEXT,
                SecretReference TEXT,
                Notes TEXT,
                UpdatedAt TEXT NOT NULL,
                FOREIGN KEY (AssetId) REFERENCES Assets(Id)
            );
        """);

        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_Assets_AssetCode ON Assets(AssetCode);");
        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_Assets_Category ON Assets(Category);");
        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_Assets_Status ON Assets(Status);");
        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_Employees_FullName ON Employees(FullName);");
        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_Assignments_AssetId ON AssetAssignments(AssetId);");
        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_Assignments_EmployeeId ON AssetAssignments(EmployeeId);");
    }

    public IReadOnlyList<Asset> GetAssets()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, AssetCode, Category, Manufacturer, Model, SerialNumber, AssetTag, Status,
                   PurchaseDate, WarrantyEndDate, OperatingSystem, BitLockerEnabled, Notes, CreatedAt, UpdatedAt
            FROM Assets
            ORDER BY AssetCode;
        """;

        using var reader = command.ExecuteReader();
        var result = new List<Asset>();

        while (reader.Read())
            result.Add(ReadAsset(reader));

        return result;
    }

    public Asset? GetAssetById(int id)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, AssetCode, Category, Manufacturer, Model, SerialNumber, AssetTag, Status,
                   PurchaseDate, WarrantyEndDate, OperatingSystem, BitLockerEnabled, Notes, CreatedAt, UpdatedAt
            FROM Assets
            WHERE Id = $id;
        """;
        command.Parameters.AddWithValue("$id", id);

        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadAsset(reader) : null;
    }

    public Asset? GetAssetByCode(string assetCode)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, AssetCode, Category, Manufacturer, Model, SerialNumber, AssetTag, Status,
                   PurchaseDate, WarrantyEndDate, OperatingSystem, BitLockerEnabled, Notes, CreatedAt, UpdatedAt
            FROM Assets
            WHERE AssetCode = $assetCode;
        """;
        command.Parameters.AddWithValue("$assetCode", assetCode);

        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadAsset(reader) : null;
    }

    public IReadOnlyList<Asset> SearchAssets(string query)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, AssetCode, Category, Manufacturer, Model, SerialNumber, AssetTag, Status,
                   PurchaseDate, WarrantyEndDate, OperatingSystem, BitLockerEnabled, Notes, CreatedAt, UpdatedAt
            FROM Assets
            WHERE AssetCode LIKE $query
               OR Category LIKE $query
               OR Manufacturer LIKE $query
               OR Model LIKE $query
               OR SerialNumber LIKE $query
               OR Status LIKE $query
               OR OperatingSystem LIKE $query
            ORDER BY AssetCode;
        """;
        command.Parameters.AddWithValue("$query", $"%{query}%");

        using var reader = command.ExecuteReader();
        var result = new List<Asset>();

        while (reader.Read())
            result.Add(ReadAsset(reader));

        return result;
    }

    public int CreateAsset(Asset asset)
    {
        asset.CreatedAt = DateTime.Now.ToString("s");
        asset.UpdatedAt = DateTime.Now.ToString("s");

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Assets (
                AssetCode, Category, Manufacturer, Model, SerialNumber, AssetTag, Status,
                PurchaseDate, WarrantyEndDate, OperatingSystem, BitLockerEnabled, Notes, CreatedAt, UpdatedAt
            )
            VALUES (
                $AssetCode, $Category, $Manufacturer, $Model, $SerialNumber, $AssetTag, $Status,
                $PurchaseDate, $WarrantyEndDate, $OperatingSystem, $BitLockerEnabled, $Notes, $CreatedAt, $UpdatedAt
            );
            SELECT last_insert_rowid();
        """;

        AddAssetParameters(command, asset);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void UpdateAsset(Asset asset)
    {
        asset.UpdatedAt = DateTime.Now.ToString("s");

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Assets
            SET AssetCode = $AssetCode,
                Category = $Category,
                Manufacturer = $Manufacturer,
                Model = $Model,
                SerialNumber = $SerialNumber,
                AssetTag = $AssetTag,
                Status = $Status,
                PurchaseDate = $PurchaseDate,
                WarrantyEndDate = $WarrantyEndDate,
                OperatingSystem = $OperatingSystem,
                BitLockerEnabled = $BitLockerEnabled,
                Notes = $Notes,
                UpdatedAt = $UpdatedAt
            WHERE Id = $Id;
        """;

        command.Parameters.AddWithValue("$Id", asset.Id);
        AddAssetParameters(command, asset);
        command.ExecuteNonQuery();
    }

    public void DeleteAsset(int id)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Assets WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    public int CountAssets()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Assets;";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public IReadOnlyList<Employee> GetEmployees()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, FullName, Email, Department, Role, Site, IsActive FROM Employees ORDER BY FullName;";

        using var reader = command.ExecuteReader();
        var result = new List<Employee>();

        while (reader.Read())
        {
            result.Add(new Employee
            {
                Id = reader.GetInt32(0),
                FullName = reader.GetString(1),
                Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Department = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                Role = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                Site = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                IsActive = reader.GetInt32(6) == 1
            });
        }

        return result;
    }

    private void SeedDemoData()
    {
        if (CountAssets() > 0)
            return;

        CreateAsset(new Asset
        {
            AssetCode = "PC-001",
            Category = "Desktop PC",
            Manufacturer = "Dell",
            Model = "OptiPlex",
            SerialNumber = "DL-PC-001",
            AssetTag = "IT-0001",
            Status = "Attivo",
            PurchaseDate = "2024-01-15",
            WarrantyEndDate = "2027-01-15",
            OperatingSystem = "Windows 11 Pro",
            BitLockerEnabled = true,
            Notes = "Postazione amministrazione"
        });

        CreateAsset(new Asset
        {
            AssetCode = "NB-001",
            Category = "Notebook",
            Manufacturer = "Lenovo",
            Model = "ThinkPad",
            SerialNumber = "LN-NB-001",
            AssetTag = "IT-0002",
            Status = "Assegnato",
            PurchaseDate = "2024-03-20",
            WarrantyEndDate = "2027-03-20",
            OperatingSystem = "Windows 11 Pro",
            BitLockerEnabled = true,
            Notes = "Notebook direzione"
        });

        CreateAsset(new Asset
        {
            AssetCode = "MAC-001",
            Category = "Mac",
            Manufacturer = "Apple",
            Model = "MacBook Pro",
            SerialNumber = "AP-MAC-001",
            AssetTag = "IT-0003",
            Status = "Attivo",
            PurchaseDate = "2024-06-10",
            WarrantyEndDate = "2026-06-10",
            OperatingSystem = "macOS",
            BitLockerEnabled = false,
            Notes = "Postazione grafica"
        });

        CreateAsset(new Asset
        {
            AssetCode = "PRN-001",
            Category = "Stampante",
            Manufacturer = "HP",
            Model = "LaserJet Pro",
            SerialNumber = "HP-PRN-001",
            AssetTag = "IT-0004",
            Status = "Attivo",
            PurchaseDate = "2023-09-05",
            WarrantyEndDate = "2026-09-05",
            OperatingSystem = "Firmware HP",
            BitLockerEnabled = false,
            Notes = "Stampante ufficio"
        });

        CreateAsset(new Asset
        {
            AssetCode = "PH-001",
            Category = "Smartphone",
            Manufacturer = "Apple",
            Model = "iPhone",
            SerialNumber = "AP-PH-001",
            AssetTag = "IT-0005",
            Status = "Assegnato",
            PurchaseDate = "2025-02-01",
            WarrantyEndDate = "2027-02-01",
            OperatingSystem = "iOS",
            BitLockerEnabled = false,
            Notes = "Telefono aziendale"
        });

        SeedEmployees();
    }

    private void SeedEmployees()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        Execute(connection, """
            INSERT INTO Employees (FullName, Email, Department, Role, Site, IsActive)
            VALUES
            ('Gabriela', 'gabriela@example.local', 'Operations', 'Operatrice', 'Sede principale', 1),
            ('Domenico Spagnoli', 'domenico@example.local', 'IT', 'Administrator', 'Sede principale', 1),
            ('Amministrazione', 'admin@example.local', 'Administration', 'Office', 'Sede principale', 1);
        """);
    }

    private static Asset ReadAsset(SqliteDataReader reader)
    {
        return new Asset
        {
            Id = reader.GetInt32(0),
            AssetCode = reader.GetString(1),
            Category = reader.GetString(2),
            Manufacturer = reader.GetString(3),
            Model = reader.GetString(4),
            SerialNumber = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
            AssetTag = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
            Status = reader.GetString(7),
            PurchaseDate = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
            WarrantyEndDate = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
            OperatingSystem = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
            BitLockerEnabled = reader.GetInt32(11) == 1,
            Notes = reader.IsDBNull(12) ? string.Empty : reader.GetString(12),
            CreatedAt = reader.GetString(13),
            UpdatedAt = reader.GetString(14)
        };
    }

    private static void AddAssetParameters(SqliteCommand command, Asset asset)
    {
        command.Parameters.AddWithValue("$AssetCode", asset.AssetCode);
        command.Parameters.AddWithValue("$Category", asset.Category);
        command.Parameters.AddWithValue("$Manufacturer", asset.Manufacturer);
        command.Parameters.AddWithValue("$Model", asset.Model);
        command.Parameters.AddWithValue("$SerialNumber", asset.SerialNumber);
        command.Parameters.AddWithValue("$AssetTag", asset.AssetTag);
        command.Parameters.AddWithValue("$Status", asset.Status);
        command.Parameters.AddWithValue("$PurchaseDate", asset.PurchaseDate);
        command.Parameters.AddWithValue("$WarrantyEndDate", asset.WarrantyEndDate);
        command.Parameters.AddWithValue("$OperatingSystem", asset.OperatingSystem);
        command.Parameters.AddWithValue("$BitLockerEnabled", asset.BitLockerEnabled ? 1 : 0);
        command.Parameters.AddWithValue("$Notes", asset.Notes);
        command.Parameters.AddWithValue("$CreatedAt", asset.CreatedAt);
        command.Parameters.AddWithValue("$UpdatedAt", asset.UpdatedAt);
    }

    private static void Execute(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
