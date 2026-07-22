using Avalonia.Controls;

namespace Accyourate.App.UIFramework.Layout;

public static class EnterpriseAdaptiveLayout
{
    public const double CompactWidth = 1080;
    public const double NarrowWidth = 920;

    public static bool IsCompact(double width) => width > 0 && width < CompactWidth;

    public static bool IsNarrow(double width) => width > 0 && width < NarrowWidth;

    public static void ArrangeMasterDetails(Grid grid, Control master, Control details, double width)
    {
        grid.Children.Clear();

        if (IsCompact(width))
        {
            grid.ColumnDefinitions = new ColumnDefinitions("*");
            grid.RowDefinitions = new RowDefinitions("*,16,Auto");

            Grid.SetColumn(master, 0);
            Grid.SetRow(master, 0);
            grid.Children.Add(master);

            Grid.SetColumn(details, 0);
            Grid.SetRow(details, 2);
            grid.Children.Add(details);
            return;
        }

        grid.ColumnDefinitions = new ColumnDefinitions("*,16,340");
        grid.RowDefinitions = new RowDefinitions("*");

        Grid.SetColumn(master, 0);
        Grid.SetRow(master, 0);
        grid.Children.Add(master);

        Grid.SetColumn(details, 2);
        Grid.SetRow(details, 0);
        grid.Children.Add(details);
    }
}
