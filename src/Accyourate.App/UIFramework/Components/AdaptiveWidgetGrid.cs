using Avalonia.Controls;

namespace Accyourate.App.UIFramework.Components;

public static class AdaptiveWidgetGrid
{
    public static WrapPanel Create()
    {
        return new WrapPanel
        {
            ItemWidth = 350,
            ItemHeight = 250
        };
    }
}
