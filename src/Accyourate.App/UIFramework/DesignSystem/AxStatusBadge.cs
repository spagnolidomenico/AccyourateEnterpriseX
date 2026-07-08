using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.DesignSystem;

public static class AxStatusBadge
{
    public static Border Create(string text, AxButtonKind kind = AxButtonKind.Secondary)
    {
        return new Border
        {
            Background = Background(kind),
            CornerRadius = new CornerRadius(999),
            Padding = new Thickness(10, 4),
            Child = new TextBlock
            {
                Text = $"{Icon(kind)} {text}",
                FontSize = AxTypography.Label,
                FontWeight = FontWeight.SemiBold,
                Foreground = kind == AxButtonKind.Secondary
                    ? UiTokens.Brush(UiTokens.TextPrimary)
                    : Brushes.White,
                TextWrapping = TextWrapping.NoWrap
            }
        };
    }

    public static Border FromStatus(string status)
    {
        var normalized = (status ?? string.Empty).Trim().ToLowerInvariant();

        var kind = normalized switch
        {
            "disponibile" or "available" or "active" or "attivo" or "ok" => AxButtonKind.Success,
            "assegnato" or "assigned" or "generated" => AxButtonKind.Primary,
            "manutenzione" or "in manutenzione" or "maintenance" or "pending" => AxButtonKind.Warning,
            "dismesso" or "archiviato" or "failed" or "errore" or "error" => AxButtonKind.Danger,
            _ => AxButtonKind.Secondary
        };

        return Create(string.IsNullOrWhiteSpace(status) ? "N/D" : status, kind);
    }

    private static IBrush Background(AxButtonKind kind) => kind switch
    {
        AxButtonKind.Primary => UiTokens.Brush(UiTokens.BrandBlue),
        AxButtonKind.Success => UiTokens.Brush(UiTokens.Success),
        AxButtonKind.Warning => UiTokens.Brush(UiTokens.Warning),
        AxButtonKind.Danger => UiTokens.Brush(UiTokens.Danger),
        _ => UiTokens.Brush(UiTokens.SurfaceAlt)
    };

    private static string Icon(AxButtonKind kind) => kind switch
    {
        AxButtonKind.Primary => "🔵",
        AxButtonKind.Success => "🟢",
        AxButtonKind.Warning => "🟠",
        AxButtonKind.Danger => "🔴",
        _ => "⚪"
    };
}
