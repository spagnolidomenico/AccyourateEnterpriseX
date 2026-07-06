using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.DesignSystem;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.UX;

public static class AxLoadingOverlay
{
    public static Border Create(string message = "Operazione in corso...")
    {
        var stack = new StackPanel
        {
            Spacing = AxSpacing.ElementSpacing,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        stack.Children.Add(new ProgressBar
        {
            IsIndeterminate = true,
            Width = 260,
            Height = 8
        });

        stack.Children.Add(new TextBlock
        {
            Text = message,
            FontWeight = FontWeight.SemiBold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        });

        return new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(22),
            Padding = new Thickness(24),
            Child = stack
        };
    }
}
