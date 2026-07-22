using Avalonia;
using Avalonia.Controls;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.DesignSystem;

/// <summary>
/// Reusable inspector surface for master-detail enterprise pages.
/// Keeps visual treatment and minimum sizing consistent across modules.
/// </summary>
public sealed class AxInspectorPanel : Border
{
    public AxInspectorPanel(Control content)
    {
        Background = UiTokens.Brush(UiTokens.Surface);
        BorderBrush = UiTokens.Brush(UiTokens.Border);
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(16);
        Padding = new Thickness(18);
        MinWidth = 320;
        MinHeight = 420;
        ClipToBounds = true;
        Child = content;
    }
}
