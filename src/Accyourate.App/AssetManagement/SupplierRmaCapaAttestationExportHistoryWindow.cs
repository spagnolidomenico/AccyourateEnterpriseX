using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class SupplierRmaCapaAttestationExportHistoryWindow : Window
{
    private readonly SupplierRmaCapaAttestationExportService _service = new();
    private readonly StackPanel _rows = new();
    private readonly TextBlock _summary = new();

    public SupplierRmaCapaAttestationExportHistoryWindow()
    {
        Title = "Storico esportazioni attestazioni CAPA"; Width = 1420; Height = 760; WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Content = Build(); Load();
    }

    private Control Build()
    {
        var root = new DockPanel { Margin = new Thickness(24) };
        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Margin = new Thickness(0, 0, 0, 12) };
        var title = new StackPanel { Children = { new TextBlock { Text = "Storico esportazioni attestazioni", FontSize = 28, FontWeight = FontWeight.Bold }, new TextBlock { Text = "File generati, filtri, operatore e verifica dell'integrita SHA-256.", Foreground = UiTokens.Brush(UiTokens.TextSecondary) } } };
        Grid.SetColumn(title, 0); header.Children.Add(title);
        var refresh = SupplierRmaCorrectiveActionsWindow.Button("Verifica integrita", Load, true); Grid.SetColumn(refresh, 1); header.Children.Add(refresh);
        DockPanel.SetDock(header, Dock.Top); root.Children.Add(header);
        DockPanel.SetDock(_summary, Dock.Top); root.Children.Add(_summary);
        root.Children.Add(new ScrollViewer { Content = _rows, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto });
        return root;
    }

    private void Load()
    {
        var values = _service.GetExports(); _rows.Children.Clear(); _rows.MinWidth = 1280;
        _rows.Children.Add(Row(new SupplierRmaCapaAttestationExportRecord { Format="Formato", ExportedAt="Data", ExportedBy="Operatore", FilterDescription="Filtri", RecordCount=0 }, true));
        foreach (var value in values) _rows.Children.Add(Row(value, false));
        _summary.Text = $"{values.Count} esportazioni · {values.Count(x => x.IsValid)} integre · {values.Count(x => !x.IsValid)} da verificare";
        _summary.Foreground = UiTokens.Brush(values.Any(x => !x.IsValid) ? UiTokens.Danger : UiTokens.TextSecondary); _summary.Margin = new Thickness(0, 0, 0, 10);
    }

    private Control Row(SupplierRmaCapaAttestationExportRecord item, bool header)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("80,150,140,250,90,100,100,140,230") };
        Cell(grid, item.Format, 0, header); Cell(grid, header ? item.ExportedAt : Date(item.ExportedAt), 1, header); Cell(grid, item.ExportedBy, 2, header); Cell(grid, item.FilterDescription, 3, header);
        Cell(grid, header ? "Record" : item.RecordCount.ToString(), 4, header); Cell(grid, header ? "Valide" : item.ValidCount.ToString(), 5, header); Cell(grid, header ? "Critiche" : (item.InvalidCount + item.MissingCount).ToString(), 6, header); Cell(grid, header ? "Integrita" : item.IntegrityStatus, 7, true);
        if (!header)
        {
            var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
            var open = SupplierRmaCorrectiveActionsWindow.Button("Apri file", () => Open(item.FilePath)); open.IsEnabled = item.FileAvailable; actions.Children.Add(open);
            var hash = SupplierRmaCorrectiveActionsWindow.Button("Apri hash", () => Open(item.FilePath + ".sha256")); hash.IsEnabled = File.Exists(item.FilePath + ".sha256"); actions.Children.Add(hash); Add(grid, actions, 8);
        }
        else Cell(grid, "Azioni", 8, true);
        return new Border { Padding = new Thickness(9), Background = UiTokens.Brush(header ? UiTokens.SurfaceAlt : UiTokens.Surface), BorderBrush = UiTokens.Brush(UiTokens.Border), BorderThickness = new Thickness(0, 0, 0, 1), Child = grid };
    }

    private static void Open(string path) { if (File.Exists(path)) Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true }); }
    private static string Date(string value) => DateTime.TryParse(value, out var date) ? date.ToString("dd/MM/yyyy HH:mm") : value;
    private static void Cell(Grid grid, string text, int column, bool bold = false) => Add(grid, new TextBlock { Text = text, FontWeight = bold ? FontWeight.Bold : FontWeight.Normal, TextTrimming = TextTrimming.CharacterEllipsis }, column);
    private static void Add(Grid grid, Control control, int column) { control.Margin = new Thickness(0, 0, 8, 0); Grid.SetColumn(control, column); grid.Children.Add(control); }
}
