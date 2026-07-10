using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.DesignSystem;

public static class AxKpiCard
{
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
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto,*"),
            MinHeight = 146,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };

        var iconBlock = new TextBlock
        {
            Text = icon,
            FontSize = 26,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 2)
        };
        Grid.SetRow(iconBlock, 0);
        layout.Children.Add(iconBlock);

        var valueBlock = new TextBlock
        {
            Text = value,
            FontSize = 36,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.NoWrap
        };
        Grid.SetRow(valueBlock, 1);
        layout.Children.Add(valueBlock);

        var titleBlock = new TextBlock
        {
            Text = title,
            FontSize = 15,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
        };
        Grid.SetRow(titleBlock, 2);
        layout.Children.Add(titleBlock);

        var subtitleBlock = new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(subtitle) ? " " : subtitle,
            FontSize = 12,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 4, 0, 2)
        };
        Grid.SetRow(subtitleBlock, 3);
        layout.Children.Add(subtitleBlock);

        var card = AxCard.Create(layout, 18);
        card.Width = 220;
        card.MinHeight = 184;
        card.Margin = new Thickness(0, 0, 12, 12);
        card.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;

        if (onClick is null)
            return card;

        var button = new Button
        {
            Content = card,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };

        button.Click += (_, _) => onClick();
        return button;
    }
}
