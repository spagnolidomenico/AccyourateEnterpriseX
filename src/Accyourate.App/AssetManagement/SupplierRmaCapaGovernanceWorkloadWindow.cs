using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.DesignSystem;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class SupplierRmaCapaGovernanceWorkloadWindow : Window
{
    private readonly SupplierRmaCapaGovernanceActionService _service = new();
    private readonly StackPanel _content = new();
    private readonly TextBlock _message = new();
    private readonly ComboBox _owner = new() { MinWidth = 210 };
    private readonly ComboBox _priority = new() { ItemsSource = new[] { "Tutte le priorita", "Bassa", "Media", "Alta", "Critica" }, SelectedIndex = 0, MinWidth = 170 };
    private readonly ComboBox _status = new() { ItemsSource = new[] { "Azioni attive", "Tutti gli stati", "Aperta", "In corso", "Scaduta", "In scadenza", "Completata" }, SelectedIndex = 0, MinWidth = 170 };

    public SupplierRmaCapaGovernanceWorkloadWindow()
    {
        Title = "Carichi di lavoro Governance CAPA";
        Width = 1320;
        Height = 790;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Content = Build();
        _owner.SelectionChanged += (_, _) => Load();
        _priority.SelectionChanged += (_, _) => Load();
        _status.SelectionChanged += (_, _) => Load();
        RefreshOwners();
        Load();
    }

    private Control Build()
    {
        var root = new DockPanel { Margin = new Thickness(24) };
        var header = AxResponsivePageHeader.Create(
            "Carichi di lavoro Governance CAPA",
            "Azioni assegnate, scadenze e priorita per responsabile.",
            SupplierRmaCorrectiveActionsWindow.Button("Aggiorna", Refresh, true));
        header.Margin = new Thickness(0, 0, 0, 12);
        DockPanel.SetDock(header, Dock.Top); root.Children.Add(header);

        var filters = AxResponsiveFilterBar.Create(_owner, _priority, _status);
        filters.Margin = new Thickness(0, 0, 0, 2);
        DockPanel.SetDock(filters, Dock.Top); root.Children.Add(filters);
        DockPanel.SetDock(_message, Dock.Top); root.Children.Add(_message);
        root.Children.Add(new ScrollViewer { Content = _content, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled });
        return root;
    }

    private void Refresh()
    {
        var selected = _owner.SelectedItem?.ToString();
        RefreshOwners(selected);
        Load();
    }

    private void RefreshOwners(string? selected = null)
    {
        var owners = new[] { "Tutti i responsabili" }.Concat(_service.GetAll().Select(x => x.Owner).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x)).ToArray();
        _owner.ItemsSource = owners;
        _owner.SelectedItem = selected is not null && owners.Contains(selected, StringComparer.OrdinalIgnoreCase) ? owners.First(x => string.Equals(x, selected, StringComparison.OrdinalIgnoreCase)) : owners[0];
    }

    private void Load()
    {
        try
        {
            var all = _service.GetAll().Where(x => x.SourceType == "Criticita Governance CAPA").ToList();
            var owner = _owner.SelectedItem?.ToString() ?? "Tutti i responsabili";
            var priority = _priority.SelectedItem?.ToString() ?? "Tutte le priorita";
            var status = _status.SelectedItem?.ToString() ?? "Azioni attive";
            var values = all
                .Where(x => owner == "Tutti i responsabili" || string.Equals(x.Owner, owner, StringComparison.OrdinalIgnoreCase))
                .Where(x => priority == "Tutte le priorita" || x.Priority == priority)
                .Where(x => status switch { "Azioni attive" => x.Status != "Completata", "Scaduta" => x.IsOverdue, "In scadenza" => x.IsDueSoon && !x.IsOverdue, "Tutti gli stati" => true, _ => x.Status == status })
                .ToList();

            _content.Children.Clear(); _content.Spacing = 16; _content.MinWidth = 0;
            var cards = new WrapPanel();
            cards.Children.Add(Card("Azioni attive", all.Count(x => x.Status != "Completata"), UiTokens.BrandBlue));
            cards.Children.Add(Card("Scadute", all.Count(x => x.IsOverdue), UiTokens.Danger));
            cards.Children.Add(Card("In scadenza", all.Count(x => x.IsDueSoon && !x.IsOverdue), UiTokens.Warning));
            cards.Children.Add(Card("Priorita critica", all.Count(x => x.Status != "Completata" && x.Priority == "Critica"), UiTokens.Danger));
            cards.Children.Add(Card("Responsabili attivi", all.Where(x => x.Status != "Completata").Select(x => x.Owner).Distinct(StringComparer.OrdinalIgnoreCase).Count(), UiTokens.Success));
            _content.Children.Add(cards);

            _content.Children.Add(new TextBlock { Text = "Carico per responsabile", FontSize = 20, FontWeight = FontWeight.Bold });
            var ownerRows = new StackPanel();
            var grouped = all.Where(x => x.Status != "Completata").GroupBy(x => x.Owner, StringComparer.OrdinalIgnoreCase).OrderByDescending(x => x.Count()).ThenBy(x => x.Key).ToList();
            if (grouped.Count == 0) ownerRows.Children.Add(Empty("Nessuna azione attiva assegnata."));
            foreach (var group in grouped) ownerRows.Children.Add(OwnerRow(group.Key, group.ToList()));
            _content.Children.Add(ownerRows);

            _content.Children.Add(new TextBlock { Text = "Azioni", FontSize = 20, FontWeight = FontWeight.Bold });
            var rows = new StackPanel { Spacing = 8 };
            foreach (var item in values) rows.Children.Add(ActionRow(item));
            if (values.Count == 0) rows.Children.Add(Empty("Nessuna azione corrisponde ai filtri selezionati."));
            _content.Children.Add(rows);
            _message.Text = $"{values.Count} azioni visualizzate su {all.Count} totali";
            _message.Foreground = UiTokens.Brush(all.Any(x => x.IsOverdue) ? UiTokens.Danger : UiTokens.TextSecondary);
            _message.Margin = new Thickness(0, 0, 0, 10);
        }
        catch (Exception ex) { _message.Text = $"Carichi non disponibili: {ex.Message}"; _message.Foreground = UiTokens.Brush(UiTokens.Danger); }
    }

    private static Control OwnerRow(string owner, IReadOnlyList<SupplierRmaCapaGovernanceAction> values)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,110,110,110,140") };
        Cell(grid, string.IsNullOrWhiteSpace(owner) ? "Non assegnato" : owner, 0, true);
        Cell(grid, values.Count.ToString(), 1);
        Cell(grid, values.Count(x => x.IsOverdue).ToString(), 2, values.Any(x => x.IsOverdue), values.Any(x => x.IsOverdue) ? UiTokens.Danger : null);
        Cell(grid, values.Count(x => x.IsDueSoon && !x.IsOverdue).ToString(), 3);
        Cell(grid, values.Count(x => x.Priority == "Critica").ToString(), 4);
        return new Border { Padding = new Thickness(10), BorderBrush = UiTokens.Brush(UiTokens.Border), BorderThickness = new Thickness(0, 0, 0, 1), Child = grid };
    }

    private Control ActionRow(SupplierRmaCapaGovernanceAction item)
    {
        var root = new StackPanel { Spacing = 9 };
        root.Children.Add(new TextBlock { Text = item.Title, FontSize = 17, FontWeight = FontWeight.Bold, TextWrapping = TextWrapping.Wrap });
        var details = new WrapPanel();
        details.Children.Add(Detail("Responsabile", item.Owner, 210));
        details.Children.Add(Detail("Priorita", item.Priority, 130));
        details.Children.Add(Detail("Scadenza", Date(item.DueDate), 130, item.IsOverdue ? UiTokens.Danger : null));
        details.Children.Add(Detail("Stato", item.Status, 130));
        details.Children.Add(Detail("Esito", item.IsOverdue ? "Scaduta" : item.IsDueSoon ? "In scadenza" : "Regolare", 150, item.IsOverdue ? UiTokens.Danger : item.IsDueSoon ? UiTokens.Warning : UiTokens.Success));
        root.Children.Add(details);
        var commands = new WrapPanel();
        var start = SupplierRmaCorrectiveActionsWindow.Button("Avvia", () => Run(() => _service.Start(item.Id, Environment.UserName))); start.IsEnabled = item.Status == "Aperta"; start.Margin = new Thickness(0, 0, 5, 5); commands.Children.Add(start);
        var history = SupplierRmaCorrectiveActionsWindow.Button("Storico", () => new SupplierRmaCapaGovernanceActionHistoryWindow(item, _service).Show(this)); history.Margin = new Thickness(0, 0, 5, 5); commands.Children.Add(history);
        root.Children.Add(commands);
        return new Border { Padding = new Thickness(14), BorderBrush = UiTokens.Brush(UiTokens.Border), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8), Background = UiTokens.Brush(UiTokens.Surface), Child = root };
    }

    private void Run(Action action) { try { action(); Load(); } catch (Exception ex) { _message.Text = ex.Message; _message.Foreground = UiTokens.Brush(UiTokens.Danger); } }
    private static Control Card(string label, int value, string color) => new Border { Width = 205, Height = 92, Margin = new Thickness(0, 0, 10, 10), Padding = new Thickness(14), BorderBrush = UiTokens.Brush(UiTokens.Border), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(10), Child = new StackPanel { Children = { new TextBlock { Text = value.ToString(), FontSize = 27, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(color) }, new TextBlock { Text = label } } } };
    private static Control Empty(string text) => new Border { Padding = new Thickness(14), Child = new TextBlock { Text = text, Foreground = UiTokens.Brush(UiTokens.TextSecondary) } };
    private static Control Detail(string label, string text, double width, string? color = null) => new StackPanel { Width = width, Margin = new Thickness(0, 0, 10, 8), Spacing = 2, Children = { new TextBlock { Text = label, FontSize = 11, Foreground = UiTokens.Brush(UiTokens.TextSecondary) }, new TextBlock { Text = string.IsNullOrWhiteSpace(text) ? "—" : text, FontWeight = FontWeight.SemiBold, Foreground = color is null ? null : UiTokens.Brush(color), TextWrapping = TextWrapping.Wrap } } };
    private static string Date(string value) => DateTime.TryParse(value, out var date) ? date.ToString("dd/MM/yyyy") : value;
    private static void Cell(Grid grid, string text, int column, bool bold = false, string? color = null) { var value = new TextBlock { Text = string.IsNullOrWhiteSpace(text) ? "—" : text, FontWeight = bold ? FontWeight.Bold : FontWeight.Normal, TextWrapping = TextWrapping.Wrap, Foreground = color is null ? null : UiTokens.Brush(color), Margin = new Thickness(0, 0, 8, 0) }; Grid.SetColumn(value, column); grid.Children.Add(value); }
    private static void Add(Grid grid, Control control, int column) { control.Margin = new Thickness(0, 0, 8, 0); Grid.SetColumn(control, column); grid.Children.Add(control); }
}
