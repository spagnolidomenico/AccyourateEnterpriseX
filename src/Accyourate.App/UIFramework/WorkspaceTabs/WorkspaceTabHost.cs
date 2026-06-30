using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.WorkspaceTabs;

public sealed class WorkspaceTabHost : DockPanel
{
    private readonly WorkspaceTabManager _manager;
    private readonly StackPanel _tabStrip = new();
    private readonly ContentControl _content = new();

    public WorkspaceTabHost(WorkspaceTabManager manager)
    {
        _manager = manager;
        _manager.Changed += Refresh;

        LastChildFill = true;
        Build();
        Refresh();
    }

    private void Build()
    {
        var header = new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(10, 8),
            Child = new ScrollViewer
            {
                HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
                Content = _tabStrip
            }
        };

        _tabStrip.Orientation = Avalonia.Layout.Orientation.Horizontal;
        _tabStrip.Spacing = 6;

        DockPanel.SetDock(header, Dock.Top);
        Children.Add(header);
        Children.Add(_content);
    }

    private void Refresh()
    {
        _tabStrip.Children.Clear();

        foreach (var tab in _manager.Tabs)
            _tabStrip.Children.Add(TabButton(tab));

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

        if (tab.CanClose)
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
        return new Border
        {
            Background = UiTokens.Brush(UiTokens.Background),
            Child = new TextBlock
            {
                Text = "Nessuna scheda aperta",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Foreground = UiTokens.Brush(UiTokens.TextSecondary),
                FontSize = 20
            }
        };
    }
}
