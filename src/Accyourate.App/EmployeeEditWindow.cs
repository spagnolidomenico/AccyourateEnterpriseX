using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.Models;

namespace Accyourate.App;

public sealed class EmployeeEditWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;
    private readonly EmployeeRecord _employee;
    private readonly TextBlock _message = new();

    private readonly TextBox _code = new();
    private readonly TextBox _firstName = new();
    private readonly TextBox _lastName = new();
    private readonly TextBox _department = new();
    private readonly TextBox _jobTitle = new();
    private readonly TextBox _email = new();
    private readonly TextBox _phone = new();
    private readonly TextBox _hireDate = new();

    public event Action? EmployeeSaved;

    public EmployeeEditWindow(DatabaseService database, CurrentUser user, EmployeeRecord employee)
    {
        _database = database;
        _user = user;
        _employee = employee;

        Title = "Accyourate Enterprise X - Modifica Dipendente";
        Width = 620;
        Height = 620;
        
        MinWidth = 1024;
        MinHeight = 680;
WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");

        LoadValues();
        Content = BuildLayout();
    }

    private void LoadValues()
    {
        _code.Text = _employee.EmployeeCode;
        _firstName.Text = _employee.FirstName;
        _lastName.Text = _employee.LastName;
        _department.Text = _employee.Department;
        _jobTitle.Text = _employee.JobTitle;
        _email.Text = _employee.Email;
        _phone.Text = _employee.Phone;
        _hireDate.Text = _employee.HireDate;
    }

    private Control BuildLayout()
    {
        var stack = new StackPanel { Margin = new Thickness(24), Spacing = 10 };

        stack.Children.Add(new TextBlock
        {
            Text = "Modifica Dipendente",
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });

        AddField(stack, "Matricola", _code);
        AddField(stack, "Nome", _firstName);
        AddField(stack, "Cognome", _lastName);
        AddField(stack, "Reparto", _department);
        AddField(stack, "Mansione", _jobTitle);
        AddField(stack, "Email", _email);
        AddField(stack, "Telefono", _phone);
        AddField(stack, "Data assunzione", _hireDate);

        _message.Foreground = Brush.Parse("#B5162B");
        stack.Children.Add(_message);

        var save = new Button
        {
            Content = "Salva modifiche",
            Background = Brush.Parse("#B5162B"),
            Foreground = Brushes.White,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(12)
        };
        save.Click += (_, _) => Save();
        stack.Children.Add(save);

        return new ScrollViewer
        {
            Content = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(18),
                Margin = new Thickness(20),
                Child = stack
            }
        };
    }

    private static void AddField(StackPanel stack, string label, TextBox textBox)
    {
        stack.Children.Add(new TextBlock { Text = label, FontWeight = FontWeight.Bold });
        stack.Children.Add(textBox);
    }

    private void Save()
    {
        _employee.EmployeeCode = _code.Text ?? "";
        _employee.FirstName = _firstName.Text ?? "";
        _employee.LastName = _lastName.Text ?? "";
        _employee.Department = _department.Text ?? "";
        _employee.JobTitle = _jobTitle.Text ?? "";
        _employee.Email = _email.Text ?? "";
        _employee.Phone = _phone.Text ?? "";
        _employee.HireDate = _hireDate.Text ?? "";

        var ok = _database.UpdateEmployee(_employee, _user.Username, out var error);
        if (!ok)
        {
            _message.Text = error;
            return;
        }

        _message.Text = "Modifiche salvate.";
        EmployeeSaved?.Invoke();
    }
}
