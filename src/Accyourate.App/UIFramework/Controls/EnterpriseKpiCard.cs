using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.Controls;

public sealed class EnterpriseKpiCard : Border
{
    private readonly TextBlock _valueText = new();
    private readonly TextBlock _labelText = new();
    private readonly TextBlock _iconText = new();

    public EnterpriseKpiCard()
    {
        Background = UiTokens.Brush(UiTokens.Surface);
        CornerRadius = new CornerRadius(18);
        Padding = new Thickness(16, 12);
        MinWidth = 150;

        var stack = new StackPanel { Spacing = 2 };

        _valueText.FontSize = 22;
        _valueText.FontWeight = FontWeight.Bold;
        _valueText.Foreground = UiTokens.Brush(UiTokens.TextPrimary);

        _labelText.FontSize = 12;
        _labelText.Foreground = UiTokens.Brush(UiTokens.TextSecondary);

        stack.Children.Add(_valueText);
        stack.Children.Add(_labelText);

        Child = stack;
        Set("•", "0", "KPI");
    }

    public EnterpriseKpiCard(string icon, string value, string label) : this()
    {
        Set(icon, value, label);
    }

    public void Set(string icon, string value, string label)
    {
        _iconText.Text = icon;
        _valueText.Text = $"{icon} {value}";
        _labelText.Text = label;
    }
}
