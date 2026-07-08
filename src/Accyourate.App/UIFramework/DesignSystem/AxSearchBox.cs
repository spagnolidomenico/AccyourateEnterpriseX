using Avalonia;
using Avalonia.Controls;

namespace Accyourate.App.UIFramework.DesignSystem;

public static class AxSearchBox
{
    public static TextBox Create(string watermark = "Cerca...", Action<string>? onChanged = null)
    {
        var box = new TextBox
        {
            Watermark = watermark,
            FontSize = 15,
            Padding = new Thickness(14, 10),
            MinWidth = 280
        };

        if (onChanged is not null)
        {
            box.TextChanged += (_, _) => onChanged(box.Text ?? string.Empty);
        }

        return box;
    }
}
