using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.DesignSystem;

public static class AxKpiCard
{
    public const double DefaultWidth = 236;
    public const double DefaultHeight = 186;

    public static Control Create(
        string icon,
        string title,
        string value,
        string subtitle = "",
        Action? onClick = null,
        AxButtonKind kind = AxButtonKind.Secondary)
    {
        var layout = new Grid
        {
            RowDefinitions = new RowDefinitions("38,54,34,*"),
            MinWidth = DefaultWidth,
            Width = DefaultWidth,
            MinHeight = DefaultHeight,
            Height = DefaultHeight
        };

        var iconText = new TextBlock
        {
            Text = icon,
            FontSize = 26,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };
        Grid.SetRow(iconText, 0);
        layout.Children.Add(iconText);

        var valueText = new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(value) ? "0" : value,
            FontSize = 40,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.NoWrap
        };
        Grid.SetRow(valueText, 1);
        layout.Children.Add(valueText);

        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 196
        };
        Grid.SetRow(titleText, 2);
        layout.Children.Add(titleText);

        var subtitleText = new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(subtitle) ? " " : subtitle,
            FontSize = 13,
            FontWeight = FontWeight.SemiBold,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 196,
            Margin = new Thickness(0, 6, 0, 2)
        };
        Grid.SetRow(subtitleText, 3);
        layout.Children.Add(subtitleText);

        var card = new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(22),
            Padding = new Thickness(20, 14, 20, 16),
            Margin = new Thickness(0, 0, 12, 12),
            MinWidth = DefaultWidth,
            Width = DefaultWidth,
            MinHeight = DefaultHeight,
            Height = DefaultHeight,
            ClipToBounds = true,
            Child = layout
        };

        if (onClick is null)
            return card;

        var button = new Button
        {
            Content = card,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            MinWidth = DefaultWidth,
            Width = DefaultWidth,
            MinHeight = DefaultHeight,
            Height = DefaultHeight,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };

        button.Click += (_, _) => onClick();
        return button;
    }
}
