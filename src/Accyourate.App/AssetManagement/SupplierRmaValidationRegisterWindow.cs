using System.Diagnostics;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class SupplierRmaValidationRegisterWindow : Window
{
    private readonly SupplierRmaValidationService _validation = new();
    private readonly SparePartRmaRepository _rma = new();
    private readonly TextBox _search = new() { Watermark = "Cerca pratica, operatore o note..." };
    private readonly ComboBox _status = new() { ItemsSource = new[] { "Tutti gli esiti", "Completo", "Da verificare", "Incompleto", "Non validata" }, SelectedIndex = 0, MinWidth = 180 };
    private readonly StackPanel _rows = new();
    private readonly TextBlock _summary = new() { Foreground = UiTokens.Brush(UiTokens.TextSecondary) };
    private readonly TextBlock _message = new();

    public SupplierRmaValidationRegisterWindow()
    {
        Title = "Registro validazioni RMA"; Width = 1220; Height = 760; MinWidth = 920; MinHeight = 560;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Content = Build();
        _search.TextChanged += (_, _) => LoadRows();
        _status.SelectionChanged += (_, _) => LoadRows();
        LoadRows();
    }

    private Control Build()
    {
        var root = new DockPanel { Margin = new Thickness(24) };
        var title = new StackPanel { Spacing = 3, Children = { new TextBlock { Text = "Registro validazioni RMA", FontSize = 28, FontWeight = FontWeight.Bold }, new TextBlock { Text = "Chiusure, rivalidazioni e fascicoli delle pratiche fornitore.", Foreground = UiTokens.Brush(UiTokens.TextSecondary) } } };
        var export = Button("Esporta CSV", Export, true);
        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Margin = new Thickness(0, 0, 0, 14) }; Add(header, title, 0); Add(header, export, 1); DockPanel.SetDock(header, Dock.Top); root.Children.Add(header);
        var filters = new Grid { ColumnDefinitions = new ColumnDefinitions("*,190"), Margin = new Thickness(0, 0, 0, 10) }; Add(filters, _search, 0); Add(filters, _status, 1); DockPanel.SetDock(filters, Dock.Top); root.Children.Add(filters);
        _summary.Margin = new Thickness(0, 0, 0, 8); DockPanel.SetDock(_summary, Dock.Top); root.Children.Add(_summary);
        DockPanel.SetDock(_message, Dock.Top); root.Children.Add(_message);
        root.Children.Add(new ScrollViewer { Content = _rows, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto });
        return root;
    }

    private void LoadRows()
    {
        try
        {
            var values = Filter(AllRows()).ToList();
            _rows.Children.Clear(); _rows.MinWidth = 1120; _rows.Children.Add(Header());
            for (var index = 0; index < values.Count; index++) _rows.Children.Add(Row(values[index], index));
            _summary.Text = $"{values.Count} registrazioni visualizzate · {values.Count(x => x.Status == "Completo")} complete · {values.Count(x => x.Status == "Non validata")} non validate";
            _message.Text = "";
        }
        catch (Exception ex) { ShowMessage($"Registro non caricato: {ex.Message}", true); }
    }

    private IReadOnlyList<RegisterRow> AllRows()
    {
        var cases = _rma.GetAll(); var closures = _validation.GetClosures(); var result = new List<RegisterRow>();
        foreach (var item in cases)
        {
            var history = closures.Where(x => x.RmaId == item.Id).ToList();
            if (history.Count == 0) result.Add(new(item.Id, item.CaseNumber, "Non validata", "", "", "", item.Status));
            else foreach (var closure in history) result.Add(new(item.Id, item.CaseNumber, closure.ValidationStatus, closure.ClosedAt, closure.ClosedBy, closure.Notes, item.Status));
        }
        return result.OrderByDescending(x => Parse(x.Date)).ThenByDescending(x => x.RmaId).ToList();
    }

    private IEnumerable<RegisterRow> Filter(IEnumerable<RegisterRow> values)
    {
        var query = (_search.Text ?? "").Trim(); var status = _status.SelectedItem?.ToString() ?? "Tutti gli esiti";
        return values.Where(x => status == "Tutti gli esiti" || x.Status == status)
            .Where(x => query.Length == 0 || $"{x.CaseNumber} {x.User} {x.Notes} {x.RmaStatus}".Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    private Control Header()
    {
        var grid = GridRow(); var names = new[] { "Pratica", "Esito", "Stato RMA", "Data", "Operatore", "Note", "Fascicolo" };
        for (var i = 0; i < names.Length; i++) Text(grid, names[i], i, true);
        return new Border { Background = UiTokens.Brush(UiTokens.SurfaceAlt), Padding = new Thickness(8), Child = grid };
    }

    private Control Row(RegisterRow value, int index)
    {
        var grid = GridRow(); Text(grid, value.CaseNumber, 0, true); Add(grid, Badge(value.Status), 1); Text(grid, value.RmaStatus, 2); Text(grid, Date(value.Date), 3); Text(grid, Blank(value.User), 4); Text(grid, Blank(value.Notes), 5);
        var path = SupplierRmaValidationService.DossierPath(value.CaseNumber); var open = Button(File.Exists(path) ? "Apri" : "Non disponibile", () => Open(path)); open.IsEnabled = File.Exists(path); Add(grid, open, 6);
        return new Border { Background = UiTokens.Brush(index % 2 == 0 ? UiTokens.Surface : UiTokens.SurfaceAlt), BorderBrush = UiTokens.Brush(UiTokens.Border), BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(8, 6), Child = grid };
    }

    private async void Export()
    {
        try
        {
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions { Title = "Esporta registro validazioni RMA", SuggestedFileName = $"Registro-validazioni-RMA-{DateTime.Today:yyyyMMdd}.csv", FileTypeChoices = new[] { new FilePickerFileType("CSV") { Patterns = new[] { "*.csv" } } } });
            var path = file?.TryGetLocalPath(); if (string.IsNullOrWhiteSpace(path)) return;
            var lines = new List<string> { "Pratica;Esito;Stato RMA;Data;Operatore;Note;Fascicolo" };
            foreach (var x in Filter(AllRows())) lines.Add(string.Join(";", new[] { x.CaseNumber, x.Status, x.RmaStatus, Date(x.Date), x.User, x.Notes, SupplierRmaValidationService.DossierPath(x.CaseNumber) }.Select(Csv)));
            File.WriteAllLines(path, lines, new UTF8Encoding(true)); ShowMessage($"Registro esportato: {path}", false);
        }
        catch (Exception ex) { ShowMessage($"Esportazione non riuscita: {ex.Message}", true); }
    }

    private static void Open(string path) { if (File.Exists(path)) Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true }); }
    private static Border Badge(string status) { var color = status == "Completo" ? UiTokens.Success : status == "Non validata" ? UiTokens.Danger : UiTokens.Warning; return new Border { BorderBrush = UiTokens.Brush(color), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8), Padding = new Thickness(6, 3), Child = new TextBlock { Text = status, Foreground = UiTokens.Brush(color), HorizontalAlignment = HorizontalAlignment.Center, FontWeight = FontWeight.SemiBold } }; }
    private static Grid GridRow() => new() { ColumnDefinitions = new ColumnDefinitions("160,130,120,135,150,*,130") };
    private static void Text(Grid grid, string text, int column, bool strong = false) => Add(grid, new TextBlock { Text = Blank(text), FontWeight = strong ? FontWeight.SemiBold : FontWeight.Normal, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis, Margin = new Thickness(3) }, column);
    private static void Add(Grid grid, Control control, int column) { control.Margin = new Thickness(0, 0, 7, 0); Grid.SetColumn(control, column); grid.Children.Add(control); }
    private static Button Button(string text, Action action, bool primary = false) { var button = new Button { Content = text, MinHeight = 34, Background = UiTokens.Brush(primary ? UiTokens.BrandBlue : UiTokens.SurfaceAlt), Foreground = primary ? Brushes.White : UiTokens.Brush(UiTokens.TextPrimary) }; button.Click += (_, _) => action(); return button; }
    private void ShowMessage(string text, bool error) { _message.Text = text; _message.Foreground = UiTokens.Brush(error ? UiTokens.Danger : UiTokens.Success); }
    private static string Date(string value) => DateTime.TryParse(value, out var date) ? date.ToString("dd/MM/yyyy HH:mm") : "—";
    private static DateTime Parse(string value) => DateTime.TryParse(value, out var date) ? date : DateTime.MinValue;
    private static string Blank(string value) => string.IsNullOrWhiteSpace(value) ? "—" : value;
    private static string Csv(string value) => $"\"{(value ?? "").Replace("\"", "\"\"")}\"";
    private sealed record RegisterRow(int RmaId, string CaseNumber, string Status, string Date, string User, string Notes, string RmaStatus);
}
