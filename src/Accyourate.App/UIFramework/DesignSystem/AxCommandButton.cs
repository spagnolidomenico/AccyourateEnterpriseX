using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.DesignSystem;

/// <summary>
/// Pulsante compatto canonico per toolbar e command bar di Accyourate Enterprise X.
/// Centralizza dimensioni, spaziature, tipografia e stati visivi.
/// </summary>
public static class AxCommandButton
{
    public static Button Create(
        string icon,
        string text,
        Action action,
        bool iconOnly = false,
        bool selected = false,
        string? toolTip = null)
    {
        var label = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = iconOnly ? 0 : 6,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        label.Children.Add(new TextBlock
        {
            Text = icon,
            FontSize = 15,
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        });

        if (!iconOnly && !string.IsNullOrWhiteSpace(text))
        {
            label.Children.Add(new TextBlock
            {
                Text = text,
                FontSize = 12,
                FontWeight = FontWeight.SemiBold,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            });
        }

        var button = new Button
        {
            Content = label,
            Background = UiTokens.Brush(selected ? UiTokens.BrandBlue : UiTokens.SurfaceAlt),
            Foreground = selected ? Brushes.White : UiTokens.Brush(UiTokens.TextPrimary),
            BorderBrush = UiTokens.Brush(selected ? UiTokens.BrandBlue : UiTokens.Border),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = iconOnly ? new Thickness(10, 7) : new Thickness(11, 7),
            MinHeight = 34,
            MinWidth = iconOnly ? 38 : 0,
            Cursor = new Cursor(StandardCursorType.Hand),
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        if (!string.IsNullOrWhiteSpace(toolTip))
            ToolTip.SetTip(button, toolTip);

        button.Click += (_, _) => action();
        return button;
    }

    public static void SetSelected(Button? button, bool selected)
    {
        if (button is null)
            return;

        button.Background = UiTokens.Brush(selected ? UiTokens.BrandBlue : UiTokens.SurfaceAlt);
        button.Foreground = selected ? Brushes.White : UiTokens.Brush(UiTokens.TextPrimary);
        button.BorderBrush = UiTokens.Brush(selected ? UiTokens.BrandBlue : UiTokens.Border);
    }
}
