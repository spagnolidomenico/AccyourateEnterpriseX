using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;

namespace Accyourate.App;

public sealed class EmployeeDetailWindow : Window
{
    private readonly EmployeeRecord _employee;

    public EmployeeDetailWindow(EmployeeRecord employee)
    {
        _employee = employee;

        Title = $"Accyourate Enterprise X - Scheda Dipendente {_employee.EmployeeCode}";
        Width = 680;
        Height = 620;
        
        MinWidth = 1024;
        MinHeight = 680;
WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");

        Content = BuildLayout();
    }

    private Control BuildLayout()
    {
        var stack = new StackPanel { Margin = new Thickness(24), Spacing = 12 };

        stack.Children.Add(new TextBlock
        {
            Text = $"Scheda Dipendente - {_employee.EmployeeCode}",
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });

        AddRow(stack, "Nome completo", _employee.FullName);
        AddRow(stack, "Reparto", _employee.Department);
        AddRow(stack, "Mansione", _employee.JobTitle);
        AddRow(stack, "Email", _employee.Email);
        AddRow(stack, "Telefono", _employee.Phone);
        AddRow(stack, "Data assunzione", _employee.HireDate);
        AddRow(stack, "Stato", _employee.IsArchived ? "Archiviato" : "Attivo");
        AddRow(stack, "Creato il", _employee.CreatedAt);

        stack.Children.Add(new Separator());

        stack.Children.Add(new TextBlock
        {
            Text = "Sezioni future",
            FontSize = 18,
            FontWeight = FontWeight.Bold
        });
        stack.Children.Add(new TextBlock { Text = "• Allegati" });
        stack.Children.Add(new TextBlock { Text = "• Storico modifiche" });
        stack.Children.Add(new TextBlock { Text = "• Asset assegnati" });
        stack.Children.Add(new TextBlock { Text = "• Documenti generati" });

        return new ScrollViewer
        {
            Content = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(20),
                Margin = new Thickness(20),
                Child = stack
            }
        };
    }

    private static void AddRow(StackPanel stack, string label, string value)
    {
        stack.Children.Add(new TextBlock
        {
            Text = label,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#555555")
        });
        stack.Children.Add(new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(value) ? "-" : value,
            FontSize = 15
        });
    }
}
