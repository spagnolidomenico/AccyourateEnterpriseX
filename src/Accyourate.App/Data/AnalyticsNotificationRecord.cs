namespace Accyourate.App.Data;

public sealed class AnalyticsNotificationRecord
{
    public string Severity { get; set; } = "Info";
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string Source { get; set; } = "";
}
