using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.HumanResources.Models;
using Accyourate.App.HumanResources.Services;
using Accyourate.App.Platform.Validation;
using Accyourate.App.UIFramework.Controls;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.HumanResources;

public sealed class HumanResourcesView : UserControl
{
    private readonly EmployeeService _employeeService = new();
    private readonly HrLookupService _lookupService = new();

    private readonly TextBox _search = new();
    private readonly ComboBox _status = new();
    private readonly StackPanel _rows = new();
    private readonly StackPanel _kpis = new();
    private readonly ContentControl _details = new();
    private readonly TextBlock _message = new();

    private IReadOnlyList<Employee> _employees = Array.Empty<Employee>();
    private IReadOnlyDictionary<int, Department> _departments = new Dictionary<int, Department>();
    private IReadOnlyDictionary<int, Site> _sites = new Dictionary<int, Site>();
    private IReadOnlyDictionary<int, HrRole> _roles = new Dictionary<int, HrRole>();
    private Employee? _selected;

    public HumanResourcesView()
    {
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();
        Load();
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
            Text = "Human Resources",
            FontSize = 32,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        header.Children.Add(new TextBlock
        {
            Text = "Gestione dipendenti, organizzazione, contratti e documenti HR.",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap
        });

        _kpis.Orientation = Avalonia.Layout.Orientation.Horizontal;
        _kpis.Spacing = 12;
        header.Children.Add(_kpis);

        _message.TextWrapping = TextWrapping.Wrap;
        _message.Foreground = UiTokens.Brush(UiTokens.BrandBlue);
        header.Children.Add(_message);

        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        var toolbar = new Grid
        {
            Margin = new Thickness(24, 0, 24, 16),
            ColumnDefinitions = new ColumnDefinitions("*,170,Auto,Auto,Auto")
        };

        _search.Watermark = "Cerca per codice, nome, email, telefono, note...";
        _search.TextChanged += (_, _) => RefreshRows();

        _status.ItemsSource = new[] { "Tutti", EmploymentStatus.Active, EmploymentStatus.OnLeave, EmploymentStatus.Suspended, EmploymentStatus.Terminated, EmploymentStatus.Candidate };
        _status.SelectedIndex = 0;
        _status.SelectionChanged += (_, _) => RefreshRows();

        Add(toolbar, _search, 0, 0);
        Add(toolbar, _status, 1, 0);

        var actions = new EnterpriseToolbar()
            .AddSecondary("↻ Aggiorna", Load, "Ricarica dipendenti")
            .AddPrimary("+ Nuovo", OpenNewEmployee, "Crea un nuovo dipendente")
            .AddPlaceholder("Esporta", "Prossimo sprint: esportazione HR");

        Grid.SetColumn(actions, 2);
        Grid.SetColumnSpan(actions, 3);
        Grid.SetRow(actions, 0);
        toolbar.Children.Add(actions);

        DockPanel.SetDock(toolbar, Dock.Top);
        root.Children.Add(toolbar);

        var content = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,390"),
            RowDefinitions = new RowDefinitions("*"),
            Margin = new Thickness(24, 0, 24, 24)
        };

        var list = new DockPanel();

