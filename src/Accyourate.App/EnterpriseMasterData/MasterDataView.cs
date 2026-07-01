using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.EnterpriseMasterData.Models;
using Accyourate.App.EnterpriseMasterData.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.EnterpriseMasterData;

public sealed class MasterDataView : UserControl
{
    private readonly MasterDataService _service = new();

    private readonly StackPanel _kpis = new();
    private readonly StackPanel _navigation = new();
    private readonly StackPanel _rows = new();
    private readonly ContentControl _details = new();
    private readonly TextBox _search = new();
    private readonly TextBlock _title = new();
    private readonly TextBlock _subtitle = new();

    private string _section = "companies";
    private object? _selected;

    public MasterDataView()
    {
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();
        LoadSection("companies");
    }

    private Control BuildLayout()
    {
        var root = new DockPanel();

        var header = new StackPanel
        {
            Margin = new Thickness(24, 20, 24, 12),
            Spacing = 8
        };

        header.Children.Add(new TextBlock
        {
            Text = "Anagrafica Aziendale",
            FontSize = 32,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        header.Children.Add(new TextBlock
        {
            Text = "Gestione aziende, sedi, reparti, dipendenti e fornitori.",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap
        });

        _kpis.Orientation = Avalonia.Layout.Orientation.Horizontal;
        _kpis.Spacing = 12;
        header.Children.Add(_kpis);

        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        var body = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("220,*,360"),
            RowDefinitions = new RowDefinitions("*"),
            Margin = new Thickness(24, 0, 24, 24)
        };

        Add(body, BuildNavigation(), 0, 0);
        Add(body, BuildList(), 1, 0);

        var detailsHost = new ScrollViewer
        {
            Content = _details,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
        };

        Add(body, detailsHost, 2, 0);

        root.Children.Add(body);
        return root;
    }

    private Control BuildNavigation()
    {
        _navigation.Spacing = 8;

        var panel = new StackPanel { Spacing = 10 };
        panel.Children.Add(new TextBlock
        {
            Text = "Sezioni",
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        panel.Children.Add(NavButton("🏢 Aziende", "companies"));
        panel.Children.Add(NavButton("📍 Sedi", "sites"));
        panel.Children.Add(NavButton("📂 Reparti", "departments"));
        panel.Children.Add(NavButton("👤 Dipendenti", "employees"));
        panel.Children.Add(NavButton("🤝 Fornitori", "suppliers"));

        return Card(panel, new Thickness(0, 0, 18, 0));
    }

    private Control BuildList()
    {
        var root = new DockPanel();

        var header = new StackPanel { Spacing = 6, Margin = new Thickness(0, 0, 0, 12) };

        _title.FontSize = 24;
        _title.FontWeight = FontWeight.Bold;
        _title.Foreground = UiTokens.Brush(UiTokens.TextPrimary);

        _subtitle.Foreground = UiTokens.Brush(UiTokens.TextSecondary);
        _subtitle.TextWrapping = TextWrapping.Wrap;

        header.Children.Add(_title);
        header.Children.Add(_subtitle);

        _search.Watermark = "Cerca...";
        _search.TextChanged += (_, _) => RefreshRows();
        header.Children.Add(_search);

        var toolbar = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 8
        };
        toolbar.Children.Add(ToolbarButton("+ Nuovo", "Prossimo sprint: creazione record"));
        toolbar.Children.Add(ToolbarButton("Modifica", "Prossimo sprint: modifica record"));
        toolbar.Children.Add(ToolbarButton("Elimina", "Prossimo sprint: eliminazione record"));
        header.Children.Add(toolbar);

        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        root.Children.Add(new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            CornerRadius = new CornerRadius(18),
            Padding = new Thickness(8),
            Child = new ScrollViewer
            {
                Content = _rows,
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
            }
        });

        return root;
    }

    private void LoadSection(string section)
    {
        _section = section;
        _selected = null;
        _search.Text = string.Empty;

        RefreshKpis();
        RefreshHeaders();
        RefreshRows();
        RefreshDetails();
    }

    private void RefreshKpis()
    {
        _kpis.Children.Clear();
        _kpis.Children.Add(Kpi("🏢", _service.CountCompanies().ToString(), "Aziende"));
        _kpis.Children.Add(Kpi("📍", _service.CountSites().ToString(), "Sedi"));
        _kpis.Children.Add(Kpi("📂", _service.CountDepartments().ToString(), "Reparti"));
        _kpis.Children.Add(Kpi("👤", _service.CountEmployees().ToString(), "Dipendenti"));
        _kpis.Children.Add(Kpi("🤝", _service.CountSuppliers().ToString(), "Fornitori"));
    }

    private void RefreshHeaders()
    {
        (_title.Text, _subtitle.Text) = _section switch
        {
            "companies" => ("Aziende", "Anagrafica aziende e dati fiscali principali."),
            "sites" => ("Sedi", "Sedi operative, uffici e stabilimenti."),
            "departments" => ("Reparti", "Aree funzionali collegate a sedi e responsabili."),
            "employees" => ("Dipendenti", "Dipendenti, collaboratori e utilizzatori degli asset."),
            "suppliers" => ("Fornitori", "Fornitori hardware, software, servizi e manutenzione."),
            _ => ("Anagrafica", "Seleziona una sezione.")
        };
    }

    private void RefreshRows()
    {
        _rows.Children.Clear();
        _rows.Spacing = 6;

        var query = (_search.Text ?? string.Empty).Trim().ToLowerInvariant();

        switch (_section)
        {
            case "companies":
                RenderCompanies(query);
                break;
            case "sites":
                RenderSites(query);
                break;
            case "departments":
                RenderDepartments(query);
                break;
            case "employees":
                RenderEmployees(query);
                break;
            case "suppliers":
                RenderSuppliers(query);
                break;
        }
    }

    private void RenderCompanies(string query)
    {
        var items = _service.GetCompanies()
            .Where(x => Match(query, x.Name, x.City, x.Email, x.VatNumber))
            .ToList();

        foreach (var item in items)
            _rows.Children.Add(Row("🏢", item.Name, $"{item.City} · {item.Email}", item));
    }

    private void RenderSites(string query)
    {
        var items = _service.GetSites()
            .Where(x => Match(query, x.Name, x.City, x.Address))
            .ToList();

        foreach (var item in items)
            _rows.Children.Add(Row("📍", item.Name, $"{item.City} ({item.Province})", item));
    }

    private void RenderDepartments(string query)
    {
        var items = _service.GetDepartments()
            .Where(x => Match(query, x.Name, x.Description))
            .ToList();

        foreach (var item in items)
            _rows.Children.Add(Row("📂", item.Name, item.Description, item));
    }

    private void RenderEmployees(string query)
    {
        var items = _service.GetEmployees()
            .Where(x => Match(query, x.FullName, x.Email, x.Role))
            .ToList();

        foreach (var item in items)
            _rows.Children.Add(Row("👤", item.FullName, $"{item.Role} · {item.Email}", item));
    }

    private void RenderSuppliers(string query)
    {
        var items = _service.GetSuppliers()
            .Where(x => Match(query, x.Name, x.Category, x.Email, x.ContactName))
            .ToList();

        foreach (var item in items)
            _rows.Children.Add(Row("🤝", item.Name, $"{item.Category} · {item.Email}", item));
    }

    private Button Row(string icon, string title, string subtitle, object item)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("48,*")
        };

