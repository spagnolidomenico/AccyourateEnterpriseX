using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.Components;

public static class UiComponentFactory
{
    public static Border Card(Control child)
    {
        return new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            CornerRadius = new CornerRadius(22),
            Padding = new Thickness(20),
            Margin = new Thickness(6),
            BoxShadow = new BoxShadows(new BoxShadow
            {
                Blur = 22,
                OffsetY = 6,
                Color = Color.Parse(UiTokens.PremiumShadow)
            }),
            Child = child
        };
    }

    public static Button PrimaryButton(string text)
    {
        return new Button
        {
            Content = text,
            Background = UiTokens.Brush(UiTokens.BrandBlue),
            Foreground = Brushes.White,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(14, 10),
            CornerRadius = new CornerRadius(12)
        };
    }

    public static TextBlock Title(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 30,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        };
    }

    public static TextBlock Body(string text)
    {
        return new TextBlock
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        };
    }
}
