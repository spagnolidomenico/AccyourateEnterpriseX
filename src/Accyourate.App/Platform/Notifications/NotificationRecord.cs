namespace Accyourate.App.Platform.Notifications;

public sealed class NotificationRecord
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = NotificationCategory.System;
    public string Priority { get; set; } = NotificationPriority.Info;
    public string CreatedAt { get; set; } = DateTime.Now.ToString("s");
    public string CreatedBy { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public string ReadAt { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
}
