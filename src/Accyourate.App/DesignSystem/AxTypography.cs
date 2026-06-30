using Avalonia.Controls;
using Avalonia.Media;

namespace Accyourate.App.DesignSystem;

public static class AxTypography
{
    public static TextBlock PageTitle(string text) => new()
    {
        Text = text,
        FontSize = 30,
        FontWeight = FontWeight.Bold,
        Foreground = AccyourateDesignTokens.Brush(AccyourateDesignTokens.TextPrimary)
    };

    public static TextBlock SectionTitle(string text) => new()
    {
        Text = text,
        FontSize = 20,
        FontWeight = FontWeight.Bold,
        Foreground = AccyourateDesignTokens.Brush(AccyourateDesignTokens.TextPrimary)
    };

    public static TextBlock Body(string text) => new()
    {
        Text = text,
        FontSize = 14,
        TextWrapping = TextWrapping.Wrap,
        Foreground = AccyourateDesignTokens.Brush(AccyourateDesignTokens.TextSecondary)
    };

    public static TextBlock Caption(string text) => new()
    {
        Text = text,
        FontSize = 12,
        TextWrapping = TextWrapping.Wrap,
        Foreground = AccyourateDesignTokens.Brush(AccyourateDesignTokens.TextMuted)
    };
}
