using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.DesignSystem;

public static class AxPageHeader
{
    public static Control Create(string title, string description, params Button[] actions)
    {
        var grid = new Grid
        {
            Margin = new Thickness(AxSpacing.PageMargin, 20, AxSpacing.PageMargin, 16),
            ColumnDefinitions = new ColumnDefinitions("*,Auto")
        };

        var titleStack = new StackPanel { Spacing = 6 };
        titleStack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = AxTypography.PageTitle,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        titleStack.Children.Add(new TextBlock
        {
            Text = description,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap
        });

        Grid.SetColumn(titleStack, 0);
        grid.Children.Add(titleStack);

        var actionStack = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = AxSpacing.MicroSpacing,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        foreach (var action in actions)
            actionStack.Children.Add(action);

        Grid.SetColumn(actionStack, 1);
        grid.Children.Add(actionStack);
        return grid;
    }
}
