using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.EnterpriseTable;

public sealed class AxEnterpriseTable<T> : Border
{
    private readonly Grid _header = new();
    private readonly StackPanel _rows = new();
    private readonly ScrollViewer _rowsScroll;
    private readonly ScrollViewer _horizontalScroll;
    private IReadOnlyList<AxEnterpriseColumn<T>> _columns = Array.Empty<AxEnterpriseColumn<T>>();
    private IReadOnlyList<T> _items = Array.Empty<T>();
    private T? _selectedItem;
    private string? _sortColumnId;
    private bool _sortAscending = true;

    public AxEnterpriseTable()
    {
        Background = UiTokens.Brush(UiTokens.Surface);
        BorderBrush = UiTokens.Brush(UiTokens.Border);
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(14);
        MinHeight = 280;

        var headerBorder = new Border
        {
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(14, 10),
            Child = _header
        };

        _rowsScroll = new ScrollViewer
        {
            Content = _rows,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };

        var content = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
            MinHeight = 0
        };
        Grid.SetRow(headerBorder, 0);
        content.Children.Add(headerBorder);
        Grid.SetRow(_rowsScroll, 1);
        content.Children.Add(_rowsScroll);

        _horizontalScroll = new ScrollViewer
        {
            Content = content,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
        };

        Child = _horizontalScroll;
    }

    public event Action<T>? SelectionChanged;
    public event Action<T>? ItemActivated;
    public event Action<string, bool>? SortRequested;

    public T? SelectedItem => _selectedItem;
    public bool CompactRows { get; set; } = true;
    public bool AlternatingRows { get; set; } = true;

    public void SetSelectedItem(T? item)
    {
        _selectedItem = item;
        RefreshSelection();
    }

    public void ConfigureColumns(IReadOnlyList<AxEnterpriseColumn<T>> columns)
    {
        _columns = columns ?? Array.Empty<AxEnterpriseColumn<T>>();
        Rebuild();
    }

    public void SetItems(IReadOnlyList<T> items)
    {
        _items = items ?? Array.Empty<T>();
        Rebuild();
    }

    private void Rebuild()
    {
        BuildHeader();
        BuildRows();
    }

    private void BuildHeader()
    {
        _header.Children.Clear();
        _header.ColumnDefinitions = BuildDefinitions();

        for (var i = 0; i < _columns.Count; i++)
        {
            var column = _columns[i];
            Control headerControl;

            if (column.IsSortable)
            {
                var sortIndicator = string.Equals(_sortColumnId, column.Id, StringComparison.Ordinal)
                    ? (_sortAscending ? "  ▲" : "  ▼")
                    : string.Empty;

                var button = new Button
                {
                    Content = column.Header + sortIndicator,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(0),
                    FontWeight = FontWeight.Bold,
                    Foreground = UiTokens.Brush(UiTokens.TextSecondary),
                    HorizontalContentAlignment = Alignment(column.Alignment),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };

                button.Click += (_, _) =>
                {
                    if (string.Equals(_sortColumnId, column.Id, StringComparison.Ordinal))
                        _sortAscending = !_sortAscending;
                    else
                    {
                        _sortColumnId = column.Id;
                        _sortAscending = true;
                    }

                    BuildHeader();
                    SortRequested?.Invoke(column.Id, _sortAscending);
                };

                headerControl = button;
            }
            else
            {
                headerControl = new TextBlock
                {
                    Text = column.Header,
                    FontWeight = FontWeight.Bold,
                    Foreground = UiTokens.Brush(UiTokens.TextSecondary),
                    TextWrapping = TextWrapping.NoWrap,
                    TextTrimming = TextTrimming.None,
                    HorizontalAlignment = Alignment(column.Alignment),
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };
            }

            Grid.SetColumn(headerControl, i);
            _header.Children.Add(headerControl);
        }
    }

    private void BuildRows()
    {
        _rows.Children.Clear();
        _rows.Spacing = 0;

        if (_items.Count == 0)
        {
            _rows.Children.Add(new Border
            {
                Padding = new Thickness(20),
                Child = new TextBlock
                {
                    Text = "Nessun dato disponibile.",
                    Foreground = UiTokens.Brush(UiTokens.TextSecondary),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                }
            });
            return;
        }

        for (var index = 0; index < _items.Count; index++)
            _rows.Children.Add(BuildRow(_items[index], index));

        RefreshSelection();
    }

    private Control BuildRow(T item, int index)
    {
        var rowHeight = CompactRows ? 46 : 54;
        var grid = new Grid
        {
            ColumnDefinitions = BuildDefinitions(),
            MinHeight = rowHeight
        };

        for (var i = 0; i < _columns.Count; i++)
        {
            var column = _columns[i];
            var cell = column.CreateCell(item);
            cell.HorizontalAlignment = Alignment(column.Alignment);
            cell.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;

            Grid.SetColumn(cell, i);
            grid.Children.Add(cell);
        }

        var normalBackground = AlternatingRows && index % 2 == 1
            ? UiTokens.Brush(UiTokens.SurfaceAlt)
            : UiTokens.Brush(UiTokens.Surface);

        var button = new Button
        {
            Content = grid,
            Background = normalBackground,
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = CompactRows ? new Thickness(14, 7) : new Thickness(14, 10),
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            MinHeight = rowHeight
        };

        button.Click += (_, _) =>
        {
            _selectedItem = item;
            SelectionChanged?.Invoke(item);
            RefreshSelection();
        };
        button.DoubleTapped += (_, _) => ItemActivated?.Invoke(item);

        button.Tag = new RowTag(item, normalBackground);
        return button;
    }

    private void RefreshSelection()
    {
        foreach (var child in _rows.Children.OfType<Button>())
        {
            if (child.Tag is not RowTag tag)
                continue;

            var selected = Equals(tag.Item, _selectedItem);
            child.Background = selected
                ? UiTokens.Brush(UiTokens.PremiumSelected)
                : tag.NormalBackground;
        }
    }

    private ColumnDefinitions BuildDefinitions()
    {
        var definitions = new ColumnDefinitions();

        foreach (var column in _columns)
        {
            definitions.Add(new ColumnDefinition
            {
                Width = new GridLength(Math.Max(column.Width, column.MinWidth)),
                MinWidth = column.MinWidth
            });
        }

        return definitions;
    }

    private static Avalonia.Layout.HorizontalAlignment Alignment(AxColumnAlignment alignment)
    {
        return alignment switch
        {
            AxColumnAlignment.Center => Avalonia.Layout.HorizontalAlignment.Center,
            AxColumnAlignment.Right => Avalonia.Layout.HorizontalAlignment.Right,
            _ => Avalonia.Layout.HorizontalAlignment.Left
        };
    }

    private sealed record RowTag(T Item, IBrush NormalBackground);
}
