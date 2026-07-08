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
        var stack = new StackPanel
        {
            Spacing = 6,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };

        stack.Children.Add(new TextBlock
        {
            Text = icon,
            FontSize = 28,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        });

        stack.Children.Add(new TextBlock
        {
            Text = value,
            FontSize = AxTypography.Kpi,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center
        });

        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = AxTypography.Body,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        });

        if (!string.IsNullOrWhiteSpace(subtitle))
        {
            stack.Children.Add(new TextBlock
            {
                Text = subtitle,
                FontSize = AxTypography.Label,
                Foreground = UiTokens.Brush(UiTokens.TextSecondary),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });
        }

        var card = AxCard.Create(stack);
        card.Width = 220;
        card.MinHeight = 138;
        card.Margin = new Thickness(0, 0, 12, 12);

        if (onClick is null)
            return card;

        var button = new Button
        {
            Content = card,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };

        button.Click += (_, _) => onClick();
        return button;
    }
}
