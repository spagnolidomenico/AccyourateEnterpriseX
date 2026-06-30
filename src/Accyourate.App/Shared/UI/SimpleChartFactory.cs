using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;

namespace Accyourate.App.Shared.UI;

public static class SimpleChartFactory
{
    public static Control HorizontalBarChart(IEnumerable<AnalyticsChartPointRecord> points, int maxWidth = 360)
    {
        var rows = new StackPanel { Spacing = 8 };
        var list = points.ToList();
        var max = list.Count == 0 ? 1 : Math.Max(1, list.Max(x => x.Value));

        foreach (var point in list)
        {
            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("130,*,55")
            };

            Add(grid, new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(point.Label) ? "N/D" : point.Label,
                TextWrapping = TextWrapping.Wrap
            }, 0);

            var barWidth = Math.Max(8, (double)point.Value / max * maxWidth);
            var bar = new Border
            {
                Background = Brush.Parse("#B5162B"),
                CornerRadius = new CornerRadius(6),
                Width = barWidth,
                Height = 18,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            };
            Add(grid, bar, 1);

            Add(grid, new TextBlock
            {
                Text = point.Value.ToString(),
                FontWeight = FontWeight.Bold
            }, 2);

            rows.Children.Add(grid);
        }

        if (list.Count == 0)
            rows.Children.Add(new TextBlock { Text = "Nessun dato disponibile." });

        return rows;
    }

    private static void Add(Grid grid, Control control, int column)
    {
        control.Margin = new Thickness(4);
        Grid.SetColumn(control, column);
        grid.Children.Add(control);
    }
}
