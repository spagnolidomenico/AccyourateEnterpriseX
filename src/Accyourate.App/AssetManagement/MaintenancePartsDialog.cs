using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Models;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class MaintenancePartsDialog : Window
{
    private readonly MaintenanceTicket _ticket;
    private readonly MaintenancePartsRepository _repository = new();
    private readonly TextBox _code = new();
    private readonly TextBox _description = new();
    private readonly TextBox _supplier = new();
    private readonly TextBox _quantity = new() { Text = "1" };
    private readonly TextBox _unitCost = new() { Text = "0" };
    private readonly TextBox _notes = new();
    private StackPanel _rows = new();
    private readonly TextBlock _total = new();
    private readonly TextBlock _message = new();

    public MaintenancePartsDialog(MaintenanceTicket ticket)
    {
        _ticket = ticket;
        Title = "Ricambi manutenzione";
        Width = 820;
        Height = 650;
        MinWidth = 720;
        MinHeight = 560;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();
        Load();
    }

    private Control BuildLayout()
    {
        var root = new DockPanel();
        var header = new StackPanel { Margin = new Thickness(24, 20, 24, 12), Spacing = 4 };
        header.Children.Add(new TextBlock
        {
            Text = "Ricambi e materiali",
            FontSize = 26,
            FontWeight = FontWeight.Bold
        });
        header.Children.Add(new TextBlock
        {
            Text = _ticket.Title,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });
        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        var form = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("110,*,150,90,110"),
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto"),
            Margin = new Thickness(24, 0, 24, 12)
        };
        AddField(form, "Codice", _code, 0);
        AddField(form, "Descrizione", _description, 1);
        AddField(form, "Fornitore", _supplier, 2);
        AddField(form, "Quantità", _quantity, 3);
        AddField(form, "Costo unitario", _unitCost, 4);
        var knownSuppliers = _repository.GetSupplierNames();
        _supplier.Watermark = knownSuppliers.Count == 0
            ? "Nuovo fornitore"
            : $"Es. {string.Join(", ", knownSuppliers.Take(2))}";
        _notes.Watermark = "Note sul componente sostituito";
        Grid.SetRow(_notes, 1);
        Grid.SetColumn(_notes, 0);
        Grid.SetColumnSpan(_notes, 4);
        _notes.Margin = new Thickness(4, 8, 4, 0);
        form.Children.Add(_notes);
        var add = Button("Aggiungi", AddPart, true);
        Grid.SetRow(add, 1);
        Grid.SetColumn(add, 4);
        form.Children.Add(add);
        Grid.SetRow(_message, 2);
        Grid.SetColumnSpan(_message, 5);
        _message.Margin = new Thickness(4, 6, 4, 0);
        form.Children.Add(_message);
        DockPanel.SetDock(form, Dock.Top);
        root.Children.Add(form);

        var footer = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,120"),
            Margin = new Thickness(24, 8, 24, 20)
        };
        _total.FontSize = 17;
        _total.FontWeight = FontWeight.Bold;
        _total.VerticalAlignment = VerticalAlignment.Center;
        footer.Children.Add(_total);
        var close = Button("Chiudi", () => Close(true), true);
        Grid.SetColumn(close, 1);
        footer.Children.Add(close);
        DockPanel.SetDock(footer, Dock.Bottom);
        root.Children.Add(footer);

        root.Children.Add(new ScrollViewer
        {
            Content = _rows,
            Margin = new Thickness(24, 0, 24, 0),
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        });
        return root;
    }

    private void Load()
    {
        var parts = _repository.GetByTicket(_ticket.Id);
        _rows.Children.Clear();
        _rows.Children.Add(HeaderRow());
        for (var index = 0; index < parts.Count; index++)
            _rows.Children.Add(PartRow(parts[index], index));
        if (parts.Count == 0)
            _rows.Children.Add(new TextBlock
            {
                Text = "Nessun ricambio registrato.",
                Foreground = UiTokens.Brush(UiTokens.TextSecondary),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(12, 24)
            });
        _total.Text = $"Totale ricambi: EUR {parts.Sum(part => part.TotalCost):N2}";
    }

    private void AddPart()
    {
        if (string.IsNullOrWhiteSpace(_description.Text))
        {
            Error("Inserisci la descrizione del ricambio.");
            return;
        }
        if (!decimal.TryParse(_quantity.Text, out var quantity) || quantity <= 0)
        {
            Error("Inserisci una quantità valida.");
            return;
        }
        if (!decimal.TryParse(_unitCost.Text, out var unitCost) || unitCost < 0)
        {
            Error("Inserisci un costo valido.");
            return;
        }
        _repository.Add(new MaintenancePart
        {
            MaintenanceTicketId = _ticket.Id,
            PartCode = _code.Text?.Trim() ?? string.Empty,
            Description = _description.Text.Trim(),
            Supplier = _supplier.Text?.Trim() ?? string.Empty,
            Quantity = quantity,
            UnitCost = unitCost,
            Notes = _notes.Text?.Trim() ?? string.Empty
        });
        _code.Text = string.Empty;
        _description.Text = string.Empty;
        _quantity.Text = "1";
        _unitCost.Text = "0";
        _notes.Text = string.Empty;
        _message.Text = "Ricambio aggiunto.";
        _message.Foreground = UiTokens.Brush(UiTokens.Success);
        Load();
    }

    private Control HeaderRow()
    {
        var grid = RowGrid();
        AddText(grid, "Codice", 0, true);
        AddText(grid, "Descrizione", 1, true);
        AddText(grid, "Fornitore", 2, true);
        AddText(grid, "Quantità", 3, true);
        AddText(grid, "Costo", 4, true);
        AddText(grid, "Totale", 5, true);
        return new Border
        {
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            Padding = new Thickness(8),
            Child = grid
        };
    }

    private Control PartRow(MaintenancePart part, int index)
    {
        var grid = RowGrid();
        AddText(grid, part.PartCode, 0);
        AddText(grid, part.Description, 1, true);
        AddText(grid, part.Supplier, 2);
        AddText(grid, part.Quantity.ToString("N2"), 3);
        AddText(grid, $"EUR {part.UnitCost:N2}", 4);
        AddText(grid, $"EUR {part.TotalCost:N2}", 5, true);
        var delete = Button("Rimuovi", () =>
        {
            _repository.Delete(part.Id);
            Load();
        });
        Grid.SetColumn(delete, 6);
        grid.Children.Add(delete);
        return new Border
        {
            Background = UiTokens.Brush(index % 2 == 0 ? UiTokens.Surface : UiTokens.SurfaceAlt),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(8, 5),
            Child = grid
        };
    }

    private static Grid RowGrid() => new()
    {
        ColumnDefinitions = new ColumnDefinitions("90,*,140,80,100,105,90")
    };

    private static void AddField(Grid grid, string label, Control control, int column)
    {
        var panel = new StackPanel { Spacing = 4, Margin = new Thickness(4) };
        panel.Children.Add(new TextBlock { Text = label, FontWeight = FontWeight.SemiBold, FontSize = 11 });
        panel.Children.Add(control);
        Grid.SetColumn(panel, column);
        grid.Children.Add(panel);
    }

    private static void AddText(Grid grid, string text, int column, bool strong = false)
    {
        var label = new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(text) ? "—" : text,
            FontWeight = strong ? FontWeight.SemiBold : FontWeight.Normal,
            Foreground = UiTokens.Brush(strong ? UiTokens.TextPrimary : UiTokens.TextSecondary),
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(4)
        };
        Grid.SetColumn(label, column);
        grid.Children.Add(label);
    }

    private static Button Button(string text, Action action, bool primary = false)
    {
        var button = new Button
        {
            Content = text,
            MinHeight = 34,
            Margin = new Thickness(4),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Background = UiTokens.Brush(primary ? UiTokens.BrandBlue : UiTokens.SurfaceAlt),
            Foreground = primary ? Brushes.White : UiTokens.Brush(UiTokens.TextPrimary)
        };
        button.Click += (_, _) => action();
        return button;
    }

    private void Error(string text)
    {
        _message.Text = text;
        _message.Foreground = UiTokens.Brush(UiTokens.Danger);
    }
}
