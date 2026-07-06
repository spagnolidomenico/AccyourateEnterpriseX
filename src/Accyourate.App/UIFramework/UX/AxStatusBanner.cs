using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.DesignSystem;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.UX;

public static class AxStatusBanner
{
    public static Border Create(string message, AxMessageKind kind = AxMessageKind.Info)
    {
        return new Border
        {
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            BorderBrush = Brush(kind),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(12),
            Child = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Spacing = AxSpacing.MicroSpacing,
                Children =
                {
                    new TextBlock { Text = Icon(kind), FontSize = 18 },
                    new TextBlock
                    {
                        Text = message,
                        Foreground = UiTokens.Brush(UiTokens.TextPrimary),
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
        };
    }

    private static IBrush Brush(AxMessageKind kind) => kind switch
    {
        AxMessageKind.Success => UiTokens.Brush(UiTokens.Success),
        AxMessageKind.Warning => UiTokens.Brush(UiTokens.Warning),
        AxMessageKind.Error => UiTokens.Brush(UiTokens.Danger),
        _ => UiTokens.Brush(UiTokens.BrandBlue)
    };

    private static string Icon(AxMessageKind kind) => kind switch
    {
        AxMessageKind.Success => "✅",
        AxMessageKind.Warning => "⚠️",
        AxMessageKind.Error => "⛔",
        _ => "ℹ️"
    };
}
