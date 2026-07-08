using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.DesignSystem;

public sealed class AxInfoPanel : StackPanel
{
    public AxInfoPanel(string title)
    {
        Spacing = AxSpacing.ElementSpacing;

        Children.Add(new TextBlock
        {
            Text = title,
            FontSize = AxTypography.SectionTitle,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
    }

    public AxInfoPanel AddItem(string label, string value, string icon = "")
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            Margin = new Thickness(0, 0, 0, 4)
        };

        var glyph = new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(icon) ? "•" : icon,
            FontSize = 18,
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top
        };

        var texts = new StackPanel { Spacing = 2 };
        texts.Children.Add(new TextBlock
        {
            Text = label,
            FontSize = AxTypography.Label,
            FontWeight = FontWeight.SemiBold,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });
        texts.Children.Add(new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(value) ? "—" : value,
            FontSize = AxTypography.Body,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            TextWrapping = TextWrapping.Wrap
        });

        Grid.SetColumn(glyph, 0);
        Grid.SetColumn(texts, 1);
        grid.Children.Add(glyph);
        grid.Children.Add(texts);

        Children.Add(AxCard.Info(grid));
        return this;
    }

    public Control ToCard()
    {
        return AxCard.Create(this);
    }
}
