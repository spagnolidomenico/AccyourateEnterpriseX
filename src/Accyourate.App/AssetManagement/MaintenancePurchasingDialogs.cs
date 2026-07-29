using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Models;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class MaintenanceSupplierDialog : Window
{
    private readonly TextBox _name = new();
    private readonly TextBox _vat = new();
    private readonly TextBox _contact = new();
    private readonly TextBox _email = new();
    private readonly TextBox _phone = new();
    private readonly TextBox _address = new();
    private readonly TextBox _city = new();
    private readonly TextBox _notes = new() { AcceptsReturn = true, MinHeight = 65 };
    private readonly TextBlock _message = new();

    public MaintenanceSupplierDialog(MaintenanceSupplier? supplier = null)
    {
        Title = supplier is null ? "Nuovo fornitore" : "Modifica fornitore";
        Width = 570;
        Height = 650;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = UiTokens.Brush(UiTokens.Background);
        if (supplier is not null)
        {
            _name.Text = supplier.Name; _vat.Text = supplier.VatNumber;
            _contact.Text = supplier.ContactPerson; _email.Text = supplier.Email;
            _phone.Text = supplier.Phone; _address.Text = supplier.Address;
            _city.Text = supplier.City; _notes.Text = supplier.Notes;
        }
        var root = new StackPanel { Margin = new Thickness(24), Spacing = 10 };
        root.Children.Add(new TextBlock { Text = Title, FontSize = 26, FontWeight = FontWeight.Bold });
        root.Children.Add(Field("Ragione sociale", _name));
        root.Children.Add(Field("Partita IVA", _vat));
        root.Children.Add(Field("Referente", _contact));
        root.Children.Add(Field("Email", _email));
        root.Children.Add(Field("Telefono", _phone));
        root.Children.Add(Field("Indirizzo", _address));
        root.Children.Add(Field("Città", _city));
        root.Children.Add(Field("Note", _notes));
        root.Children.Add(_message);
        var actions = new Grid { ColumnDefinitions = new ColumnDefinitions("*,120,120") };
        var cancel = Button("Annulla", false); cancel.Click += (_, _) => Close(null);
        var save = Button("Salva", true); save.Click += (_, _) => Confirm(supplier?.Id ?? 0);
        Add(actions, cancel, 1); Add(actions, save, 2); root.Children.Add(actions);
        Content = new ScrollViewer { Content = root };
    }

    private void Confirm(int id)
    {
        if (string.IsNullOrWhiteSpace(_name.Text))
        {
            _message.Text = "Inserisci la ragione sociale.";
            _message.Foreground = UiTokens.Brush(UiTokens.Danger);
            return;
        }
        Close(new MaintenanceSupplier
        {
            Id = id, Name = _name.Text.Trim(), VatNumber = _vat.Text?.Trim() ?? "",
            ContactPerson = _contact.Text?.Trim() ?? "", Email = _email.Text?.Trim() ?? "",
            Phone = _phone.Text?.Trim() ?? "", Address = _address.Text?.Trim() ?? "",
            City = _city.Text?.Trim() ?? "", Notes = _notes.Text?.Trim() ?? ""
        });
    }

    private static Control Field(string label, Control control) => new StackPanel
    {
        Spacing = 4, Children = { new TextBlock { Text = label, FontWeight = FontWeight.SemiBold }, control }
    };
    private static Button Button(string text, bool primary) => new()
    {
        Content = text, Height = 38, Margin = new Thickness(5),
        Background = UiTokens.Brush(primary ? UiTokens.BrandBlue : UiTokens.SurfaceAlt),
        Foreground = primary ? Brushes.White : UiTokens.Brush(UiTokens.TextPrimary)
    };
    private static void Add(Grid grid, Control control, int column)
    { Grid.SetColumn(control, column); grid.Children.Add(control); }
}

