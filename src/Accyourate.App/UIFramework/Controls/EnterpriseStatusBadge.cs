using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.Controls;

public sealed class EnterpriseStatusBadge : Border
{
    private readonly TextBlock _text = new();

    public EnterpriseStatusBadge()
    {
        Background = UiTokens.Brush(UiTokens.SurfaceAlt);
        CornerRadius = new CornerRadius(12);
        Padding = new Thickness(10, 5);

        _text.FontWeight = FontWeight.SemiBold;
        _text.Foreground = UiTokens.Brush(UiTokens.BrandBlue);
        _text.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;

        Child = _text;
    }

    public EnterpriseStatusBadge(string status) : this()
    {
        Status = status;
    }

    public string Status
    {
        get => _text.Text ?? string.Empty;
        set => _text.Text = string.IsNullOrWhiteSpace(value) ? "—" : value;
    }
}
