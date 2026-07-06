using Avalonia.Controls;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.DesignSystem;

public static class AxStatusMessage
{
    public static TextBlock Create()
    {
        return new TextBlock
        {
            Foreground = UiTokens.Brush(UiTokens.BrandBlue),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        };
    }

    public static void Set(TextBlock target, string message, bool isError = false)
    {
        target.Text = message;
        target.Foreground = UiTokens.Brush(isError ? UiTokens.Danger : UiTokens.BrandBlue);
    }
}
