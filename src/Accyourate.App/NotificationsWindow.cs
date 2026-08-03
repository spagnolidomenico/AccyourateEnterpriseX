using Avalonia.Controls;
using Accyourate.App.Platform.Notifications;

namespace Accyourate.App;

public sealed class NotificationsWindow : Window
{
    public NotificationsWindow()
    {
        Title = "Accyourate Enterprise X - Centro notifiche";
        Width = 1080;
        Height = 720;
        MinWidth = 900;
        MinHeight = 600;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Content = new NotificationCenterView();
    }
}
