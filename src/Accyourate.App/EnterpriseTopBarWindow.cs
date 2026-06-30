using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.Models;

namespace Accyourate.App;

public sealed class EnterpriseTopBarWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;

    public EnterpriseTopBarWindow(DatabaseService database, CurrentUser user)
    {
        _database = database;
        _user = user;

        Title = "Accyourate Enterprise X - Top Bar Preview";
        Width = 1080;
        Height = 460;
        MinWidth = 960;
        MinHeight = 420;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");
        Content = BuildLayout();
    }

    private Control BuildLayout()
    {
        var root = new DockPanel();

        var top = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("260,*,140,120,140,160"),
            Background = Brush.Parse("#111827"),
            Height = 62
        };

        Add(top, new TextBlock
        {
            Text = "Accyourate Enterprise X",
            Foreground = Brushes.White,
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(14, 0)
        }, 0);

        Add(top, Button("🔎 Cerca globale", () => new GlobalSearchWindow(_database).Show()), 1);
        Add(top, Button("🔔 Notifiche", () => new NotificationsWindow().Show()), 2);
        Add(top, Button("🎨 Tema", () => new ThemePersonalizationWindow(_database, _user).Show()), 3);
        Add(top, Button("⚙️ Impostazioni", () => new SettingsWindow(_database).Show()), 4);

        Add(top, new TextBlock
        {
            Text = $"👤 {_user.Username}",
            Foreground = Brushes.White,
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Margin = new Avalonia.Thickness(8, 0, 14, 0)
        }, 5);

        DockPanel.SetDock(top, Dock.Top);
        root.Children.Add(top);

        var body = new StackPanel { Margin = new Avalonia.Thickness(24), Spacing = 16 };
        body.Children.Add(new TextBlock
        {
            Text = "Top Bar Preview",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });
        body.Children.Add(new TextBlock
        {
            Text = "Questa è la base della futura barra superiore globale: ricerca, notifiche, tema, impostazioni e utente sempre disponibili.",
            TextWrapping = TextWrapping.Wrap
        });
        body.Children.Add(new TextBlock { Text = "Nella release successiva potremo integrarla direttamente nella finestra principale." });

        root.Children.Add(body);
        return root;
    }

    private static Button Button(string text, Action action)
    {
        var button = new Button
        {
            Content = text,
            Background = Brush.Parse("#374151"),
            Foreground = Brushes.White,
            Padding = new Avalonia.Thickness(10, 8),
            Margin = new Avalonia.Thickness(4)
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static void Add(Grid grid, Control control, int column)
    {
        Grid.SetColumn(control, column);
        grid.Children.Add(control);
    }
}
