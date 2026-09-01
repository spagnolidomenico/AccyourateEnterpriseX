using Microsoft.Data.Sqlite;
using System.Text;
using Accyourate.App.Platform.Pdf;
using Accyourate.App.Platform.Settings;
using Accyourate.App.Platform.Notifications;

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
    private readonly SettingsService _settings = new();

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
        var value = Current(user);
        var latest = GetAll().FirstOrDefault();
        if (latest is not null && Same(latest, value)) return false;
        Insert(value); return true;
    }

    public bool CaptureDaily(string user)
    {
        var latest = GetAll().FirstOrDefault();
        if (latest is not null && DateTime.TryParse(latest.CapturedAt, out var captured) && captured.Date == DateTime.Today) return false;
        var current = Current(user); Insert(current);
        if (latest is not null) PublishRegression(latest, current);
        return true;
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

    public string ExportCsv()
    {
        var values = GetAll().OrderBy(x => x.Id).ToList(); if (values.Count == 0) throw new InvalidOperationException("Registra almeno una rilevazione prima dell'esportazione.");
        var builder = new StringBuilder("Data;Criticita;Avvisi;Azioni attive;Azioni scadute;Completate;Verifiche fallite;Operatore\r\n");
        foreach (var x in values) builder.AppendLine(string.Join(";", new[] { Csv(Date(x.CapturedAt)), x.CriticalCount.ToString(), x.WarningCount.ToString(), x.ActiveActions.ToString(), x.OverdueActions.ToString(), x.CompletedActions.ToString(), x.FailedVerifications.ToString(), Csv(x.CapturedBy) }));
        var path = Path.Combine(Folder(), $"Trend-Criticita-CAPA-{DateTime.Now:yyyyMMdd-HHmmss}.csv"); File.WriteAllText(path, builder.ToString(), new UTF8Encoding(true)); return path;
    }

    public string ExportPdf()
    {
        var values = GetAll(); if (values.Count == 0) throw new InvalidOperationException("Registra almeno una rilevazione prima dell'esportazione.");
        var latest = values[0]; var first = values[^1]; var settings = _settings.Load(); var template = settings.DocumentTemplate ?? new DocumentTemplateSettings();
        var document = new SimplePdfDocument { Title = "Storico e trend criticita Governance CAPA" }; Brand(document, settings, template, $"CAPA-TREND-{DateTime.Now:yyyyMMdd-HHmm}");
        document.AddTitle("Storico e trend criticita Governance CAPA"); document.AddKeyValue("Data elaborazione", DateTime.Now.ToString("dd/MM/yyyy HH:mm")); document.AddKeyValue("Operatore", Environment.UserName); document.AddKeyValue("Rilevazioni", values.Count.ToString());
        document.AddHeading("Situazione piu recente"); document.AddStatus("Esito", latest.CriticalCount == 0 ? "Conforme" : $"{latest.CriticalCount} criticita"); document.AddKeyValue("Avvisi", latest.WarningCount.ToString()); document.AddKeyValue("Azioni attive", latest.ActiveActions.ToString()); document.AddKeyValue("Azioni scadute", latest.OverdueActions.ToString()); document.AddKeyValue("Azioni completate", latest.CompletedActions.ToString()); document.AddKeyValue("Verifiche non superate", latest.FailedVerifications.ToString());
        document.AddHeading("Variazione dalla baseline"); document.AddKeyValue("Criticita", Difference(latest.CriticalCount, first.CriticalCount)); document.AddKeyValue("Avvisi", Difference(latest.WarningCount, first.WarningCount)); document.AddKeyValue("Azioni scadute", Difference(latest.OverdueActions, first.OverdueActions)); document.AddKeyValue("Azioni completate", Difference(latest.CompletedActions, first.CompletedActions));
        document.AddHeading("Rilevazioni storiche"); foreach (var x in values) { document.AddText($"{Date(x.CapturedAt)} - {x.CapturedBy}", 10); document.AddText($"Criticita {x.CriticalCount} | Avvisi {x.WarningCount} | Attive {x.ActiveActions} | Scadute {x.OverdueActions} | Completate {x.CompletedActions} | Verifiche fallite {x.FailedVerifications}", 9); }
        document.AddSignaturePair("Responsabile qualita", "Responsabile processo"); return new PdfExportService().Export(document, Folder(), $"Trend-Criticita-CAPA-{DateTime.Now:yyyyMMdd-HHmmss}");
    }

    private static bool Same(SupplierRmaCapaGovernanceCriticalityTrendPoint left, SupplierRmaCapaGovernanceCriticalityTrendPoint right) => left.CriticalCount == right.CriticalCount && left.WarningCount == right.WarningCount && left.ActiveActions == right.ActiveActions && left.OverdueActions == right.OverdueActions && left.CompletedActions == right.CompletedActions && left.FailedVerifications == right.FailedVerifications;

    private SupplierRmaCapaGovernanceCriticalityTrendPoint Current(string user)
    {
        var snapshot = _dashboard.Load(); var actions = _actions.GetAll().Where(x => x.SourceType == "Criticita Governance CAPA").ToList();
        return new SupplierRmaCapaGovernanceCriticalityTrendPoint { CapturedAt = DateTime.Now.ToString("s"), CriticalCount = snapshot.CriticalCount, WarningCount = snapshot.ReviewsDue + snapshot.RetentionDue + snapshot.PeriodicReviewRetentionsDue, ActiveActions = actions.Count(x => x.Status != "Completata"), OverdueActions = actions.Count(x => x.IsOverdue), CompletedActions = actions.Count(x => x.Status == "Completata"), FailedVerifications = actions.Sum(x => _actions.History(x.Id).Count(e => e.EventType == "Verifica non superata")), CapturedBy = user };
    }

    private void Insert(SupplierRmaCapaGovernanceCriticalityTrendPoint value)
    {
        using var connection = Open(); using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO SupplierRmaCapaGovernanceCriticalityTrend(CapturedAt,CriticalCount,WarningCount,ActiveActions,OverdueActions,CompletedActions,FailedVerifications,CapturedBy) VALUES($at,$critical,$warning,$active,$overdue,$completed,$failed,$user);";
        command.Parameters.AddWithValue("$at", value.CapturedAt); command.Parameters.AddWithValue("$critical", value.CriticalCount); command.Parameters.AddWithValue("$warning", value.WarningCount); command.Parameters.AddWithValue("$active", value.ActiveActions); command.Parameters.AddWithValue("$overdue", value.OverdueActions); command.Parameters.AddWithValue("$completed", value.CompletedActions); command.Parameters.AddWithValue("$failed", value.FailedVerifications); command.Parameters.AddWithValue("$user", value.CapturedBy); command.ExecuteNonQuery();
    }

    private static void PublishRegression(SupplierRmaCapaGovernanceCriticalityTrendPoint previous, SupplierRmaCapaGovernanceCriticalityTrendPoint current)
    {
        var criticalDelta = current.CriticalCount - previous.CriticalCount; var overdueDelta = current.OverdueActions - previous.OverdueActions; var failedDelta = current.FailedVerifications - previous.FailedVerifications;
        if (criticalDelta <= 0 && overdueDelta <= 0 && failedDelta <= 0) return;
        var changes = new List<string>(); if (criticalDelta > 0) changes.Add($"criticita +{criticalDelta}"); if (overdueDelta > 0) changes.Add($"azioni scadute +{overdueDelta}"); if (failedDelta > 0) changes.Add($"verifiche non superate +{failedDelta}");
        new NotificationService().Publish("Peggioramento trend Governance CAPA", $"La rilevazione giornaliera segnala: {string.Join(", ", changes)}.", NotificationCategory.Asset, criticalDelta > 0 || overdueDelta > 0 ? NotificationPriority.Critical : NotificationPriority.High, "Automazione Governance CAPA", "open-rma-capa-criticality-trend", current.Id.ToString());
    }

    private static string Difference(int current, int baseline) { var value = current - baseline; return value == 0 ? "Nessuna variazione" : value > 0 ? $"+{value}" : value.ToString(); }
    private static string Date(string value) => DateTime.TryParse(value, out var date) ? date.ToString("dd/MM/yyyy HH:mm") : value;
    private static string Csv(string value) => $"\"{(value ?? "").Replace("\"", "\"\"")}\"";
    private static string Folder() { var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Accyourate Enterprise X", "Governance CAPA", "Trend criticita"); Directory.CreateDirectory(folder); return folder; }
    private static void Brand(SimplePdfDocument document, ApplicationSettings settings, DocumentTemplateSettings template, string code)
    {
        document.Branding.CompanyName = string.IsNullOrWhiteSpace(settings.Company.LegalName) ? settings.Company.CompanyName : settings.Company.LegalName;
        document.Branding.CompanyDetailLines.AddRange(new[] { settings.Company.Address, string.Join(" ", new[] { settings.Company.City, settings.Company.Province }.Where(x => !string.IsNullOrWhiteSpace(x))), string.Join(" - ", new[] { settings.Company.Phone, settings.Company.Email }.Where(x => !string.IsNullOrWhiteSpace(x))) }.Where(x => !string.IsNullOrWhiteSpace(x)));
        document.Branding.HeaderLayout = template.HeaderLayout; document.Branding.LogoPath = settings.Company.LogoPath; document.Branding.LogoSize = template.LogoSize; document.Branding.LogoPosition = template.LogoPosition; document.Branding.PrimaryColor = template.PrimaryColor; document.Branding.DocumentLabel = "TREND CRITICITA GOVERNANCE CAPA"; document.Branding.DocumentCode = code; document.Branding.DocumentVersion = template.DocumentVersion; document.Branding.FooterText = template.FooterText; document.Branding.ConfidentialityText = template.ConfidentialityText; document.Branding.ShowLogo = template.ShowLogo; document.Branding.ShowCompanyDetails = template.ShowCompanyDetails; document.Branding.ShowDocumentMetadata = template.ShowDocumentMetadata; document.Branding.ShowFooter = template.ShowFooter; document.Branding.ShowPageNumber = template.ShowPageNumber; document.Branding.ShowPrintTimestamp = template.ShowPrintTimestamp;
    }

    private SqliteConnection Open() { var connection = new SqliteConnection(_connectionString); connection.Open(); return connection; }
}
