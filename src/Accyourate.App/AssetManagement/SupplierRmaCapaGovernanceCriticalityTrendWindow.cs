using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class SupplierRmaCapaGovernanceCriticalityTrendWindow : Window
{
    private readonly SupplierRmaCapaGovernanceCriticalityTrendService _service = new();
    private readonly StackPanel _content = new();
    private readonly TextBlock _message = new();

    public SupplierRmaCapaGovernanceCriticalityTrendWindow()
    {
        Title = "Trend criticita Governance CAPA"; Width = 1180; Height = 760; WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Content = Build(); var captured = _service.CaptureDaily("Automazione Governance CAPA"); Load();
        if (captured) { _message.Text = "Rilevazione giornaliera automatica registrata."; _message.Foreground = UiTokens.Brush(UiTokens.Success); }
    }

    private Control Build()
    {
        var root = new DockPanel { Margin = new Thickness(24) };
        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Margin = new Thickness(0, 0, 0, 14) };
        var title = new StackPanel { Children = { new TextBlock { Text = "Storico e trend criticita CAPA", FontSize = 28, FontWeight = FontWeight.Bold }, new TextBlock { Text = "Rilevazioni consolidate per confrontare l'andamento della governance.", Foreground = UiTokens.Brush(UiTokens.TextSecondary) } } };
        Grid.SetColumn(title, 0); header.Children.Add(title);
        var commands = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        commands.Children.Add(SupplierRmaCorrectiveActionsWindow.Button("Registra rilevazione", Capture, true)); commands.Children.Add(SupplierRmaCorrectiveActionsWindow.Button("Esporta CSV", () => Export(false))); commands.Children.Add(SupplierRmaCorrectiveActionsWindow.Button("Report PDF", () => Export(true))); commands.Children.Add(SupplierRmaCorrectiveActionsWindow.Button("Rimuovi duplicati", RemoveDuplicates)); commands.Children.Add(SupplierRmaCorrectiveActionsWindow.Button("Aggiorna", Load));
        Grid.SetColumn(commands, 1); header.Children.Add(commands); DockPanel.SetDock(header, Dock.Top); root.Children.Add(header);
        _message.Margin = new Thickness(0, 0, 0, 10); DockPanel.SetDock(_message, Dock.Top); root.Children.Add(_message);
        root.Children.Add(new ScrollViewer { Content = _content, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto }); return root;
    }

    private void Capture()
    {
        try { var created = _service.Capture(Environment.UserName); Load(); _message.Text = created ? "Rilevazione registrata." : "Nessuna variazione: rilevazione non duplicata."; _message.Foreground = UiTokens.Brush(created ? UiTokens.Success : UiTokens.TextSecondary); }
        catch (Exception ex) { _message.Text = ex.Message; _message.Foreground = UiTokens.Brush(UiTokens.Danger); }
    }

    private void RemoveDuplicates()
    {
        try { var removed = _service.RemoveConsecutiveDuplicates(); Load(); _message.Text = removed == 0 ? "Nessun duplicato rilevato." : $"{removed} rilevazioni duplicate eliminate."; _message.Foreground = UiTokens.Brush(removed == 0 ? UiTokens.TextSecondary : UiTokens.Success); }
        catch (Exception ex) { _message.Text = ex.Message; _message.Foreground = UiTokens.Brush(UiTokens.Danger); }
    }

    private void Export(bool pdf)
    {
        try { var path = pdf ? _service.ExportPdf() : _service.ExportCsv(); _message.Text = $"File creato: {Path.GetFileName(path)}"; _message.Foreground = UiTokens.Brush(UiTokens.Success); Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true }); }
        catch (Exception ex) { _message.Text = $"Esportazione non riuscita: {ex.Message}"; _message.Foreground = UiTokens.Brush(UiTokens.Danger); }
    }

    private void Load()
    {
        try
        {
            var values = _service.GetAll(); _content.Children.Clear(); _content.Spacing = 12;
            if (values.Count == 0) { _content.Children.Add(new TextBlock { Text = "Nessuna rilevazione presente. Premi Registra rilevazione per creare la baseline.", Foreground = UiTokens.Brush(UiTokens.TextSecondary), TextWrapping = TextWrapping.Wrap }); return; }
            var latest = values[0]; var previous = values.Count > 1 ? values[1] : null;
            var cards = new WrapPanel(); cards.Children.Add(Card("Criticita", latest.CriticalCount, Delta(latest.CriticalCount, previous?.CriticalCount), latest.CriticalCount == 0 ? UiTokens.Success : UiTokens.Danger)); cards.Children.Add(Card("Avvisi", latest.WarningCount, Delta(latest.WarningCount, previous?.WarningCount), UiTokens.Warning)); cards.Children.Add(Card("Azioni attive", latest.ActiveActions, Delta(latest.ActiveActions, previous?.ActiveActions), UiTokens.BrandBlue)); cards.Children.Add(Card("Azioni scadute", latest.OverdueActions, Delta(latest.OverdueActions, previous?.OverdueActions), latest.OverdueActions == 0 ? UiTokens.Success : UiTokens.Danger)); cards.Children.Add(Card("Completate", latest.CompletedActions, Delta(latest.CompletedActions, previous?.CompletedActions), UiTokens.Success)); cards.Children.Add(Card("Verifiche fallite", latest.FailedVerifications, Delta(latest.FailedVerifications, previous?.FailedVerifications), latest.FailedVerifications == 0 ? UiTokens.Success : UiTokens.Warning)); _content.Children.Add(cards);
            _content.Children.Add(new TextBlock { Text = "Rilevazioni", FontSize = 20, FontWeight = FontWeight.Bold });
            foreach (var item in values) _content.Children.Add(Row(item));
            _message.Text = $"{values.Count} rilevazioni registrate · ultima {Date(item: latest.CapturedAt)}"; _message.Foreground = UiTokens.Brush(UiTokens.TextSecondary);
        }
        catch (Exception ex) { _message.Text = $"Trend non disponibile: {ex.Message}"; _message.Foreground = UiTokens.Brush(UiTokens.Danger); }
    }

    private static Control Card(string label, int value, string delta, string color) => new Border { Width = 170, Height = 100, Margin = new Thickness(0, 0, 10, 10), Padding = new Thickness(13), BorderBrush = UiTokens.Brush(UiTokens.Border), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(10), Child = new StackPanel { Children = { new TextBlock { Text = value.ToString(), FontSize = 25, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(color) }, new TextBlock { Text = label }, new TextBlock { Text = delta, FontSize = 11, Foreground = UiTokens.Brush(UiTokens.TextSecondary) } } } };
    private static Control Row(SupplierRmaCapaGovernanceCriticalityTrendPoint x) { var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("170,100,100,120,110,120,130,*") }; Cell(grid, Date(x.CapturedAt), 0, true); Cell(grid, x.CriticalCount.ToString(), 1); Cell(grid, x.WarningCount.ToString(), 2); Cell(grid, x.ActiveActions.ToString(), 3); Cell(grid, x.OverdueActions.ToString(), 4); Cell(grid, x.CompletedActions.ToString(), 5); Cell(grid, x.FailedVerifications.ToString(), 6); Cell(grid, x.CapturedBy, 7); return new Border { Padding = new Thickness(10), BorderBrush = UiTokens.Brush(UiTokens.Border), BorderThickness = new Thickness(0, 0, 0, 1), Child = grid }; }
    private static void Cell(Grid grid, string text, int column, bool bold = false) { var value = new TextBlock { Text = text, FontWeight = bold ? FontWeight.SemiBold : FontWeight.Normal, TextTrimming = TextTrimming.CharacterEllipsis }; Grid.SetColumn(value, column); grid.Children.Add(value); }
    private static string Delta(int current, int? previous) { if (previous is null) return "Baseline iniziale"; var value = current - previous.Value; return value == 0 ? "Nessuna variazione" : value > 0 ? $"+{value} dalla precedente" : $"{value} dalla precedente"; }
    private static string Date(string item) => DateTime.TryParse(item, out var date) ? date.ToString("dd/MM/yyyy HH:mm") : item;
}
