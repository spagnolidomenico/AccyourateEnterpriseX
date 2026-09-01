using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.DesignSystem;

public sealed record AxResponsiveRecordField(string Label, string Value, double Width = 160, string? Color = null);

public static class AxResponsiveRecordCard
{
    public static Control Create(string title, IEnumerable<AxResponsiveRecordField> fields, params Control[] actions)
    {
        var root = new StackPanel { Spacing = 9 };
        root.Children.Add(new TextBlock { Text = title, FontSize = 17, FontWeight = FontWeight.Bold, TextWrapping = TextWrapping.Wrap });
        var details = new WrapPanel();
        foreach (var field in fields)
        {
            details.Children.Add(new StackPanel
            {
                Width = field.Width,
                Margin = new Thickness(0, 0, 10, 8),
                Spacing = 2,
                Children =
                {
                    new TextBlock { Text = field.Label, FontSize = 11, Foreground = UiTokens.Brush(UiTokens.TextSecondary) },
                    new TextBlock { Text = string.IsNullOrWhiteSpace(field.Value) ? "—" : field.Value, FontWeight = FontWeight.SemiBold, Foreground = UiTokens.Brush(field.Color ?? UiTokens.TextPrimary), TextWrapping = TextWrapping.Wrap }
                }
            });
        }
        root.Children.Add(details);
        if (actions.Length > 0)
        {
            var commands = new WrapPanel();
            foreach (var action in actions) { action.Margin = new Thickness(0, 0, 6, 6); commands.Children.Add(action); }
            root.Children.Add(commands);
        }
        return new Border { Padding = new Thickness(14), BorderBrush = UiTokens.Brush(UiTokens.Border), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8), Background = UiTokens.Brush(UiTokens.Surface), Child = root };
    }
}
