using Microsoft.Data.Sqlite;
using Accyourate.App.Platform.Notifications;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SupplierRmaCapaGovernanceCriticalityNotificationService
{
    private readonly string _connectionString;
    private readonly SupplierRmaCapaGovernanceDashboardService _dashboard = new();
    private readonly SupplierRmaCapaGovernanceActionService _actions = new();

    public SupplierRmaCapaGovernanceCriticalityNotificationService(string? databasePath = null)
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AccyourateEnterpriseX");
        Directory.CreateDirectory(folder);
        _connectionString = $"Data Source={databasePath ?? Path.Combine(folder, "accyourate-assets.db")}";
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS SupplierRmaCapaGovernanceCriticalityNotifications(NotificationKey TEXT PRIMARY KEY,Criticality TEXT NOT NULL,NotificationType TEXT NOT NULL,CreatedAt TEXT NOT NULL);";
        command.ExecuteNonQuery();
    }

    public int Publish(NotificationService? notifications = null)
    {
        notifications ??= new NotificationService();
        var count = 0;
        var actions = _actions.GetAll().Where(x => x.SourceType == "Criticita Governance CAPA").ToList();
        foreach (var issue in Criticalities(_dashboard.Load()).Where(x => x.Value > 0))
        {
            var related = actions.Where(x => x.SourceReference == issue.Key).OrderByDescending(x => x.Id).ToList();
            var active = related.FirstOrDefault(x => x.Status != "Completata");
            var completed = related.FirstOrDefault(x => x.Status == "Completata");
            if (active is null && completed is null)
                count += PublishOnce(notifications, issue.Key, "non-assegnata", "Criticita CAPA non presa in carico", $"{issue.Key}: {issue.Value} elementi richiedono assegnazione.", NotificationPriority.Critical, "");
            else if (active is null && completed is not null)
                count += PublishOnce(notifications, issue.Key, "chiusura-non-verificata", "Chiusura criticita CAPA non verificata", $"{issue.Key}: l'azione risulta completata ma la criticita e ancora presente.", NotificationPriority.Critical, completed.Id.ToString());
            else if (active?.IsOverdue == true)
                count += PublishOnce(notifications, issue.Key, $"scaduta-{active.Id}", "Escalation criticita CAPA", $"{issue.Key}: azione #{active.Id:D6} scaduta, responsabile {active.Owner}.", NotificationPriority.Critical, active.Id.ToString());
        }
        count += _actions.PublishAlerts(notifications);
        return count;
    }

    private int PublishOnce(NotificationService notifications, string criticality, string type, string title, string message, string priority, string payload)
    {
        var key = $"capa-criticality:{criticality}:{type}:{DateTime.Today:yyyyMMdd}";
        using var connection = Open();
        using var check = connection.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM SupplierRmaCapaGovernanceCriticalityNotifications WHERE NotificationKey=$key;";
        check.Parameters.AddWithValue("$key", key);
        if (Convert.ToInt32(check.ExecuteScalar()) > 0) return 0;
        notifications.Publish(title, message, NotificationCategory.Asset, priority, "Governance CAPA", "open-rma-capa-governance-actions", payload);
        using var insert = connection.CreateCommand();
        insert.CommandText = "INSERT INTO SupplierRmaCapaGovernanceCriticalityNotifications(NotificationKey,Criticality,NotificationType,CreatedAt) VALUES($key,$criticality,$type,$created);";
        insert.Parameters.AddWithValue("$key", key); insert.Parameters.AddWithValue("$criticality", criticality); insert.Parameters.AddWithValue("$type", type); insert.Parameters.AddWithValue("$created", DateTime.Now.ToString("s"));
        insert.ExecuteNonQuery(); return 1;
    }

    private static Dictionary<string, int> Criticalities(SupplierRmaCapaGovernanceSnapshot x) => new(StringComparer.OrdinalIgnoreCase)
    {
        ["Documenti fascicolo mancanti"] = x.MissingDocuments,
        ["Riesami fascicolo scaduti"] = x.ReviewsOverdue,
        ["Attestazioni fascicolo non valide"] = x.InvalidAttestations,
        ["Archivi attestazione mancanti"] = x.MissingAttestationArchives,
        ["Esportazioni modificate"] = x.InvalidExports,
        ["File esportazione mancanti"] = x.MissingExports,
        ["Conservazioni fascicolo scadute"] = x.RetentionOverdue,
        ["Riesami Governance scaduti"] = x.PeriodicReviewsOverdue,
        ["Attestazioni riesame non valide"] = x.InvalidPeriodicReviewAttestations,
        ["Conservazioni riesame non valide"] = x.InvalidPeriodicReviewRetentions
    };

    private SqliteConnection Open() { var connection = new SqliteConnection(_connectionString); connection.Open(); return connection; }
}
