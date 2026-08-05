using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Models;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class SupplierRmaAuditDashboardWindow : Window
{
    private readonly SparePartRmaRepository _rma = new();
    private readonly SupplierRmaValidationService _validation = new();
    private readonly StackPanel _kpis = new() { Orientation = Orientation.Horizontal, Spacing = 10 };
    private readonly StackPanel _attention = new();
    private readonly StackPanel _recent = new();
    private readonly TextBlock _message = new();

    public SupplierRmaAuditDashboardWindow()
    {
        Title = "Dashboard audit RMA"; Width = 1260; Height = 780; MinWidth = 980; MinHeight = 620;
        WindowStartupLocation = WindowStartupLocation.CenterOwner; Content = Build(); LoadData();
    }

    private Control Build()
    {
        var root = new DockPanel { Margin = new Thickness(24) };
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        actions.Children.Add(Button("Registro validazioni", () => new SupplierRmaValidationRegisterWindow().Show(this)));
        actions.Children.Add(Button("Aggiorna", LoadData, true));
        var head = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Margin = new Thickness(0, 0, 0, 14) };
        Add(head, new StackPanel { Spacing = 3, Children = { new TextBlock { Text = "Dashboard audit RMA", FontSize = 28, FontWeight = FontWeight.Bold }, new TextBlock { Text = "Completezza dei fascicoli, scadenze e tempi di chiusura.", Foreground = UiTokens.Brush(UiTokens.TextSecondary) } } }, 0); Add(head, actions, 1);
        DockPanel.SetDock(head, Dock.Top); root.Children.Add(head);
        _kpis.Margin = new Thickness(0, 0, 0, 12); DockPanel.SetDock(_kpis, Dock.Top); root.Children.Add(_kpis);
        DockPanel.SetDock(_message, Dock.Top); root.Children.Add(_message);
        var content = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*"), RowDefinitions = new RowDefinitions("*") };
        Add(content, Panel("Richiedono attenzione", _attention), 0); Add(content, Panel("Validazioni recenti", _recent), 1);
        root.Children.Add(content); return root;
    }

    private void LoadData()
    {
        try
        {
            var cases = _rma.GetAll(); var closures = _validation.GetClosures();
            var validatedIds = closures.Select(x => x.RmaId).Distinct().ToHashSet();
            var complete = cases.Count(x => validatedIds.Contains(x.Id));
            var notValidated = cases.Count - complete;
            var overdue = cases.Count(IsOverdue);
            var closedWithoutValidation = cases.Count(x => x.Status == SparePartRmaStatus.Closed && !validatedIds.Contains(x.Id));
            var durations = closures.GroupBy(x => x.RmaId).Select(x => x.OrderByDescending(y => Parse(y.ClosedAt)).First()).Join(cases, x => x.RmaId, x => x.Id, (closure, rma) => (Start: Parse(rma.CreatedAt), End: Parse(closure.ClosedAt))).Where(x => x.Start > DateTime.MinValue && x.End >= x.Start).Select(x => (x.End - x.Start).TotalDays).ToList();
            _kpis.Children.Clear();
            _kpis.Children.Add(Kpi("Pratiche RMA", cases.Count, UiTokens.BrandBlue));
            _kpis.Children.Add(Kpi("Fascicoli completi", complete, UiTokens.Success));
            _kpis.Children.Add(Kpi("Non validati", notValidated, notValidated > 0 ? UiTokens.Warning : UiTokens.Success));
            _kpis.Children.Add(Kpi("Scaduti", overdue, overdue > 0 ? UiTokens.Danger : UiTokens.Success));
            _kpis.Children.Add(Kpi("Chiusi non validati", closedWithoutValidation, closedWithoutValidation > 0 ? UiTokens.Danger : UiTokens.Success));
            _kpis.Children.Add(Kpi("Tempo medio", durations.Count == 0 ? "—" : $"{durations.Average():N1} gg", UiTokens.BrandBlue));
            BuildAttention(cases, validatedIds); BuildRecent(closures); _message.Text = "";
        }
        catch (Exception ex) { _message.Text = $"Dashboard non caricata: {ex.Message}"; _message.Foreground = UiTokens.Brush(UiTokens.Danger); }
    }

    private void BuildAttention(IReadOnlyList<SparePartRmaCase> cases, HashSet<int> validatedIds)
    {
        _attention.Children.Clear();
        var values = cases.Where(x => IsOverdue(x) || !validatedIds.Contains(x.Id)).OrderByDescending(IsOverdue).ThenBy(x => Parse(x.DueDate)).Take(12).ToList();
        if (values.Count == 0) { _attention.Children.Add(Empty("Nessuna criticità rilevata.")); return; }
        foreach (var item in values)
        {
            var reason = IsOverdue(item) ? "Pratica scaduta" : item.Status == SparePartRmaStatus.Closed ? "Chiusa senza validazione" : "Fascicolo non validato";
            var color = IsOverdue(item) || item.Status == SparePartRmaStatus.Closed ? UiTokens.Danger : UiTokens.Warning;
            _attention.Children.Add(Line(item.CaseNumber, $"{reason} · Stato {item.Status}", color, null));
        }
    }

    private void BuildRecent(IReadOnlyList<SupplierRmaDossierClosure> closures)
    {
        _recent.Children.Clear(); var values = closures.Take(12).ToList();
        if (values.Count == 0) { _recent.Children.Add(Empty("Nessuna validazione registrata.")); return; }
        foreach (var item in values)
        {
            var path = SupplierRmaValidationService.DossierPath(item.CaseNumber);
            _recent.Children.Add(Line(item.CaseNumber, $"{Date(item.ClosedAt)} · {item.ClosedBy} · {item.ValidationStatus}", UiTokens.Success, File.Exists(path) ? () => Open(path) : null));
        }
    }

    private static Control Panel(string title, Control child) => new Border { Margin = new Thickness(0, 0, 10, 0), Padding = new Thickness(14), Background = UiTokens.Brush(UiTokens.Surface), BorderBrush = UiTokens.Brush(UiTokens.Border), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(12), Child = new StackPanel { Spacing = 10, Children = { new TextBlock { Text = title, FontSize = 18, FontWeight = FontWeight.Bold }, new ScrollViewer { Content = child, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto } } } };
    private static Control Line(string title, string detail, string color, Action? open)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("8,*,Auto"), Margin = new Thickness(0, 0, 0, 4) };
        Add(grid, new Border { Width = 4, Background = UiTokens.Brush(color), CornerRadius = new CornerRadius(2) }, 0);
        Add(grid, new StackPanel { Spacing = 2, Children = { new TextBlock { Text = title, FontWeight = FontWeight.SemiBold }, new TextBlock { Text = detail, Foreground = UiTokens.Brush(UiTokens.TextSecondary), TextWrapping = TextWrapping.Wrap } } }, 1);
        if (open is not null) Add(grid, Button("Apri", open), 2);
        return new Border { Padding = new Thickness(8), BorderBrush = UiTokens.Brush(UiTokens.Border), BorderThickness = new Thickness(0, 0, 0, 1), Child = grid };
    }

    private static Control Kpi(string label, int value, string color) => Kpi(label, value.ToString(), color);
    private static Control Kpi(string label, string value, string color) => new Border { Width = 180, Padding = new Thickness(13, 10), Background = UiTokens.Brush(UiTokens.Surface), BorderBrush = UiTokens.Brush(UiTokens.Border), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(12), Child = new StackPanel { Spacing = 2, Children = { new TextBlock { Text = value, FontSize = 23, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(color) }, new TextBlock { Text = label, Foreground = UiTokens.Brush(UiTokens.TextSecondary) } } } };
    private static TextBlock Empty(string text) => new() { Text = text, Foreground = UiTokens.Brush(UiTokens.TextSecondary), Margin = new Thickness(6) };
    private static bool IsOverdue(SparePartRmaCase item) => item.Status is not (SparePartRmaStatus.Closed or SparePartRmaStatus.Cancelled) && DateTime.TryParse(item.DueDate, out var due) && due.Date < DateTime.Today;
    private static DateTime Parse(string value) => DateTime.TryParse(value, out var date) ? date : DateTime.MinValue;
    private static string Date(string value) => DateTime.TryParse(value, out var date) ? date.ToString("dd/MM/yyyy HH:mm") : "—";
    private static void Open(string path) => Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
    private static Button Button(string text, Action action, bool primary = false) { var button = new Button { Content = text, MinHeight = 34, Background = UiTokens.Brush(primary ? UiTokens.BrandBlue : UiTokens.SurfaceAlt), Foreground = primary ? Brushes.White : UiTokens.Brush(UiTokens.TextPrimary) }; button.Click += (_, _) => action(); return button; }
    private static void Add(Grid grid, Control control, int column) { control.Margin = new Thickness(0, 0, 8, 0); Grid.SetColumn(control, column); grid.Children.Add(control); }
}
