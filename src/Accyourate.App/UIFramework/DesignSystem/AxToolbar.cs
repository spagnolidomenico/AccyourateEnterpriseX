using Avalonia;
using Avalonia.Controls;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.DesignSystem;

public sealed class AxToolbar : Border
{
    private readonly StackPanel _left = new();
    private readonly StackPanel _right = new();

    public AxToolbar()
    {
        Background = UiTokens.Brush(UiTokens.Surface);
        BorderBrush = UiTokens.Brush(UiTokens.Border);
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(18);
        Padding = new Thickness(12);
        Margin = new Thickness(0, 0, 0, 12);

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto")
        };

        _left.Orientation = Avalonia.Layout.Orientation.Horizontal;
        _left.Spacing = AxSpacing.MicroSpacing;
        _right.Orientation = Avalonia.Layout.Orientation.Horizontal;
        _right.Spacing = AxSpacing.MicroSpacing;

        Grid.SetColumn(_left, 0);
        Grid.SetColumn(_right, 1);
        grid.Children.Add(_left);
        grid.Children.Add(_right);

        Child = grid;
    }

    public AxToolbar AddLeft(Control control)
    {
        _left.Children.Add(control);
        return this;
    }

    public AxToolbar AddRight(Control control)
    {
        _right.Children.Add(control);
        return this;
    }

    public AxToolbar AddSeparator()
    {
        _left.Children.Add(new Border
        {
            Width = 1,
            Margin = new Thickness(8, 4),
            Background = UiTokens.Brush(UiTokens.Border)
        });
        return this;
    }
}
