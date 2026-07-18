using Avalonia.Controls;

namespace Accyourate.App.UIFramework.EnterpriseTable;

public sealed class AxEnterpriseColumn<T>
{
    public string Id { get; init; } = string.Empty;
    public string Header { get; init; } = string.Empty;
    public double MinWidth { get; init; } = 120;
    public double Width { get; init; } = 160;
    public AxColumnAlignment Alignment { get; init; } = AxColumnAlignment.Left;
    public bool IsSortable { get; init; }
    public Func<T, string>? TextSelector { get; init; }
    public Func<T, Control>? CellFactory { get; init; }

    public Control CreateCell(T item)
    {
        if (CellFactory is not null)
            return CellFactory(item);

        return new TextBlock
        {
            Text = TextSelector?.Invoke(item) ?? string.Empty,
            TextWrapping = Avalonia.Media.TextWrapping.NoWrap,
            TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
    }
}
