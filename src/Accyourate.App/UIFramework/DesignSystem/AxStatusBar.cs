using Avalonia;
using Avalonia.Controls;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.DesignSystem;

public static class AxStatusBar
{
    public static Border Create(string version = "0.9.0 RC1", string status = "Pronto")
    {
        var stack = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 18
        };

        stack.Children.Add(Item("Accyourate Enterprise X", version));
        stack.Children.Add(Item("Stato", status));
        stack.Children.Add(Item("Ambiente", "Locale"));
        stack.Children.Add(Item("Database", "SQLite"));

        return new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(1, 1, 0, 0),
            Padding = new Thickness(12, 8),
            Child = stack
        };
    }

    private static Control Item(string label, string value)
    {
        return new TextBlock
        {
            Text = $"{label}: {value}",
            FontSize = 12,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = Avalonia.Media.TextWrapping.NoWrap
        };
    }
}
