using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Shared.Theme;

namespace Accyourate.App.Shared.UI;

public static class UiFactory
{
    public static TextBlock PageTitle(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = AppTheme.PrimaryBrush
        };
    }

    public static TextBlock SectionTitle(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = AppTheme.PrimaryBrush
        };
    }

    public static Border Card(Control content)
    {
        return new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(18),
            Child = content
        };
    }

    public static Button PrimaryButton(string text)
    {
        return new Button
        {
            Content = text,
            Background = AppTheme.PrimaryBrush,
            Foreground = Brushes.White,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(12, 8)
        };
    }

    public static Button SecondaryButton(string text)
    {
        return new Button
        {
            Content = text,
            Padding = new Thickness(12, 8)
        };
    }
}
