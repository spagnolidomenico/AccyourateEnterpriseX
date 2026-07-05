using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;
using System.Diagnostics;

namespace Accyourate.App.Platform.Documents;

public sealed class DocumentCenterView : UserControl
{
    private readonly DocumentService _service = new();
    private readonly StackPanel _rows = new();
    private readonly ContentControl _details = new();
    private readonly TextBox _search = new();
    private readonly ComboBox _category = new();
    private readonly TextBlock _message = new();
    private IReadOnlyList<DocumentRecord> _documents = Array.Empty<DocumentRecord>();
    private DocumentRecord? _selected;

    public DocumentCenterView()
    {
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();
        Load();
    }

    private Control BuildLayout()
    {
        var root = new DockPanel();
        var header = new StackPanel { Margin = new Thickness(24,20,24,12), Spacing = 8 };
        header.Children.Add(new TextBlock { Text = "Centro Documenti", FontSize = 32, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary) });
        header.Children.Add(new TextBlock { Text = "Archivio centrale dei documenti generati o collegati alla piattaforma.", Foreground = UiTokens.Brush(UiTokens.TextSecondary), TextWrapping = TextWrapping.Wrap });
        _message.Foreground = UiTokens.Brush(UiTokens.BrandBlue); _message.TextWrapping = TextWrapping.Wrap; header.Children.Add(_message);
        DockPanel.SetDock(header, Dock.Top); root.Children.Add(header);

        var toolbar = new Grid { Margin = new Thickness(24,0,24,16), ColumnDefinitions = new ColumnDefinitions("*,190,Auto") };
        _search.Watermark = "Cerca documento, numero, dipendente, asset...";
        _search.TextChanged += (_, _) => RefreshRows();
        _category.ItemsSource = new[] { "Tutti", DocumentCategory.DeliveryReport, DocumentCategory.HumanResources, DocumentCategory.Asset, DocumentCategory.Generic };
        _category.SelectedIndex = 0; _category.SelectionChanged += (_, _) => RefreshRows();
        Add(toolbar, _search, 0, 0); Add(toolbar, _category, 1, 0); Add(toolbar, Button("↻ Aggiorna", Load), 2, 0);
        DockPanel.SetDock(toolbar, Dock.Top); root.Children.Add(toolbar);

