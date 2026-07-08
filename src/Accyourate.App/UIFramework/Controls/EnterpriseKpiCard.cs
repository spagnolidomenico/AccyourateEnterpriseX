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
        BorderBrush = UiTokens.Brush(UiTokens.Border);
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(20);
        Padding = new Thickness(16, 14);
        Width = 168;
        MinHeight = 126;

        var stack = new StackPanel
        {
            Spacing = 6,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        _iconText.FontSize = 24;
        _iconText.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;

        _valueText.FontSize = 28;
        _valueText.FontWeight = FontWeight.Bold;
        _valueText.Foreground = UiTokens.Brush(UiTokens.TextPrimary);
        _valueText.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
        _valueText.TextAlignment = TextAlignment.Center;

        _labelText.FontSize = 12;
        _labelText.FontWeight = FontWeight.SemiBold;
        _labelText.Foreground = UiTokens.Brush(UiTokens.TextSecondary);
        _labelText.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
        _labelText.TextAlignment = TextAlignment.Center;
        _labelText.TextWrapping = TextWrapping.Wrap;
        _labelText.MaxWidth = 136;

        stack.Children.Add(_iconText);
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
        _valueText.Text = value;
        _labelText.Text = label;
    }
}
