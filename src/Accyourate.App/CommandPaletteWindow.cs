using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.Models;
using Accyourate.App.UIFramework.Shell;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App;

public sealed class CommandPaletteWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;
    private readonly Action<string, string> _navigate;
    private readonly TextBox _search = new();
    private readonly StackPanel _results = new();

    public CommandPaletteWindow(DatabaseService database, CurrentUser user, Action<string, string> navigate)
    {
        _database = database;
        _user = user;
        _navigate = navigate;

        Title = "Command Palette";
        Width = 760;
        Height = 620;
        MinWidth = 680;
        MinHeight = 520;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();
        Refresh();
    }

    private Control BuildLayout()
    {
        var stack = new StackPanel { Margin = new Thickness(22), Spacing = 14 };

        stack.Children.Add(new TextBlock
        {
            Text = "Command Palette",
            FontSize = 26,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        _search.Watermark = "Cerca modulo o comando...";
        _search.TextChanged += (_, _) => Refresh();
        stack.Children.Add(_search);

        stack.Children.Add(new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(14),
            Child = new ScrollViewer
            {
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                Content = _results
            }
        });

        return stack;
    }

    private void Refresh()
    {
        _results.Children.Clear();
        _results.Spacing = 6;

        var q = (_search.Text ?? "").Trim().ToLowerInvariant();

        var modules = new[]
        {
            new ShellModuleDescriptor { Id = "workspace-home", Title = "Workspace Home", Section = "Centro Operativo", Icon = "⌂" },
            new ShellModuleDescriptor { Id = "control-room", Title = "Enterprise Control Room", Section = "Centro Operativo", Icon = "🧩" },
            new ShellModuleDescriptor { Id = "ai-assistant", Title = "Enterprise AI Assistant", Section = "Intelligence", Icon = "AI" },
            new ShellModuleDescriptor { Id = "ai-catalog", Title = "AI Intent Catalog", Section = "Intelligence", Icon = "AI" },
            new ShellModuleDescriptor { Id = "action-engine", Title = "Action Engine", Section = "Intelligence", Icon = "AX" },
            new ShellModuleDescriptor { Id = "dashboard", Title = "Dashboard", Section = "Centro Operativo", Icon = "▥" },
            new ShellModuleDescriptor { Id = "analytics", Title = "Analytics", Section = "Centro Operativo", Icon = "▧" },
            new ShellModuleDescriptor { Id = "medical", Title = "Medical Device Suite", Section = "Medical", Icon = "⌁" },
            new ShellModuleDescriptor { Id = "digital-twin", Title = "Digital Twin Platform", Section = "Medical", Icon = "DT" },
            new ShellModuleDescriptor { Id = "branding", Title = "Branding Center", Section = "Amministrazione", Icon = "🏷" },
            new ShellModuleDescriptor { Id = "design-system", Title = "Design System", Section = "Amministrazione", Icon = "🎛" },
            new ShellModuleDescriptor { Id = "architecture", Title = "Enterprise Architecture", Section = "Amministrazione", Icon = "🏗" }
        };

        foreach (var module in modules.Where(m => string.IsNullOrWhiteSpace(q) || $"{m.Title} {m.Section}".ToLowerInvariant().Contains(q)))
        {
            var button = new Button
            {
                Content = $"{module.Icon}  {module.Title}    —    {module.Section}",
                Background = Brushes.Transparent,
                Foreground = UiTokens.Brush(UiTokens.TextPrimary),
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                Padding = new Thickness(12, 9)
            };

            button.Click += (_, _) =>
            {
                _navigate(module.Id, module.Title);
                Close();
            };

            _results.Children.Add(button);
        }
    }
}
