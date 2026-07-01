using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.Controls;

public sealed class EnterpriseToolbar : StackPanel
{
    public EnterpriseToolbar()
    {
        Orientation = Avalonia.Layout.Orientation.Horizontal;
        Spacing = 8;
    }

    public EnterpriseToolbar AddPrimary(string text, Action action, string? tooltip = null)
    {
        Children.Add(Button(text, action, UiTokens.BrandBlue, Brushes.White, tooltip, true));
        return this;
    }

    public EnterpriseToolbar AddSecondary(string text, Action action, string? tooltip = null)
    {
        Children.Add(Button(text, action, UiTokens.Surface, UiTokens.Brush(UiTokens.TextPrimary), tooltip, false));
        return this;
    }

    public EnterpriseToolbar AddDanger(string text, Action action, string? tooltip = null)
    {
        Children.Add(Button(text, action, UiTokens.SurfaceAlt, UiTokens.Brush(UiTokens.Danger), tooltip, true));
        return this;
    }

    public EnterpriseToolbar AddPlaceholder(string text, string tooltip)
    {
        Children.Add(Button(text, () => { }, UiTokens.Surface, UiTokens.Brush(UiTokens.TextPrimary), tooltip, false));
        return this;
    }

    private static Button Button(
        string text,
        Action action,
        string backgroundToken,
        IBrush foreground,
        string? tooltip,
        bool bold)
    {
        var b = new Button
        {
            Content = text,
            Background = UiTokens.Brush(backgroundToken),
            Foreground = foreground,
            FontWeight = bold ? FontWeight.Bold : FontWeight.Normal,
            Padding = new Thickness(10, 8),
            CornerRadius = new CornerRadius(12)
        };

        b.Click += (_, _) => action();

        if (!string.IsNullOrWhiteSpace(tooltip))
            ToolTip.SetTip(b, tooltip);

        return b;
    }
}
