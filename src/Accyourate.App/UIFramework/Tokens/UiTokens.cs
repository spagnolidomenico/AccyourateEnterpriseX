using Avalonia.Media;

namespace Accyourate.App.UIFramework.Tokens;

public static class UiTokens
{
    public const string BrandBlue = "#0A84FF";
    public const string BrandAccent = "#B5162B";
    public const string Background = "#F5F5F7";
    public const string Surface = "#FFFFFF";
    public const string SurfaceAlt = "#F2F2F7";
    public const string Sidebar = "#FBFBFD";
    public const string Border = "#E5E5EA";
    public const string TextPrimary = "#1D1D1F";
    public const string TextSecondary = "#6E6E73";
    public const string Success = "#34C759";
    public const string Warning = "#FF9F0A";
    public const string Danger = "#FF3B30";
    public const string Info = "#0A84FF";


    public const string PremiumBlueSoft = "#E8F1FF";
    public const string PremiumSurfaceGlass = "#FAFAFC";
    public const string PremiumShadow = "#18000000";
    public const string PremiumHover = "#EEF2FF";
    public const string PremiumSelected = "#E8F1FF";


    public const string DarkBackground = "#0B1020";
    public const string DarkSurface = "#111827";
    public const string DarkSurfaceAlt = "#1F2937";
    public const string DarkSidebar = "#0F172A";
    public const string DarkBorder = "#273244";
    public const string DarkTextPrimary = "#F9FAFB";
    public const string DarkTextSecondary = "#CBD5E1";

    public static string BackgroundFor(UiThemeMode mode) => mode == UiThemeMode.Dark ? DarkBackground : Background;
    public static string SurfaceFor(UiThemeMode mode) => mode == UiThemeMode.Dark ? DarkSurface : Surface;
    public static string SurfaceAltFor(UiThemeMode mode) => mode == UiThemeMode.Dark ? DarkSurfaceAlt : SurfaceAlt;
    public static string SidebarFor(UiThemeMode mode) => mode == UiThemeMode.Dark ? DarkSidebar : Sidebar;
    public static string BorderFor(UiThemeMode mode) => mode == UiThemeMode.Dark ? DarkBorder : Border;
    public static string TextPrimaryFor(UiThemeMode mode) => mode == UiThemeMode.Dark ? DarkTextPrimary : TextPrimary;
    public static string TextSecondaryFor(UiThemeMode mode) => mode == UiThemeMode.Dark ? DarkTextSecondary : TextSecondary;

    public static IBrush Brush(string color) => Avalonia.Media.Brush.Parse(color);
}
