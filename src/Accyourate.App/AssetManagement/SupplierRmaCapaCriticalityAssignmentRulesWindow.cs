using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class SupplierRmaCapaCriticalityAssignmentRulesWindow : Window
{
    private readonly SupplierRmaCapaCriticalityAssignmentRuleService _service = new(); private readonly StackPanel _rows = new(); private readonly TextBlock _message = new();
    public SupplierRmaCapaCriticalityAssignmentRulesWindow() { Title = "Regole assegnazione criticita CAPA"; Width = 1100; Height = 720; WindowStartupLocation = WindowStartupLocation.CenterOwner; Content = Build(); Load(); }
    private Control Build() { var root = new DockPanel { Margin = new Thickness(24) }; var head = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Margin = new Thickness(0, 0, 0, 14) }; var title = new StackPanel { Children = { new TextBlock { Text = "Regole di assegnazione criticita", FontSize = 28, FontWeight = FontWeight.Bold }, new TextBlock { Text = "Responsabili, priorita e tempi predefiniti per ogni anomalia.", Foreground = UiTokens.Brush(UiTokens.TextSecondary) } } }; Grid.SetColumn(title, 0); head.Children.Add(title); var refresh = SupplierRmaCorrectiveActionsWindow.Button("Aggiorna", Load); Grid.SetColumn(refresh, 1); head.Children.Add(refresh); DockPanel.SetDock(head, Dock.Top); root.Children.Add(head); DockPanel.SetDock(_message, Dock.Top); root.Children.Add(_message); root.Children.Add(new ScrollViewer { Content = _rows }); return root; }
    private void Load() { try { _rows.Children.Clear(); _rows.Spacing = 4; foreach (var rule in _service.GetAll()) _rows.Children.Add(Row(rule)); _message.Text = "Le regole vengono applicate alle nuove prese in carico."; _message.Foreground = UiTokens.Brush(UiTokens.TextSecondary); _message.Margin = new Thickness(0, 0, 0, 10); } catch (Exception ex) { _message.Text = ex.Message; _message.Foreground = UiTokens.Brush(UiTokens.Danger); } }
    private Control Row(SupplierRmaCapaCriticalityAssignmentRule rule) { var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,220,100,100,120") }; Cell(grid, rule.Criticality, 0, true); Cell(grid, rule.DefaultOwner, 1); Cell(grid, rule.Priority, 2); Cell(grid, $"{rule.DueDays} gg", 3); var edit = SupplierRmaCorrectiveActionsWindow.Button("Modifica", () => new SupplierRmaCapaCriticalityAssignmentRuleDialog(rule, _service, Load).Show(this), true); Grid.SetColumn(edit, 4); grid.Children.Add(edit); return new Border { Padding = new Thickness(10, 8), BorderBrush = UiTokens.Brush(UiTokens.Border), BorderThickness = new Thickness(0, 0, 0, 1), Child = grid }; }
    private static void Cell(Grid grid, string text, int column, bool bold = false) { var value = new TextBlock { Text = text, FontWeight = bold ? FontWeight.SemiBold : FontWeight.Normal, TextWrapping = TextWrapping.Wrap }; Grid.SetColumn(value, column); grid.Children.Add(value); }
}

public sealed class SupplierRmaCapaCriticalityAssignmentRuleDialog : Window
{
    private readonly SupplierRmaCapaCriticalityAssignmentRule _rule; private readonly SupplierRmaCapaCriticalityAssignmentRuleService _service; private readonly Action _saved; private readonly TextBox _owner = new(); private readonly ComboBox _priority = new() { ItemsSource = new[] { "Bassa", "Media", "Alta", "Critica" } }; private readonly TextBox _days = new(); private readonly TextBlock _message = new();
    public SupplierRmaCapaCriticalityAssignmentRuleDialog(SupplierRmaCapaCriticalityAssignmentRule rule, SupplierRmaCapaCriticalityAssignmentRuleService service, Action saved) { _rule = rule; _service = service; _saved = saved; _owner.Text = rule.DefaultOwner; _priority.SelectedItem = rule.Priority; _days.Text = rule.DueDays.ToString(); Title = "Modifica regola assegnazione"; Width = 580; Height = 440; WindowStartupLocation = WindowStartupLocation.CenterOwner; var root = new StackPanel { Margin = new Thickness(24), Spacing = 9, Children = { new TextBlock { Text = "Regola di assegnazione", FontSize = 25, FontWeight = FontWeight.Bold }, new TextBlock { Text = rule.Criticality, TextWrapping = TextWrapping.Wrap, FontWeight = FontWeight.SemiBold } } }; Field(root, "Responsabile predefinito", _owner); Field(root, "Priorita", _priority); Field(root, "Scadenza entro (giorni)", _days); root.Children.Add(_message); root.Children.Add(SupplierRmaCorrectiveActionsWindow.Button("Salva regola", Save, true)); Content = root; }
    private void Save() { try { if (!int.TryParse(_days.Text, out var days)) throw new InvalidOperationException("Indica un numero valido di giorni."); _service.Save(_rule.Criticality, _owner.Text ?? "", _priority.SelectedItem?.ToString() ?? "Alta", days, Environment.UserName); _saved(); Close(); } catch (Exception ex) { _message.Text = ex.Message; _message.Foreground = UiTokens.Brush(UiTokens.Danger); } }
    private static void Field(Panel root, string label, Control control) { root.Children.Add(new TextBlock { Text = label, FontWeight = FontWeight.SemiBold }); root.Children.Add(control); }
}
