using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;

namespace Accyourate.App;

public sealed class WorkflowWindow : Window
{
    private readonly DatabaseService _database;
    private readonly StackPanel _rowsPanel = new();
    private readonly TextBox _search = new();
    private readonly ComboBox _entityType = new();

    public WorkflowWindow(DatabaseService database)
    {
        _database = database;

        Title = "Accyourate Enterprise X - Workflow & Eventi";
        Width = 1180;
        Height = 760;
        
        MinWidth = 1024;
        MinHeight = 680;
WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");

        Content = BuildLayout();
        RefreshRows();
    }

    private Control BuildLayout()
    {
        var scroll = new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
        var stack = new StackPanel { Margin = new Thickness(24), Spacing = 16 };

        stack.Children.Add(new TextBlock
        {
            Text = "Workflow & Cronologia Eventi",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });

        stack.Children.Add(new TextBlock
        {
            Text = "Developer 3.0: motore eventi riutilizzabile per Asset IT e futuri Dispositivi Medici."
        });

        stack.Children.Add(BuildFilters());

        stack.Children.Add(new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(18),
            Child = _rowsPanel
        });

        scroll.Content = stack;
        return scroll;
    }

    private Control BuildFilters()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("220,*,100")
        };

        _entityType.ItemsSource = new[] { "Tutti", "AssetIT", "DispositivoMedico", "CapoTessile", "ControlUnit" };
        _entityType.SelectedIndex = 0;
        AddControl(grid, _entityType, 0, 0);

        _search.Watermark = "Cerca per codice, stato, evento, note, utente...";
        AddControl(grid, _search, 1, 0);

        var button = new Button { Content = "Cerca" };
        button.Click += (_, _) => RefreshRows();
        AddControl(grid, button, 2, 0);

        return new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(14),
            Child = grid
        };
    }

    private void RefreshRows()
    {
        _rowsPanel.Children.Clear();
        _rowsPanel.Spacing = 8;

        var selected = _entityType.SelectedItem?.ToString() ?? "Tutti";
        var type = selected == "Tutti" ? null : selected;
        var rows = _database.GetWorkflowEvents(type, _search.Text, 200);

        _rowsPanel.Children.Add(new TextBlock
        {
            Text = $"Eventi workflow ({rows.Count})",
            FontSize = 18,
            FontWeight = FontWeight.Bold
        });

        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("150,120,120,130,130,150,220,120")
        };

        AddHeader(header, "Data", 0);
        AddHeader(header, "Tipo", 1);
        AddHeader(header, "Codice", 2);
        AddHeader(header, "Da", 3);
        AddHeader(header, "A", 4);
        AddHeader(header, "Evento", 5);
        AddHeader(header, "Note", 6);
        AddHeader(header, "Utente", 7);
        _rowsPanel.Children.Add(header);

        foreach (var row in rows)
            _rowsPanel.Children.Add(BuildRow(row));
    }

    private static Control BuildRow(WorkflowEventRecord row)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("150,120,120,130,130,150,220,120"),
            Margin = new Thickness(0, 4)
        };

        AddText(grid, row.CreatedAt, 0);
        AddText(grid, row.EntityType, 1);
        AddText(grid, row.EntityCode, 2);
        AddText(grid, row.FromStatus, 3);
        AddText(grid, row.ToStatus, 4);
        AddText(grid, row.EventType, 5);
        AddText(grid, row.Notes, 6);
        AddText(grid, row.CreatedBy, 7);

        return grid;
    }

    private static void AddHeader(Grid grid, string text, int column)
    {
        var label = new TextBlock
        {
            Text = text,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B"),
            Margin = new Thickness(4)
        };
        AddControl(grid, label, column, 0);
    }

    private static void AddText(Grid grid, string text, int column)
    {
        var block = new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(text) ? "-" : text,
            Margin = new Thickness(4)
        };
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
