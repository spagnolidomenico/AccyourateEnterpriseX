using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.DesignSystem;

public static class AxButton
{
    public static Button Create(string text, Action action, AxButtonKind kind = AxButtonKind.Secondary)
    {
        var button = new Button
        {
            Content = text,
            Background = Background(kind),
            Foreground = Foreground(kind),
            FontWeight = kind == AxButtonKind.Secondary ? FontWeight.Normal : FontWeight.Bold,
            Padding = new Thickness(12, 8),
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(AxSpacing.MicroSpacing, 0, 0, 0)
        };

        button.Click += (_, _) => action();
        return button;
    }

    private static IBrush Background(AxButtonKind kind) => kind switch
    {
        AxButtonKind.Primary => UiTokens.Brush(UiTokens.BrandBlue),
        AxButtonKind.Danger => UiTokens.Brush(UiTokens.Danger),
        AxButtonKind.Success => UiTokens.Brush(UiTokens.Success),
        AxButtonKind.Warning => UiTokens.Brush(UiTokens.Warning),
        _ => UiTokens.Brush(UiTokens.Surface)
    };

    private static IBrush Foreground(AxButtonKind kind)
    {
        return kind == AxButtonKind.Secondary ? UiTokens.Brush(UiTokens.TextPrimary) : Brushes.White;
    }
}
