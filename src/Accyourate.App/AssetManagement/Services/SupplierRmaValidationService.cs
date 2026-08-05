using Accyourate.App.AssetManagement.Models;
using Microsoft.Data.Sqlite;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SupplierRmaValidationItem
{
    public string Label { get; init; } = "";
    public bool IsValid { get; init; }
    public bool IsRequired { get; init; }
    public string Detail { get; init; } = "";
}

public sealed class SupplierRmaValidationResult
{
    public IReadOnlyList<SupplierRmaValidationItem> Items { get; init; } = Array.Empty<SupplierRmaValidationItem>();
    public bool CanClose => Items.Where(x => x.IsRequired).All(x => x.IsValid);
    public string Status => CanClose ? "Completo" : Items.Any(x => x.IsValid) ? "Da verificare" : "Incompleto";
}

public sealed class SupplierRmaDossierClosure
{
    public int Id { get; init; }
    public int RmaId { get; init; }
    public string CaseNumber { get; init; } = "";
    public string ValidationStatus { get; init; } = "";
    public string Notes { get; init; } = "";
    public string ClosedAt { get; init; } = "";
    public string ClosedBy { get; init; } = "";
}

public sealed class SupplierRmaValidationService
{
    private readonly string _connectionString;
    private readonly SupplierRmaPortalRepository _portal;

    public SupplierRmaValidationService(string? databasePath = null)
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AccyourateEnterpriseX");
        Directory.CreateDirectory(folder);
        var path = databasePath ?? Path.Combine(folder, "accyourate-assets.db");
        _connectionString = $"Data Source={path}";
        _portal = new SupplierRmaPortalRepository(path);
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS SupplierRmaDossierClosures(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                RmaId INTEGER NOT NULL,
                CaseNumber TEXT NOT NULL,
                ValidationStatus TEXT NOT NULL,
                Notes TEXT,
                ClosedAt TEXT NOT NULL,
                ClosedBy TEXT NOT NULL);
            CREATE INDEX IF NOT EXISTS IX_SupplierRmaDossierClosures_Rma ON SupplierRmaDossierClosures(RmaId,ClosedAt);
            """;
        command.ExecuteNonQuery();
    }

    public SupplierRmaValidationResult Validate(SparePartRmaCase rma, SparePartInventoryItem? item, MaintenanceSupplier? supplier)
    {
        var communications = _portal.GetAllCommunications().Where(x => x.RmaId == rma.Id).ToList();
        var attachments = _portal.GetAttachments(rma.SupplierId).Where(x => x.RmaId == rma.Id).ToList();
        var openFollowUps = communications.Count(x => x.Status == "Aperta" && !string.IsNullOrWhiteSpace(x.FollowUpDate));
        return new SupplierRmaValidationResult { Items = new List<SupplierRmaValidationItem>
        {
            Check("Pratica chiusa", rma.Status == SparePartRmaStatus.Closed, true, $"Stato attuale: {rma.Status}"),
            Check("Ricambio identificato", item is not null && rma.InventoryItemId > 0, true, item is null ? "Ricambio non trovato" : $"{item.PartCode} - {item.Description}"),
            Check("Fornitore associato", supplier is not null && rma.SupplierId > 0, true, supplier?.Name ?? "Fornitore non specificato"),
            Check("Quantità valida", rma.Quantity > 0, true, rma.Quantity.ToString("N2")),
            Check("Autorizzazione RMA", !string.IsNullOrWhiteSpace(rma.AuthorizationNumber), true, Dash(rma.AuthorizationNumber)),
            Check("Esito registrato", !string.IsNullOrWhiteSpace(rma.Outcome), true, Dash(rma.Outcome)),
            Check("Comunicazioni archiviate", communications.Count > 0, true, $"{communications.Count} comunicazioni"),
            Check("Solleciti conclusi", openFollowUps == 0, true, openFollowUps == 0 ? "Nessun sollecito aperto" : $"{openFollowUps} solleciti ancora aperti"),
            Check("Allegati disponibili", attachments.Any(x => x.IsAvailable), true, $"{attachments.Count(x => x.IsAvailable)} disponibili su {attachments.Count}"),
            Check("Tracking spedizione", !string.IsNullOrWhiteSpace(rma.TrackingNumber), false, Dash(rma.TrackingNumber)),
            Check("Note pratica", !string.IsNullOrWhiteSpace(rma.Notes), false, string.IsNullOrWhiteSpace(rma.Notes) ? "Nessuna nota" : "Presenti")
        }};
    }

    public void RecordClosure(SparePartRmaCase rma, SupplierRmaValidationResult validation, string notes, string user)
    {
        if (!validation.CanClose) throw new InvalidOperationException("Il fascicolo contiene elementi obbligatori mancanti.");
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO SupplierRmaDossierClosures(RmaId,CaseNumber,ValidationStatus,Notes,ClosedAt,ClosedBy) VALUES($rma,$case,$status,$notes,$date,$user);";
        command.Parameters.AddWithValue("$rma", rma.Id); command.Parameters.AddWithValue("$case", rma.CaseNumber);
        command.Parameters.AddWithValue("$status", validation.Status); command.Parameters.AddWithValue("$notes", notes.Trim());
        command.Parameters.AddWithValue("$date", DateTime.Now.ToString("s")); command.Parameters.AddWithValue("$user", user);
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<SupplierRmaDossierClosure> GetClosures()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id,RmaId,CaseNumber,ValidationStatus,Notes,ClosedAt,ClosedBy FROM SupplierRmaDossierClosures ORDER BY ClosedAt DESC,Id DESC;";
        using var reader = command.ExecuteReader();
        var values = new List<SupplierRmaDossierClosure>();
        while (reader.Read()) values.Add(new SupplierRmaDossierClosure
        {
            Id = reader.GetInt32(0), RmaId = reader.GetInt32(1), CaseNumber = Text(reader, 2),
            ValidationStatus = Text(reader, 3), Notes = Text(reader, 4), ClosedAt = Text(reader, 5), ClosedBy = Text(reader, 6)
        });
        return values;
    }

    public static string DossierPath(string caseNumber) => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Accyourate Enterprise X", "Fascicoli RMA", $"Fascicolo-{caseNumber}.zip");

    private static SupplierRmaValidationItem Check(string label, bool valid, bool required, string detail) => new() { Label = label, IsValid = valid, IsRequired = required, Detail = detail };
    private static string Dash(string value) => string.IsNullOrWhiteSpace(value) ? "Non presente" : value;
    private static string Text(SqliteDataReader reader, int index) => reader.IsDBNull(index) ? "" : reader.GetString(index);
    private SqliteConnection Open() { var connection = new SqliteConnection(_connectionString); connection.Open(); return connection; }
}
