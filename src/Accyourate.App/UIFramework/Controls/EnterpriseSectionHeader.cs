using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.Controls;

public sealed class EnterpriseSectionHeader : StackPanel
{
    private readonly TextBlock _title = new();
    private readonly TextBlock _subtitle = new();

    public EnterpriseSectionHeader()
    {
        Spacing = 6;

        _title.FontSize = 28;
        _title.FontWeight = FontWeight.Bold;
        _title.Foreground = UiTokens.Brush(UiTokens.TextPrimary);

        _subtitle.Foreground = UiTokens.Brush(UiTokens.TextSecondary);
        _subtitle.TextWrapping = TextWrapping.Wrap;

        Children.Add(_title);
        Children.Add(_subtitle);
    }

    public EnterpriseSectionHeader(string title, string subtitle) : this()
    {
        Set(title, subtitle);
    }

    public void Set(string title, string subtitle)
    {
        _title.Text = title;
        _subtitle.Text = subtitle;
    }
}
