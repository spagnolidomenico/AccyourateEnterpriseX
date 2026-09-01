using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.DesignSystem;

public static class AxResponsivePageHeader
{
    public static Control Create(string title, string description, params Control[] actions)
    {
        var root = new StackPanel { Spacing = 12 };
        root.Children.Add(new StackPanel
        {
            Spacing = 3,
            Children =
            {
                new TextBlock
                {
                    Text = title,
                    FontSize = 28,
                    FontWeight = FontWeight.Bold,
                    Foreground = UiTokens.Brush(UiTokens.TextPrimary),
                    TextWrapping = TextWrapping.Wrap
                },
                new TextBlock
                {
                    Text = description,
                    Foreground = UiTokens.Brush(UiTokens.TextSecondary),
                    TextWrapping = TextWrapping.Wrap
                }
            }
        });

        if (actions.Length == 0) return root;
        var commands = new WrapPanel();
        foreach (var action in actions)
        {
            action.Margin = new Thickness(0, 0, 8, 8);
            commands.Children.Add(action);
        }
        root.Children.Add(commands);
        return root;
    }
}
