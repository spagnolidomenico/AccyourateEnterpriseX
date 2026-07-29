using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.Models;
using Accyourate.App.UIFramework.EnterpriseTable;

namespace Accyourate.App;

public sealed class EmployeesWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;

    private readonly StackPanel _rowsPanel = new();
    private readonly TextBlock _message = new();
    private readonly TextBox _search = new();
    private readonly CheckBox _includeArchived = new();

    private readonly TextBox _code = new();
    private readonly TextBox _firstName = new();
    private readonly TextBox _lastName = new();
    private readonly TextBox _department = new();
    private readonly TextBox _jobTitle = new();
    private readonly TextBox _email = new();
    private readonly TextBox _phone = new();
    private readonly TextBox _hireDate = new();

    public EmployeesWindow(DatabaseService database, CurrentUser user)
    {
        _database = database;
        _user = user;

        Title = "Accyourate Enterprise X - Persone / Dipendenti";
        Width = 1320;
        Height = 860;
        
        MinWidth = 1180;
        MinHeight = 760;
WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");

        Content = BuildLayout();
        RefreshRows();
    }

    private Control BuildLayout()
    {
        var root = new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
        var stack = new StackPanel { Margin = new Thickness(24), Spacing = 16 };

        stack.Children.Add(new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Text = "Persone / Dipendenti",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });

        stack.Children.Add(new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Text = "Developer 1.4: creazione, modifica, dettaglio, ricerca, export CSV e archiviazione."
        });

        stack.Children.Add(BuildFormCard());
        stack.Children.Add(BuildSearchCard());

        _message.Foreground = Brush.Parse("#B5162B");
        stack.Children.Add(_message);

        stack.Children.Add(new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(18),
            Child = _rowsPanel
        });

        root.Content = stack;
        return root;
    }

    private Control BuildFormCard()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("120,145,145,145,145,185,125,125,110"),
            RowDefinitions = new RowDefinitions("Auto,Auto")
        };

        AddLabel(grid, "Matricola", 0, 0);
        AddLabel(grid, "Nome", 1, 0);
        AddLabel(grid, "Cognome", 2, 0);
        AddLabel(grid, "Reparto", 3, 0);
        AddLabel(grid, "Mansione", 4, 0);
        AddLabel(grid, "Email", 5, 0);
        AddLabel(grid, "Telefono", 6, 0);
        AddLabel(grid, "Assunzione", 7, 0);

        _code.Watermark = "DIP001";
        _firstName.Watermark = "Mario";
        _lastName.Watermark = "Rossi";
        _department.Watermark = "Produzione";
        _jobTitle.Watermark = "Tecnico";
        _email.Watermark = "email";
        _phone.Watermark = "telefono";
        _hireDate.Watermark = "2026-01-01";

        AddControl(grid, _code, 0, 1);
        AddControl(grid, _firstName, 1, 1);
        AddControl(grid, _lastName, 2, 1);
        AddControl(grid, _department, 3, 1);
        AddControl(grid, _jobTitle, 4, 1);
        AddControl(grid, _email, 5, 1);
        AddControl(grid, _phone, 6, 1);
        AddControl(grid, _hireDate, 7, 1);

        var create = new Button
        {
            Content = "Crea",
            Background = Brush.Parse("#B5162B"),
            Foreground = Brushes.White,
            FontWeight = FontWeight.Bold,
            VerticalAlignment = VerticalAlignment.Bottom
        };
        create.Click += (_, _) => CreateEmployee();
        AddControl(grid, create, 8, 1);

        return new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(18),
            Child = grid
        };
    }

    private Control BuildSearchCard()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,180,100,130")
        };

        _search.Watermark = "Cerca per matricola, nome, cognome, reparto, mansione...";
        AddControl(grid, _search, 0, 0);

        _includeArchived.Content = "Includi archiviati";
        AddControl(grid, _includeArchived, 1, 0);

        var searchButton = new Button { Content = "Cerca" };
        searchButton.Click += (_, _) => RefreshRows();
        AddControl(grid, searchButton, 2, 0);

        var exportButton = new Button { Content = "Esporta CSV" };
        exportButton.Click += (_, _) => ExportCsv();
        AddControl(grid, exportButton, 3, 0);

        return new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(14),
            Child = grid
        };
    }

    private void ExportCsv()
    {
        var exportsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Accyourate Enterprise X", "exports");
        Directory.CreateDirectory(exportsDir);
        var path = Path.Combine(exportsDir, $"dipendenti_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

        _database.ExportEmployeesCsv(path, _search.Text, _includeArchived.IsChecked == true, _user.Username);
        _message.Text = $"Export creato: {path}";
    }

    private void CreateEmployee()
    {
        var employee = new EmployeeRecord
        {
            EmployeeCode = _code.Text ?? "",
            FirstName = _firstName.Text ?? "",
            LastName = _lastName.Text ?? "",
            Department = _department.Text ?? "",
            JobTitle = _jobTitle.Text ?? "",
            Email = _email.Text ?? "",
            Phone = _phone.Text ?? "",
            HireDate = _hireDate.Text ?? ""
        };

        var ok = _database.CreateEmployee(employee, _user.Username, out var error);

        if (!ok)
        {
            _message.Text = error;
            return;
        }

        _message.Text = "Dipendente creato correttamente.";
        ClearForm();
        RefreshRows();
    }

    private void ClearForm()
    {
        _code.Text = "";
        _firstName.Text = "";
        _lastName.Text = "";
        _department.Text = "";
        _jobTitle.Text = "";
        _email.Text = "";
        _phone.Text = "";
        _hireDate.Text = "";
    }

    private void RefreshRows()
    {
        _rowsPanel.Children.Clear();
        _rowsPanel.Spacing = 8;

        var rows = _database.GetEmployees(_search.Text, _includeArchived.IsChecked == true);

        _rowsPanel.Children.Add(new TextBlock
        {
            Text = $"Dipendenti ({rows.Count})",
            FontSize = 18,
            FontWeight = FontWeight.Bold
        });

        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions(AxTableLayout.EmployeeColumns)
        };

        AddHeader(header, "Matricola", 0);
        AddHeader(header, "Nome", 1);
        AddHeader(header, "Cognome", 2);
        AddHeader(header, "Reparto", 3);
        AddHeader(header, "Mansione", 4);
        AddHeader(header, "Email", 5);
        AddHeader(header, "Stato", 6, true);
        AddHeader(header, "Scheda", 7, true);
        AddHeader(header, "Modifica", 8, true);
        AddHeader(header, "Archivio", 9, true);
        AddHeader(header, "Stampa", 10, true);
        _rowsPanel.Children.Add(header);

        foreach (var employee in rows)
            _rowsPanel.Children.Add(BuildRow(employee));
    }

    private Control BuildRow(EmployeeRecord employee)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions(AxTableLayout.EmployeeColumns),
            Margin = new Thickness(0, 4),
            MinHeight = 44
        };

        AddText(grid, employee.EmployeeCode, 0);
        AddText(grid, employee.FirstName, 1);
        AddText(grid, employee.LastName, 2);
        AddText(grid, employee.Department, 3);
        AddText(grid, employee.JobTitle, 4);
        AddText(grid, employee.Email, 5);
        AddText(grid, employee.IsArchived ? "Archiv." : "Attivo", 6, true);

        var detail = AxTableLayout.ActionButton("Apri");
        detail.Click += (_, _) => new EmployeeDetailWindow(employee).Show();
        AddControl(grid, detail, 7, 0);

        var edit = AxTableLayout.ActionButton("Modifica");
        edit.Click += (_, _) =>
        {
            var win = new EmployeeEditWindow(_database, _user, employee);
            win.EmployeeSaved += RefreshRows;
            win.Show();
        };
        AddControl(grid, edit, 8, 0);

        var archive = AxTableLayout.ActionButton(employee.IsArchived ? "Ripristina" : "Archivia");
        archive.Click += (_, _) =>
        {
            _database.ArchiveEmployee(employee.Id, !employee.IsArchived, _user.Username);
            RefreshRows();
        };
        AddControl(grid, archive, 9, 0);

        var print = AxTableLayout.ActionButton("Scheda");
        print.Click += (_, _) => CreateEmployeeSheet(employee);
        AddControl(grid, print, 10, 0);

        return grid;
    }

    private void CreateEmployeeSheet(EmployeeRecord employee)
    {
        var exportsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Accyourate Enterprise X", "exports");
        Directory.CreateDirectory(exportsDir);
        var path = Path.Combine(exportsDir, $"scheda_{employee.EmployeeCode}_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

        File.WriteAllText(path,
$@"ACCYOURATE ENTERPRISE X
SCHEDA DIPENDENTE

Matricola: {employee.EmployeeCode}
Nome: {employee.FirstName}
Cognome: {employee.LastName}
Reparto: {employee.Department}
Mansione: {employee.JobTitle}
Email: {employee.Email}
Telefono: {employee.Phone}
Data assunzione: {employee.HireDate}
Stato: {(employee.IsArchived ? "Archiviato" : "Attivo")}
Creato il: {employee.CreatedAt}
");

        _message.Text = $"Scheda creata: {path}";
    }

    private static void AddLabel(Grid grid, string text, int column, int row)
    {
        var label = new TextBlock { Text = text, FontWeight = FontWeight.Bold, Margin = new Thickness(4) };
        AddControl(grid, label, column, row);
    }

    private static void AddHeader(Grid grid, string text, int column, bool centered = false)
    {
        var label = AxTableLayout.Header(text, centered);
        AddControl(grid, label, column, 0);
    }

    private static void AddText(Grid grid, string text, int column, bool centered = false)
    {
        var block = AxTableLayout.CellText(text, centered);
        AddControl(grid, block, column, 0);
    }

    private static void AddControl(Grid grid, Control control, int column, int row)
    {
        control.Margin = new Thickness(4);
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
