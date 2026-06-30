using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.Models;
using Accyourate.App.Shared.UI;

namespace Accyourate.App;

public sealed class AnalyticsDashboardWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;
    private readonly WrapPanel _kpiPanel = new();
    private readonly StackPanel _eventsPanel = new();
    private readonly StackPanel _notificationsPanel = new();
    private readonly StackPanel _statusChartPanel = new();
    private readonly StackPanel _volumeChartPanel = new();
    private readonly TextBlock _lastRefresh = new();

    public AnalyticsDashboardWindow(DatabaseService database, CurrentUser user)
    {
        _database = database;
        _user = user;

        Title = "Accyourate Enterprise X - Analytics Dashboard KPI";
        Width = 1320;
        Height = 860;
        MinWidth = 1180;
        MinHeight = 760;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");

        Content = BuildLayout();
        RefreshDashboard();
    }


    private Control BuildLayout()
    {
        var scroll = new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
        };

        var stack = new StackPanel { Margin = new Avalonia.Thickness(24), Spacing = 12 };

        stack.Children.Add(new TextBlock
        {
            Text = "Analytics Dashboard KPI",
            FontSize = 30,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });

        stack.Children.Add(new TextBlock
        {
            Text = "RC 6.1.3: dashboard più compatta, grafici subito visibili e pulsante Aggiorna sempre raggiungibile.",
            TextWrapping = TextWrapping.Wrap
        });

        var toolbar = new DockPanel
        {
            LastChildFill = true
        };

        var refresh = new Button
        {
            Content = "Aggiorna dashboard",
            Background = Brush.Parse("#B5162B"),
            Foreground = Brushes.White,
            FontWeight = FontWeight.Bold,
            MinWidth = 180,
            Padding = new Avalonia.Thickness(12, 8)
        };
        refresh.Click += (_, _) => RefreshDashboard();
        DockPanel.SetDock(refresh, Dock.Right);
        toolbar.Children.Add(refresh);

        _lastRefresh.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        toolbar.Children.Add(_lastRefresh);

        stack.Children.Add(Card("Azioni", toolbar));

        var chartsGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*")
        };
        AddControl(chartsGrid, Card("Grafico stati dispositivi", _statusChartPanel), 0, 0);
        AddControl(chartsGrid, Card("Volumi operativi", _volumeChartPanel), 1, 0);
        stack.Children.Add(chartsGrid);

        _kpiPanel.ItemWidth = 220;
        _kpiPanel.ItemHeight = 115;
        stack.Children.Add(Card("KPI operativi", _kpiPanel));

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*")
        };

        AddControl(grid, Card("Ultimi eventi Digital Twin", _eventsPanel), 0, 0);
        AddControl(grid, Card("Notifiche operative", _notificationsPanel), 1, 0);

        stack.Children.Add(grid);

        var roadmap = new StackPanel { Spacing = 6 };
        roadmap.Children.Add(new TextBlock { Text = "Prossimi step dashboard", FontSize = 18, FontWeight = FontWeight.Bold, Foreground = Brush.Parse("#B5162B") });
        roadmap.Children.Add(new TextBlock { Text = "• RC 6.1.4: grafici più evoluti e filtri." });
        roadmap.Children.Add(new TextBlock { Text = "• RC 6.1.5: widget configurabili e dashboard per ruolo." });
        roadmap.Children.Add(new TextBlock { Text = "• 6.2: Report Engine PDF/Excel." });
        stack.Children.Add(Card("Roadmap Analytics", roadmap));

        scroll.Content = stack;
        return scroll;
    }

    private void RefreshDashboard()
    {
        _lastRefresh.Text = $"Ultimo aggiornamento: {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Utente: {_user.Username}";
        RefreshKpis();
        RefreshEvents();
        RefreshNotifications();
        RefreshCharts();
    }

    private void RefreshKpis()
    {
        _kpiPanel.Children.Clear();

        foreach (var kpi in _database.GetAnalyticsKpis())
            _kpiPanel.Children.Add(KpiCard(kpi));
    }


    private void RefreshCharts()
    {
        _statusChartPanel.Children.Clear();
        _statusChartPanel.Children.Add(SimpleChartFactory.HorizontalBarChart(_database.GetMedicalDeviceStatusChart()));

        _volumeChartPanel.Children.Clear();
        _volumeChartPanel.Children.Add(SimpleChartFactory.HorizontalBarChart(_database.GetOperationalVolumeChart()));
    }

    private Control KpiCard(AnalyticsKpiRecord kpi)
    {
        var stack = new StackPanel { Spacing = 4 };

        stack.Children.Add(new TextBlock
        {
            Text = kpi.Area,
            FontSize = 12,
            Foreground = Brush.Parse("#666666")
        });

        stack.Children.Add(new TextBlock
        {
            Text = kpi.Title,
            FontSize = 16,
            FontWeight = FontWeight.Bold
        });

        stack.Children.Add(new TextBlock
        {
            Text = kpi.Value,
            FontSize = 26,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });

        stack.Children.Add(new TextBlock
        {
            Text = kpi.Subtitle,
            TextWrapping = TextWrapping.Wrap,
            Foreground = Brush.Parse("#555555")
        });

        return new Border
        {
            Background = Brushes.White,
            CornerRadius = new Avalonia.CornerRadius(14),
            Padding = new Avalonia.Thickness(10),
            Margin = new Avalonia.Thickness(4),
            Child = stack
        };
    }

    private void RefreshEvents()
    {
        _eventsPanel.Children.Clear();
        _eventsPanel.Spacing = 8;

        var events = _database.GetWorkflowEvents(null, null, 12);
        if (events.Count == 0)
        {
            _eventsPanel.Children.Add(new TextBlock { Text = "Nessun evento disponibile." });
            return;
        }

        foreach (var ev in events)
        {
            _eventsPanel.Children.Add(new TextBlock
            {
                Text = $"{ev.CreatedAt} | {ev.EntityType} {ev.EntityCode} | {ev.EventType} | {ev.ToStatus}",
                TextWrapping = TextWrapping.Wrap
            });
        }
    }

    private void RefreshNotifications()
    {
        _notificationsPanel.Children.Clear();
        _notificationsPanel.Spacing = 8;

        foreach (var n in _database.GetAnalyticsNotifications())
        {
            _notificationsPanel.Children.Add(new Border
            {
                Background = Brush.Parse("#F7F7F6"),
                CornerRadius = new Avalonia.CornerRadius(10),
                Padding = new Avalonia.Thickness(10),
                Child = new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        new TextBlock { Text = $"{n.Severity} - {n.Title}", FontWeight = FontWeight.Bold, Foreground = Brush.Parse("#B5162B") },
                        new TextBlock { Text = n.Message, TextWrapping = TextWrapping.Wrap },
                        new TextBlock { Text = $"Fonte: {n.Source}", Foreground = Brush.Parse("#666666") }
                    }
                }
            });
        }
    }

    private static Border Card(string title, Control content)
    {
        return new Border
        {
            Background = Brushes.White,
            CornerRadius = new Avalonia.CornerRadius(14),
            Padding = new Avalonia.Thickness(18),
            Child = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    new TextBlock { Text = title, FontSize = 20, FontWeight = FontWeight.Bold, Foreground = Brush.Parse("#B5162B") },
                    content
                }
            }
        };
    }

    private static void AddControl(Grid grid, Control control, int column, int row)
    {
        control.Margin = new Avalonia.Thickness(4);
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
