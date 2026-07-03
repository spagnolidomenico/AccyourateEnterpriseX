using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.HumanResources.Models;
using Accyourate.App.HumanResources.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.HumanResources;

public sealed class EmployeeEditDialog : Window
{
    private readonly HrLookupService _lookup;
    private readonly Employee? _original;

    private readonly TextBox _code = new();
    private readonly TextBox _firstName = new();
    private readonly TextBox _lastName = new();
    private readonly TextBox _email = new();
    private readonly TextBox _phone = new();
    private readonly TextBox _hireDate = new();
    private readonly TextBox _terminationDate = new();
    private readonly TextBox _notes = new();
    private readonly ComboBox _role = new();
    private readonly ComboBox _department = new();
    private readonly ComboBox _site = new();
    private readonly ComboBox _status = new();
    private readonly TextBlock _validation = new();

    public EmployeeEditDialog(HrLookupService lookup, Employee? employee = null)
    {
        _lookup = lookup;
        _original = employee;

        Title = employee is null ? "Nuovo dipendente" : "Modifica dipendente";
        Width = 760;
        Height = 720;
        MinWidth = 680;
        MinHeight = 620;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = UiTokens.Brush(UiTokens.Background);

        Content = BuildLayout();
        LoadData();
    }

    private Control BuildLayout()
    {
        var root = new DockPanel();

        var header = new StackPanel
        {
            Margin = new Thickness(24, 20, 24, 12),
            Spacing = 4
        };

        header.Children.Add(new TextBlock
        {
            Text = _original is null ? "Nuovo dipendente" : "Modifica dipendente",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        header.Children.Add(new TextBlock
        {
            Text = "Inserisci i dati principali del dipendente.",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });

        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        var footer = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,120,120"),
            Margin = new Thickness(24, 12, 24, 20)
        };

        _validation.Foreground = UiTokens.Brush(UiTokens.Danger);
        _validation.TextWrapping = TextWrapping.Wrap;
        Add(footer, _validation, 0, 0);

        var cancel = DialogButton("Annulla", UiTokens.Surface, UiTokens.TextPrimary);
        cancel.Click += (_, _) => Close(null);
        Add(footer, cancel, 1, 0);

        var save = DialogButton("Salva", UiTokens.BrandBlue, null, true);
        save.Foreground = Brushes.White;
        save.Click += (_, _) => Save();
        Add(footer, save, 2, 0);

        DockPanel.SetDock(footer, Dock.Bottom);
        root.Children.Add(footer);

        _notes.AcceptsReturn = true;
        _notes.Height = 80;
        _notes.TextWrapping = TextWrapping.Wrap;

        var form = new StackPanel
        {
            Margin = new Thickness(24, 0, 24, 12),
            Spacing = 12
        };

        var row1 = TwoColumns(Field("Codice dipendente", _code), Field("Stato", _status));
        var row2 = TwoColumns(Field("Nome *", _firstName), Field("Cognome *", _lastName));
        var row3 = TwoColumns(Field("Email", _email), Field("Telefono", _phone));
        var row4 = TwoColumns(Field("Ruolo *", _role), Field("Reparto *", _department));
        var row5 = TwoColumns(Field("Sede *", _site), Field("Data assunzione", _hireDate));
        var row6 = TwoColumns(Field("Data cessazione", _terminationDate), new Border());

        form.Children.Add(row1);
        form.Children.Add(row2);
        form.Children.Add(row3);
        form.Children.Add(row4);
        form.Children.Add(row5);
        form.Children.Add(row6);
        form.Children.Add(Field("Note", _notes));

        root.Children.Add(new ScrollViewer
        {
            Content = form,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        });

        return root;
    }

    private void LoadData()
    {
        var roles = _lookup.GetRoles().ToList();
        var departments = _lookup.GetDepartments().ToList();
        var sites = _lookup.GetSites().ToList();

        _role.ItemsSource = roles;
        _department.ItemsSource = departments;
        _site.ItemsSource = sites;
        _status.ItemsSource = new[] { EmploymentStatus.Active, EmploymentStatus.OnLeave, EmploymentStatus.Suspended, EmploymentStatus.Terminated, EmploymentStatus.Candidate };

        if (_original is null)
        {
            _status.SelectedIndex = 0;
            _role.SelectedIndex = roles.Count > 0 ? 0 : -1;
            _department.SelectedIndex = departments.Count > 0 ? 0 : -1;
            _site.SelectedIndex = sites.Count > 0 ? 0 : -1;
            return;
        }

        _code.Text = _original.EmployeeCode;
        _firstName.Text = _original.FirstName;
        _lastName.Text = _original.LastName;
        _email.Text = _original.Email;
        _phone.Text = _original.Phone;
        _hireDate.Text = _original.HireDate;
        _terminationDate.Text = _original.TerminationDate;
        _notes.Text = _original.Notes;
        _status.SelectedItem = _original.EmploymentStatus;

        _role.SelectedItem = roles.FirstOrDefault(x => x.Id == _original.RoleId);
        _department.SelectedItem = departments.FirstOrDefault(x => x.Id == _original.DepartmentId);
        _site.SelectedItem = sites.FirstOrDefault(x => x.Id == _original.SiteId);
    }

    private void Save()
    {
        _validation.Text = string.Empty;

        if (_role.SelectedItem is not HrRole role)
        {
            _validation.Text = "Seleziona un ruolo.";
            return;
        }

        if (_department.SelectedItem is not Department department)
        {
            _validation.Text = "Seleziona un reparto.";
            return;
        }

        if (_site.SelectedItem is not Site site)
        {
            _validation.Text = "Seleziona una sede.";
            return;
        }

        var employee = new Employee
        {
            Id = _original?.Id ?? 0,
            EmployeeCode = (_code.Text ?? string.Empty).Trim(),
            FirstName = (_firstName.Text ?? string.Empty).Trim(),
            LastName = (_lastName.Text ?? string.Empty).Trim(),
            Email = (_email.Text ?? string.Empty).Trim(),
            Phone = (_phone.Text ?? string.Empty).Trim(),
            RoleId = role.Id,
            DepartmentId = department.Id,
            SiteId = site.Id,
            ManagerId = _original?.ManagerId,
            EmploymentStatus = _status.SelectedItem?.ToString() ?? EmploymentStatus.Active,
            HireDate = (_hireDate.Text ?? string.Empty).Trim(),
            TerminationDate = (_terminationDate.Text ?? string.Empty).Trim(),
            Notes = (_notes.Text ?? string.Empty).Trim(),
            CreatedAt = _original?.CreatedAt ?? DateTime.Now.ToString("s"),
            UpdatedAt = DateTime.Now.ToString("s")
        };

        Close(employee);
    }

    private static Grid TwoColumns(Control left, Control right)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*")
        };

        Add(grid, left, 0, 0);
        Add(grid, right, 1, 0);
        return grid;
    }

    private static StackPanel Field(string label, Control input)
    {
        return new StackPanel
        {
            Spacing = 4,
            Margin = new Thickness(0, 0, 8, 0),
            Children =
            {
                new TextBlock
                {
                    Text = label,
                    FontSize = 12,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = UiTokens.Brush(UiTokens.TextSecondary)
                },
                input
            }
        };
    }

    private static Button DialogButton(string text, string backgroundToken, string? foregroundToken, bool bold = false)
    {
        return new Button
        {
            Content = text,
            Background = UiTokens.Brush(backgroundToken),
            Foreground = foregroundToken is null ? UiTokens.Brush(UiTokens.TextPrimary) : UiTokens.Brush(foregroundToken),
            FontWeight = bold ? FontWeight.Bold : FontWeight.Normal,
            Padding = new Thickness(14, 10),
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(8, 0)
        };
    }

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
