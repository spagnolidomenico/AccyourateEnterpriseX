using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.Controls;

public sealed class EnterpriseDetailsPanel : Border
{
    private readonly StackPanel _content = new();

    public EnterpriseDetailsPanel()
    {
        Background = UiTokens.Brush(UiTokens.Surface);
        CornerRadius = new CornerRadius(22);
        Padding = new Thickness(18);

        _content.Spacing = 12;
        Child = _content;
    }

    public EnterpriseDetailsPanel SetHeader(string icon, string title, string subtitle)
    {
        _content.Children.Clear();

        _content.Children.Add(new TextBlock
        {
            Text = $"{icon} {title}",
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            TextWrapping = TextWrapping.Wrap,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        _content.Children.Add(new TextBlock
        {
            Text = subtitle,
            TextWrapping = TextWrapping.Wrap,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });

        _content.Children.Add(new Separator { Margin = new Thickness(0, 6) });
        return this;
    }

    public EnterpriseDetailsPanel AddInfo(string label, string value)
    {
        var stack = new StackPanel { Spacing = 2 };
        stack.Children.Add(new TextBlock
        {
            Text = label,
            FontSize = 12,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });
        stack.Children.Add(new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(value) ? "—" : value,
            FontWeight = FontWeight.SemiBold,
            TextWrapping = TextWrapping.Wrap,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        _content.Children.Add(new Border
        {
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(12),
            Child = stack
        });

        return this;
    }

    public EnterpriseDetailsPanel AddControl(Control control)
    {
        _content.Children.Add(control);
        return this;
    }
}
