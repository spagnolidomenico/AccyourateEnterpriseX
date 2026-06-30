namespace Accyourate.App.Core.Notifications;

public sealed class NotificationRecord
{
    public string Severity { get; set; } = "Info";
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string SourceModule { get; set; } = "";
    public string CreatedAt { get; set; } = "";
}
