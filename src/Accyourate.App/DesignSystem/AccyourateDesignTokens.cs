using Avalonia.Media;
using Accyourate.App.UIFramework.Foundation;

namespace Accyourate.App.DesignSystem;

/// <summary>
/// Legacy compatibility layer for the original DesignSystem namespace.
/// New components belong to UIFramework and should use the canonical M3 tokens.
/// </summary>
[Obsolete("Use AxSemanticTokens, AxLayoutTokens and AxTypographyTokens from UIFramework.Foundation.")]
public static class AccyourateDesignTokens
{
    public const string BrandPrimary = AxSemanticTokens.BrandPrimary;
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
    public const string Purple = AxSemanticTokens.Highlight;

    public const double RadiusSmall = AxLayoutTokens.RadiusSmall;
    public const double RadiusMedium = AxLayoutTokens.RadiusMedium;
    public const double RadiusLarge = AxLayoutTokens.RadiusLarge;
    public const double SpaceS = AxLayoutTokens.Space2;
    public const double SpaceM = AxLayoutTokens.Space3;
    public const double SpaceL = AxLayoutTokens.Space4;
    public const double SpaceXL = AxLayoutTokens.Space6;

    public static IBrush Brush(string color) => Avalonia.Media.Brush.Parse(color);
}
