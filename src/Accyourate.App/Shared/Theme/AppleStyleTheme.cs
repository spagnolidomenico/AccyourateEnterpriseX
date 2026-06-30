using Avalonia.Media;

namespace Accyourate.App.Shared.Theme;

public static class AppleStyleTheme
{
    public const string WindowBackground = "#F5F5F7";
    public const string SidebarBackground = "#FBFBFD";
    public const string TopBarBackground = "#FFFFFF";
    public const string CardBackground = "#FFFFFF";
    public const string PrimaryBlue = "#0A84FF";
    public const string TextPrimary = "#1D1D1F";
    public const string TextSecondary = "#6E6E73";
    public const string Border = "#E5E5EA";
    public const string Selection = "#E8F1FF";
    public const string Green = "#34C759";
    public const string Orange = "#FF9F0A";
    public const string Red = "#FF3B30";
    public const string Purple = "#8E5CF7";

    public static IBrush WindowBrush => Brush.Parse(WindowBackground);
    public static IBrush SidebarBrush => Brush.Parse(SidebarBackground);
    public static IBrush CardBrush => Brush.Parse(CardBackground);
    public static IBrush PrimaryBrush => Brush.Parse(PrimaryBlue);
    public static IBrush TextPrimaryBrush => Brush.Parse(TextPrimary);
    public static IBrush TextSecondaryBrush => Brush.Parse(TextSecondary);
}
