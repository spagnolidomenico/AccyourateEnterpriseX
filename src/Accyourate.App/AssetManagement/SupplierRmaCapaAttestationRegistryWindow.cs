using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.DesignSystem;
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
        var head = AxResponsivePageHeader.Create("Registro attestazioni CAPA", "Impronte, validita, approvatori e verbali.", SupplierRmaCorrectiveActionsWindow.Button("Storico esportazioni", () => new SupplierRmaCapaAttestationExportHistoryWindow().Show(this)), SupplierRmaCorrectiveActionsWindow.Button("Esporta CSV", () => Export(false)), SupplierRmaCorrectiveActionsWindow.Button("Report PDF", () => Export(true)), SupplierRmaCorrectiveActionsWindow.Button("Verifica tutte", VerifyAll, true));
        head.Margin = new Thickness(0, 0, 0, 12); DockPanel.SetDock(head, Dock.Top); root.Children.Add(head);
        _search.MinWidth = 320; _status.MinWidth = 190;
        var filters = AxResponsiveFilterBar.Create(_search, _status); DockPanel.SetDock(filters, Dock.Top); root.Children.Add(filters);
        DockPanel.SetDock(_summary, Dock.Top); root.Children.Add(_summary);
        root.Children.Add(new ScrollViewer { Content = _rows, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto });
        return root;
    }

    private void VerifyAll() { _service.PublishInvalidNotifications(); Load(); }

    private void Load()
    {
        var all = _service.GetAll(); var query = (_search.Text ?? "").Trim(); var status = _status.SelectedItem?.ToString() ?? "Tutti gli stati";
        _filtered = all.Where(x => status == "Tutti gli stati" || x.ValidationStatus == status).Where(x => query.Length == 0 || $"{x.CaseNumber} {x.Approver} {x.Role} {x.Revision}".Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
        _rows.Children.Clear(); _rows.MinWidth = 0; _rows.Spacing = 8;
        foreach (var item in _filtered) _rows.Children.Add(Row(item));
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

    private Control Row(SupplierRmaCapaAttestation item)
    {
        var archive = SupplierRmaCorrectiveActionsWindow.Button("Archivio", () => Open(item.ArchivePath)); archive.IsEnabled = item.ArchiveAvailable;
        var report = SupplierRmaCorrectiveActionsWindow.Button("Verbale", () => Open(item.ReportPath)); report.IsEnabled = item.ReportAvailable;
        return AxResponsiveRecordCard.Create(item.CaseNumber, new[] { new AxResponsiveRecordField("Revisione", item.Revision, 100), new AxResponsiveRecordField("Approvatore", item.Approver, 190), new AxResponsiveRecordField("Ruolo", item.Role, 170), new AxResponsiveRecordField("Data", Date(item.AttestedAt), 170), new AxResponsiveRecordField("Stato", item.ValidationStatus, 170, item.IsValid ? UiTokens.Success : UiTokens.Danger) }, archive, report);
    }

    private static void Open(string path) { if (File.Exists(path)) Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true }); }
    private static string Date(string value) => DateTime.TryParse(value, out var date) ? date.ToString("dd/MM/yyyy HH:mm") : value;
    private static void Cell(Grid grid, string text, int column, bool strong = false) => Add(grid, new TextBlock { Text = text, FontWeight = strong ? FontWeight.Bold : FontWeight.Normal, TextTrimming = TextTrimming.CharacterEllipsis }, column);
    private static void Add(Grid grid, Control control, int column) { control.Margin = new Thickness(0, 0, 8, 0); Grid.SetColumn(control, column); grid.Children.Add(control); }
}
