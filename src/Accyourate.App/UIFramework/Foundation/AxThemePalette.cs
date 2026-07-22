using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.Foundation;

public readonly record struct AxThemePalette(
    string Background,
    string Surface,
    string SurfaceSubtle,
    string NavigationSurface,
    string Border,
    string TextPrimary,
    string TextSecondary)
{
    public static AxThemePalette For(UiThemeMode mode) => mode switch
    {
        UiThemeMode.Dark => new(
            AxSemanticTokens.DarkBackground,
            AxSemanticTokens.DarkSurface,
            AxSemanticTokens.DarkSurfaceSubtle,
            AxSemanticTokens.DarkNavigationSurface,
            AxSemanticTokens.DarkBorder,
            AxSemanticTokens.DarkTextPrimary,
            AxSemanticTokens.DarkTextSecondary),
        _ => new(
            AxSemanticTokens.Background,
            AxSemanticTokens.Surface,
            AxSemanticTokens.SurfaceSubtle,
            AxSemanticTokens.NavigationSurface,
            AxSemanticTokens.Border,
            AxSemanticTokens.TextPrimary,
            AxSemanticTokens.TextSecondary)
    };
}
