using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.DesignSystem;
using Accyourate.App.UIFramework.Tokens;
using Accyourate.App.Platform.Relations;

namespace Accyourate.App.HumanResources.Enterprise;

public sealed class HumanResourcesEnterpriseView : UserControl
{
    private readonly HumanResourcesEnterpriseService _service = new();
    private readonly EmployeeRelationsService _relations = new();
    private readonly Action<string, string>? _navigate;
    private readonly StackPanel _employees = new();
    private readonly ContentControl _entityPage = new();
    private TextBox? _search;
    private IReadOnlyList<HumanResourcesEmployeeRow> _rows = Array.Empty<HumanResourcesEmployeeRow>();
    private HumanResourcesEmployeeRow? _selected;

    public HumanResourcesEnterpriseView(Action<string, string>? navigate = null)
    {
        _navigate = navigate;
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();
        LoadEmployees();
    }

    private Control BuildLayout()
    {
        var snapshot = _service.Load();
        var page = new StackPanel { Margin = new Thickness(24), Spacing = 18 };
        page.Children.Add(AxPageHeader.Create("Human Resources Enterprise", "Fascicolo digitale dipendenti, asset assegnati, documenti e timeline HR.", AxButton.Create("+ Nuovo dipendente", () => Navigate("human-resources", "Human Resources"), AxButtonKind.Primary), AxButton.Create("Apri HR classico", () => Navigate("human-resources", "Human Resources"), AxButtonKind.Secondary)));
        page.Children.Add(Kpis(snapshot));
        page.Children.Add(Toolbar());
        var main = new Grid { ColumnDefinitions = new ColumnDefinitions("*,18,430") };
        Add(main, EmployeesPanel(), 0, 0);
        Add(main, _entityPage, 2, 0);
        page.Children.Add(main);
        _entityPage.Content = EmployeeEntityPage(null);
        return new ScrollViewer { Content = page, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto };
    }

    private Control Kpis(HumanResourcesEnterpriseSnapshot snapshot)
    {
        var wrap = new WrapPanel { ItemWidth = 232, ItemHeight = 152 };
        wrap.Children.Add(AxKpiCard.Create("👥", "Dipendenti", snapshot.Employees.ToString(), "Anagrafica HR"));
        wrap.Children.Add(AxKpiCard.Create("✅", "Attivi", snapshot.ActiveEmployees.ToString(), "Dipendenti operativi"));
        wrap.Children.Add(AxKpiCard.Create("💻", "Asset assegnati", snapshot.AssignedAssets.ToString(), "Collegati al personale"));
        wrap.Children.Add(AxKpiCard.Create("📄", "Documenti", snapshot.Documents.ToString(), "Archivio collegato"));
        return wrap;
    }

    private Control Toolbar()
    {
        _search = AxSearchBox.Create("Cerca dipendente, matricola, reparto...", _ => LoadEmployees());
        return new AxToolbar()
            .AddLeft(_search)
            .AddLeft(Filter("Reparto"))
            .AddLeft(Filter("Ruolo"))
            .AddLeft(Filter("Stato"))
            .AddRight(AxButton.Create("Aggiorna", LoadEmployees, AxButtonKind.Secondary))
            .AddRight(AxButton.Create("Esporta", () => { }, AxButtonKind.Success));
    }

    private static ComboBox Filter(string placeholder) => new() { PlaceholderText = placeholder, MinWidth = 130, Padding = new Thickness(10, 8) };

