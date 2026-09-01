using Microsoft.Data.Sqlite;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SupplierRmaCapaCriticalityAssignmentRule
{
    public string Criticality { get; init; } = "";
    public string DefaultOwner { get; init; } = "";
    public string Priority { get; init; } = "Alta";
    public int DueDays { get; init; } = 14;
    public string UpdatedAt { get; init; } = "";
    public string UpdatedBy { get; init; } = "";
}

public sealed class SupplierRmaCapaCriticalityAssignmentRuleService
{
    private readonly string _connectionString;
    public SupplierRmaCapaCriticalityAssignmentRuleService(string? databasePath = null)
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AccyourateEnterpriseX"); Directory.CreateDirectory(folder); _connectionString = $"Data Source={databasePath ?? Path.Combine(folder, "accyourate-assets.db")}";
        using var connection = Open(); using var command = connection.CreateCommand(); command.CommandText = "CREATE TABLE IF NOT EXISTS SupplierRmaCapaCriticalityAssignmentRules(Criticality TEXT PRIMARY KEY,DefaultOwner TEXT NOT NULL,Priority TEXT NOT NULL,DueDays INTEGER NOT NULL,UpdatedAt TEXT NOT NULL,UpdatedBy TEXT NOT NULL);"; command.ExecuteNonQuery(); Seed(connection);
    }

    public IReadOnlyList<SupplierRmaCapaCriticalityAssignmentRule> GetAll()
    {
        using var connection = Open(); using var command = connection.CreateCommand(); command.CommandText = "SELECT Criticality,DefaultOwner,Priority,DueDays,UpdatedAt,UpdatedBy FROM SupplierRmaCapaCriticalityAssignmentRules ORDER BY Criticality;"; using var reader = command.ExecuteReader(); var values = new List<SupplierRmaCapaCriticalityAssignmentRule>(); while (reader.Read()) values.Add(new SupplierRmaCapaCriticalityAssignmentRule { Criticality = reader.GetString(0), DefaultOwner = reader.GetString(1), Priority = reader.GetString(2), DueDays = reader.GetInt32(3), UpdatedAt = reader.GetString(4), UpdatedBy = reader.GetString(5) }); return values;
    }

    public SupplierRmaCapaCriticalityAssignmentRule? Get(string criticality) => GetAll().FirstOrDefault(x => x.Criticality == criticality);

    public void Save(string criticality, string owner, string priority, int dueDays, string user)
    {
        if (string.IsNullOrWhiteSpace(owner)) throw new InvalidOperationException("Il responsabile predefinito e obbligatorio."); if (priority is not ("Bassa" or "Media" or "Alta" or "Critica")) throw new InvalidOperationException("Priorita non valida."); if (dueDays < 1 || dueDays > 365) throw new InvalidOperationException("I giorni di scadenza devono essere compresi tra 1 e 365.");
        using var connection = Open(); using var command = connection.CreateCommand(); command.CommandText = "INSERT INTO SupplierRmaCapaCriticalityAssignmentRules(Criticality,DefaultOwner,Priority,DueDays,UpdatedAt,UpdatedBy) VALUES($c,$o,$p,$d,$at,$u) ON CONFLICT(Criticality) DO UPDATE SET DefaultOwner=$o,Priority=$p,DueDays=$d,UpdatedAt=$at,UpdatedBy=$u;"; command.Parameters.AddWithValue("$c", criticality); command.Parameters.AddWithValue("$o", owner.Trim()); command.Parameters.AddWithValue("$p", priority); command.Parameters.AddWithValue("$d", dueDays); command.Parameters.AddWithValue("$at", DateTime.Now.ToString("s")); command.Parameters.AddWithValue("$u", user); command.ExecuteNonQuery();
    }

    private void Seed(SqliteConnection connection)
    {
        var critical = new[] { "Documenti fascicolo mancanti", "Riesami fascicolo scaduti", "Attestazioni fascicolo non valide", "Archivi attestazione mancanti", "Esportazioni modificate", "File esportazione mancanti", "Conservazioni fascicolo scadute", "Riesami Governance scaduti", "Attestazioni riesame non valide", "Conservazioni riesame non valide" };
        var warnings = new[] { "Riesami fascicolo in scadenza", "Conservazioni fascicolo in scadenza", "Conservazioni riesame da gestire" };
        foreach (var title in critical) InsertSeed(connection, title, "Critica", 7); foreach (var title in warnings) InsertSeed(connection, title, "Alta", 14);
    }
    private static void InsertSeed(SqliteConnection connection, string title, string priority, int days) { using var command = connection.CreateCommand(); command.CommandText = "INSERT OR IGNORE INTO SupplierRmaCapaCriticalityAssignmentRules(Criticality,DefaultOwner,Priority,DueDays,UpdatedAt,UpdatedBy) VALUES($c,$o,$p,$d,$at,'Sistema');"; command.Parameters.AddWithValue("$c", title); command.Parameters.AddWithValue("$o", Environment.UserName); command.Parameters.AddWithValue("$p", priority); command.Parameters.AddWithValue("$d", days); command.Parameters.AddWithValue("$at", DateTime.Now.ToString("s")); command.ExecuteNonQuery(); }
    private SqliteConnection Open() { var connection = new SqliteConnection(_connectionString); connection.Open(); return connection; }
}