        var content = new Grid { ColumnDefinitions = new ColumnDefinitions("*,390"), Margin = new Thickness(24,0,24,24) };
        var list = new DockPanel();
        var h = new Grid { ColumnDefinitions = new ColumnDefinitions("120,*,150,130,140"), Margin = new Thickness(0,0,0,8) };
        Add(h, Header("Numero"), 0, 0); Add(h, Header("Titolo"), 1, 0); Add(h, Header("Categoria"), 2, 0); Add(h, Header("Collegamento"), 3, 0); Add(h, Header("Data"), 4, 0);
        DockPanel.SetDock(h, Dock.Top); list.Children.Add(h);
        list.Children.Add(new Border { Background = UiTokens.Brush(UiTokens.Surface), CornerRadius = new CornerRadius(18), Padding = new Thickness(8), Child = new ScrollViewer { Content = _rows, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto } });
        Add(content, list, 0, 0);
        _details.Content = EmptyDetails();
        Add(content, new ScrollViewer { Content = _details, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto }, 1, 0);
        root.Children.Add(content);
        return root;
    }

    private void Load()
    {
        try
        {
            var keep = _selected?.Id;
            _documents = _service.GetLatest(200);
            _selected = keep.HasValue ? _documents.FirstOrDefault(x => x.Id == keep.Value) : _documents.FirstOrDefault();
            RefreshRows();
            _details.Content = _selected is not null ? DetailsCard(_selected) : EmptyDetails();
            ShowMessage($"Caricati {_documents.Count} documenti.");
        }
        catch (Exception ex) { ShowMessage($"Errore caricamento documenti: {ex.Message}", true); }
    }

    private void RefreshRows()
    {
        _rows.Children.Clear(); _rows.Spacing = 6;
        var q = (_search.Text ?? "").Trim().ToLowerInvariant();
        var cat = _category.SelectedItem?.ToString() ?? "Tutti";
        var filtered = _documents.Where(d => (string.IsNullOrWhiteSpace(q) || $"{d.DocumentNumber} {d.Title} {d.FileName} {d.RelatedEntityLabel} {d.Category}".ToLowerInvariant().Contains(q)) && (cat == "Tutti" || d.Category == cat)).ToList();
        if (filtered.Count == 0) { _rows.Children.Add(new TextBlock { Text = "Nessun documento trovato.", Margin = new Thickness(12), Foreground = UiTokens.Brush(UiTokens.TextSecondary) }); return; }
        foreach (var d in filtered) _rows.Children.Add(Row(d));
    }

    private Button Row(DocumentRecord d)
    {
        var g = new Grid { ColumnDefinitions = new ColumnDefinitions("120,*,150,130,140") };
        Add(g, Cell(d.DocumentNumber, true), 0, 0); Add(g, Cell(d.Title, true), 1, 0); Add(g, Cell(d.Category), 2, 0); Add(g, Cell(d.RelatedEntityLabel), 3, 0); Add(g, Cell(FormatDate(d.CreatedAt)), 4, 0);
        var b = new Button { Content = g, Background = _selected?.Id == d.Id ? UiTokens.Brush(UiTokens.PremiumSelected) : Brushes.Transparent, Padding = new Thickness(8), CornerRadius = new CornerRadius(12) };
        b.Click += (_, _) => { _selected = d; _details.Content = DetailsCard(d); RefreshRows(); };
        b.DoubleTapped += (_, _) => OpenDocument(d);
        return b;
    }

    private Control DetailsCard(DocumentRecord d)
    {
        var s = new StackPanel { Spacing = 12 };
        s.Children.Add(new TextBlock { Text = d.Title, FontSize = 24, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary), TextWrapping = TextWrapping.Wrap });
        s.Children.Add(new TextBlock { Text = $"{d.DocumentNumber} · {d.Category}", Foreground = UiTokens.Brush(UiTokens.TextSecondary), TextWrapping = TextWrapping.Wrap });
        var actions = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*"), Margin = new Thickness(0,4,0,8) };
        Add(actions, SmallButton("Apri", () => OpenDocument(d)), 0, 0); Add(actions, SmallButton("Cartella", () => OpenFolder(d), false), 1, 0); s.Children.Add(actions);
        s.Children.Add(Info("File", d.FileName)); s.Children.Add(Info("Percorso", d.FilePath)); s.Children.Add(Info("Dimensione", FormatSize(d.SizeBytes))); s.Children.Add(Info("Creato il", FormatDate(d.CreatedAt))); s.Children.Add(Info("Creato da", d.CreatedBy)); s.Children.Add(Info("Collegamento", $"{d.RelatedEntityType} {d.RelatedEntityId} {d.RelatedEntityLabel}".Trim())); s.Children.Add(Info("Note", d.Notes));
        return Card(s);
    }

    private Control EmptyDetails() => Card(new TextBlock { Text = "Seleziona un documento per visualizzare dettagli e azioni.", TextWrapping = TextWrapping.Wrap, Foreground = UiTokens.Brush(UiTokens.TextSecondary) });

    private void OpenDocument(DocumentRecord d)
    {
        try { if (string.IsNullOrWhiteSpace(d.FilePath) || !File.Exists(d.FilePath)) { ShowMessage("Documento non trovato nel percorso indicato.", true); return; } Process.Start(new ProcessStartInfo { FileName = d.FilePath, UseShellExecute = true }); }
        catch (Exception ex) { ShowMessage($"Errore apertura documento: {ex.Message}", true); }
    }

    private void OpenFolder(DocumentRecord d)
    {
        try { var folder = Path.GetDirectoryName(d.FilePath); if (string.IsNullOrWhiteSpace(folder)) { ShowMessage("Cartella documento non disponibile.", true); return; } Directory.CreateDirectory(folder); Process.Start(new ProcessStartInfo { FileName = folder, UseShellExecute = true }); }
        catch (Exception ex) { ShowMessage($"Errore apertura cartella: {ex.Message}", true); }
    }

    private void ShowMessage(string text, bool error = false) { _message.Text = text; _message.Foreground = UiTokens.Brush(error ? UiTokens.Danger : UiTokens.BrandBlue); }
    private static TextBlock Header(string t) => new() { Text = t, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextSecondary), Margin = new Thickness(10,0) };
    private static TextBlock Cell(string t, bool strong = false) => new() { Text = string.IsNullOrWhiteSpace(t) ? "—" : t, FontWeight = strong ? FontWeight.Bold : FontWeight.Normal, Foreground = UiTokens.Brush(strong ? UiTokens.TextPrimary : UiTokens.TextSecondary), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, TextWrapping = TextWrapping.NoWrap, Margin = new Thickness(10,0) };
    private static Button Button(string t, Action a) { var b = new Button { Content = t, Background = UiTokens.Brush(UiTokens.Surface), Foreground = UiTokens.Brush(UiTokens.TextPrimary), Padding = new Thickness(10,8), CornerRadius = new CornerRadius(12), Margin = new Thickness(8,0,0,0) }; b.Click += (_, _) => a(); return b; }
    private static Button SmallButton(string t, Action a, bool primary = true) { var b = new Button { Content = t, Background = UiTokens.Brush(primary ? UiTokens.BrandBlue : UiTokens.SurfaceAlt), Foreground = primary ? Brushes.White : UiTokens.Brush(UiTokens.TextPrimary), FontWeight = FontWeight.Bold, Padding = new Thickness(10,8), CornerRadius = new CornerRadius(12), Margin = new Thickness(4) }; b.Click += (_, _) => a(); return b; }
    private static Border Info(string l, string v) => new() { Background = UiTokens.Brush(UiTokens.SurfaceAlt), CornerRadius = new CornerRadius(12), Padding = new Thickness(12), Child = new StackPanel { Spacing = 2, Children = { new TextBlock { Text = l, FontSize = 12, Foreground = UiTokens.Brush(UiTokens.TextSecondary) }, new TextBlock { Text = string.IsNullOrWhiteSpace(v) ? "—" : v, FontWeight = FontWeight.SemiBold, TextWrapping = TextWrapping.Wrap, Foreground = UiTokens.Brush(UiTokens.TextPrimary) } } } };
    private static Border Card(Control c) => new() { Background = UiTokens.Brush(UiTokens.Surface), CornerRadius = new CornerRadius(22), Padding = new Thickness(18), Margin = new Thickness(16,0,0,0), Child = c };
    private static string FormatDate(string v) => DateTime.TryParse(v, out var d) ? d.ToString("dd/MM/yyyy HH:mm") : v;
    private static string FormatSize(long b) => b <= 0 ? "—" : b < 1024 ? $"{b} B" : b < 1024 * 1024 ? $"{b/1024.0:0.0} KB" : $"{b/1024.0/1024.0:0.0} MB";
    private static void Add(Grid g, Control c, int col, int row) { Grid.SetColumn(c, col); Grid.SetRow(c, row); g.Children.Add(c); }
}
