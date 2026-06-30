using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Accyourate.App.Shared.UI;

public static class ResponsiveUi
{
    public const double DefaultMinWidth = 1024;
    public const double DefaultMinHeight = 680;

    public static void ApplyWindowDefaults(Window window, double width = 1180, double height = 780)
    {
        window.Width = width;
        window.Height = height;
        window.MinWidth = DefaultMinWidth;
        window.MinHeight = DefaultMinHeight;
        window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        window.Background = Brush.Parse("#F7F7F6");
    }

    public static ScrollViewer PageScroll(Control content)
    {
        return new ScrollViewer
        {
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Content = content
        };
    }

    public static StackPanel PageStack()
    {
        return new StackPanel
        {
            Margin = new Thickness(24),
            Spacing = 16
        };
    }

    public static Border Card(Control content)
    {
        return new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(18),
            Margin = new Thickness(0, 0, 0, 8),
            Child = content
        };
    }

    public static TextBlock Hint(string text)
    {
        return new TextBlock
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            Foreground = Brush.Parse("#555555")
        };
    }
}
