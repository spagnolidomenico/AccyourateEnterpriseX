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
    private readonly ScrollViewer _horizontalScroll;
    private IReadOnlyList<AxEnterpriseColumn<T>> _columns = Array.Empty<AxEnterpriseColumn<T>>();
    private IReadOnlyList<T> _items = Array.Empty<T>();
    private T? _selectedItem;

    public AxEnterpriseTable()
    {
        Background = UiTokens.Brush(UiTokens.Surface);
        BorderBrush = UiTokens.Brush(UiTokens.Border);
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(16);

        var content = new StackPanel();

        var headerBorder = new Border
        {
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(12, 10),
            Child = _header
        };

        content.Children.Add(headerBorder);
        content.Children.Add(_rows);

        _horizontalScroll = new ScrollViewer
        {
            Content = content,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
        };

        Child = _horizontalScroll;
    }

    public event Action<T>? SelectionChanged;

    public T? SelectedItem => _selectedItem;

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
            var text = new TextBlock
            {
                Text = column.Header,
                FontWeight = FontWeight.Bold,
                Foreground = UiTokens.Brush(UiTokens.TextSecondary),
                TextWrapping = TextWrapping.NoWrap,
                TextTrimming = TextTrimming.None,
                HorizontalAlignment = Alignment(column.Alignment),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            Grid.SetColumn(text, i);
            _header.Children.Add(text);
        }
    }

    private void BuildRows()
    {
        _rows.Children.Clear();
        _rows.Spacing = 4;

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

        foreach (var item in _items)
            _rows.Children.Add(BuildRow(item));
    }

    private Control BuildRow(T item)
    {
        var grid = new Grid
        {
            ColumnDefinitions = BuildDefinitions(),
            MinHeight = 52
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

        var button = new Button
        {
            Content = grid,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(12, 8),
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            MinHeight = 52
        };

        button.Click += (_, _) =>
        {
            _selectedItem = item;
            SelectionChanged?.Invoke(item);
            RefreshSelection();
        };

        button.Tag = item;
        return button;
    }

    private void RefreshSelection()
    {
        foreach (var child in _rows.Children.OfType<Button>())
        {
            var selected = Equals(child.Tag, _selectedItem);
            child.Background = UiTokens.Brush(selected ? UiTokens.PremiumSelected : UiTokens.Surface);
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
}
