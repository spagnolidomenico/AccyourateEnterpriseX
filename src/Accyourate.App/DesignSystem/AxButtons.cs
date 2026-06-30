using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Accyourate.App.DesignSystem;

public static class AxButtons
{
    public static Button Primary(string text) => Base(text, AccyourateDesignTokens.BrandPrimary, "#FFFFFF");
    public static Button Secondary(string text) => Base(text, AccyourateDesignTokens.SurfaceAlt, AccyourateDesignTokens.TextPrimary);
    public static Button Success(string text) => Base(text, AccyourateDesignTokens.Success, "#FFFFFF");
    public static Button Warning(string text) => Base(text, AccyourateDesignTokens.Warning, "#FFFFFF");
    public static Button Danger(string text) => Base(text, AccyourateDesignTokens.Danger, "#FFFFFF");

    private static Button Base(string text, string background, string foreground) => new()
    {
        Content = text,
        Background = Brush.Parse(background),
        Foreground = Brush.Parse(foreground),
        FontWeight = FontWeight.SemiBold,
        Padding = new Thickness(14, 9),
        CornerRadius = new CornerRadius(AccyourateDesignTokens.RadiusMedium)
    };
}
