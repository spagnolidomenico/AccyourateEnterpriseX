using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.Controls;

public sealed class EnterpriseKpiCard : Border
{
    private readonly TextBlock _valueText = new();
    private readonly TextBlock _labelText = new();
    private readonly TextBlock _subtitleText = new();
    private readonly TextBlock _iconText = new();

    public EnterpriseKpiCard()
    {
        Background = UiTokens.Brush(UiTokens.Surface);
        BorderBrush = UiTokens.Brush(UiTokens.Border);
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(16);
        Padding = new Thickness(16, 14);
        MinWidth = 210;
        MinHeight = 92;

        var iconHost = new Border
        {
            Width = 46,
            Height = 46,
            CornerRadius = new CornerRadius(14),
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Child = _iconText
        };

        _iconText.FontSize = 22;
        _iconText.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
        _iconText.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;

        _valueText.FontSize = 28;
        _valueText.FontWeight = FontWeight.Bold;
        _valueText.Foreground = UiTokens.Brush(UiTokens.TextPrimary);

        _labelText.FontSize = 13;
        _labelText.FontWeight = FontWeight.SemiBold;
        _labelText.Foreground = UiTokens.Brush(UiTokens.TextPrimary);

        _subtitleText.FontSize = 11;
        _subtitleText.Foreground = UiTokens.Brush(UiTokens.TextSecondary);
        _subtitleText.TextWrapping = TextWrapping.Wrap;

        var text = new StackPanel
        {
            Spacing = 1,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        text.Children.Add(_valueText);
        text.Children.Add(_labelText);
        text.Children.Add(_subtitleText);

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("46,14,*")
        };
        Grid.SetColumn(iconHost, 0);
        grid.Children.Add(iconHost);
        Grid.SetColumn(text, 2);
        grid.Children.Add(text);

        Child = grid;
        Set("•", "0", "KPI", string.Empty);
    }

    public EnterpriseKpiCard(string icon, string value, string label, string subtitle = "") : this()
    {
        Set(icon, value, label, subtitle);
    }

    public void Set(string icon, string value, string label, string subtitle = "")
    {
        _iconText.Text = icon;
        _valueText.Text = value;
        _labelText.Text = label;
        _subtitleText.Text = subtitle;
        _subtitleText.IsVisible = !string.IsNullOrWhiteSpace(subtitle);
    }
}
