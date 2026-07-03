using Microsoft.Data.Sqlite;

namespace Accyourate.App.HumanResources.Database;

public sealed class HrDatabase
{
    private readonly string _databasePath;

    public HrDatabase(string? databasePath = null)
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccyourateEnterpriseX");

        Directory.CreateDirectory(folder);

        _databasePath = databasePath ?? Path.Combine(folder, "accyourate-hr.db");
        Initialize();
    }

    public string DatabasePath => _databasePath;
    public string ConnectionString => $"Data Source={_databasePath}";

    public SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();

        return connection;
    }

    private void Initialize()
    {
        using var connection = OpenConnection();

        Execute(connection, """
            CREATE TABLE IF NOT EXISTS Sites (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Code TEXT NOT NULL UNIQUE,
                Name TEXT NOT NULL,
                Address TEXT,
                City TEXT,
                Province TEXT,
                Country TEXT,
                IsMain INTEGER NOT NULL DEFAULT 0,
                IsActive INTEGER NOT NULL DEFAULT 1,
                Notes TEXT
            );
        """);

        Execute(connection, """
            CREATE TABLE IF NOT EXISTS Departments (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Code TEXT NOT NULL UNIQUE,
                Name TEXT NOT NULL,
                SiteId INTEGER NOT NULL,
                ManagerId INTEGER,
                IsActive INTEGER NOT NULL DEFAULT 1,
                Notes TEXT,
                FOREIGN KEY (SiteId) REFERENCES Sites(Id)
            );
        """);

        Execute(connection, """
            CREATE TABLE IF NOT EXISTS Roles (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Code TEXT NOT NULL UNIQUE,
                Name TEXT NOT NULL,
                Area TEXT,
                IsActive INTEGER NOT NULL DEFAULT 1,
                Notes TEXT
            );
        """);

        Execute(connection, """
            CREATE TABLE IF NOT EXISTS Employees (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                EmployeeCode TEXT NOT NULL UNIQUE,
                FirstName TEXT NOT NULL,
                LastName TEXT NOT NULL,
                Email TEXT,
                Phone TEXT,
                RoleId INTEGER NOT NULL,
                DepartmentId INTEGER NOT NULL,
                SiteId INTEGER NOT NULL,
                ManagerId INTEGER,
                EmploymentStatus TEXT NOT NULL,
                HireDate TEXT,
                TerminationDate TEXT,
                Notes TEXT,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                FOREIGN KEY (RoleId) REFERENCES Roles(Id),
                FOREIGN KEY (DepartmentId) REFERENCES Departments(Id),
                FOREIGN KEY (SiteId) REFERENCES Sites(Id),
                FOREIGN KEY (ManagerId) REFERENCES Employees(Id)
            );
        """);

        Execute(connection, """
            CREATE TABLE IF NOT EXISTS EmploymentContracts (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                EmployeeId INTEGER NOT NULL,
                ContractType TEXT NOT NULL,
                StartDate TEXT,
                EndDate TEXT,
                JobTitle TEXT,
                Level TEXT,
                Status TEXT,
                Notes TEXT,
                FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE CASCADE
            );
        """);

        Execute(connection, """
            CREATE TABLE IF NOT EXISTS EmployeeDocuments (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                EmployeeId INTEGER NOT NULL,
                DocumentType TEXT,
                Title TEXT NOT NULL,
                FilePath TEXT,
                ExpirationDate TEXT,
                UploadedAt TEXT,
                UploadedBy TEXT,
                Notes TEXT,
                FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE CASCADE
            );
        """);

        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_Employees_FullName ON Employees(LastName, FirstName);");
        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_Employees_Email ON Employees(Email);");
        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_Employees_Status ON Employees(EmploymentStatus);");
        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_Contracts_EmployeeId ON EmploymentContracts(EmployeeId);");
        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_Documents_EmployeeId ON EmployeeDocuments(EmployeeId);");

        Seed(connection);
    }

    private static void Seed(SqliteConnection connection)
    {
        using var count = connection.CreateCommand();
        count.CommandText = "SELECT COUNT(*) FROM Sites;";
        if (Convert.ToInt32(count.ExecuteScalar()) > 0)
            return;

        Execute(connection, """
            INSERT INTO Sites (Code, Name, Address, City, Province, Country, IsMain, IsActive, Notes)
            VALUES ('HQ', 'Sede principale', '', '', '', 'Italia', 1, 1, 'Seed iniziale HR');
        """);

        Execute(connection, """
            INSERT INTO Departments (Code, Name, SiteId, IsActive, Notes)
            VALUES ('ADM', 'Amministrazione', 1, 1, 'Seed iniziale HR');
        """);

        Execute(connection, """
            INSERT INTO Roles (Code, Name, Area, IsActive, Notes)
            VALUES ('ADMIN', 'Administrator', 'Management', 1, 'Seed iniziale HR');
        """);
    }

    private static void Execute(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
