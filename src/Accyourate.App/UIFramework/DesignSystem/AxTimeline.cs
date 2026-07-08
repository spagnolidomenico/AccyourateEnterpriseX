using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.DesignSystem;

public sealed class AxTimeline : StackPanel
{
    public AxTimeline(string title = "Timeline")
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

    public AxTimeline AddEvent(string title, string description = "", string time = "", string icon = "•")
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("36,*")
        };

        Grid.SetColumn(new TextBlock
        {
            Text = icon,
            FontSize = 20,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        }, 0);

        var text = new StackPanel { Spacing = 3 };
        text.Children.Add(new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(time) ? title : $"{time} · {title}",
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            TextWrapping = TextWrapping.Wrap
        });

        if (!string.IsNullOrWhiteSpace(description))
        {
            text.Children.Add(new TextBlock
            {
                Text = description,
                Foreground = UiTokens.Brush(UiTokens.TextSecondary),
                TextWrapping = TextWrapping.Wrap
            });
        }

        Grid.SetColumn(text, 1);
        grid.Children.Add(text);

        Children.Add(AxCard.Info(grid));
        return this;
    }

    public Control ToCard()
    {
        return AxCard.Create(this);
    }
}
