using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.Platform.Update;

public sealed class UpdateCenterView : UserControl
{
    private readonly UpdateService _service = new();
    private readonly StackPanel _content = new();
    private readonly TextBlock _message = new();
    private VersionManifest _manifest = new();

    public UpdateCenterView()
    {
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();
        Load();
    }

    private Control BuildLayout()
    {
        var root = new DockPanel();
        var header = new Grid { Margin = new Thickness(24, 20, 24, 16), ColumnDefinitions = new ColumnDefinitions("*,180,180") };
        var title = new StackPanel { Spacing = 6 };
        title.Children.Add(new TextBlock { Text = "Update Center", FontSize = 34, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary) });
        title.Children.Add(new TextBlock { Text = "Gestione versione, changelog e predisposizione aggiornamenti.", Foreground = UiTokens.Brush(UiTokens.TextSecondary), TextWrapping = TextWrapping.Wrap });
        Add(header, title, 0, 0);
        Add(header, ToolbarButton("Controlla aggiornamenti", CheckUpdates, true), 1, 0);
        Add(header, ToolbarButton("Esporta manifest", ExportManifest, false), 2, 0);
        DockPanel.SetDock(header, Dock.Top); root.Children.Add(header);
        _message.Margin = new Thickness(24, 0, 24, 12); _message.Foreground = UiTokens.Brush(UiTokens.BrandBlue); _message.TextWrapping = TextWrapping.Wrap;
        DockPanel.SetDock(_message, Dock.Top); root.Children.Add(_message);
        root.Children.Add(new ScrollViewer { Content = _content, Margin = new Thickness(24, 0, 24, 24), VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto });
        return root;
    }

    private void Load() { _manifest = _service.GetInstalledManifest(); Render(); ShowMessage("Update Center caricato."); }
    private void CheckUpdates() { _manifest = _service.CheckForUpdates(); Render(); ShowMessage("Verifica aggiornamenti completata."); }
    private void ExportManifest()
    {
        try { ShowMessage($"Manifest esportato: {_service.ExportLocalManifest()}"); }
        catch (Exception ex) { ShowMessage($"Errore esportazione manifest: {ex.Message}", true); }
    }

    private void Render()
    {
        _content.Children.Clear(); _content.Spacing = 16;
        _content.Children.Add(Section("Versione installata", new[] { Info("Prodotto", _manifest.Product), Info("Versione installata", _manifest.InstalledVersion), Info("Ultima versione", _manifest.LatestVersion), Info("Canale", _manifest.Channel), Info("Stato", _manifest.Status), Info("Data release", _manifest.ReleaseDate) }));
        _content.Children.Add(Section("Aggiornamenti", new[] { Info("Download", string.IsNullOrWhiteSpace(_manifest.DownloadUrl) ? "Non configurato" : _manifest.DownloadUrl), Info("Note", _manifest.Notes), Info("Backup pre-update", "Predisposto: usare Backup Center prima di installare aggiornamenti."), Info("Firma digitale", "Predisposizione futura.") }));
        var releaseStack = new StackPanel { Spacing = 8 };
        foreach (var note in _service.GetReleaseNotes()) releaseStack.Children.Add(Info($"{note.Version} · {note.Date}", $"{note.Title} — {note.Notes}"));
        _content.Children.Add(Card(new StackPanel { Spacing = 12, Children = { new TextBlock { Text = "Release notes", FontSize = 22, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary) }, releaseStack } }));
    }

    private void ShowMessage(string message, bool isError = false) { _message.Text = message; _message.Foreground = UiTokens.Brush(isError ? UiTokens.Danger : UiTokens.BrandBlue); }
    private static Control Section(string title, IEnumerable<Control> items)
    {
        var wrap = new WrapPanel { ItemWidth = 350, ItemHeight = 96 };
        foreach (var item in items) wrap.Children.Add(item);
        return Card(new StackPanel { Spacing = 12, Children = { new TextBlock { Text = title, FontSize = 22, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary) }, wrap } });
    }
    private static Control Info(string label, string value) => new Border { Background = UiTokens.Brush(UiTokens.SurfaceAlt), CornerRadius = new CornerRadius(14), Padding = new Thickness(12), Margin = new Thickness(0, 0, 12, 12), Child = new StackPanel { Spacing = 4, Children = { new TextBlock { Text = label, FontSize = 12, FontWeight = FontWeight.SemiBold, Foreground = UiTokens.Brush(UiTokens.TextSecondary) }, new TextBlock { Text = string.IsNullOrWhiteSpace(value) ? "—" : value, FontWeight = FontWeight.Bold, TextWrapping = TextWrapping.Wrap, Foreground = UiTokens.Brush(UiTokens.TextPrimary) } } } };
    private static Border Card(Control child) => new() { Background = UiTokens.Brush(UiTokens.Surface), BorderBrush = UiTokens.Brush(UiTokens.Border), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(22), Padding = new Thickness(18), Child = child };
    private static Button ToolbarButton(string text, Action action, bool primary) { var b = new Button { Content = text, Background = UiTokens.Brush(primary ? UiTokens.BrandBlue : UiTokens.Surface), Foreground = primary ? Brushes.White : UiTokens.Brush(UiTokens.TextPrimary), FontWeight = primary ? FontWeight.Bold : FontWeight.Normal, Padding = new Thickness(10, 8), CornerRadius = new CornerRadius(12), Margin = new Thickness(8, 0, 0, 0) }; b.Click += (_, _) => action(); return b; }
    private static void Add(Grid grid, Control control, int column, int row) { Grid.SetColumn(control, column); Grid.SetRow(control, row); grid.Children.Add(control); }
}
