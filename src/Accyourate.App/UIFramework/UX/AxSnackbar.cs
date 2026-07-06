using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.DesignSystem;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.UX;

public sealed class AxSnackbar : Border
{
    private readonly TextBlock _text = new();

    public AxSnackbar()
    {
        Background = UiTokens.Brush(UiTokens.Surface);
        BorderBrush = UiTokens.Brush(UiTokens.Border);
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(16);
        Padding = new Thickness(14);
        IsVisible = false;

        Child = _text;
    }

    public async void Show(string message, AxMessageKind kind = AxMessageKind.Info, int milliseconds = 3500)
    {
        _text.Text = $"{Icon(kind)} {message}";
        _text.Foreground = UiTokens.Brush(kind == AxMessageKind.Error ? UiTokens.Danger : UiTokens.TextPrimary);
        IsVisible = true;

        await Task.Delay(milliseconds);
        IsVisible = false;
    }

    private static string Icon(AxMessageKind kind) => kind switch
    {
        AxMessageKind.Success => "✅",
        AxMessageKind.Warning => "⚠️",
        AxMessageKind.Error => "⛔",
        _ => "ℹ️"
    };
}
