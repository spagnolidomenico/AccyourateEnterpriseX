using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;
using Accyourate.App.AssetManagement;

namespace Accyourate.App.Platform.Notifications;

public sealed class NotificationCenterView : UserControl
{
    private readonly NotificationService _service;
    private readonly StackPanel _rows = new();
    private readonly ComboBox _filter = new();
    private readonly TextBlock _counter = new();

    public NotificationCenterView(NotificationService? service = null)
    {
        _service = service ?? new NotificationService();
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();
        Load();
    }

    private Control BuildLayout()
    {
        var root = new DockPanel();

        var header = new Grid
        {
            Margin = new Thickness(24, 20, 24, 12),
            ColumnDefinitions = new ColumnDefinitions("*,180,180")
        };

        var title = new StackPanel { Spacing = 4 };
        title.Children.Add(new TextBlock
        {
            Text = "Notification Center",
            FontSize = 32,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        title.Children.Add(_counter);
        _counter.Foreground = UiTokens.Brush(UiTokens.TextSecondary);

        Add(header, title, 0, 0);

        _filter.ItemsSource = new[] { "Tutte", "Non lette", "System", "Asset", "MasterData", "AI", "Security" };
        _filter.SelectedIndex = 0;
        _filter.SelectionChanged += (_, _) => Load();
        Add(header, _filter, 1, 0);

        var markAll = new Button
        {
            Content = "Segna tutte lette",
            Background = UiTokens.Brush(UiTokens.BrandBlue),
            Foreground = Brushes.White,
            FontWeight = FontWeight.Bold,
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(12, 8),
            Margin = new Thickness(8, 0, 0, 0)
        };
        markAll.Click += (_, _) =>
        {
            _service.MarkAllAsRead();
            Load();
        };
        Add(header, markAll, 2, 0);

        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        root.Children.Add(new Border
        {
            Margin = new Thickness(24, 0, 24, 24),
            Padding = new Thickness(12),
            Background = UiTokens.Brush(UiTokens.Surface),
            CornerRadius = new CornerRadius(18),
            Child = new ScrollViewer
            {
                Content = _rows,
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
            }
        });

        return root;
    }

    private void Load()
    {
        _rows.Children.Clear();
        _rows.Spacing = 8;

        var selected = _filter.SelectedItem?.ToString() ?? "Tutte";
        var unreadOnly = selected == "Non lette";

        var items = _service.GetLatest(80, unreadOnly).ToList();

        if (selected is not "Tutte" and not "Non lette")
            items = items.Where(x => x.Category == selected).ToList();

        var unread = _service.CountUnread();
        _counter.Text = unread == 0
            ? "Nessuna notifica non letta."
            : $"{unread} notifiche non lette.";

        if (items.Count == 0)
        {
            _rows.Children.Add(new TextBlock
            {
                Text = "Nessuna notifica disponibile.",
                Margin = new Thickness(12),
                Foreground = UiTokens.Brush(UiTokens.TextSecondary)
            });
            return;
        }

        foreach (var item in items)
            _rows.Children.Add(NotificationCard(item));
    }

    private Control NotificationCard(NotificationRecord item)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("46,*,120"),
            Margin = new Thickness(0, 0, 0, 4)
        };

        Add(grid, new Border
        {
            Width = 36,
            Height = 36,
            Background = UiTokens.Brush(PriorityColor(item.Priority)),
            CornerRadius = new CornerRadius(18),
            Child = new TextBlock
            {
                Text = PriorityIcon(item.Priority),
                Foreground = Brushes.White,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                FontWeight = FontWeight.Bold
            }
        }, 0, 0);

        var text = new StackPanel { Spacing = 3 };
        text.Children.Add(new TextBlock
        {
            Text = item.Title,
            FontWeight = item.IsRead ? FontWeight.Normal : FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        text.Children.Add(new TextBlock
        {
            Text = item.Message,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap
        });
        text.Children.Add(new TextBlock
        {
            Text = $"{item.Category} · {FormatDate(item.CreatedAt)} · {item.CreatedBy}",
            FontSize = 11,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });
        Add(grid, text, 1, 0);

        var actions = new StackPanel { Spacing = 5 };
        if (item.Action == "open-supplier-portal" && int.TryParse(item.Payload, out var supplierId))
        {
            var open = new Button
            {
                Content = "Apri portale",
                Background = UiTokens.Brush(UiTokens.BrandBlue),
                Foreground = Brushes.White,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8, 6)
            };
            open.Click += (_, _) =>
            {
                _service.MarkAsRead(item.Id);
                new SupplierRmaPortalWindow(supplierId).Show();
                Load();
            };
            actions.Children.Add(open);
        }
        if (item.Action == "open-supplier-followups")
        {
            var openDashboard = new Button
            {
                Content = "Apri cruscotto",
                Background = UiTokens.Brush(UiTokens.BrandBlue),
                Foreground = Brushes.White,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8, 6)
            };
            openDashboard.Click += (_, _) =>
            {
                _service.MarkAsRead(item.Id);
                new SupplierFollowUpDashboardWindow().Show();
                Load();
            };
            actions.Children.Add(openDashboard);
        }
        var governanceAction = item.Action == "open-rma-capa-governance-actions"
            || (item.Action == "open-rma-corrective-actions" && item.CreatedBy.Contains("Governance CAPA", StringComparison.OrdinalIgnoreCase));
        if (governanceAction)
        {
            var openGovernance = new Button
            {
                Content = "Apri piano",
                Background = UiTokens.Brush(UiTokens.BrandBlue),
                Foreground = Brushes.White,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8, 6)
            };
            openGovernance.Click += (_, _) =>
            {
                _service.MarkAsRead(item.Id);
                new SupplierRmaCapaGovernanceActionsWindow().Show();
                Load();
            };
            actions.Children.Add(openGovernance);
        }
        var button = new Button
        {
            Content = item.IsRead ? "Letta" : "Segna letta",
            IsEnabled = !item.IsRead,
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(8, 6)
        };
        button.Click += (_, _) =>
        {
            _service.MarkAsRead(item.Id);
            Load();
        };
        actions.Children.Add(button);
        Add(grid, actions, 2, 0);

        return new Border
        {
            Padding = new Thickness(12),
            Background = item.IsRead ? UiTokens.Brush(UiTokens.Surface) : UiTokens.Brush(UiTokens.SurfaceAlt),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            Child = grid
        };
    }

    private static string PriorityColor(string priority)
    {
        return priority switch
        {
            NotificationPriority.Critical => UiTokens.Danger,
            NotificationPriority.High => UiTokens.Warning,
            NotificationPriority.Normal => UiTokens.BrandBlue,
            _ => UiTokens.Info
        };
    }

    private static string PriorityIcon(string priority)
    {
        return priority switch
        {
            NotificationPriority.Critical => "!",
            NotificationPriority.High => "!",
            NotificationPriority.Normal => "i",
            _ => "•"
        };
    }

    private static string FormatDate(string value)
    {
        return DateTime.TryParse(value, out var date)
            ? date.ToString("dd/MM/yyyy HH:mm")
            : value;
    }

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
