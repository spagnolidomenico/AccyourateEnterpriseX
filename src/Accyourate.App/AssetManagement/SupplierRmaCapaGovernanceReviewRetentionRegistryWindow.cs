using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class SupplierRmaCapaGovernanceReviewRetentionRegistryWindow : Window
{
    private readonly SupplierRmaCapaGovernanceReviewService _reviews = new();
    private readonly SupplierRmaCapaGovernanceReviewRetentionService _retention = new();
    private readonly TextBox _search = new() { Watermark = "Cerca numero riesame, responsabile o custode..." };
    private readonly ComboBox _status = new() { ItemsSource = new[] { "Tutti gli stati", "Valida", "In scadenza", "Scaduta", "Non valida", "Superata" }, SelectedIndex = 0 };
    private readonly StackPanel _kpis = new() { Orientation = Orientation.Horizontal, Spacing = 10 };
    private readonly StackPanel _rows = new();
    private readonly TextBlock _summary = new();
    private readonly TextBlock _message = new();
    private List<Entry> _entries = new();

    public SupplierRmaCapaGovernanceReviewRetentionRegistryWindow()
    {
        Title = "Registro conservazioni riesami Governance CAPA";
        Width = 1480;
        Height = 780;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Content = Build();
        _search.TextChanged += (_, _) => Render();
        _status.SelectionChanged += (_, _) => Render();
        LoadData();
    }

    private Control Build()
    {
        var root = new DockPanel { Margin = new Thickness(24) };
        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Margin = new Thickness(0, 0, 0, 14) };
        Add(header, new StackPanel
        {
            Spacing = 3,
            Children =
            {
                new TextBlock { Text = "Registro conservazioni Governance CAPA", FontSize = 28, FontWeight = FontWeight.Bold },
                new TextBlock { Text = "Audit centralizzato, scadenze e integrita degli archivi dei riesami.", Foreground = UiTokens.Brush(UiTokens.TextSecondary) }
            }
        }, 0);
        var commands = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        commands.Children.Add(SupplierRmaCorrectiveActionsWindow.Button("Report PDF", ExportReport, true));
        commands.Children.Add(SupplierRmaCorrectiveActionsWindow.Button("Verifica integrita", LoadData, true));
        commands.Children.Add(SupplierRmaCorrectiveActionsWindow.Button("Aggiorna", LoadData));
        Add(header, commands, 1);
        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        DockPanel.SetDock(_kpis, Dock.Top);
        root.Children.Add(_kpis);
        var filters = new Grid { ColumnDefinitions = new ColumnDefinitions("*,210"), Margin = new Thickness(0, 12, 0, 8) };
        Add(filters, _search, 0);
        Add(filters, _status, 1);
        DockPanel.SetDock(filters, Dock.Top);
        root.Children.Add(filters);
        DockPanel.SetDock(_message, Dock.Top);
        root.Children.Add(_message);
        DockPanel.SetDock(_summary, Dock.Top);
        root.Children.Add(_summary);
        root.Children.Add(new ScrollViewer
        {
            Content = _rows,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        });
        return root;
    }

    private void ExportReport()
    {
        try
        {
            var path = new SupplierRmaCapaGovernanceReviewRetentionReportService().Export(_search.Text ?? "", _status.SelectedItem?.ToString() ?? "Tutti gli stati");
            Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
            _message.Text = "Report audit generato correttamente.";
            _message.Foreground = UiTokens.Brush(UiTokens.Success);
        }
        catch (Exception exception)
        {
            _message.Text = $"Errore generazione report: {exception.Message}";
            _message.Foreground = UiTokens.Brush(UiTokens.Danger);
        }
    }

    private void LoadData()
    {
        try
        {
            _retention.PublishAlerts();
            _entries = _reviews.GetAll()
                .SelectMany(review => _retention.GetAll(review.Id).Select(record => new Entry(review, record)))
                .OrderByDescending(x => x.Record.Id)
                .ToList();
            _message.Text = "";
            RenderKpis();
            Render();
        }
        catch (Exception exception)
        {
            _message.Text = $"Errore caricamento registro: {exception.Message}";
            _message.Foreground = UiTokens.Brush(UiTokens.Danger);
        }
    }

    private void RenderKpis()
    {
        _kpis.Children.Clear();
        _kpis.Children.Add(Kpi("Conservazioni", _entries.Count, UiTokens.BrandBlue));
        _kpis.Children.Add(Kpi("Valide", _entries.Count(x => x.Record.ValidationStatus == "Valida"), UiTokens.Success));
        _kpis.Children.Add(Kpi("In scadenza", _entries.Count(x => x.Record.ValidationStatus == "In scadenza"), UiTokens.Warning));
        _kpis.Children.Add(Kpi("Scadute", _entries.Count(x => x.Record.ValidationStatus == "Conservazione scaduta"), UiTokens.Danger));
        _kpis.Children.Add(Kpi("Non valide", _entries.Count(x => x.Record.ValidationStatus is "Archivio mancante" or "Archivio modificato"), UiTokens.Danger));
        _kpis.Children.Add(Kpi("Superate", _entries.Count(x => x.Record.ValidationStatus == "Conservazione superata"), UiTokens.TextSecondary));
    }

    private void Render()
    {
        var term = (_search.Text ?? "").Trim();
        var selected = _status.SelectedItem?.ToString() ?? "Tutti gli stati";
        var filtered = _entries.Where(x => MatchesSearch(x, term) && MatchesStatus(x.Record, selected)).ToList();
        _rows.Children.Clear();
        _rows.MinWidth = 1320;
        _rows.Children.Add(Header());
        foreach (var entry in filtered) _rows.Children.Add(Row(entry));
        _summary.Text = $"{filtered.Count} di {_entries.Count} conservazioni visualizzate";
        _summary.Foreground = UiTokens.Brush(UiTokens.TextSecondary);
        _summary.Margin = new Thickness(0, 0, 0, 8);
    }

    private static bool MatchesSearch(Entry entry, string term) => string.IsNullOrWhiteSpace(term)
        || entry.Review.Id.ToString("D6").Contains(term, StringComparison.OrdinalIgnoreCase)
        || entry.Review.Reviewer.Contains(term, StringComparison.OrdinalIgnoreCase)
        || entry.Record.Custodian.Contains(term, StringComparison.OrdinalIgnoreCase)
        || entry.Review.Outcome.Contains(term, StringComparison.OrdinalIgnoreCase);

    private static bool MatchesStatus(SupplierRmaCapaGovernanceReviewRetentionRecord record, string selected) => selected switch
    {
        "Valida" => record.ValidationStatus == "Valida",
        "In scadenza" => record.ValidationStatus == "In scadenza",
        "Scaduta" => record.ValidationStatus == "Conservazione scaduta",
        "Non valida" => record.ValidationStatus is "Archivio mancante" or "Archivio modificato",
        "Superata" => record.ValidationStatus == "Conservazione superata",
        _ => true
    };

    private static Control Kpi(string label, int value, string color) => new Border
    {
        Width = 190,
        Padding = new Thickness(14, 10),
        Background = UiTokens.Brush(UiTokens.Surface),
        BorderBrush = UiTokens.Brush(UiTokens.Border),
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(10),
        Child = new StackPanel
        {
            Children =
            {
                new TextBlock { Text = value.ToString(), FontSize = 24, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(color) },
                new TextBlock { Text = label, Foreground = UiTokens.Brush(UiTokens.TextSecondary) }
            }
        }
    };

    private static Control Header()
    {
        var grid = GridLayout();
        Cell(grid, "Riesame", 0, true); Cell(grid, "Rev.", 1, true); Cell(grid, "Responsabile", 2, true);
        Cell(grid, "Custode", 3, true); Cell(grid, "Archiviazione", 4, true); Cell(grid, "Conservazione", 5, true);
        Cell(grid, "Stato", 6, true); Cell(grid, "SHA-256", 7, true); Cell(grid, "Comandi", 8, true);
        return new Border { Padding = new Thickness(9), Background = UiTokens.Brush(UiTokens.SurfaceAlt), Child = grid };
    }

    private static Control Row(Entry entry)
    {
        var record = entry.Record;
        var grid = GridLayout();
        Cell(grid, entry.Review.Id.ToString("D6"), 0, true); Cell(grid, $"R{record.Revision}", 1, true);
        Cell(grid, entry.Review.Reviewer, 2); Cell(grid, record.Custodian, 3); Cell(grid, DateTimeValue(record.ArchivedAt), 4);
        Cell(grid, DateValue(record.RetentionUntil), 5, record.IsExpired); Cell(grid, record.ValidationStatus, 6, !record.IsValid);
        Cell(grid, record.ArchiveHash.Length > 14 ? record.ArchiveHash[..14] + "..." : record.ArchiveHash, 7);
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5 };
        var open = SupplierRmaCorrectiveActionsWindow.Button("Archivio", () => Open(record.ArchivePath), true);
        open.IsEnabled = record.ArchiveAvailable;
        actions.Children.Add(open);
        Add(grid, actions, 8);
        return new Border { Padding = new Thickness(9), BorderBrush = UiTokens.Brush(UiTokens.Border), BorderThickness = new Thickness(0, 0, 0, 1), Child = grid };
    }

    private static Grid GridLayout() => new() { ColumnDefinitions = new ColumnDefinitions("90,65,170,170,145,145,170,170,120") };
    private static void Open(string path) { if (File.Exists(path)) Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true }); }
    private static string DateValue(string value) => DateTime.TryParse(value, out var date) ? date.ToString("dd/MM/yyyy") : value;
    private static string DateTimeValue(string value) => DateTime.TryParse(value, out var date) ? date.ToString("dd/MM/yyyy HH:mm") : value;
    private static void Cell(Grid grid, string text, int column, bool alert = false) => Add(grid, new TextBlock
    {
        Text = text,
        FontWeight = alert ? FontWeight.Bold : FontWeight.Normal,
        Foreground = alert ? UiTokens.Brush(UiTokens.Danger) : UiTokens.Brush(UiTokens.TextPrimary),
        TextTrimming = TextTrimming.CharacterEllipsis
    }, column);
    private static void Add(Grid grid, Control control, int column) { control.Margin = new Thickness(0, 0, 8, 0); Grid.SetColumn(control, column); grid.Children.Add(control); }
    private sealed record Entry(SupplierRmaCapaGovernanceReview Review, SupplierRmaCapaGovernanceReviewRetentionRecord Record);
}
