using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Core.Notifications;
using Accyourate.App.Shared.UI;

namespace Accyourate.App;

public sealed class NotificationsWindow : Window
{
    private readonly NotificationService _service = new();

    public NotificationsWindow()
    {
        Title = "Accyourate Enterprise X - Notifiche";
        Width = 840;
        Height = 620;
        
        MinWidth = 1024;
        MinHeight = 680;
WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");
        Content = BuildLayout();
    }

    private Control BuildLayout()
    {
        var scroll = new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
        var stack = new StackPanel { Margin = new Thickness(24), Spacing = 16 };

        stack.Children.Add(UiFactory.PageTitle("Centro Notifiche"));
        stack.Children.Add(new TextBlock
        {
            Text = "Foundation 4.1.5: base per notifiche su certificazioni, manutenzioni, lavaggi, non conformità e backup."
        });

        var list = new StackPanel { Spacing = 10 };
        foreach (var n in _service.GetSystemNotifications())
        {
            list.Children.Add(new TextBlock
            {
                Text = $"{n.Severity} - {n.Title}: {n.Message}"
            });
        }

        list.Children.Add(new Separator());
        list.Children.Add(UiFactory.SectionTitle("Notifiche future"));
        list.Children.Add(new TextBlock { Text = "• Certificazioni in scadenza" });
        list.Children.Add(new TextBlock { Text = "• Manutenzioni programmate" });
        list.Children.Add(new TextBlock { Text = "• Capi tessili oltre soglia lavaggi" });
        list.Children.Add(new TextBlock { Text = "• Dispositivi non conformi" });
        list.Children.Add(new TextBlock { Text = "• Backup non eseguiti" });

        stack.Children.Add(UiFactory.Card(list));
        scroll.Content = stack;
        return scroll;
    }
}
