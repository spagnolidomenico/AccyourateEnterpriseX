using Avalonia;
using Avalonia.Controls;

namespace Accyourate.App.UIFramework.DesignSystem;

public static class AxResponsiveFilterBar
{
    public static Control Create(params Control[] filters)
    {
        var panel = new WrapPanel();
        foreach (var filter in filters)
        {
            filter.Margin = new Thickness(0, 0, 10, 8);
            panel.Children.Add(filter);
        }
        return panel;
    }
}
