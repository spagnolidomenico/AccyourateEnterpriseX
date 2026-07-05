namespace Accyourate.App.Platform.Dashboard;

public sealed class DashboardSnapshot
{
    public int Employees { get; set; }
    public int ActiveEmployees { get; set; }
    public int Assets { get; set; }
    public int AssignedAssets { get; set; }
    public int DeliveryReports { get; set; }
    public int GeneratedDeliveryReports { get; set; }
    public int Documents { get; set; }
    public int UnreadNotifications { get; set; }
    public int AuditEvents { get; set; }
    public string LastRefresh { get; set; } = DateTime.Now.ToString("s");
}
