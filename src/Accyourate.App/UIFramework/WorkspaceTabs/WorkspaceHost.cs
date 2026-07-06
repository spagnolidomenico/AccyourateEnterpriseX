using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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
    private ScrollViewer? _tabScroll;

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
            RowDefinitions = new RowDefinitions("Auto,Auto"),
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto,Auto")
        };

        var left = AxButton.Create("◀", ScrollLeft);
        left.MinWidth = 34;
        left.Padding = new Thickness(8, 6);
        Grid.SetColumn(left, 0);
        Grid.SetRow(left, 0);
        headerGrid.Children.Add(left);

        _tabScroll = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = _tabStrip,
            MinHeight = 50,
            Padding = new Thickness(0, 0, 0, 10)
        };

        _tabStrip.Orientation = Avalonia.Layout.Orientation.Horizontal;
        _tabStrip.Spacing = 6;
        _tabStrip.Margin = new Thickness(0, 0, 0, 4);

        Grid.SetColumn(_tabScroll, 1);
        Grid.SetRow(_tabScroll, 0);
        Grid.SetColumnSpan(_tabScroll, 1);
        headerGrid.Children.Add(_tabScroll);

        var right = AxButton.Create("▶", ScrollRight);
        right.MinWidth = 34;
        right.Padding = new Thickness(8, 6);
        Grid.SetColumn(right, 2);
        Grid.SetRow(right, 0);
        headerGrid.Children.Add(right);

        _tabInfo.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        _tabInfo.Margin = new Thickness(10, 0);
        _tabInfo.Foreground = UiTokens.Brush(UiTokens.TextSecondary);
        Grid.SetColumn(_tabInfo, 3);
        Grid.SetRow(_tabInfo, 0);
        headerGrid.Children.Add(_tabInfo);

        var actions = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Spacing = 6,
            Margin = new Thickness(0, 6, 0, 0)
        };

        actions.Children.Add(AxButton.Create("Chiudi altre", () =>
        {
            if (!string.IsNullOrWhiteSpace(_manager.ActiveTabId))
                _manager.CloseOthers(_manager.ActiveTabId);
        }));

        actions.Children.Add(AxButton.Create("Chiudi tutte", _manager.CloseAllClosable));

        Grid.SetColumn(actions, 1);
        Grid.SetColumnSpan(actions, 3);
        Grid.SetRow(actions, 1);
        headerGrid.Children.Add(actions);

        var header = new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(10, 8, 10, 4),
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
            Spacing = 8,
            MinWidth = 150,
            MaxWidth = 230
        };

        var title = new TextBlock
        {
            Text = $"{tab.Icon} {tab.Title}",
            Foreground = UiTokens.Brush(isActive ? UiTokens.BrandBlue : UiTokens.TextPrimary),
            FontWeight = isActive ? FontWeight.Bold : FontWeight.Normal,
            TextTrimming = TextTrimming.CharacterEllipsis,
            TextWrapping = TextWrapping.NoWrap,
            MaxWidth = tab.CanClose && !tab.IsPinned ? 165 : 190
        };
        ToolTip.SetTip(title, tab.Title);

        row.Children.Add(title);

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
            CornerRadius = new CornerRadius(12),
            MinWidth = 160,
            MaxWidth = 250
        };
        ToolTip.SetTip(button, tab.Title);

        button.Click += (_, _) => _manager.Activate(tab.Id);
        return button;
    }

    private void ScrollLeft()
    {
        if (_tabScroll is null)
            return;

        var offset = _tabScroll.Offset;
        _tabScroll.Offset = new Vector(Math.Max(0, offset.X - 220), offset.Y);
    }

    private void ScrollRight()
    {
        if (_tabScroll is null)
            return;

        var offset = _tabScroll.Offset;
        _tabScroll.Offset = new Vector(offset.X + 220, offset.Y);
    }

    private static Control EmptyState()
    {
        return AxEmptyState.Create("📑", "Nessuna scheda aperta", "Apri un modulo dal menu laterale per iniziare.");
    }
}
