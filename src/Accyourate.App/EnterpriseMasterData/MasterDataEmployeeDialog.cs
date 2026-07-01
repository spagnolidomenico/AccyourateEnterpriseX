using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.EnterpriseMasterData.Models;
using Accyourate.App.EnterpriseMasterData.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.EnterpriseMasterData;

public sealed class MasterDataEmployeeDialog : Window
{
    private readonly MasterDataService _service;
    private readonly EmployeeMasterData _employee;

    private readonly TextBox _fullName = new();
    private readonly TextBox _email = new();
    private readonly TextBox _phone = new();
    private readonly TextBox _role = new();
    private readonly ComboBox _department = new();
    private readonly ComboBox _site = new();
    private readonly CheckBox _isActive = new();
    private readonly TextBox _notes = new();
    private readonly TextBlock _validation = new();

    public MasterDataEmployeeDialog(MasterDataService service, EmployeeMasterData? employee = null)
    {
        _service = service;
        _employee = Clone(employee) ?? new EmployeeMasterData
        {
            IsActive = true,
            DepartmentId = _service.GetDepartments().FirstOrDefault()?.Id ?? 0,
            SiteId = _service.GetSites().FirstOrDefault()?.Id ?? 0
        };

        Title = _employee.Id == 0 ? "Nuovo Dipendente" : $"Modifica {_employee.FullName}";
        Width = 720;
        Height = 680;
        MinWidth = 620;
        MinHeight = 560;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();

        LoadEmployee();
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
            Text = _employee.Id == 0 ? "Nuovo Dipendente" : "Modifica Dipendente",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        header.Children.Add(new TextBlock
        {
            Text = "Gestisci i dati anagrafici principali del dipendente o collaboratore.",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap
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

        var form = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto,Auto"),
            Margin = new Thickness(24, 0, 24, 12)
        };

        _department.ItemsSource = _service.GetDepartments();
        _department.DisplayMemberBinding = new Avalonia.Data.Binding("Name");

        _site.ItemsSource = _service.GetSites();
        _site.DisplayMemberBinding = new Avalonia.Data.Binding("Name");

        Add(form, Field("Nome completo *", _fullName), 0, 0);
        Add(form, Field("Email", _email), 1, 0);
        Add(form, Field("Telefono", _phone), 0, 1);
        Add(form, Field("Ruolo", _role), 1, 1);
        Add(form, Field("Reparto", _department), 0, 2);
        Add(form, Field("Sede", _site), 1, 2);
        Add(form, Field("Attivo", _isActive), 0, 3);

        _notes.AcceptsReturn = true;
        _notes.Height = 110;
        _notes.TextWrapping = TextWrapping.Wrap;

        var notesField = Field("Note", _notes);
        Grid.SetColumn(notesField, 0);
        Grid.SetColumnSpan(notesField, 2);
        Grid.SetRow(notesField, 4);
        form.Children.Add(notesField);

        root.Children.Add(new ScrollViewer
        {
            Content = form,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        });

        return root;
    }

    private void LoadEmployee()
    {
        _fullName.Text = _employee.FullName;
        _email.Text = _employee.Email;
        _phone.Text = _employee.Phone;
        _role.Text = _employee.Role;
        _isActive.IsChecked = _employee.IsActive;
        _notes.Text = _employee.Notes;

        _department.SelectedItem = _service.GetDepartments().FirstOrDefault(x => x.Id == _employee.DepartmentId);
        _site.SelectedItem = _service.GetSites().FirstOrDefault(x => x.Id == _employee.SiteId);
    }

    private void Save()
    {
        _validation.Text = string.Empty;

        var fullName = (_fullName.Text ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            _validation.Text = "Il nome completo è obbligatorio.";
            return;
        }

        _employee.FullName = fullName;
        _employee.Email = (_email.Text ?? string.Empty).Trim();
        _employee.Phone = (_phone.Text ?? string.Empty).Trim();
        _employee.Role = (_role.Text ?? string.Empty).Trim();
        _employee.DepartmentId = (_department.SelectedItem as Department)?.Id ?? 0;
        _employee.SiteId = (_site.SelectedItem as Site)?.Id ?? 0;
        _employee.IsActive = _isActive.IsChecked == true;
        _employee.Notes = (_notes.Text ?? string.Empty).Trim();

        Close(_employee);
    }

    private static StackPanel Field(string label, Control input)
    {
        input.Margin = new Thickness(0, 6, 0, 0);

        return new StackPanel
        {
            Margin = new Thickness(8, 8),
            Spacing = 2,
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

    private static EmployeeMasterData? Clone(EmployeeMasterData? employee)
    {
        if (employee is null)
            return null;

        return new EmployeeMasterData
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Email = employee.Email,
            Phone = employee.Phone,
            Role = employee.Role,
            DepartmentId = employee.DepartmentId,
            SiteId = employee.SiteId,
            IsActive = employee.IsActive,
            Notes = employee.Notes
        };
    }

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
