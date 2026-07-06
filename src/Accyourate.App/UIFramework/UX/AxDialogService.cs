using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.DesignSystem;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.UX;

public static class AxDialogService
{
    public static async Task<AxDialogResult> ConfirmAsync(
        Window owner,
        string title,
        string message,
        string confirmText = "Conferma",
        string cancelText = "Annulla",
        AxMessageKind kind = AxMessageKind.Warning)
    {
        var dialog = BuildDialog(title, message, confirmText, cancelText, kind);
        var result = await dialog.ShowDialog<AxDialogResult>(owner);
        return result;
    }

    public static async Task ShowMessageAsync(
        Window owner,
        string title,
        string message,
        AxMessageKind kind = AxMessageKind.Info)
    {
        var dialog = BuildDialog(title, message, "OK", string.Empty, kind);
        await dialog.ShowDialog<AxDialogResult>(owner);
    }

    private static Window BuildDialog(string title, string message, string confirmText, string cancelText, AxMessageKind kind)
    {
        var dialog = new Window
        {
            Width = 460,
            Height = 250,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Title = title,
            Background = UiTokens.Brush(UiTokens.Background)
        };

        var root = new StackPanel
        {
            Margin = new Thickness(AxSpacing.PageMargin),
            Spacing = AxSpacing.ElementSpacing
        };

        root.Children.Add(new TextBlock
        {
            Text = Icon(kind),
            FontSize = 34,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
        });

        root.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = AxTypography.SectionTitle,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            TextWrapping = TextWrapping.Wrap
        });

        root.Children.Add(new TextBlock
        {
            Text = message,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap
        });

        var actions = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Spacing = AxSpacing.MicroSpacing
        };

        if (!string.IsNullOrWhiteSpace(cancelText))
        {
            actions.Children.Add(AxButton.Create(cancelText, () => dialog.Close(AxDialogResult.Cancel), AxButtonKind.Secondary));
        }

        actions.Children.Add(AxButton.Create(confirmText, () => dialog.Close(AxDialogResult.Confirm), ButtonKind(kind)));
        root.Children.Add(actions);

        dialog.Content = AxCard.Create(root);
        return dialog;
    }

    private static string Icon(AxMessageKind kind) => kind switch
    {
        AxMessageKind.Success => "✅",
        AxMessageKind.Warning => "⚠️",
        AxMessageKind.Error => "⛔",
        _ => "ℹ️"
    };

    private static AxButtonKind ButtonKind(AxMessageKind kind) => kind switch
    {
        AxMessageKind.Error => AxButtonKind.Danger,
        AxMessageKind.Warning => AxButtonKind.Warning,
        AxMessageKind.Success => AxButtonKind.Success,
        _ => AxButtonKind.Primary
    };
}
