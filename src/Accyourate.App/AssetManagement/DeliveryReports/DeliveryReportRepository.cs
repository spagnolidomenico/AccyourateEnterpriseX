using Microsoft.Data.Sqlite;

namespace Accyourate.App.AssetManagement.DeliveryReports;

public sealed class DeliveryReportRepository
{
    private readonly string _databasePath;

    public DeliveryReportRepository(string? databasePath = null)
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccyourateEnterpriseX");

        Directory.CreateDirectory(folder);

        _databasePath = databasePath ?? Path.Combine(folder, "accyourate-assets.db");
        Initialize();
    }

    private string ConnectionString => $"Data Source={_databasePath}";

    public void Initialize()
    {
        using var connection = OpenConnection();

        Execute(connection, """
            CREATE TABLE IF NOT EXISTS DeliveryReports (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ReportNumber TEXT NOT NULL UNIQUE,
                AssignmentId INTEGER NOT NULL,
                AssetId INTEGER NOT NULL,
                AssetEmployeeId INTEGER NOT NULL,
                EmployeeName TEXT NOT NULL,
                AssetCode TEXT NOT NULL,
                ReportDate TEXT NOT NULL,
                Status TEXT NOT NULL,
                PdfPath TEXT,
                Notes TEXT,
                CreatedBy TEXT,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );
        """);

        Execute(connection, """
            CREATE TABLE IF NOT EXISTS DeliveryReportItems (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                DeliveryReportId INTEGER NOT NULL,
                AssetId INTEGER NOT NULL,
                AssetCode TEXT NOT NULL,
                Description TEXT NOT NULL,
                SerialNumber TEXT,
                Condition TEXT,
                Notes TEXT,
                FOREIGN KEY (DeliveryReportId) REFERENCES DeliveryReports(Id) ON DELETE CASCADE
            );
        """);

        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_DeliveryReports_ReportNumber ON DeliveryReports(ReportNumber);");
        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_DeliveryReports_AssignmentId ON DeliveryReports(AssignmentId);");
        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_DeliveryReports_AssetId ON DeliveryReports(AssetId);");
        Execute(connection, "CREATE INDEX IF NOT EXISTS IX_DeliveryReports_EmployeeName ON DeliveryReports(EmployeeName);");
    }

    public int Create(DeliveryReport report, IReadOnlyList<DeliveryReportItem> items)
    {
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();

        if (string.IsNullOrWhiteSpace(report.ReportNumber))
            report.ReportNumber = NextReportNumber(connection, transaction);

        var now = DateTime.Now.ToString("s");
        report.CreatedAt = now;
        report.UpdatedAt = now;

        int id;

        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO DeliveryReports (
                    ReportNumber, AssignmentId, AssetId, AssetEmployeeId, EmployeeName, AssetCode,
                    ReportDate, Status, PdfPath, Notes, CreatedBy, CreatedAt, UpdatedAt
                )
                VALUES (
                    $ReportNumber, $AssignmentId, $AssetId, $AssetEmployeeId, $EmployeeName, $AssetCode,
                    $ReportDate, $Status, $PdfPath, $Notes, $CreatedBy, $CreatedAt, $UpdatedAt
                );
                SELECT last_insert_rowid();
            """;

            AddReportParameters(command, report);
            id = Convert.ToInt32(command.ExecuteScalar());
        }

        foreach (var item in items)
            CreateItem(connection, transaction, id, item);

        transaction.Commit();
        return id;
    }


    public DeliveryReport? GetById(int id)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, ReportNumber, AssignmentId, AssetId, AssetEmployeeId, EmployeeName, AssetCode,
                   ReportDate, Status, PdfPath, Notes, CreatedBy, CreatedAt, UpdatedAt
            FROM DeliveryReports
            WHERE Id = $Id;
        """;
        command.Parameters.AddWithValue("$Id", id);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadReport(reader) : null;
    }

    public void UpdatePdfPath(int id, string pdfPath, string status)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE DeliveryReports
            SET PdfPath = $PdfPath,
                Status = $Status,
                UpdatedAt = $UpdatedAt
            WHERE Id = $Id;
        """;
        command.Parameters.AddWithValue("$Id", id);
        command.Parameters.AddWithValue("$PdfPath", pdfPath);
        command.Parameters.AddWithValue("$Status", status);
        command.Parameters.AddWithValue("$UpdatedAt", DateTime.Now.ToString("s"));
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<DeliveryReport> GetLatest(int limit = 50)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT Id, ReportNumber, AssignmentId, AssetId, AssetEmployeeId, EmployeeName, AssetCode,
                   ReportDate, Status, PdfPath, Notes, CreatedBy, CreatedAt, UpdatedAt
            FROM DeliveryReports
            ORDER BY ReportDate DESC
            LIMIT $limit;
        """;
        command.Parameters.AddWithValue("$limit", Math.Max(1, limit));

        using var reader = command.ExecuteReader();
        var result = new List<DeliveryReport>();

        while (reader.Read())
            result.Add(ReadReport(reader));

        return result;
    }

    public IReadOnlyList<DeliveryReport> GetByAssetId(int assetId)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT Id, ReportNumber, AssignmentId, AssetId, AssetEmployeeId, EmployeeName, AssetCode,
                   ReportDate, Status, PdfPath, Notes, CreatedBy, CreatedAt, UpdatedAt
            FROM DeliveryReports
            WHERE AssetId = $AssetId
            ORDER BY ReportDate DESC;
        """;
        command.Parameters.AddWithValue("$AssetId", assetId);

        using var reader = command.ExecuteReader();
        var result = new List<DeliveryReport>();

        while (reader.Read())
            result.Add(ReadReport(reader));

        return result;
    }

    public IReadOnlyList<DeliveryReport> GetByEmployeeName(string employeeName)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT Id, ReportNumber, AssignmentId, AssetId, AssetEmployeeId, EmployeeName, AssetCode,
                   ReportDate, Status, PdfPath, Notes, CreatedBy, CreatedAt, UpdatedAt
            FROM DeliveryReports
            WHERE EmployeeName = $EmployeeName
            ORDER BY ReportDate DESC;
        """;
        command.Parameters.AddWithValue("$EmployeeName", employeeName);

        using var reader = command.ExecuteReader();
        var result = new List<DeliveryReport>();

        while (reader.Read())
            result.Add(ReadReport(reader));

        return result;
    }

    public IReadOnlyList<DeliveryReportItem> GetItems(int deliveryReportId)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT Id, DeliveryReportId, AssetId, AssetCode, Description, SerialNumber, Condition, Notes
            FROM DeliveryReportItems
            WHERE DeliveryReportId = $DeliveryReportId
            ORDER BY Id;
        """;
        command.Parameters.AddWithValue("$DeliveryReportId", deliveryReportId);

        using var reader = command.ExecuteReader();
        var result = new List<DeliveryReportItem>();

        while (reader.Read())
        {
            result.Add(new DeliveryReportItem
            {
                Id = reader.GetInt32(0),
                DeliveryReportId = reader.GetInt32(1),
                AssetId = reader.GetInt32(2),
                AssetCode = ReadString(reader, 3),
                Description = ReadString(reader, 4),
                SerialNumber = ReadString(reader, 5),
                Condition = ReadString(reader, 6),
                Notes = ReadString(reader, 7)
            });
        }

        return result;
    }

    private static void CreateItem(SqliteConnection connection, SqliteTransaction transaction, int reportId, DeliveryReportItem item)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;

        command.CommandText = """
            INSERT INTO DeliveryReportItems (
                DeliveryReportId, AssetId, AssetCode, Description, SerialNumber, Condition, Notes
            )
            VALUES (
                $DeliveryReportId, $AssetId, $AssetCode, $Description, $SerialNumber, $Condition, $Notes
            );
        """;

        command.Parameters.AddWithValue("$DeliveryReportId", reportId);
        command.Parameters.AddWithValue("$AssetId", item.AssetId);
        command.Parameters.AddWithValue("$AssetCode", item.AssetCode);
        command.Parameters.AddWithValue("$Description", item.Description);
        command.Parameters.AddWithValue("$SerialNumber", item.SerialNumber);
        command.Parameters.AddWithValue("$Condition", item.Condition);
        command.Parameters.AddWithValue("$Notes", item.Notes);
        command.ExecuteNonQuery();
    }

    private static string NextReportNumber(SqliteConnection connection, SqliteTransaction transaction)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT IFNULL(MAX(Id), 0) + 1 FROM DeliveryReports;";
        var next = Convert.ToInt32(command.ExecuteScalar());
        return $"VRB-{DateTime.Now:yyyy}-{next:0000}";
    }

    private static void AddReportParameters(SqliteCommand command, DeliveryReport report)
    {
        command.Parameters.AddWithValue("$ReportNumber", report.ReportNumber);
        command.Parameters.AddWithValue("$AssignmentId", report.AssignmentId);
        command.Parameters.AddWithValue("$AssetId", report.AssetId);
        command.Parameters.AddWithValue("$AssetEmployeeId", report.AssetEmployeeId);
        command.Parameters.AddWithValue("$EmployeeName", report.EmployeeName);
        command.Parameters.AddWithValue("$AssetCode", report.AssetCode);
        command.Parameters.AddWithValue("$ReportDate", report.ReportDate);
        command.Parameters.AddWithValue("$Status", report.Status);
        command.Parameters.AddWithValue("$PdfPath", report.PdfPath);
        command.Parameters.AddWithValue("$Notes", report.Notes);
        command.Parameters.AddWithValue("$CreatedBy", report.CreatedBy);
        command.Parameters.AddWithValue("$CreatedAt", report.CreatedAt);
        command.Parameters.AddWithValue("$UpdatedAt", report.UpdatedAt);
    }

    private static DeliveryReport ReadReport(SqliteDataReader reader)
    {
        return new DeliveryReport
        {
            Id = reader.GetInt32(0),
            ReportNumber = ReadString(reader, 1),
            AssignmentId = reader.GetInt32(2),
            AssetId = reader.GetInt32(3),
            AssetEmployeeId = reader.GetInt32(4),
            EmployeeName = ReadString(reader, 5),
            AssetCode = ReadString(reader, 6),
            ReportDate = ReadString(reader, 7),
            Status = ReadString(reader, 8),
            PdfPath = ReadString(reader, 9),
            Notes = ReadString(reader, 10),
            CreatedBy = ReadString(reader, 11),
            CreatedAt = ReadString(reader, 12),
            UpdatedAt = ReadString(reader, 13)
        };
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();

        return connection;
    }

    private static void Execute(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static string ReadString(SqliteDataReader reader, int index)
    {
        return reader.IsDBNull(index) ? string.Empty : reader.GetString(index);
    }
}
