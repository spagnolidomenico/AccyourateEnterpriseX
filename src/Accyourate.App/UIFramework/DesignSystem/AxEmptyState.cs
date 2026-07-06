using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.DesignSystem;

public static class AxEmptyState
{
    public static Control Create(string icon, string title, string description)
    {
        var stack = new StackPanel
        {
            Spacing = AxSpacing.MicroSpacing,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Thickness(AxSpacing.PageMargin)
        };

        stack.Children.Add(new TextBlock { Text = icon, FontSize = 36, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center });
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = AxTypography.CardTitle,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        });
        stack.Children.Add(new TextBlock
        {
            Text = description,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        });

        return AxCard.Create(stack);
    }
}