        var tableHeader = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("110,*,150,150,120,120"),
            Margin = new Thickness(0, 0, 0, 8)
        };

        Add(tableHeader, Header("Codice"), 0, 0);
        Add(tableHeader, Header("Nome"), 1, 0);
        Add(tableHeader, Header("Reparto"), 2, 0);
        Add(tableHeader, Header("Ruolo"), 3, 0);
        Add(tableHeader, Header("Sede"), 4, 0);
        Add(tableHeader, Header("Stato"), 5, 0);

        DockPanel.SetDock(tableHeader, Dock.Top);
        list.Children.Add(tableHeader);

        list.Children.Add(new Border
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

        Add(content, list, 0, 0);

        _details.Content = EmptyDetails();

        Add(content, new ScrollViewer
        {
            Content = _details,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
        }, 1, 0);

        root.Children.Add(content);
        return root;
    }

    private void Load()
    {
        try
        {
            var keepId = _selected?.Id;

            _departments = _lookupService.GetDepartments().ToDictionary(x => x.Id);
            _sites = _lookupService.GetSites().ToDictionary(x => x.Id);
            _roles = _lookupService.GetRoles().ToDictionary(x => x.Id);
            _employees = _employeeService.GetAll();

            RefreshKpis();
            RefreshRows();

            _selected = keepId.HasValue
                ? _employees.FirstOrDefault(x => x.Id == keepId.Value)
                : _employees.FirstOrDefault();

            _details.Content = _selected is not null ? DetailsCard(_selected) : EmptyDetails();
            ShowMessage("Human Resources caricato.");
        }
        catch (Exception ex)
        {
            ShowMessage($"Errore caricamento HR: {ex.Message}", true);
        }
    }

    private void RefreshKpis()
    {
        _kpis.Children.Clear();

        _kpis.Children.Add(Kpi("👥", _employees.Count.ToString(), "Dipendenti"));
        _kpis.Children.Add(Kpi("✓", _employees.Count(x => x.EmploymentStatus == EmploymentStatus.Active).ToString(), "Attivi"));
        _kpis.Children.Add(Kpi("🏢", _departments.Count.ToString(), "Reparti"));
        _kpis.Children.Add(Kpi("📍", _sites.Count.ToString(), "Sedi"));
    }

    private void RefreshRows()
    {
        _rows.Children.Clear();
        _rows.Spacing = 6;

        var query = (_search.Text ?? string.Empty).Trim().ToLowerInvariant();
        var selectedStatus = _status.SelectedItem?.ToString() ?? "Tutti";

        var filtered = _employees.Where(e =>
            (string.IsNullOrWhiteSpace(query) ||
             $"{e.EmployeeCode} {e.FullName} {e.Email} {e.Phone} {e.Notes}".ToLowerInvariant().Contains(query)) &&
            (selectedStatus == "Tutti" || e.EmploymentStatus == selectedStatus))
            .ToList();

        if (filtered.Count == 0)
        {
            _rows.Children.Add(new TextBlock
            {
                Text = "Nessun dipendente trovato.",
                Margin = new Thickness(12),
                Foreground = UiTokens.Brush(UiTokens.TextSecondary)
            });
            return;
        }

        foreach (var employee in filtered)
            _rows.Children.Add(Row(employee));
    }

    private Button Row(Employee employee)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("110,*,150,150,120,120")
        };

        Add(grid, Cell(employee.EmployeeCode, true), 0, 0);
        Add(grid, Cell(employee.FullName, true), 1, 0);
        Add(grid, Cell(DepartmentName(employee.DepartmentId)), 2, 0);
        Add(grid, Cell(RoleName(employee.RoleId)), 3, 0);
        Add(grid, Cell(SiteName(employee.SiteId)), 4, 0);
        Add(grid, StatusBadge(employee.EmploymentStatus), 5, 0);

        var button = new Button
        {
            Content = grid,
            Background = _selected?.Id == employee.Id ? UiTokens.Brush(UiTokens.PremiumSelected) : Brushes.Transparent,
            Padding = new Thickness(8),
            CornerRadius = new CornerRadius(12)
        };

        button.Click += (_, _) =>
        {
            _selected = employee;
            _details.Content = DetailsCard(employee);
            RefreshRows();
        };

        button.DoubleTapped += (_, _) => OpenEditEmployee(employee);

        return button;
    }

    private Control DetailsCard(Employee employee)
    {
        var stack = new StackPanel { Spacing = 12 };

        stack.Children.Add(new TextBlock
        {
            Text = employee.FullName,
            FontSize = 26,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        stack.Children.Add(new TextBlock
        {
            Text = $"{employee.EmployeeCode} · {employee.Email}",
            FontSize = 15,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap
        });

        var actions = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            Margin = new Thickness(0, 4, 0, 8)
        };
        Add(actions, SmallButton("Modifica", () => OpenEditEmployee(employee)), 0, 0);
        Add(actions, SmallButton("Elimina", () => DeleteEmployee(employee), true), 1, 0);
        stack.Children.Add(actions);

        stack.Children.Add(Section("Anagrafica"));
        stack.Children.Add(Info("Email", employee.Email));
        stack.Children.Add(Info("Telefono", employee.Phone));
        stack.Children.Add(Info("Stato", employee.EmploymentStatus));
        stack.Children.Add(Info("Assunzione", FormatDate(employee.HireDate)));

        stack.Children.Add(Section("Organizzazione"));
        stack.Children.Add(Info("Reparto", DepartmentName(employee.DepartmentId)));
        stack.Children.Add(Info("Ruolo", RoleName(employee.RoleId)));
        stack.Children.Add(Info("Sede", SiteName(employee.SiteId)));
        stack.Children.Add(Info("Manager", employee.ManagerId?.ToString() ?? "—"));

        stack.Children.Add(Section("Moduli collegati"));
        stack.Children.Add(Info("Asset assegnati", "Disponibile nel prossimo sprint HR-Asset."));
        stack.Children.Add(Info("Documenti", "Predisposizione Employee Documents."));
        stack.Children.Add(Info("Timeline", "Predisposizione Audit Timeline."));

        stack.Children.Add(Section("Note"));
        stack.Children.Add(Info("Note", employee.Notes));

        return Card(stack);
    }

    private Control EmptyDetails()
    {
        return Card(new TextBlock
        {
            Text = "Seleziona un dipendente per vedere i dettagli.",
            TextWrapping = TextWrapping.Wrap,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });
    }

    private async void OpenNewEmployee()
    {
        try
        {
            var dialog = new EmployeeEditDialog(_lookupService);
            var result = await ShowEmployeeDialog(dialog);
            if (result is null)
                return;

            _employeeService.Create(result, "Human Resources");
            ShowMessage("Dipendente creato correttamente.");
            Load();
        }
        catch (ValidationException ex)
        {
            ShowMessage(ex.Result.ToDisplayMessage(), true);
        }
        catch (Exception ex)
        {
            ShowMessage($"Errore creazione dipendente: {ex.Message}", true);
        }
    }

    private async void OpenEditEmployee(Employee employee)
    {
        try
        {
            var dialog = new EmployeeEditDialog(_lookupService, employee);
            var result = await ShowEmployeeDialog(dialog);
            if (result is null)
                return;

            _employeeService.Update(result, "Human Resources");
            ShowMessage("Dipendente aggiornato correttamente.");
            Load();
        }
        catch (ValidationException ex)
        {
            ShowMessage(ex.Result.ToDisplayMessage(), true);
        }
        catch (Exception ex)
        {
            ShowMessage($"Errore modifica dipendente: {ex.Message}", true);
        }
    }

    private void DeleteEmployee(Employee employee)
    {
        try
        {
            _employeeService.Delete(employee.Id, "Human Resources");
            ShowMessage("Dipendente eliminato correttamente.");
            _selected = null;
            Load();
        }
        catch (Exception ex)
        {
            ShowMessage($"Errore eliminazione dipendente: {ex.Message}", true);
        }
    }

    private async Task<Employee?> ShowEmployeeDialog(EmployeeEditDialog dialog)
    {
        var owner = TopLevel.GetTopLevel(this) as Window;
        if (owner is not null)
            return await dialog.ShowDialog<Employee?>(owner);

        dialog.Show();
        return null;
    }

    private string DepartmentName(int id) => _departments.TryGetValue(id, out var value) ? value.Name : "—";
    private string SiteName(int id) => _sites.TryGetValue(id, out var value) ? value.Name : "—";
    private string RoleName(int id) => _roles.TryGetValue(id, out var value) ? value.Name : "—";

    private void ShowMessage(string text, bool isError = false)
    {
        _message.Text = text;
        _message.Foreground = UiTokens.Brush(isError ? UiTokens.Danger : UiTokens.BrandBlue);
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
            MinWidth = 150,
            Child = stack
        };
    }

    private static TextBlock Header(string text) => new()
    {
        Text = text,
        FontWeight = FontWeight.Bold,
        Foreground = UiTokens.Brush(UiTokens.TextSecondary),
        Margin = new Thickness(10, 0)
    };

    private static TextBlock Cell(string text, bool strong = false) => new()
    {
        Text = text,
        FontWeight = strong ? FontWeight.Bold : FontWeight.Normal,
        Foreground = UiTokens.Brush(strong ? UiTokens.TextPrimary : UiTokens.TextSecondary),
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        TextWrapping = TextWrapping.NoWrap,
        Margin = new Thickness(10, 0)
    };

    private static Border StatusBadge(string status)
    {
        return new Border
        {
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(10, 5),
            Margin = new Thickness(8, 0),
            Child = new TextBlock
            {
                Text = status,
                FontWeight = FontWeight.SemiBold,
                Foreground = UiTokens.Brush(status == EmploymentStatus.Active ? UiTokens.Success : UiTokens.BrandBlue),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            }
        };
    }

    private static TextBlock Section(string text) => new()
    {
        Text = text,
        FontWeight = FontWeight.Bold,
        Foreground = UiTokens.Brush(UiTokens.TextPrimary),
        Margin = new Thickness(0, 8, 0, 0)
    };

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

    private static Button SmallButton(string text, Action action, bool danger = false)
    {
        var b = new Button
        {
            Content = text,
            Background = UiTokens.Brush(danger ? UiTokens.SurfaceAlt : UiTokens.BrandBlue),
            Foreground = danger ? UiTokens.Brush(UiTokens.Danger) : Brushes.White,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(10, 8),
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(4)
        };
        b.Click += (_, _) => action();
        return b;
    }

    private static Border Card(Control child)
    {
        return new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            CornerRadius = new CornerRadius(22),
            Padding = new Thickness(18),
            Margin = new Thickness(16, 0, 0, 0),
            Child = child
        };
    }

    private static string FormatDate(string value)
    {
        return DateTime.TryParse(value, out var date)
            ? date.ToString("dd/MM/yyyy")
            : value;
    }

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
