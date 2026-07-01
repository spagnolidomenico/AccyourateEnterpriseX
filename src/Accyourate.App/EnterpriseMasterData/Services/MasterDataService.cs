using Microsoft.Data.Sqlite;
using Accyourate.App.EnterpriseMasterData.Models;

namespace Accyourate.App.EnterpriseMasterData.Services;

public sealed class MasterDataService
{
    private readonly string _databasePath;

    public MasterDataService(string? databasePath = null)
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccyourateEnterpriseX");

        Directory.CreateDirectory(folder);

        _databasePath = databasePath ?? Path.Combine(folder, "accyourate-master-data.db");

        Initialize();
        SeedDemoData();
    }

    private string ConnectionString => $"Data Source={_databasePath}";

    public void Initialize()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        Execute(connection, """
            CREATE TABLE IF NOT EXISTS Companies (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                VatNumber TEXT,
                FiscalCode TEXT,
                Address TEXT,
                City TEXT,
                Province TEXT,
                Country TEXT,
                Email TEXT,
                Phone TEXT,
                Website TEXT,
                Notes TEXT
            );
        """);

        Execute(connection, """
            CREATE TABLE IF NOT EXISTS Sites (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CompanyId INTEGER NOT NULL,
                Name TEXT NOT NULL,
                Address TEXT,
                City TEXT,
                Province TEXT,
                Country TEXT,
                IsMainSite INTEGER NOT NULL DEFAULT 0,
                Notes TEXT,
                FOREIGN KEY (CompanyId) REFERENCES Companies(Id)
            );
        """);

        Execute(connection, """
            CREATE TABLE IF NOT EXISTS Departments (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT,
                SiteId INTEGER NOT NULL,
                ManagerEmployeeId INTEGER NOT NULL DEFAULT 0,
                FOREIGN KEY (SiteId) REFERENCES Sites(Id)
            );
        """);

        Execute(connection, """
            CREATE TABLE IF NOT EXISTS Employees (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FullName TEXT NOT NULL,
                Email TEXT,
                Phone TEXT,
                Role TEXT,
                DepartmentId INTEGER NOT NULL DEFAULT 0,
                SiteId INTEGER NOT NULL DEFAULT 0,
                IsActive INTEGER NOT NULL DEFAULT 1,
                Notes TEXT,
                FOREIGN KEY (DepartmentId) REFERENCES Departments(Id),
                FOREIGN KEY (SiteId) REFERENCES Sites(Id)
            );
        """);

        Execute(connection, """
            CREATE TABLE IF NOT EXISTS Suppliers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                VatNumber TEXT,
                ContactName TEXT,
                Email TEXT,
                Phone TEXT,
                Category TEXT,
                Notes TEXT
            );
        """);

        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_Companies_Name ON Companies(Name);");
        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_Sites_Name ON Sites(Name);");
        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_Departments_Name ON Departments(Name);");
        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_Employees_FullName ON Employees(FullName);");
        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_Suppliers_Name ON Suppliers(Name);");
    }

    public IReadOnlyList<Company> GetCompanies()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Name, VatNumber, FiscalCode, Address, City, Province, Country, Email, Phone, Website, Notes
            FROM Companies
            ORDER BY Name;
        """;

        using var reader = command.ExecuteReader();
        var result = new List<Company>();

        while (reader.Read())
        {
            result.Add(new Company
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                VatNumber = ReadString(reader, 2),
                FiscalCode = ReadString(reader, 3),
                Address = ReadString(reader, 4),
                City = ReadString(reader, 5),
                Province = ReadString(reader, 6),
                Country = ReadString(reader, 7),
                Email = ReadString(reader, 8),
                Phone = ReadString(reader, 9),
                Website = ReadString(reader, 10),
                Notes = ReadString(reader, 11)
            });
        }

        return result;
    }

    public IReadOnlyList<Site> GetSites()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, CompanyId, Name, Address, City, Province, Country, IsMainSite, Notes
            FROM Sites
            ORDER BY IsMainSite DESC, Name;
        """;

        using var reader = command.ExecuteReader();
        var result = new List<Site>();

        while (reader.Read())
        {
            result.Add(new Site
            {
                Id = reader.GetInt32(0),
                CompanyId = reader.GetInt32(1),
                Name = reader.GetString(2),
                Address = ReadString(reader, 3),
                City = ReadString(reader, 4),
                Province = ReadString(reader, 5),
                Country = ReadString(reader, 6),
                IsMainSite = reader.GetInt32(7) == 1,
                Notes = ReadString(reader, 8)
            });
        }

        return result;
    }

    public IReadOnlyList<Department> GetDepartments()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Name, Description, SiteId, ManagerEmployeeId
            FROM Departments
            ORDER BY Name;
        """;

        using var reader = command.ExecuteReader();
        var result = new List<Department>();

        while (reader.Read())
        {
            result.Add(new Department
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = ReadString(reader, 2),
                SiteId = reader.GetInt32(3),
                ManagerEmployeeId = reader.GetInt32(4)
            });
        }

        return result;
    }

    public IReadOnlyList<EmployeeMasterData> GetEmployees()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, FullName, Email, Phone, Role, DepartmentId, SiteId, IsActive, Notes
            FROM Employees
            ORDER BY FullName;
        """;

        using var reader = command.ExecuteReader();
        var result = new List<EmployeeMasterData>();

        while (reader.Read())
        {
            result.Add(new EmployeeMasterData
            {
                Id = reader.GetInt32(0),
                FullName = reader.GetString(1),
                Email = ReadString(reader, 2),
                Phone = ReadString(reader, 3),
                Role = ReadString(reader, 4),
                DepartmentId = reader.GetInt32(5),
                SiteId = reader.GetInt32(6),
                IsActive = reader.GetInt32(7) == 1,
                Notes = ReadString(reader, 8)
            });
        }

        return result;
    }

    public IReadOnlyList<Supplier> GetSuppliers()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Name, VatNumber, ContactName, Email, Phone, Category, Notes
            FROM Suppliers
            ORDER BY Name;
        """;

        using var reader = command.ExecuteReader();
        var result = new List<Supplier>();

        while (reader.Read())
        {
            result.Add(new Supplier
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                VatNumber = ReadString(reader, 2),
                ContactName = ReadString(reader, 3),
                Email = ReadString(reader, 4),
                Phone = ReadString(reader, 5),
                Category = ReadString(reader, 6),
                Notes = ReadString(reader, 7)
            });
        }

        return result;
    }

    public int CountCompanies() => Count("Companies");
    public int CountSites() => Count("Sites");
    public int CountDepartments() => Count("Departments");
    public int CountEmployees() => Count("Employees");
    public int CountSuppliers() => Count("Suppliers");

    private int Count(string table)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {table};";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private void SeedDemoData()
    {
        if (CountCompanies() > 0)
            return;

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        Execute(connection, """
            INSERT INTO Companies (Name, VatNumber, FiscalCode, Address, City, Province, Country, Email, Phone, Website, Notes)
            VALUES ('Accyourate Group', '', '', 'Sede principale', 'L''Aquila', 'AQ', 'Italia', 'info@accyourate.local', '', '', 'Azienda demo principale');
        """);

        Execute(connection, """
            INSERT INTO Sites (CompanyId, Name, Address, City, Province, Country, IsMainSite, Notes)
            VALUES
            (1, 'Sede principale', 'Sede principale', 'L''Aquila', 'AQ', 'Italia', 1, 'Sede demo'),
            (1, 'Sede operativa', 'Sede operativa', 'L''Aquila', 'AQ', 'Italia', 0, 'Sede secondaria demo');
        """);

        Execute(connection, """
            INSERT INTO Departments (Name, Description, SiteId, ManagerEmployeeId)
            VALUES
            ('IT', 'Gestione sistemi, asset e sicurezza', 1, 0),
            ('Amministrazione', 'Ufficio amministrativo', 1, 0),
            ('Operations', 'Area operativa', 1, 0),
            ('Medical R&D', 'Ricerca e sviluppo medicale', 2, 0);
        """);

        Execute(connection, """
            INSERT INTO Employees (FullName, Email, Phone, Role, DepartmentId, SiteId, IsActive, Notes)
            VALUES
            ('Domenico Spagnoli', 'domenico@accyourate.local', '', 'Administrator', 1, 1, 1, 'Utente demo'),
            ('Gabriela', 'gabriela@accyourate.local', '', 'Operatrice', 3, 1, 1, 'Utente demo'),
            ('Amministrazione', 'admin@accyourate.local', '', 'Office', 2, 1, 1, 'Utente demo');
        """);

        Execute(connection, """
            INSERT INTO Suppliers (Name, VatNumber, ContactName, Email, Phone, Category, Notes)
            VALUES
            ('Dell Italia', '', '', 'support@dell.example', '', 'Hardware', 'Fornitore demo'),
            ('Apple Business', '', '', 'business@apple.example', '', 'Hardware', 'Fornitore demo'),
            ('HP Partner', '', '', 'partner@hp.example', '', 'Stampanti', 'Fornitore demo'),
            ('Assistenza IT Esterna', '', '', 'service@example.local', '', 'Servizi IT', 'Fornitore demo');
        """);
    }

    private static string ReadString(SqliteDataReader reader, int index)
    {
        return reader.IsDBNull(index) ? string.Empty : reader.GetString(index);
    }

    private static void Execute(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
