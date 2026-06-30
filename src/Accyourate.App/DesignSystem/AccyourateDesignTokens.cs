using Avalonia.Media;

namespace Accyourate.App.DesignSystem;

public static class AccyourateDesignTokens
{
    public const string BrandPrimary = "#0A84FF";
    public const string BrandAccent = "#B5162B";
    public const string Background = "#F5F5F7";
    public const string Surface = "#FFFFFF";
    public const string SurfaceAlt = "#F2F2F7";
    public const string Sidebar = "#FBFBFD";
    public const string Border = "#E5E5EA";
    public const string TextPrimary = "#1D1D1F";
    public const string TextSecondary = "#6E6E73";
    public const string TextMuted = "#8E8E93";
    public const string Success = "#34C759";
    public const string Warning = "#FF9F0A";
    public const string Danger = "#FF3B30";
    public const string Info = "#0A84FF";
    public const string Purple = "#8E5CF7";

    public const double RadiusSmall = 8;
    public const double RadiusMedium = 12;
    public const double RadiusLarge = 16;
    public const double SpaceS = 8;
    public const double SpaceM = 12;
    public const double SpaceL = 16;
    public const double SpaceXL = 24;

    public static IBrush Brush(string color) => Avalonia.Media.Brush.Parse(color);
}
