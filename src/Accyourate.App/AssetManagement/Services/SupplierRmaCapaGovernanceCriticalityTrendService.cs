using Microsoft.Data.Sqlite;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SupplierRmaCapaGovernanceCriticalityTrendPoint
{
    public int Id { get; init; }
    public string CapturedAt { get; init; } = "";
    public int CriticalCount { get; init; }
    public int WarningCount { get; init; }
    public int ActiveActions { get; init; }
    public int OverdueActions { get; init; }
    public int CompletedActions { get; init; }
    public int FailedVerifications { get; init; }
    public string CapturedBy { get; init; } = "";
}

public sealed class SupplierRmaCapaGovernanceCriticalityTrendService
{
    private readonly string _connectionString;
    private readonly SupplierRmaCapaGovernanceDashboardService _dashboard = new();
    private readonly SupplierRmaCapaGovernanceActionService _actions = new();

    public SupplierRmaCapaGovernanceCriticalityTrendService(string? databasePath = null)
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AccyourateEnterpriseX");
        Directory.CreateDirectory(folder);
        _connectionString = $"Data Source={databasePath ?? Path.Combine(folder, "accyourate-assets.db")}";
        using var connection = Open(); using var command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS SupplierRmaCapaGovernanceCriticalityTrend(Id INTEGER PRIMARY KEY AUTOINCREMENT,CapturedAt TEXT NOT NULL,CriticalCount INTEGER NOT NULL,WarningCount INTEGER NOT NULL,ActiveActions INTEGER NOT NULL,OverdueActions INTEGER NOT NULL,CompletedActions INTEGER NOT NULL,FailedVerifications INTEGER NOT NULL,CapturedBy TEXT NOT NULL);";
        command.ExecuteNonQuery();
    }

    public bool Capture(string user)
    {
        var snapshot = _dashboard.Load();
        var actions = _actions.GetAll().Where(x => x.SourceType == "Criticita Governance CAPA").ToList();
        var failed = actions.Sum(x => _actions.History(x.Id).Count(e => e.EventType == "Verifica non superata"));
        var value = new SupplierRmaCapaGovernanceCriticalityTrendPoint
        {
            CapturedAt = DateTime.Now.ToString("s"), CriticalCount = snapshot.CriticalCount,
            WarningCount = snapshot.ReviewsDue + snapshot.RetentionDue + snapshot.PeriodicReviewRetentionsDue,
            ActiveActions = actions.Count(x => x.Status != "Completata"), OverdueActions = actions.Count(x => x.IsOverdue),
            CompletedActions = actions.Count(x => x.Status == "Completata"), FailedVerifications = failed, CapturedBy = user
        };
        var latest = GetAll().FirstOrDefault();
        if (latest is not null && Same(latest, value)) return false;
        using var connection = Open(); using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO SupplierRmaCapaGovernanceCriticalityTrend(CapturedAt,CriticalCount,WarningCount,ActiveActions,OverdueActions,CompletedActions,FailedVerifications,CapturedBy) VALUES($at,$critical,$warning,$active,$overdue,$completed,$failed,$user);SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$at", value.CapturedAt); command.Parameters.AddWithValue("$critical", value.CriticalCount); command.Parameters.AddWithValue("$warning", value.WarningCount); command.Parameters.AddWithValue("$active", value.ActiveActions); command.Parameters.AddWithValue("$overdue", value.OverdueActions); command.Parameters.AddWithValue("$completed", value.CompletedActions); command.Parameters.AddWithValue("$failed", value.FailedVerifications); command.Parameters.AddWithValue("$user", user);
        command.ExecuteScalar(); return true;
    }

    public int RemoveConsecutiveDuplicates()
    {
        var values = GetAll().OrderBy(x => x.Id).ToList(); var remove = new List<int>(); SupplierRmaCapaGovernanceCriticalityTrendPoint? previous = null;
        foreach (var item in values) { if (previous is not null && Same(previous, item)) remove.Add(item.Id); else previous = item; }
        if (remove.Count == 0) return 0;
        using var connection = Open(); using var transaction = connection.BeginTransaction();
        foreach (var id in remove) { using var command = connection.CreateCommand(); command.Transaction = transaction; command.CommandText = "DELETE FROM SupplierRmaCapaGovernanceCriticalityTrend WHERE Id=$id;"; command.Parameters.AddWithValue("$id", id); command.ExecuteNonQuery(); }
        transaction.Commit(); return remove.Count;
    }

    public IReadOnlyList<SupplierRmaCapaGovernanceCriticalityTrendPoint> GetAll()
    {
        using var connection = Open(); using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id,CapturedAt,CriticalCount,WarningCount,ActiveActions,OverdueActions,CompletedActions,FailedVerifications,CapturedBy FROM SupplierRmaCapaGovernanceCriticalityTrend ORDER BY Id DESC;";
        using var reader = command.ExecuteReader(); var values = new List<SupplierRmaCapaGovernanceCriticalityTrendPoint>();
        while (reader.Read()) values.Add(new SupplierRmaCapaGovernanceCriticalityTrendPoint { Id = reader.GetInt32(0), CapturedAt = reader.GetString(1), CriticalCount = reader.GetInt32(2), WarningCount = reader.GetInt32(3), ActiveActions = reader.GetInt32(4), OverdueActions = reader.GetInt32(5), CompletedActions = reader.GetInt32(6), FailedVerifications = reader.GetInt32(7), CapturedBy = reader.GetString(8) });
        return values;
    }

    private static bool Same(SupplierRmaCapaGovernanceCriticalityTrendPoint left, SupplierRmaCapaGovernanceCriticalityTrendPoint right) => left.CriticalCount == right.CriticalCount && left.WarningCount == right.WarningCount && left.ActiveActions == right.ActiveActions && left.OverdueActions == right.OverdueActions && left.CompletedActions == right.CompletedActions && left.FailedVerifications == right.FailedVerifications;

    private SqliteConnection Open() { var connection = new SqliteConnection(_connectionString); connection.Open(); return connection; }
}
