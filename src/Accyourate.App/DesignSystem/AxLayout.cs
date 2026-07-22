using Avalonia;
using Avalonia.Controls;
using Accyourate.App.UIFramework.Foundation;

namespace Accyourate.App.DesignSystem;

public static class AxLayout
{
    public static StackPanel Page() => new()
    {
        Margin = new Thickness(24),
        Spacing = 18
    };

    public static ScrollViewer ScrollPage(Control content) => new()
    {
        VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
        HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
        Content = content
    };

    public static Border PageShell(Control content) => new()
    {
        Background = AxThemeManager.Brush(AxSemanticTokens.Background),
        Child = content
    };
}
