using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Models;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class MaintenancePurchasingView : UserControl
{
    private readonly MaintenancePurchasingRepository _repository = new();
    private readonly MaintenanceRepository _maintenance = new();
    private readonly MaintenancePartsRepository _parts = new();
    private readonly SparePartsInventoryRepository _inventory = new();
    private readonly SparePartLocationsRepository _locations = new();
    private readonly SparePartReplenishmentRepository _replenishment = new();
    private readonly MaintenancePurchaseOrderPdfService _pdf = new();
    private readonly TextBox _search = new();
    private readonly ComboBox _status = new();
    private readonly StackPanel _rows = new();
    private readonly TextBlock _summary = new();
    private readonly TextBlock _message = new();
    private readonly Grid _kpis = new() { ColumnDefinitions = new ColumnDefinitions("*,*,*,*,*") };
    private bool _suppliersMode;

    public MaintenancePurchasingView()
    {
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();
        Load();
    }

    private Control BuildLayout()
    {
        var root = new DockPanel();
        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Margin = new Thickness(24, 20, 24, 12) };
        var heading = new StackPanel { Spacing = 4, Children =
        {
            new TextBlock { Text = "Acquisti e Fornitori", FontSize = 30, FontWeight = FontWeight.Bold },
            new TextBlock { Text = "Ordini ricambi, consegne, costi e anagrafiche fornitori.", Foreground = UiTokens.Brush(UiTokens.TextSecondary) }
        }};
        header.Children.Add(heading);
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        actions.Children.Add(Button("Nuovo fornitore", NewSupplier));
        actions.Children.Add(Button("Nuovo ordine", NewOrder, true));
        actions.Children.Add(Button("Ordini", () => { _suppliersMode = false; Load(); }));
        actions.Children.Add(Button("Fornitori", () => { _suppliersMode = true; Load(); }));
        Grid.SetColumn(actions, 1); header.Children.Add(actions);
        DockPanel.SetDock(header, Dock.Top); root.Children.Add(header);

        _kpis.Margin = new Thickness(24, 0, 24, 10);
        DockPanel.SetDock(_kpis, Dock.Top); root.Children.Add(_kpis);

        var filters = new Grid { ColumnDefinitions = new ColumnDefinitions("*,180"), Margin = new Thickness(24, 0, 24, 8) };
        _search.Watermark = "Cerca ordine, fornitore o articolo...";
        _search.TextChanged += (_, _) => Load();
        filters.Children.Add(_search);
        _status.ItemsSource = new[] { "Tutti gli stati", PurchaseOrderStatus.Draft, PurchaseOrderStatus.Sent, PurchaseOrderStatus.Confirmed, PurchaseOrderStatus.Received, PurchaseOrderStatus.Cancelled };
        _status.SelectedIndex = 0; _status.SelectionChanged += (_, _) => Load();
        Grid.SetColumn(_status, 1); filters.Children.Add(_status);
        DockPanel.SetDock(filters, Dock.Top); root.Children.Add(filters);
        _message.Margin = new Thickness(24, 0, 24, 6); DockPanel.SetDock(_message, Dock.Top); root.Children.Add(_message);
        _summary.Margin = new Thickness(24, 0, 24, 8); _summary.Foreground = UiTokens.Brush(UiTokens.TextSecondary);
        DockPanel.SetDock(_summary, Dock.Top); root.Children.Add(_summary);
        root.Children.Add(new ScrollViewer
        {
            Content = _rows, Margin = new Thickness(24, 0, 24, 24),
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        });
        return root;
    }

    private void Load()
    {
        try
        {
            _message.Text = "";
            var suppliers = _repository.GetSuppliers();
            var supplierMap = suppliers.ToDictionary(item => item.Id);
            var orders = _repository.GetOrders();
            BuildKpis(orders);
            _rows.Children.Clear();
            if (_suppliersMode) LoadSuppliers(suppliers, orders);
            else LoadOrders(orders, supplierMap);
        }
        catch (Exception ex) { Show($"Errore caricamento acquisti: {ex.Message}", true); }
    }

    private void BuildKpis(IReadOnlyList<MaintenancePurchaseOrder> orders)
    {
        _kpis.Children.Clear();
        AddKpi(0, "Bozze", orders.Count(item => item.Status == PurchaseOrderStatus.Draft), UiTokens.BrandBlue);
        AddKpi(1, "Inviati", orders.Count(item => item.Status == PurchaseOrderStatus.Sent), UiTokens.Warning);
        AddKpi(2, "In ritardo", orders.Count(IsLate), UiTokens.Danger);
        AddKpi(3, "Ricevuti", orders.Count(item => item.Status == PurchaseOrderStatus.Received), UiTokens.Success);
        AddKpi(4, "Valore ordini", $"EUR {orders.Where(item => item.Status != PurchaseOrderStatus.Cancelled).Sum(item => item.Total):N2}", UiTokens.BrandBlue);
    }

    private void AddKpi(int column, string label, object value, string color)
    {
        var card = Kpi(label, value, color);
        Grid.SetColumn(card, column);
        _kpis.Children.Add(card);
    }

    private void LoadOrders(IReadOnlyList<MaintenancePurchaseOrder> orders, IReadOnlyDictionary<int, MaintenanceSupplier> suppliers)
    {
        _rows.MinWidth = 1120; _rows.Children.Add(OrderHeader());
        var query = (_search.Text ?? "").Trim();
        var visible = orders.Where(order => _status.SelectedIndex <= 0 || order.Status == _status.SelectedItem?.ToString())
            .Where(order =>
            {
                suppliers.TryGetValue(order.SupplierId, out var supplier);
                var text = $"{order.OrderNumber} {supplier?.Name} {string.Join(" ", order.Lines.Select(line => line.Description))}";
                return query.Length == 0 || text.Contains(query, StringComparison.OrdinalIgnoreCase);
            }).ToList();
        for (var i = 0; i < visible.Count; i++)
        {
            suppliers.TryGetValue(visible[i].SupplierId, out var supplier);
            _rows.Children.Add(OrderRow(visible[i], supplier, i));
        }
        _summary.Text = $"{visible.Count} ordini visualizzati";
    }

    private void LoadSuppliers(IReadOnlyList<MaintenanceSupplier> suppliers, IReadOnlyList<MaintenancePurchaseOrder> orders)
    {
        _rows.MinWidth = 900;
        var query = (_search.Text ?? "").Trim();
        var visible = suppliers.Where(item => query.Length == 0 || $"{item.Name} {item.VatNumber} {item.ContactPerson} {item.Email}".Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
        for (var i = 0; i < visible.Count; i++)
        {
            var supplier = visible[i];
            var panel = new Grid { ColumnDefinitions = new ColumnDefinitions("220,140,180,180,110,100") };
            AddText(panel, supplier.Name, 0, true); AddText(panel, supplier.VatNumber, 1);
            AddText(panel, supplier.ContactPerson, 2); AddText(panel, supplier.Email, 3);
            AddText(panel, orders.Count(order => order.SupplierId == supplier.Id).ToString(), 4);
            Add(panel, Button("Modifica", () => EditSupplier(supplier)), 5);
            _rows.Children.Add(Row(panel, i));
        }
        _summary.Text = $"{visible.Count} fornitori visualizzati";
    }

    private Control OrderHeader()
    {
        var grid = OrderGrid();
        foreach (var item in new[] { ("Ordine",0),("Fornitore",1),("Stato",2),("Data",3),("Consegna",4),("Totale",5),("PDF",6),("Avanza",7),("Ricevi",8) })
            AddText(grid, item.Item1, item.Item2, true);
        return new Border { Background = UiTokens.Brush(UiTokens.SurfaceAlt), Padding = new Thickness(9), Child = grid };
    }

    private Control OrderRow(MaintenancePurchaseOrder order, MaintenanceSupplier? supplier, int index)
    {
        var grid = OrderGrid();
        AddText(grid, order.OrderNumber, 0, true); AddText(grid, supplier?.Name ?? $"Fornitore #{order.SupplierId}", 1);
        Add(grid, Badge(order.Status, IsLate(order) ? UiTokens.Danger : StatusColor(order.Status)), 2);
        AddText(grid, Date(order.OrderDate), 3); AddText(grid, Date(order.ExpectedDate), 4, false, IsLate(order));
        AddText(grid, $"EUR {order.Total:N2}", 5, true);
        Add(grid, Button("PDF", () => OpenPdf(order, supplier)), 6);
        var advance = Button("Avanza", () => Advance(order)); advance.IsEnabled = order.Status is PurchaseOrderStatus.Draft or PurchaseOrderStatus.Sent;
        Add(grid, advance, 7);
        var receive = Button("Ricevi", () => Receive(order, supplier)); receive.IsEnabled = order.Status == PurchaseOrderStatus.Confirmed;
        Add(grid, receive, 8);
        return Row(grid, index, IsLate(order));
    }

    private async void NewSupplier()
    {
        try
        {
            var owner = TopLevel.GetTopLevel(this) as Window; if (owner is null) return;
            var supplier = await new MaintenanceSupplierDialog().ShowDialog<MaintenanceSupplier?>(owner);
            if (supplier is null) return;
            _repository.SaveSupplier(supplier);
            Show("Fornitore salvato. Se esisteva già, i dati sono stati aggiornati.");
            Load();
        }
        catch (Exception ex) { Show($"Errore salvataggio fornitore: {ex.Message}", true); }
    }
    private async void EditSupplier(MaintenanceSupplier existing)
    {
        try
        {
            var owner = TopLevel.GetTopLevel(this) as Window; if (owner is null) return;
            var supplier = await new MaintenanceSupplierDialog(existing).ShowDialog<MaintenanceSupplier?>(owner);
            if (supplier is null) return;
            _repository.SaveSupplier(supplier);
            Show("Fornitore aggiornato.");
            Load();
        }
        catch (Exception ex) { Show($"Errore aggiornamento fornitore: {ex.Message}", true); }
    }
    private async void NewOrder()
    {
        try
        {
            var suppliers = _repository.GetSuppliers();
            if (suppliers.Count == 0) { Show("Crea prima almeno un fornitore.", true); return; }
            var owner = TopLevel.GetTopLevel(this) as Window; if (owner is null) return;
            var order = await new MaintenancePurchaseOrderDialog(suppliers, _maintenance.GetAll())
                .ShowDialog<MaintenancePurchaseOrder?>(owner);
            if (order is null) return; _repository.CreateOrder(order); Show($"Ordine {order.OrderNumber} creato."); Load();
        }
        catch (Exception ex) { Show($"Errore creazione ordine: {ex.Message}", true); }
    }
    private void Advance(MaintenancePurchaseOrder order)
    {
        var status = order.Status == PurchaseOrderStatus.Draft ? PurchaseOrderStatus.Sent : PurchaseOrderStatus.Confirmed;
        _repository.SetStatus(order.Id, status); Show($"Ordine {status.ToLowerInvariant()}."); Load();
    }
    private async void Receive(MaintenancePurchaseOrder order, MaintenanceSupplier? supplier)
    {
        var owner=TopLevel.GetTopLevel(this) as Window;if(owner is null)return;
        var availableLocations=_locations.GetLocations().Where(x=>x.IsActive).ToList();
        if(availableLocations.Count==0){Show("Crea prima un'ubicazione in Ubicazioni magazzino.",true);return;}
        var destination=await new ReceiptLocationDialog(availableLocations).ShowDialog<SparePartWarehouseLocation?>(owner);
        if(destination is null)return;
        _repository.SetStatus(order.Id, PurchaseOrderStatus.Received);
        foreach (var line in order.Lines)
        {
            var code=string.IsNullOrWhiteSpace(line.PartCode) ? $"ART-{line.Id:D6}" : line.PartCode;
            _inventory.Receive(
                code,
                line.Description,
                supplier?.Name ?? "",
                line.Quantity,
                line.UnitCost,
                order.OrderNumber);
            var item=_inventory.GetItems().First(x=>string.Equals(x.PartCode,code,StringComparison.OrdinalIgnoreCase));
            _locations.ReceiveIntoLocation(item.Id,destination.Id,line.Quantity,item.Quantity);
        }
        if (order.MaintenanceTicketId > 0)
            foreach (var line in order.Lines)
            {
                _parts.Add(new MaintenancePart
                {
                    MaintenanceTicketId = order.MaintenanceTicketId, PartCode = line.PartCode,
                    Description = line.Description, Supplier = supplier?.Name ?? "",
                    Quantity = line.Quantity, UnitCost = line.UnitCost,
                    Notes = $"Ricevuto con ordine {order.OrderNumber}"
                });
                _inventory.Consume(
                    string.IsNullOrWhiteSpace(line.PartCode) ? $"ART-{line.Id:D6}" : line.PartCode,
                    line.Quantity,
                    $"Manutenzione #{order.MaintenanceTicketId}",
                    $"Utilizzo diretto da ordine {order.OrderNumber}");
            }
        _replenishment.CompleteByOrderId(order.Id);
        Show("Ordine ricevuto e ricambi collegati all'intervento."); Load();
    }
    private void OpenPdf(MaintenancePurchaseOrder order, MaintenanceSupplier? supplier)
    {
        try
        {
            supplier ??= _repository.GetSuppliers().First(item => item.Id == order.SupplierId);
            var path = _pdf.Generate(order, supplier, "Acquisti e Fornitori");
            _repository.SetPdfPath(order.Id, path);
            Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
        }
        catch (Exception ex) { Show($"Errore PDF ordine: {ex.Message}", true); }
    }

    private static bool IsLate(MaintenancePurchaseOrder order) =>
        order.Status is not (PurchaseOrderStatus.Received or PurchaseOrderStatus.Cancelled) &&
        DateTime.TryParse(order.ExpectedDate, out var expected) && expected.Date < DateTime.Today;
    private static string Date(string value) => DateTime.TryParse(value, out var date) ? date.ToString("dd/MM/yyyy") : "—";
    private static Grid OrderGrid() => new() { ColumnDefinitions = new ColumnDefinitions("145,210,120,100,105,110,80,90,90") };
    private static Border Row(Control child, int index, bool danger = false) => new()
    {
        Background = UiTokens.Brush(index % 2 == 0 ? UiTokens.Surface : UiTokens.SurfaceAlt),
        BorderBrush = UiTokens.Brush(danger ? UiTokens.Danger : UiTokens.Border),
        BorderThickness = danger ? new Thickness(3,0,0,1) : new Thickness(0,0,0,1),
        Padding = new Thickness(9,6), Child = child
    };
    private static string StatusColor(string status) => status switch
    { PurchaseOrderStatus.Received => UiTokens.Success, PurchaseOrderStatus.Cancelled => UiTokens.Danger, PurchaseOrderStatus.Confirmed => UiTokens.Warning, _ => UiTokens.BrandBlue };
    private static Control Badge(string text, string color) => new Border
    {
        BorderBrush = UiTokens.Brush(color), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9),
        Padding = new Thickness(7,4), Margin = new Thickness(3),
        Child = new TextBlock { Text = text, Foreground = UiTokens.Brush(color), HorizontalAlignment = HorizontalAlignment.Center, FontWeight = FontWeight.Bold, FontSize = 11 }
    };
    private static Control Kpi(string label, object value, string color) => new Border
    {
        Background = UiTokens.Brush(UiTokens.Surface), BorderBrush = UiTokens.Brush(UiTokens.Border),
        BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(10), Padding = new Thickness(14,9), Margin = new Thickness(0,0,10,0),
        Child = new StackPanel { Children = { new TextBlock { Text = value.ToString(), FontSize = 22, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(color) }, new TextBlock { Text = label, Foreground = UiTokens.Brush(UiTokens.TextSecondary) } } }
    };
    private static Button Button(string text, Action action, bool primary = false)
    {
        var button = new Button { Content = text, MinHeight = 34, Margin = new Thickness(3), Background = UiTokens.Brush(primary ? UiTokens.BrandBlue : UiTokens.SurfaceAlt), Foreground = primary ? Brushes.White : UiTokens.Brush(UiTokens.TextPrimary) };
        button.Click += (_, _) => action(); return button;
    }
    private static void AddText(Grid grid, string text, int column, bool strong = false, bool danger = false) =>
        Add(grid, new TextBlock { Text = string.IsNullOrWhiteSpace(text) ? "—" : text, FontWeight = strong ? FontWeight.SemiBold : FontWeight.Normal, Foreground = UiTokens.Brush(danger ? UiTokens.Danger : strong ? UiTokens.TextPrimary : UiTokens.TextSecondary), TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(3) }, column);
    private static void Add(Grid grid, Control control, int column) { Grid.SetColumn(control, column); grid.Children.Add(control); }
    private void Show(string text, bool error = false) { _message.Text = text; _message.Foreground = UiTokens.Brush(error ? UiTokens.Danger : UiTokens.Success); }
}