        Add(grid, new TextBlock
        {
            Text = icon,
            FontSize = 24,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        }, 0, 0);

        var text = new StackPanel { Spacing = 2 };
        text.Children.Add(new TextBlock
        {
            Text = title,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        text.Children.Add(new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(subtitle) ? "—" : subtitle,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap
        });
        Add(grid, text, 1, 0);

        var selected = ReferenceEquals(_selected, item);

        var button = new Button
        {
            Content = grid,
            Background = selected ? UiTokens.Brush(UiTokens.PremiumSelected) : Brushes.Transparent,
            Padding = new Thickness(10),
            CornerRadius = new CornerRadius(14)
        };

        button.Click += (_, _) =>
        {
            _selected = item;
            RefreshRows();
            RefreshDetails();
        };

        return button;
    }

    private void RefreshDetails()
    {
        if (_selected is null)
        {
            _details.Content = Card(new TextBlock
            {
                Text = "Seleziona un elemento per vedere i dettagli.",
                TextWrapping = TextWrapping.Wrap,
                Foreground = UiTokens.Brush(UiTokens.TextSecondary)
            }, new Thickness(18, 0, 0, 0));
            return;
        }

        _details.Content = _selected switch
        {
            Company company => CompanyDetails(company),
            Site site => SiteDetails(site),
            Department department => DepartmentDetails(department),
            EmployeeMasterData employee => EmployeeDetails(employee),
            Supplier supplier => SupplierDetails(supplier),
            _ => Card(new TextBlock { Text = "Dettaglio non disponibile." }, new Thickness(18, 0, 0, 0))
        };
    }

    private Control CompanyDetails(Company item)
    {
        var stack = DetailsBase("🏢", item.Name, "Azienda");
        stack.Children.Add(Info("Partita IVA", item.VatNumber));
        stack.Children.Add(Info("Codice fiscale", item.FiscalCode));
        stack.Children.Add(Info("Indirizzo", item.Address));
        stack.Children.Add(Info("Città", $"{item.City} {item.Province}"));
        stack.Children.Add(Info("Email", item.Email));
        stack.Children.Add(Info("Telefono", item.Phone));
        stack.Children.Add(Info("Sito web", item.Website));
        stack.Children.Add(Info("Note", item.Notes));
        return Card(stack, new Thickness(18, 0, 0, 0));
    }

