using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.Components;

public static class UiTypography
{
    public static TextBlock Display(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 34,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        };
    }

    public static TextBlock Subtitle(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 15,
            TextWrapping = TextWrapping.Wrap,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        };
    }

    public static TextBlock Label(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        };
    }
}
