using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;

namespace Accyourate.App;

public sealed class EnterpriseDashboardWindow : Window
{
    private readonly DatabaseService _database;
    private readonly StackPanel _metricsPanel = new();
    private readonly StackPanel _eventsPanel = new();

    public EnterpriseDashboardWindow(DatabaseService database)
    {
        _database = database;

        Title = "Accyourate Enterprise X - Enterprise Dashboard";
        Width = 1180;
        Height = 780;
        
        MinWidth = 1024;
        MinHeight = 680;
WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");

        Content = BuildLayout();
        Refresh();
    }

    private Control BuildLayout()
    {
        var scroll = new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
        var stack = new StackPanel { Margin = new Avalonia.Thickness(24), Spacing = 16 };

        stack.Children.Add(new TextBlock
        {
            Text = "Enterprise Dashboard",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });

        stack.Children.Add(new TextBlock
        {
            Text = "Versione 5.5: KPI operativi, stato piattaforma e ultime attività."
        });

        stack.Children.Add(Card(_metricsPanel));
        stack.Children.Add(Card(_eventsPanel));

        scroll.Content = stack;
        return scroll;
    }

    private void Refresh()
    {
        _metricsPanel.Children.Clear();
        _metricsPanel.Spacing = 10;
        _metricsPanel.Children.Add(new TextBlock { Text = "KPI principali", FontSize = 20, FontWeight = FontWeight.Bold });

        var wrap = new WrapPanel { ItemWidth = 245, ItemHeight = 110 };
        foreach (var m in _database.GetDashboardMetrics())
        {
            wrap.Children.Add(new Border
            {
                Background = Brush.Parse("#F7F7F6"),
                CornerRadius = new Avalonia.CornerRadius(12),
                Padding = new Avalonia.Thickness(14),
                Margin = new Avalonia.Thickness(6),
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock { Text = m.Title, FontWeight = FontWeight.Bold },
                        new TextBlock { Text = m.Value, FontSize = 30, Foreground = Brush.Parse("#B5162B") },
                        new TextBlock { Text = m.Description, Foreground = Brush.Parse("#555555") }
                    }
                }
            });
        }
        _metricsPanel.Children.Add(wrap);

        _eventsPanel.Children.Clear();
        _eventsPanel.Spacing = 8;
        _eventsPanel.Children.Add(new TextBlock { Text = "Ultimi eventi Digital Twin", FontSize = 20, FontWeight = FontWeight.Bold });

        foreach (var ev in _database.GetWorkflowEvents(null, null, 12))
        {
            _eventsPanel.Children.Add(new TextBlock
            {
                Text = $"{ev.CreatedAt} | {ev.EntityType} {ev.EntityCode} | {ev.EventType} | {ev.FromStatus} → {ev.ToStatus}"
            });
        }
    }

    private static Border Card(Control content)
    {
        return new Border
        {
            Background = Brushes.White,
            CornerRadius = new Avalonia.CornerRadius(14),
            Padding = new Avalonia.Thickness(18),
            Child = content
        };
    }
}