    private Control SiteDetails(Site item)
    {
        var stack = DetailsBase("📍", item.Name, item.IsMainSite ? "Sede principale" : "Sede");
        stack.Children.Add(Info("Indirizzo", item.Address));
        stack.Children.Add(Info("Città", $"{item.City} {item.Province}"));
        stack.Children.Add(Info("Nazione", item.Country));
        stack.Children.Add(Info("Note", item.Notes));
        return Card(stack, new Thickness(18, 0, 0, 0));
    }

    private Control DepartmentDetails(Department item)
    {
        var stack = DetailsBase("📂", item.Name, "Reparto");
        stack.Children.Add(Info("Descrizione", item.Description));
        stack.Children.Add(Info("SiteId", item.SiteId.ToString()));
        stack.Children.Add(Info("ManagerEmployeeId", item.ManagerEmployeeId.ToString()));
        return Card(stack, new Thickness(18, 0, 0, 0));
    }

    private Control EmployeeDetails(EmployeeMasterData item)
    {
        var stack = DetailsBase("👤", item.FullName, item.IsActive ? "Attivo" : "Non attivo");
        stack.Children.Add(Info("Email", item.Email));
        stack.Children.Add(Info("Telefono", item.Phone));
        stack.Children.Add(Info("Ruolo", item.Role));
        stack.Children.Add(Info("DepartmentId", item.DepartmentId.ToString()));
        stack.Children.Add(Info("SiteId", item.SiteId.ToString()));
        stack.Children.Add(Info("Note", item.Notes));
        return Card(stack, new Thickness(18, 0, 0, 0));
    }

    private Control SupplierDetails(Supplier item)
    {
        var stack = DetailsBase("🤝", item.Name, item.Category);
        stack.Children.Add(Info("Partita IVA", item.VatNumber));
        stack.Children.Add(Info("Contatto", item.ContactName));
        stack.Children.Add(Info("Email", item.Email));
        stack.Children.Add(Info("Telefono", item.Phone));
        stack.Children.Add(Info("Note", item.Notes));
        return Card(stack, new Thickness(18, 0, 0, 0));
    }

    private static StackPanel DetailsBase(string icon, string title, string subtitle)
    {
        var stack = new StackPanel { Spacing = 12 };
        stack.Children.Add(new TextBlock
        {
            Text = $"{icon} {title}",
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            TextWrapping = TextWrapping.Wrap
        });
        stack.Children.Add(new TextBlock
        {
            Text = subtitle,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap
        });
        stack.Children.Add(new Separator { Margin = new Thickness(0, 6) });
        return stack;
    }

    private Button NavButton(string label, string section)
    {
        var button = new Button
        {
            Content = label,
            Background = _section == section ? UiTokens.Brush(UiTokens.PremiumSelected) : Brushes.Transparent,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Padding = new Thickness(12, 10),
            CornerRadius = new CornerRadius(12)
        };

        button.Click += (_, _) => LoadSection(section);
        return button;
    }

    private static Border Kpi(string icon, string value, string label)
    {
        var stack = new StackPanel { Spacing = 2 };
        stack.Children.Add(new TextBlock
        {
            Text = $"{icon} {value}",
            FontSize = 22,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        stack.Children.Add(new TextBlock
        {
            Text = label,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            FontSize = 12
        });

        return new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            CornerRadius = new CornerRadius(18),
            Padding = new Thickness(16, 12),
            MinWidth = 140,
            Child = stack
        };
    }

    private static Border Info(string label, string value)
    {
        var stack = new StackPanel { Spacing = 2 };
        stack.Children.Add(new TextBlock
        {
            Text = label,
            FontSize = 12,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });
        stack.Children.Add(new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(value) ? "—" : value,
            FontWeight = FontWeight.SemiBold,
            TextWrapping = TextWrapping.Wrap,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        return new Border
        {
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(12),
            Child = stack
        };
    }

    private static Button ToolbarButton(string text, string tooltip)
    {
        var b = new Button
        {
            Content = text,
            Background = UiTokens.Brush(UiTokens.Surface),
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            Padding = new Thickness(10, 8),
            CornerRadius = new CornerRadius(12)
        };
        ToolTip.SetTip(b, tooltip);
        return b;
    }

    private static Border Card(Control child, Thickness margin)
    {
        return new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            CornerRadius = new CornerRadius(22),
            Padding = new Thickness(18),
            Margin = margin,
            Child = child
        };
    }

    private static bool Match(string query, params string[] values)
    {
        if (string.IsNullOrWhiteSpace(query))
            return true;

        return values.Any(v => (v ?? string.Empty).ToLowerInvariant().Contains(query));
    }

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
