namespace Accyourate.App.Core.Notifications;

public sealed class NotificationService
{
    public List<NotificationRecord> GetSystemNotifications()
    {
        return new List<NotificationRecord>
        {
            new NotificationRecord
            {
                Severity = "Info",
                Title = "Sistema notifiche pronto",
                Message = "La base del motore notifiche è stata predisposta per scadenze, manutenzioni, lavaggi e backup.",
                SourceModule = "Core",
                CreatedAt = DateTime.UtcNow.ToString("O")
            }
        };
    }
}
