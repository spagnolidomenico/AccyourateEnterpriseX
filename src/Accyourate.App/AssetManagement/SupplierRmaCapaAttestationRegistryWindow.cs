using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class SupplierRmaCapaAttestationRegistryWindow : Window
{
    private readonly SupplierRmaCapaAttestationService _service = new();
    private readonly SupplierRmaCapaAttestationExportService _export = new();
    private readonly StackPanel _rows = new();
    private readonly TextBox _search = new() { Watermark = "Cerca pratica, approvatore, ruolo..." };
    private readonly ComboBox _status = new() { ItemsSource = new[] { "Tutti gli stati", "Valida", "Non valida", "Archivio mancante" }, SelectedIndex = 0, MinWidth = 190 };
    private readonly TextBlock _summary = new();
    private IReadOnlyList<SupplierRmaCapaAttestation> _filtered = Array.Empty<SupplierRmaCapaAttestation>();

    public SupplierRmaCapaAttestationRegistryWindow()
    {
        Title = "Registro attestazioni CAPA"; Width = 1320; Height = 760; WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Content = Build(); _search.TextChanged += (_, _) => Load(); _status.SelectionChanged += (_, _) => Load(); VerifyAll();
    }

    private Control Build()
    {
        var root = new DockPanel { Margin = new Thickness(24) };
        var head = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Margin = new Thickness(0, 0, 0, 12) };
        var title = new StackPanel { Children = { new TextBlock { Text = "Registro attestazioni CAPA", FontSize = 28, FontWeight = FontWeight.Bold }, new TextBlock { Text = "Impronte, validita, approvatori e verbali.", Foreground = UiTokens.Brush(UiTokens.TextSecondary) } } };
        Grid.SetColumn(title, 0); head.Children.Add(title);
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        actions.Children.Add(SupplierRmaCorrectiveActionsWindow.Button("Storico esportazioni", () => new SupplierRmaCapaAttestationExportHistoryWindow().Show(this)));
        actions.Children.Add(SupplierRmaCorrectiveActionsWindow.Button("Esporta CSV", () => Export(false)));
        actions.Children.Add(SupplierRmaCorrectiveActionsWindow.Button("Report PDF", () => Export(true)));
        actions.Children.Add(SupplierRmaCorrectiveActionsWindow.Button("Verifica tutte", VerifyAll, true));
        Grid.SetColumn(actions, 1); head.Children.Add(actions); DockPanel.SetDock(head, Dock.Top); root.Children.Add(head);
        var filters = new Grid { ColumnDefinitions = new ColumnDefinitions("*,190"), Margin = new Thickness(0, 0, 0, 8) };
        Add(filters, _search, 0); Add(filters, _status, 1); DockPanel.SetDock(filters, Dock.Top); root.Children.Add(filters);
        DockPanel.SetDock(_summary, Dock.Top); root.Children.Add(_summary);
        root.Children.Add(new ScrollViewer { Content = _rows, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto });
        return root;
    }

    private void VerifyAll() { _service.PublishInvalidNotifications(); Load(); }

    private void Load()
    {
        var all = _service.GetAll(); var query = (_search.Text ?? "").Trim(); var status = _status.SelectedItem?.ToString() ?? "Tutti gli stati";
        _filtered = all.Where(x => status == "Tutti gli stati" || x.ValidationStatus == status).Where(x => query.Length == 0 || $"{x.CaseNumber} {x.Approver} {x.Role} {x.Revision}".Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
        _rows.Children.Clear(); _rows.MinWidth = 1150;
        _rows.Children.Add(Row(new SupplierRmaCapaAttestation { CaseNumber = "Pratica", Revision = "Rev.", Approver = "Approvatore", Role = "Ruolo", AttestedAt = "Data" }, true));
        foreach (var item in _filtered) _rows.Children.Add(Row(item, false));
        _summary.Text = $"{_filtered.Count} attestazioni · {all.Count(x => x.IsValid)} valide · {all.Count(x => !x.IsValid)} non valide";
        _summary.Foreground = UiTokens.Brush(UiTokens.TextSecondary); _summary.Margin = new Thickness(0, 0, 0, 8);
    }

    private void Export(bool pdf)
    {
        try
        {
            var path = pdf ? _export.ExportPdf(_filtered, FilterDescription()) : _export.ExportCsv(_filtered, FilterDescription());
            _summary.Text = $"Esportazione completata: {Path.GetFileName(path)}"; _summary.Foreground = UiTokens.Brush(UiTokens.Success); Open(path);
        }
        catch (Exception exception)
        {
            _summary.Text = $"Errore esportazione: {exception.Message}"; _summary.Foreground = UiTokens.Brush(UiTokens.Danger);
        }
    }

    private string FilterDescription()
    {
        var status = _status.SelectedItem?.ToString() ?? "Tutti gli stati"; var search = (_search.Text ?? "").Trim();
        return search.Length == 0 ? $"Stato: {status}" : $"Stato: {status}; ricerca: {search}";
    }

    private Control Row(SupplierRmaCapaAttestation item, bool header)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("170,70,170,170,150,140,260") };
        Cell(grid, item.CaseNumber, 0, header); Cell(grid, item.Revision, 1, header); Cell(grid, item.Approver, 2, header); Cell(grid, item.Role, 3, header); Cell(grid, header ? item.AttestedAt : Date(item.AttestedAt), 4, header); Cell(grid, header ? "Stato" : item.ValidationStatus, 5, true);
        if (!header)
        {
            var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
            var archive = SupplierRmaCorrectiveActionsWindow.Button("Archivio", () => Open(item.ArchivePath)); archive.IsEnabled = item.ArchiveAvailable; actions.Children.Add(archive);
            var report = SupplierRmaCorrectiveActionsWindow.Button("Verbale", () => Open(item.ReportPath)); report.IsEnabled = item.ReportAvailable; actions.Children.Add(report); Add(grid, actions, 6);
        }
        return new Border { Padding = new Thickness(9), Background = UiTokens.Brush(header ? UiTokens.SurfaceAlt : UiTokens.Surface), BorderBrush = UiTokens.Brush(UiTokens.Border), BorderThickness = new Thickness(0, 0, 0, 1), Child = grid };
    }

    private static void Open(string path) { if (File.Exists(path)) Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true }); }
    private static string Date(string value) => DateTime.TryParse(value, out var date) ? date.ToString("dd/MM/yyyy HH:mm") : value;
    private static void Cell(Grid grid, string text, int column, bool strong = false) => Add(grid, new TextBlock { Text = text, FontWeight = strong ? FontWeight.Bold : FontWeight.Normal, TextTrimming = TextTrimming.CharacterEllipsis }, column);
    private static void Add(Grid grid, Control control, int column) { control.Margin = new Thickness(0, 0, 8, 0); Grid.SetColumn(control, column); grid.Children.Add(control); }
}
