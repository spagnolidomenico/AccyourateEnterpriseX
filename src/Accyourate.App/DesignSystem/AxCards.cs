using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Accyourate.App.DesignSystem;

public static class AxCards
{
    public static Border Card(Control content) => new()
    {
        Background = AccyourateDesignTokens.Brush(AccyourateDesignTokens.Surface),
        CornerRadius = new CornerRadius(AccyourateDesignTokens.RadiusLarge),
        Padding = new Thickness(AccyourateDesignTokens.SpaceL),
        Margin = new Thickness(6),
        BoxShadow = new BoxShadows(new BoxShadow
        {
            Blur = 18,
            Spread = 0,
            OffsetY = 4,
            Color = Color.Parse("#12000000")
        }),
        Child = content
    };

    public static Border Kpi(string icon, string title, string value, string subtitle, string color)
    {
        var row = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = AccyourateDesignTokens.SpaceL
        };

        row.Children.Add(new Border
        {
            Width = 58,
            Height = 58,
            Background = AccyourateDesignTokens.Brush(color),
            CornerRadius = new CornerRadius(14),
            Child = new TextBlock
            {
                Text = icon,
                Foreground = Brushes.White,
                FontSize = 26,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            }
        });

        var text = new StackPanel();
        text.Children.Add(new TextBlock { Text = title, Foreground = AccyourateDesignTokens.Brush(AccyourateDesignTokens.TextPrimary) });
        text.Children.Add(new TextBlock { Text = value, FontSize = 28, FontWeight = FontWeight.Bold, Foreground = AccyourateDesignTokens.Brush(AccyourateDesignTokens.TextPrimary) });
        text.Children.Add(new TextBlock { Text = subtitle, Foreground = AccyourateDesignTokens.Brush(AccyourateDesignTokens.TextSecondary) });
        row.Children.Add(text);

        return Card(row);
    }
}
