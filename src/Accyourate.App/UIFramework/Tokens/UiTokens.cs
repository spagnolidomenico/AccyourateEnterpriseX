using Avalonia.Media;
using Accyourate.App.UIFramework.Foundation;

namespace Accyourate.App.UIFramework.Tokens;

/// <summary>
/// Backward-compatible token facade. New foundation code should consume
/// AxSemanticTokens, AxLayoutTokens and AxTypographyTokens directly.
/// </summary>
public static class UiTokens
{
    public const string BrandBlue = AxSemanticTokens.BrandPrimary;
    public const string BrandAccent = AxSemanticTokens.BrandAccent;
    public const string Background = AxSemanticTokens.Background;
    public const string Surface = AxSemanticTokens.Surface;
    public const string SurfaceAlt = AxSemanticTokens.SurfaceSubtle;
    public const string Sidebar = AxSemanticTokens.NavigationSurface;
    public const string Border = AxSemanticTokens.Border;
    public const string TextPrimary = AxSemanticTokens.TextPrimary;
    public const string TextSecondary = AxSemanticTokens.TextSecondary;
    public const string TextMuted = AxSemanticTokens.TextMuted;
    public const string Success = AxSemanticTokens.Success;
    public const string Warning = AxSemanticTokens.Warning;
    public const string Danger = AxSemanticTokens.Danger;
    public const string Info = AxSemanticTokens.Info;

    public const string PremiumBlueSoft = AxSemanticTokens.Selection;
    public const string PremiumSurfaceGlass = AxSemanticTokens.GlassSurface;
    public const string PremiumShadow = AxSemanticTokens.Shadow;
    public const string PremiumHover = AxSemanticTokens.Hover;
    public const string PremiumSelected = AxSemanticTokens.Selection;

    public const string DarkBackground = AxSemanticTokens.DarkBackground;
    public const string DarkSurface = AxSemanticTokens.DarkSurface;
    public const string DarkSurfaceAlt = AxSemanticTokens.DarkSurfaceSubtle;
    public const string DarkSidebar = AxSemanticTokens.DarkNavigationSurface;
    public const string DarkBorder = AxSemanticTokens.DarkBorder;
    public const string DarkTextPrimary = AxSemanticTokens.DarkTextPrimary;
    public const string DarkTextSecondary = AxSemanticTokens.DarkTextSecondary;

    public static string BackgroundFor(UiThemeMode mode) => AxThemePalette.For(mode).Background;
    public static string SurfaceFor(UiThemeMode mode) => AxThemePalette.For(mode).Surface;
    public static string SurfaceAltFor(UiThemeMode mode) => AxThemePalette.For(mode).SurfaceSubtle;
    public static string SidebarFor(UiThemeMode mode) => AxThemePalette.For(mode).NavigationSurface;
    public static string BorderFor(UiThemeMode mode) => AxThemePalette.For(mode).Border;
    public static string TextPrimaryFor(UiThemeMode mode) => AxThemePalette.For(mode).TextPrimary;
    public static string TextSecondaryFor(UiThemeMode mode) => AxThemePalette.For(mode).TextSecondary;

    public static IBrush Brush(string color) => Avalonia.Media.Brush.Parse(color);
}
