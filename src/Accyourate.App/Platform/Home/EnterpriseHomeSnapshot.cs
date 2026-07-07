namespace Accyourate.App.Platform.Home;

public sealed class EnterpriseHomeSnapshot
{
    public int Employees { get; set; }
    public int Assets { get; set; }
    public int Documents { get; set; }
    public int DeliveryReports { get; set; }
    public int UnreadNotifications { get; set; }
    public int BackupCount { get; set; }
    public string LastBackup { get; set; } = "Non disponibile";
    public string Version { get; set; } = "0.9.0 RC1";
    public string UpdateStatus { get; set; } = "OK";
    public string DatabaseStatus { get; set; } = "SQLite";
}