    private Control EmployeesPanel()
    {
        var stack = new StackPanel { Spacing = 12 };
        stack.Children.Add(new TextBlock { Text = "Elenco dipendenti", FontSize = AxTypography.SectionTitle, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary) });
        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("*,130,130,110") };
        header.Children.Add(Header("Dipendente", 0)); header.Children.Add(Header("Reparto", 1)); header.Children.Add(Header("Ruolo", 2)); header.Children.Add(Header("Stato", 3));
        stack.Children.Add(new Border { Background = UiTokens.Brush(UiTokens.SurfaceAlt), CornerRadius = new CornerRadius(14), Padding = new Thickness(12), Child = header });
        stack.Children.Add(new ScrollViewer { Content = _employees, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, MaxHeight = 520 });
        return AxCard.Create(stack);
    }

    private void LoadEmployees()
    {
        _rows = _service.LoadEmployees(_search?.Text ?? string.Empty);
        _employees.Children.Clear();
        _employees.Spacing = 6;
        if (_rows.Count == 0)
        {
            _employees.Children.Add(AxEmptyState.Create("👥", "Nessun dipendente trovato", "Crea un dipendente oppure modifica i filtri di ricerca."));
            _selected = null;
            _entityPage.Content = EmployeeEntityPage(null);
            return;
        }
        foreach (var row in _rows) _employees.Children.Add(EmployeeRow(row));
        _selected ??= _rows.FirstOrDefault();
        _entityPage.Content = EmployeeEntityPage(_selected);
    }

    private Button EmployeeRow(HumanResourcesEmployeeRow employee)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,130,130,110") };
        var name = new StackPanel { Spacing = 2 };
        name.Children.Add(new TextBlock { Text = string.IsNullOrWhiteSpace(employee.FullName) ? "Dipendente senza nome" : employee.FullName, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary) });
        name.Children.Add(new TextBlock { Text = string.IsNullOrWhiteSpace(employee.EmployeeCode) ? employee.Email : $"{employee.EmployeeCode} · {employee.Email}", FontSize = 12, Foreground = UiTokens.Brush(UiTokens.TextSecondary), TextWrapping = TextWrapping.Wrap });
        Add(grid, name, 0, 0); Add(grid, Cell(employee.Department), 1, 0); Add(grid, Cell(employee.Role), 2, 0); Add(grid, AxStatusBadge.FromStatus(employee.EmploymentStatus), 3, 0);
        var button = new Button { Content = grid, Background = UiTokens.Brush(_selected?.Id == employee.Id ? UiTokens.PremiumSelected : UiTokens.Surface), BorderBrush = UiTokens.Brush(UiTokens.Border), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(14), Padding = new Thickness(12), HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };
        button.Click += (_, _) => { _selected = employee; _entityPage.Content = EmployeeEntityPage(employee); LoadEmployees(); };
        return button;
    }

    private Control EmployeeEntityPage(HumanResourcesEmployeeRow? employee)
    {
        var stack = new StackPanel { Spacing = 14 };
        stack.Children.Add(new TextBlock { Text = employee is null ? "Entity Page Dipendente" : employee.FullName, FontSize = AxTypography.SectionTitle, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary), TextWrapping = TextWrapping.Wrap });
        if (employee is null)
        {
            stack.Children.Add(AxEmptyState.Create("👤", "Seleziona un dipendente", "Il fascicolo mostrerà anagrafica, asset, documenti, verbali e timeline."));
            return AxCard.Create(stack);
        }
        var relations = _relations.Load(employee.Id.ToString(), employee.FullName);
        stack.Children.Add(new AxInfoPanel("Anagrafica").AddItem("Matricola", employee.EmployeeCode, "🏷️").AddItem("Email", employee.Email, "✉️").AddItem("Telefono", employee.Phone, "☎️").AddItem("Reparto", employee.Department, "🏢").AddItem("Ruolo", employee.Role, "💼").ToCard());
        stack.Children.Add(new AxInfoPanel("Relazioni").AddItem("Asset assegnati", relations.Assets.Count.ToString(), "💻").AddItem("Documenti collegati", relations.Documents.Count.ToString(), "📄").AddItem("Verbali collegati", relations.DeliveryReports.Count.ToString(), "📦").AddItem("Stato", string.IsNullOrWhiteSpace(employee.EmploymentStatus) ? "N/D" : employee.EmploymentStatus, "✅").ToCard());
        stack.Children.Add(RelationsPanel("Asset collegati", relations.Assets));
        stack.Children.Add(RelationsPanel("Documenti collegati", relations.Documents));
        stack.Children.Add(RelationsPanel("Verbali collegati", relations.DeliveryReports));
        var timeline = new AxTimeline("Timeline HR");
        foreach (var item in _service.EmployeeTimeline(employee)) timeline.AddEvent(item, "", "", "•");
        stack.Children.Add(timeline.ToCard());
        return AxCard.Create(stack);
    }

    private Control RelationsPanel(string title, IReadOnlyList<EnterpriseRelationItem> items)
    {
        var stack = new StackPanel { Spacing = 8 };
        stack.Children.Add(new TextBlock { Text = title, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary) });
        if (items.Count == 0)
        {
            stack.Children.Add(new TextBlock { Text = "Nessun collegamento presente.", Foreground = UiTokens.Brush(UiTokens.TextSecondary), TextWrapping = TextWrapping.Wrap });
            return AxCard.Create(stack);
        }
        foreach (var item in items)
        {
            var row = new StackPanel { Spacing = 3 };
            row.Children.Add(new TextBlock { Text = $"{item.Icon} {item.Title}", FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary), TextWrapping = TextWrapping.Wrap });
            row.Children.Add(new TextBlock { Text = item.Subtitle, Foreground = UiTokens.Brush(UiTokens.TextSecondary), FontSize = 12, TextWrapping = TextWrapping.Wrap });
            var button = new Button { Content = row, Background = UiTokens.Brush(UiTokens.SurfaceAlt), CornerRadius = new CornerRadius(12), Padding = new Thickness(10), HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };
            button.Click += (_, _) => { if (!string.IsNullOrWhiteSpace(item.OpenModuleId)) Navigate(item.OpenModuleId, item.OpenModuleTitle); };
            stack.Children.Add(button);
        }
        return AxCard.Create(stack);
    }

    private static TextBlock Header(string text, int column) { var block = new TextBlock { Text = text, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextSecondary) }; Grid.SetColumn(block, column); return block; }
    private static TextBlock Cell(string text) => new() { Text = string.IsNullOrWhiteSpace(text) ? "—" : text, Foreground = UiTokens.Brush(UiTokens.TextPrimary), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
    private void Navigate(string moduleId, string title) => _navigate?.Invoke(moduleId, title);
    private static void Add(Grid grid, Control control, int column, int row) { Grid.SetColumn(control, column); Grid.SetRow(control, row); grid.Children.Add(control); }
}
