using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Accyourate.App.UIFramework.EnterpriseTable;

/// <summary>
/// Shared, Avalonia 11-compatible layout rules for grid-based enterprise tables.
/// </summary>
public static class AxTableLayout
{
    public const string EmployeeColumns = "90,115,115,120,140,170,90,96,108,116,100";
    public const string AssetColumns = "85,115,90,115,120,105,96,160,88,116,108,116";
    public const string MedicalDeviceColumns = "90,115,105,120,95,110,96,96,88,96,112,116";

    public static Button ActionButton(string content) => new()
    {
        Content = content,
        MinHeight = 34,
        MinWidth = 0,
        Padding = new Thickness(8, 4),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        HorizontalContentAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        VerticalContentAlignment = VerticalAlignment.Center,
        FontSize = 12,
        FontWeight = FontWeight.SemiBold
    };

    public static TextBlock Header(string text, bool centered) => new()
    {
        Text = text,
        FontWeight = FontWeight.Bold,
        Foreground = Brush.Parse("#B5162B"),
        TextWrapping = TextWrapping.NoWrap,
        TextTrimming = TextTrimming.CharacterEllipsis,
        HorizontalAlignment = centered ? HorizontalAlignment.Center : HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Center
    };

    public static TextBlock CellText(string text, bool centered = false) => new()
    {
        Text = string.IsNullOrWhiteSpace(text) ? "-" : text,
        TextWrapping = TextWrapping.NoWrap,
        TextTrimming = TextTrimming.CharacterEllipsis,
        HorizontalAlignment = centered ? HorizontalAlignment.Center : HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Center
    };
}
