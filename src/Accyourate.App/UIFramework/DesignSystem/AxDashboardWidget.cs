using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.DesignSystem;

public static class AxDashboardWidget
{
    public static Control Create(string title, Control content)
    {
        var stack = new StackPanel
        {
            Spacing = AxSpacing.ElementSpacing
        };

        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = AxTypography.SectionTitle,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        stack.Children.Add(content);
        return AxCard.Create(stack);
    }
}
