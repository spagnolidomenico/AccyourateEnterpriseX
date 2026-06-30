using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.ActionEngine;
using Accyourate.App.ActionEngine.DigitalTwin;
using Accyourate.App.Data;
using Accyourate.App.DigitalTwin;
using Accyourate.App.EnterpriseSearch;
using Accyourate.App.Models;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App;

public sealed class UniversalCommandBarWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;
    private readonly Action<string, string>? _navigate;
    private readonly EnterpriseSearchService _searchService;
    private readonly EnterpriseActionEngine _actionEngine;

    private readonly TextBox _searchBox = new();
    private readonly StackPanel _results = new();
    private readonly TextBlock _status = new();

    public UniversalCommandBarWindow(DatabaseService database, CurrentUser user, Action<string, string>? navigate = null)
    {
        _database = database;
        _user = user;
        _navigate = navigate;

        _searchService = new EnterpriseSearchService();
        _searchService.Register(new DigitalTwinSearchProvider(new DigitalTwinService(_database)));

        var registry = new CapabilityRegistry();
        DigitalTwinCapabilities.RegisterAll(registry);
        _actionEngine = new EnterpriseActionEngine(registry);

        Title = "Universal Command Bar";
        Width = 920;
        Height = 720;
        MinWidth = 820;
        MinHeight = 620;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();

        RefreshResults();
    }

    private Control BuildLayout()
    {
        var root = new StackPanel
        {
            Margin = new Thickness(24),
            Spacing = 16
        };

        root.Children.Add(new TextBlock
        {
            Text = "Universal Command Bar",
            FontSize = 30,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        root.Children.Add(new TextBlock
        {
            Text = "Cerca dispositivi, apri moduli o esegui comandi tramite Action Engine.",
            TextWrapping = TextWrapping.Wrap,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });

        _searchBox.Watermark = "Cerca, apri o chiedi qualcosa... es. TOP001, offline, batteria, ECG";
        _searchBox.FontSize = 18;
        _searchBox.Height = 48;
        _searchBox.TextChanged += (_, _) => RefreshResults();
        root.Children.Add(_searchBox);

        root.Children.Add(new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            CornerRadius = new CornerRadius(22),
            Padding = new Thickness(14),
            Child = new ScrollViewer
            {
                Content = _results,
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
            }
        });

        _status.Foreground = UiTokens.Brush(UiTokens.BrandBlue);
        _status.TextWrapping = TextWrapping.Wrap;
        root.Children.Add(_status);

        return root;
    }

    private void RefreshResults()
    {
        _results.Children.Clear();
        _results.Spacing = 8;

        var query = _searchBox.Text ?? string.Empty;
        var results = _searchService.Search(query);

        if (results.Count == 0)
        {
            _results.Children.Add(new TextBlock
            {
                Text = "Nessun risultato. Prova con TOP001, Digital Twin, offline, batteria o ECG.",
                Foreground = UiTokens.Brush(UiTokens.TextSecondary),
                Margin = new Thickness(12)
            });
            return;
        }

        foreach (var result in results)
            _results.Children.Add(ResultButton(result));
    }

    private Button ResultButton(SearchResult result)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("58,*,120")
        };

        Add(grid, new Border
        {
            Width = 42,
            Height = 42,
            Background = UiTokens.Brush(UiTokens.PremiumHover),
            CornerRadius = new CornerRadius(12),
            Child = new TextBlock
            {
                Text = result.Icon,
                Foreground = UiTokens.Brush(UiTokens.BrandBlue),
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            }
        }, 0, 0);

        var text = new StackPanel { Spacing = 2 };
        text.Children.Add(new TextBlock
        {
            Text = result.Title,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        text.Children.Add(new TextBlock
        {
            Text = result.Subtitle,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap
        });
        Add(grid, text, 1, 0);

        Add(grid, new TextBlock
        {
            Text = result.Type,
            Foreground = UiTokens.Brush(UiTokens.BrandBlue),
            FontWeight = FontWeight.SemiBold,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        }, 2, 0);

        var button = new Button
        {
            Content = grid,
            Background = Brushes.Transparent,
            Padding = new Thickness(10),
            CornerRadius = new CornerRadius(14)
        };

        button.Click += (_, _) => ExecuteResult(result);
        return button;
    }

    private void ExecuteResult(SearchResult result)
    {
        var request = new ActionRequest
        {
            ActionId = result.ActionId,
            ModuleId = result.ModuleId,
            Query = result.Title,
            Parameters = result.Parameters
        };

        var actionResult = _actionEngine.Execute(request, new ActionContext(_database, _user));

        if (actionResult.Success)
        {
            _status.Text = $"✅ {actionResult.Message}";

            if (!string.IsNullOrWhiteSpace(actionResult.ModuleId))
                _navigate?.Invoke(actionResult.ModuleId, result.Type == "Digital Twin" ? "Digital Twin Platform" : result.Title);
        }
        else
        {
            _status.Text = $"❌ {actionResult.Message}";
        }
    }

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
