using Avalonia;
using Avalonia.Controls;

namespace Accyourate.App.UIFramework.Controls;

public sealed class EnterpriseSearchBar : TextBox
{
    public EnterpriseSearchBar()
    {
        Watermark = "Cerca...";
        Height = 42;
    }

    public EnterpriseSearchBar(string watermark) : this()
    {
        Watermark = watermark;
    }

    public string Query => Text?.Trim() ?? string.Empty;
}
