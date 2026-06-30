using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.ActionEngine;
using Accyourate.App.ActionEngine.DigitalTwin;
using Accyourate.App.Data;
using Accyourate.App.Models;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App;

public sealed class ActionEngineWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;
    private readonly EnterpriseActionEngine _engine;
    private readonly ActionIntentParser _parser = new();

    private readonly TextBox _command = new();
    private readonly StackPanel _log = new();
    private readonly StackPanel _capabilities = new();

    public ActionEngineWindow(DatabaseService database, CurrentUser user)
    {
        _database = database;
        _user = user;

        var registry = new CapabilityRegistry();
        DigitalTwinCapabilities.RegisterAll(registry);
        _engine = new EnterpriseActionEngine(registry);

        Title = "Accyourate Enterprise X 10.0 RC1 - Action Engine";
        Width = 1180;
        Height = 820;
        MinWidth = 1040;
        MinHeight = 720;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();

        LoadCapabilities();
        AddLog("Action Engine pronto. Prova: Apri il Digital Twin del dispositivo TOP001");
    }

    private Control BuildLayout()
    {
        var root = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,360")
        };

        var left = new DockPanel();

        var header = new StackPanel { Margin = new Thickness(24), Spacing = 8 };
        header.Children.Add(new TextBlock
        {
            Text = "Enterprise Action Engine",
            FontSize = 32,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        header.Children.Add(new TextBlock
        {
            Text = "Foundation RC1: interpreta richieste operative, verifica capability e produce risultati standardizzati.",
            TextWrapping = TextWrapping.Wrap,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });
        DockPanel.SetDock(header, Dock.Top);
        left.Children.Add(header);

        var input = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,120"),
            Margin = new Thickness(24, 0, 24, 18)
        };

        _command.Watermark = "Scrivi un comando, es. Apri il Digital Twin del dispositivo TOP001";
        Add(input, _command, 0, 0);

        var run = new Button
        {
            Content = "Esegui",
            Background = UiTokens.Brush(UiTokens.BrandBlue),
            Foreground = Brushes.White,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(14, 10),
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(10, 0, 0, 0)
        };
        run.Click += (_, _) => ExecuteCommand();
        Add(input, run, 1, 0);
        DockPanel.SetDock(input, Dock.Top);
        left.Children.Add(input);

        var quick = new WrapPanel { Margin = new Thickness(24, 0, 24, 18), ItemWidth = 265, ItemHeight = 44 };
        quick.Children.Add(Quick("Apri il Digital Twin del dispositivo TOP001"));
        quick.Children.Add(Quick("Mostrami dispositivi con batteria sotto il 20%"));
        quick.Children.Add(Quick("Mostrami dispositivi offline"));
        quick.Children.Add(Quick("Mostra telemetria TOP001"));
        quick.Children.Add(Quick("Mostra ECG TOP001"));
        DockPanel.SetDock(quick, Dock.Top);
        left.Children.Add(quick);

        left.Children.Add(new ScrollViewer
        {
            Content = _log,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        });

        Add(root, left, 0, 0);

        var right = new StackPanel { Margin = new Thickness(18), Spacing = 12 };
        right.Children.Add(new TextBlock
        {
            Text = "Capability registrate",
            FontSize = 22,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        right.Children.Add(new TextBlock
        {
            Text = "L'AI potrà eseguire solo capability registrate e autorizzate.",
            TextWrapping = TextWrapping.Wrap,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });
        right.Children.Add(new ScrollViewer
        {
            Content = _capabilities,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        });

        Add(root, Card(right), 1, 0);

        return root;
    }

    private Button Quick(string text)
    {
        var b = new Button
        {
            Content = text,
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            Padding = new Thickness(10, 8),
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(4)
        };
        b.Click += (_, _) =>
        {
            _command.Text = text;
            ExecuteCommand();
        };
        return b;
    }

    private void ExecuteCommand()
    {
        var query = _command.Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(query))
            return;

        var request = _parser.Parse(query);
        var context = new ActionContext(_database, _user);
        var result = _engine.Execute(request, context);

        AddLog($"Comando: {query}");
        AddLog($"ActionId: {request.ActionId}");
        AddLog(result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}");

        if (!string.IsNullOrWhiteSpace(result.SuggestedNavigation))
            AddLog($"Navigazione suggerita: {result.SuggestedNavigation}");

        _command.Text = "";
    }

    private void LoadCapabilities()
    {
        _capabilities.Children.Clear();
        _capabilities.Spacing = 8;

        foreach (var cap in _engine.GetCapabilities())
        {
            _capabilities.Children.Add(Card(new StackPanel
            {
                Spacing = 4,
                Children =
                {
                    new TextBlock { Text = cap.DisplayName, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary) },
                    new TextBlock { Text = cap.Id, FontSize = 12, Foreground = UiTokens.Brush(UiTokens.BrandBlue) },
                    new TextBlock { Text = cap.Description, TextWrapping = TextWrapping.Wrap, Foreground = UiTokens.Brush(UiTokens.TextSecondary) }
                }
            }));
        }
    }

    private void AddLog(string text)
    {
        _log.Children.Add(new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(14),
            Margin = new Thickness(24, 6),
            Child = new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                Foreground = UiTokens.Brush(UiTokens.TextPrimary)
            }
        });
    }

    private static Border Card(Control child)
    {
        return new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            CornerRadius = new CornerRadius(18),
            Padding = new Thickness(16),
            Margin = new Thickness(6),
            Child = child
        };
    }

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
