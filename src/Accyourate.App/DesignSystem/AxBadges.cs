using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Accyourate.App.DesignSystem;

public static class AxBadges
{
    public static Border Status(string text, string kind)
    {
        var color = kind switch
        {
            "success" => AccyourateDesignTokens.Success,
            "warning" => AccyourateDesignTokens.Warning,
            "danger" => AccyourateDesignTokens.Danger,
            "info" => AccyourateDesignTokens.Info,
            _ => AccyourateDesignTokens.TextMuted
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