public sealed class MaintenancePurchaseOrderDialog : Window
{
    private readonly ComboBox _supplier = new();
    private readonly ComboBox _ticket = new();
    private readonly TextBox _expected = new() { Watermark = "gg/mm/aaaa" };
    private readonly TextBox _notes = new();
    private readonly TextBox _code = new();
    private readonly TextBox _description = new();
    private readonly TextBox _quantity = new() { Text = "1" };
    private readonly TextBox _cost = new() { Text = "0" };
    private readonly StackPanel _linesPanel = new();
    private readonly TextBlock _total = new();
    private readonly TextBlock _message = new();
    private readonly List<MaintenancePurchaseOrderLine> _lines = new();

    public MaintenancePurchaseOrderDialog(
        IReadOnlyList<MaintenanceSupplier> suppliers,
        IReadOnlyList<MaintenanceTicket> tickets)
    {
        Title = "Nuovo ordine ricambi";
        Width = 760;
        Height = 720;
        MinWidth = 680;
        MinHeight = 620;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = UiTokens.Brush(UiTokens.Background);
        _supplier.ItemsSource = suppliers.Select(item => new SupplierChoice(item)).ToList();
        _supplier.SelectedIndex = suppliers.Count > 0 ? 0 : -1;
        var ticketChoices = new List<TicketChoice> { new(null) };
        ticketChoices.AddRange(tickets.Select(item => new TicketChoice(item)));
        _ticket.ItemsSource = ticketChoices;
        _ticket.SelectedIndex = 0;
        Content = BuildLayout();
        RefreshLines();
    }

    private Control BuildLayout()
    {
        var root = new DockPanel();
        var top = new StackPanel { Margin = new Thickness(24, 20, 24, 10), Spacing = 8 };
        top.Children.Add(new TextBlock { Text = "Nuovo ordine ricambi", FontSize = 26, FontWeight = FontWeight.Bold });
        var metadata = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*,150") };
        AddField(metadata, "Fornitore", _supplier, 0);
        AddField(metadata, "Intervento collegato", _ticket, 1);
        AddField(metadata, "Consegna prevista", _expected, 2);
        top.Children.Add(metadata);
        top.Children.Add(Field("Note ordine", _notes));
        var line = new Grid { ColumnDefinitions = new ColumnDefinitions("100,*,90,110,100") };
        AddField(line, "Codice", _code, 0);
        AddField(line, "Descrizione", _description, 1);
        AddField(line, "Quantità", _quantity, 2);
        AddField(line, "Costo unitario", _cost, 3);
        var add = Button("Aggiungi", AddLine, true); Grid.SetColumn(add, 4); line.Children.Add(add);
        top.Children.Add(line);
        top.Children.Add(_message);
        DockPanel.SetDock(top, Dock.Top); root.Children.Add(top);

        var footer = new Grid { ColumnDefinitions = new ColumnDefinitions("*,120,150"), Margin = new Thickness(24, 10, 24, 20) };
        _total.FontSize = 17; _total.FontWeight = FontWeight.Bold; footer.Children.Add(_total);
        var cancel = Button("Annulla", () => Close(null)); Add(footer, cancel, 1);
        var save = Button("Crea ordine", Confirm, true); Add(footer, save, 2);
        DockPanel.SetDock(footer, Dock.Bottom); root.Children.Add(footer);
        root.Children.Add(new ScrollViewer { Content = _linesPanel, Margin = new Thickness(24, 0) });
        return root;
    }

    private void AddLine()
    {
        if (string.IsNullOrWhiteSpace(_description.Text))
        { Error("Inserisci la descrizione."); return; }
        if (!decimal.TryParse(_quantity.Text, out var quantity) || quantity <= 0)
        { Error("Quantità non valida."); return; }
        if (!decimal.TryParse(_cost.Text, out var cost) || cost < 0)
        { Error("Costo non valido."); return; }
        _lines.Add(new MaintenancePurchaseOrderLine
        {
            PartCode = _code.Text?.Trim() ?? "", Description = _description.Text.Trim(),
            Quantity = quantity, UnitCost = cost
        });
        _code.Text = ""; _description.Text = ""; _quantity.Text = "1"; _cost.Text = "0";
        _message.Text = ""; RefreshLines();
    }

