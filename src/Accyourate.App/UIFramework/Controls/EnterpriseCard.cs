using Avalonia;
using Avalonia.Controls;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.Controls;

public sealed class EnterpriseCard : Border
{
    public EnterpriseCard()
    {
        Background = UiTokens.Brush(UiTokens.Surface);
        CornerRadius = new CornerRadius(22);
        Padding = new Thickness(18);
    }

    public EnterpriseCard(Control child) : this()
    {
        Child = child;
    }
}
