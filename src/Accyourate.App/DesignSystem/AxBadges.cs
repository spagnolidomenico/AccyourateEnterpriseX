using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Foundation;

namespace Accyourate.App.DesignSystem;

public static class AxBadges
{
    public static Border Status(string text, string kind)
    {
        var color = kind switch
        {
            "success" => AxSemanticTokens.Success,
            "warning" => AxSemanticTokens.Warning,
            "danger" => AxSemanticTokens.Danger,
            "info" => AxSemanticTokens.Info,
            _ => AxSemanticTokens.TextMuted
        };

        return new Border
        {
            Background = Brush.Parse(color),
            CornerRadius = new CornerRadius(999),
            Padding = new Thickness(10, 4),
            Child = new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                FontWeight = FontWeight.SemiBold,
                FontSize = 12
            }
        };
    }
}