    private void RefreshLines()
    {
        _linesPanel.Children.Clear();
        for (var index = 0; index < _lines.Count; index++)
        {
            var item = _lines[index];
            var row = new Grid { ColumnDefinitions = new ColumnDefinitions("100,*,90,120,90"), Margin = new Thickness(0, 3) };
            AddText(row, item.PartCode, 0); AddText(row, item.Description, 1);
            AddText(row, item.Quantity.ToString("N2"), 2);
            AddText(row, $"EUR {item.Total:N2}", 3);
            var remove = Button("Rimuovi", () => { _lines.Remove(item); RefreshLines(); }); Add(row, remove, 4);
            _linesPanel.Children.Add(new Border
            {
                Background = UiTokens.Brush(index % 2 == 0 ? UiTokens.Surface : UiTokens.SurfaceAlt),
                BorderBrush = UiTokens.Brush(UiTokens.Border), BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(8), Child = row
            });
        }
        if (_lines.Count == 0)
            _linesPanel.Children.Add(new TextBlock
            {
                Text = "Aggiungi almeno una riga all'ordine.",
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = UiTokens.Brush(UiTokens.TextSecondary), Margin = new Thickness(12, 24)
            });
        _total.Text = $"Totale ordine: EUR {_lines.Sum(item => item.Total):N2}";
    }

    private void Confirm()
    {
        if (_supplier.SelectedItem is not SupplierChoice supplier)
        { Error("Seleziona un fornitore."); return; }
        if (_lines.Count == 0)
        { Error("Aggiungi almeno un ricambio."); return; }
        var expected = DateTime.TryParse(_expected.Text, out var date) ? date.ToString("s") : _expected.Text?.Trim() ?? "";
        Close(new MaintenancePurchaseOrder
        {
            SupplierId = supplier.Supplier.Id,
            MaintenanceTicketId = (_ticket.SelectedItem as TicketChoice)?.Ticket?.Id ?? 0,
            Status = PurchaseOrderStatus.Draft,
            OrderDate = DateTime.Today.ToString("s"),
            ExpectedDate = expected,
            Notes = _notes.Text?.Trim() ?? "",
            Lines = _lines.ToList()
        });
    }

    private void Error(string text)
    { _message.Text = text; _message.Foreground = UiTokens.Brush(UiTokens.Danger); }
    private static Control Field(string label, Control control) => new StackPanel
    { Spacing = 4, Children = { new TextBlock { Text = label, FontWeight = FontWeight.SemiBold }, control } };
    private static void AddField(Grid grid, string label, Control control, int column)
    { var field = Field(label, control); field.Margin = new Thickness(4); Add(grid, field, column); }
    private static void AddText(Grid grid, string text, int column)
    { Add(grid, new TextBlock { Text = string.IsNullOrWhiteSpace(text) ? "—" : text, VerticalAlignment = VerticalAlignment.Center }, column); }
    private static Button Button(string text, Action action, bool primary = false)
    {
        var button = new Button
        {
            Content = text, MinHeight = 36, Margin = new Thickness(4),
            Background = UiTokens.Brush(primary ? UiTokens.BrandBlue : UiTokens.SurfaceAlt),
            Foreground = primary ? Brushes.White : UiTokens.Brush(UiTokens.TextPrimary)
        };
        button.Click += (_, _) => action(); return button;
    }
    private static void Add(Grid grid, Control control, int column)
    { Grid.SetColumn(control, column); grid.Children.Add(control); }
    private sealed record SupplierChoice(MaintenanceSupplier Supplier)
    { public override string ToString() => Supplier.Name; }
    private sealed record TicketChoice(MaintenanceTicket? Ticket)
    { public override string ToString() => Ticket is null ? "Nessun intervento" : $"#{Ticket.Id} · {Ticket.Title}"; }
}
