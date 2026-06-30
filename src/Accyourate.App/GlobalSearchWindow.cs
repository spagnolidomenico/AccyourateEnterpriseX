using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;

namespace Accyourate.App;

public sealed class GlobalSearchWindow : Window
{
    private readonly DatabaseService _database;
    private readonly TextBox _search = new();
    private readonly StackPanel _results = new();

    public GlobalSearchWindow(DatabaseService database)
    {
        _database = database;

        Title = "Accyourate Enterprise X - Ricerca Globale";
        Width = 920;
        Height = 680;
        
        MinWidth = 1024;
        MinHeight = 680;
WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");

        Content = BuildLayout();
    }

    private Control BuildLayout()
    {
        var stack = new StackPanel { Margin = new Avalonia.Thickness(24), Spacing = 16 };

        stack.Children.Add(new TextBlock
        {
            Text = "Ricerca Globale",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,120") };
        _search.Watermark = "Cerca persone, asset, dispositivi, documenti...";
        AddControl(grid, _search, 0, 0);

        var button = new Button { Content = "Cerca" };
        button.Click += (_, _) => Refresh();
        AddControl(grid, button, 1, 0);

        stack.Children.Add(Card(grid));
        stack.Children.Add(Card(_results));

        return new ScrollViewer { Content = stack };
    }

    private void Refresh()
    {
        _results.Children.Clear();
        _results.Spacing = 8;

        var rows = _database.GlobalSearch(_search.Text);
        _results.Children.Add(new TextBlock { Text = $"Risultati ({rows.Count})", FontSize = 18, FontWeight = FontWeight.Bold });

        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("120,130,260,360") };
        AddHeader(header, "Area", 0);
        AddHeader(header, "Codice", 1);
        AddHeader(header, "Titolo", 2);
        AddHeader(header, "Descrizione", 3);
        _results.Children.Add(header);

        foreach (var r in rows)
        {
            var row = new Grid { ColumnDefinitions = new ColumnDefinitions("120,130,260,360") };
            AddText(row, r.Area, 0);
            AddText(row, r.Code, 1);
            AddText(row, r.Title, 2);
            AddText(row, r.Description, 3);
            _results.Children.Add(row);
        }
    }

    private static Border Card(Control content)
    {
        return new Border
        {
            Background = Brushes.White,
            CornerRadius = new Avalonia.CornerRadius(14),
            Padding = new Avalonia.Thickness(18),
            Child = content
        };
    }

    private static void AddHeader(Grid grid, string text, int column) => AddControl(grid, new TextBlock { Text = text, FontWeight = FontWeight.Bold, Foreground = Brush.Parse("#B5162B") }, column, 0);
    private static void AddText(Grid grid, string text, int column) => AddControl(grid, new TextBlock { Text = string.IsNullOrWhiteSpace(text) ? "-" : text }, column, 0);

    private static void AddControl(Grid grid, Control control, int column, int row)
    {
        control.Margin = new Avalonia.Thickness(4);
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
