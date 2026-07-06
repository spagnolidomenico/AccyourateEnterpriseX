using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.Platform.Search;

public sealed class EnterpriseSearchView : UserControl
{
    private readonly EnterpriseSearchService _service = new();
    private readonly Action<string, string>? _navigate;
    private readonly TextBox _query = new();
    private readonly StackPanel _results = new();
    private readonly TextBlock _message = new();

    public EnterpriseSearchView(Action<string, string>? navigate = null)
    {
        _navigate = navigate;
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();
    }

    private Control BuildLayout()
    {
        var root = new DockPanel();
        var header = new StackPanel { Margin = new Thickness(24,20,24,16), Spacing = 8 };
        header.Children.Add(new TextBlock { Text = "Ricerca Enterprise", FontSize = 34, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary) });
        header.Children.Add(new TextBlock { Text = "Cerca dipendenti, asset, verbali, documenti, notifiche e audit.", Foreground = UiTokens.Brush(UiTokens.TextSecondary), TextWrapping = TextWrapping.Wrap });
        _query.Watermark = "Scrivi almeno 2 caratteri...";
        _query.FontSize = 18; _query.Padding = new Thickness(14,12); _query.TextChanged += (_, _) => Search();
        header.Children.Add(_query);
        _message.Foreground = UiTokens.Brush(UiTokens.TextSecondary); header.Children.Add(_message);
        DockPanel.SetDock(header, Dock.Top); root.Children.Add(header);
        root.Children.Add(new ScrollViewer { Content = _results, Margin = new Thickness(24,0,24,24), VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto });
        return root;
    }

    private void Search()
    {
        _results.Children.Clear();
        var text = (_query.Text ?? "").Trim();
        if (text.Length < 2) { _message.Text = "Inserisci almeno 2 caratteri per avviare la ricerca."; return; }
        var results = _service.Search(new SearchRequest { Query = text, Limit = 150 });
        _message.Text = $"{results.Count} risultati trovati.";
        if (results.Count == 0) { _results.Children.Add(Empty("Nessun risultato trovato.")); return; }
        foreach (var group in results.GroupBy(x => x.Category).OrderBy(x => x.Key))
        {
            _results.Children.Add(new TextBlock { Text = group.Key, FontSize = 22, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary), Margin = new Thickness(0,12,0,8) });
            foreach (var result in group) _results.Children.Add(ResultCard(result));
        }
    }

    private Button ResultCard(SearchResult r)
    {
        var stack = new StackPanel { Spacing = 4 };
        stack.Children.Add(new TextBlock { Text = $"{r.Icon} {r.Title}", FontSize = 17, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary), TextWrapping = TextWrapping.Wrap });
        stack.Children.Add(new TextBlock { Text = r.Subtitle, Foreground = UiTokens.Brush(UiTokens.TextSecondary), TextWrapping = TextWrapping.Wrap });
        stack.Children.Add(new TextBlock { Text = $"{r.EntityType} · {r.EntityId} · {r.Source}", FontSize = 12, Foreground = UiTokens.Brush(UiTokens.TextSecondary) });
        var b = new Button { Content = stack, Background = UiTokens.Brush(UiTokens.Surface), BorderBrush = UiTokens.Brush(UiTokens.Border), BorderThickness = new Thickness(1), Padding = new Thickness(14), CornerRadius = new CornerRadius(16), Margin = new Thickness(0,0,0,8), HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };
        b.Click += (_, _) => { if (!string.IsNullOrWhiteSpace(r.OpenAction)) _navigate?.Invoke(r.OpenAction, r.Category); };
        return b;
    }

    private static Border Empty(string text) => new() { Background = UiTokens.Brush(UiTokens.Surface), CornerRadius = new CornerRadius(18), Padding = new Thickness(18), Child = new TextBlock { Text = text, Foreground = UiTokens.Brush(UiTokens.TextSecondary) } };
}
