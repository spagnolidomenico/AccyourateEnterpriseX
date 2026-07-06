using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.DesignSystem;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.WorkspaceTabs;

public sealed class WorkspaceHost : DockPanel
{
    private readonly WorkspaceTabManager _manager;
    private readonly StackPanel _tabStrip = new();
    private readonly ContentControl _content = new();
    private readonly TextBlock _tabInfo = new();

    public WorkspaceHost(WorkspaceTabManager manager)
    {
        _manager = manager;
        _manager.Changed += Refresh;

        LastChildFill = true;
        Build();
        Refresh();
    }

    private void Build()
    {
        var headerGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto")
        };

        var scroll = new ScrollViewer
        {
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            Content = _tabStrip
        };

        _tabStrip.Orientation = Avalonia.Layout.Orientation.Horizontal;
        _tabStrip.Spacing = 6;

        Grid.SetColumn(scroll, 0);
        headerGrid.Children.Add(scroll);

        _tabInfo.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        _tabInfo.Margin = new Thickness(10, 0);
        _tabInfo.Foreground = UiTokens.Brush(UiTokens.TextSecondary);
        Grid.SetColumn(_tabInfo, 1);
        headerGrid.Children.Add(_tabInfo);

        var actions = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 6
        };

        actions.Children.Add(AxButton.Create("Chiudi altre", () =>
        {
            if (!string.IsNullOrWhiteSpace(_manager.ActiveTabId))
                _manager.CloseOthers(_manager.ActiveTabId);
        }));

        actions.Children.Add(AxButton.Create("Chiudi tutte", _manager.CloseAllClosable));

        Grid.SetColumn(actions, 2);
        headerGrid.Children.Add(actions);

        var header = new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(10, 8),
            Child = headerGrid
        };

        DockPanel.SetDock(header, Dock.Top);
        Children.Add(header);
        Children.Add(_content);
    }

    private void Refresh()
    {
        _tabStrip.Children.Clear();

        foreach (var tab in _manager.Tabs)
            _tabStrip.Children.Add(TabButton(tab));

        _tabInfo.Text = $"Schede: {_manager.Tabs.Count}";
        _content.Content = _manager.ActiveTab?.Content ?? EmptyState();
    }

    private Control TabButton(WorkspaceTab tab)
    {
        var isActive = _manager.ActiveTab?.Id == tab.Id;

        var row = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 8
        };

        row.Children.Add(new TextBlock
        {
            Text = $"{tab.Icon} {tab.Title}",
            Foreground = UiTokens.Brush(isActive ? UiTokens.BrandBlue : UiTokens.TextPrimary),
            FontWeight = isActive ? FontWeight.Bold : FontWeight.Normal
        });

        if (tab.IsPinned)
        {
            row.Children.Add(new TextBlock
            {
                Text = "📌",
                Foreground = UiTokens.Brush(UiTokens.TextSecondary)
            });
        }

        if (tab.CanClose && !tab.IsPinned)
        {
            var close = new Button
            {
                Content = "×",
                Background = Brushes.Transparent,
                Foreground = UiTokens.Brush(UiTokens.TextSecondary),
                Padding = new Thickness(4, 0),
                MinWidth = 24
            };

            close.Click += (_, e) =>
            {
                e.Handled = true;
                _manager.Close(tab.Id);
            };

            row.Children.Add(close);
        }

        var button = new Button
        {
            Content = row,
            Background = UiTokens.Brush(isActive ? UiTokens.PremiumSelected : UiTokens.SurfaceAlt),
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            Padding = new Thickness(12, 8),
            CornerRadius = new CornerRadius(12)
        };

        button.Click += (_, _) => _manager.Activate(tab.Id);
        return button;
    }

    private static Control EmptyState()
    {
        return AxEmptyState.Create("📑", "Nessuna scheda aperta", "Apri un modulo dal menu laterale per iniziare.");
    }
}
