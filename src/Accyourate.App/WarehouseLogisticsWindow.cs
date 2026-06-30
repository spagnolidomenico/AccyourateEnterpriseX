using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.Models;

namespace Accyourate.App;

public sealed class WarehouseLogisticsWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;

    private readonly TextBlock _message = new();
    private readonly StackPanel _locationsPanel = new();
    private readonly StackPanel _movementsPanel = new();
    private readonly StackPanel _shipmentsPanel = new();

    private readonly TextBox _locationCode = new();
    private readonly TextBox _warehouse = new();
    private readonly TextBox _aisle = new();
    private readonly TextBox _shelf = new();
    private readonly TextBox _level = new();

    private readonly ComboBox _movementDevice = new();
    private readonly ComboBox _movementType = new();
    private readonly ComboBox _fromLocation = new();
    private readonly ComboBox _toLocation = new();
    private readonly TextBox _movementReason = new();
    private readonly TextBox _movementOperator = new();

    private readonly ComboBox _shipmentDevice = new();
    private readonly TextBox _shipmentCode = new();
    private readonly TextBox _destination = new();
    private readonly TextBox _tracking = new();
    private readonly TextBox _shipDate = new();
    private readonly TextBox _shipmentOperator = new();

    private List<MedicalDeviceRecord> _devices = new();
    private List<WarehouseLocationRecord> _locations = new();

    public WarehouseLogisticsWindow(DatabaseService database, CurrentUser user)
    {
        _database = database;
        _user = user;

        Title = "Accyourate Enterprise X - Warehouse & Logistics";
        Width = 1320;
        Height = 860;
        
        MinWidth = 1180;
        MinHeight = 760;
WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");

        LoadData();
        Content = BuildLayout();
        Refresh();
    }

    private void LoadData()
    {
        _devices = _database.GetMedicalDevices(null, false);
        _locations = _database.GetWarehouseLocations();
    }

    private Control BuildLayout()
    {
        var scroll = new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
        var stack = new StackPanel { Margin = new Thickness(24), Spacing = 16 };

        stack.Children.Add(new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Text = "Warehouse & Logistics",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });

        stack.Children.Add(new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Text = "RC 4.2: ubicazioni, movimentazioni, spedizioni, rientri e timeline Digital Twin."
        });

        stack.Children.Add(BuildLocationForm());
        stack.Children.Add(BuildMovementForm());
        stack.Children.Add(BuildShipmentForm());

        _message.Foreground = Brush.Parse("#B5162B");
        stack.Children.Add(_message);

        stack.Children.Add(Card("Ubicazioni", _locationsPanel));
        stack.Children.Add(Card("Movimentazioni", _movementsPanel));
        stack.Children.Add(Card("Spedizioni e rientri", _shipmentsPanel));

        scroll.Content = stack;
        return scroll;
    }

    private Control BuildLocationForm()
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("120,150,100,100,100,100"), RowDefinitions = new RowDefinitions("Auto,Auto") };
        AddLabel(grid, "Codice", 0, 0);
        AddLabel(grid, "Magazzino", 1, 0);
        AddLabel(grid, "Corsia", 2, 0);
        AddLabel(grid, "Scaffale", 3, 0);
        AddLabel(grid, "Ripiano", 4, 0);

        _locationCode.Watermark = "MAG-A-01";
        _warehouse.Watermark = "Magazzino A";
        _aisle.Watermark = "A";
        _shelf.Watermark = "01";
        _level.Watermark = "1";

        AddControl(grid, _locationCode, 0, 1);
        AddControl(grid, _warehouse, 1, 1);
        AddControl(grid, _aisle, 2, 1);
        AddControl(grid, _shelf, 3, 1);
        AddControl(grid, _level, 4, 1);

        var create = PrimaryButton("Crea");
        create.Click += (_, _) => CreateLocation();
        AddControl(grid, create, 5, 1);

        return Card("Nuova ubicazione", grid);
    }

    private Control BuildMovementForm()
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("220,140,140,140,180,130,100"), RowDefinitions = new RowDefinitions("Auto,Auto") };
        AddLabel(grid, "Dispositivo", 0, 0);
        AddLabel(grid, "Tipo", 1, 0);
        AddLabel(grid, "Da", 2, 0);
        AddLabel(grid, "A", 3, 0);
        AddLabel(grid, "Motivo", 4, 0);
        AddLabel(grid, "Operatore", 5, 0);

        _movementDevice.ItemsSource = DeviceItems();
        _movementDevice.SelectedIndex = _movementDevice.ItemCount > 0 ? 0 : -1;
        _movementType.ItemsSource = new[] { "Entrata", "Uscita", "Trasferimento", "Inventario", "Rientro" };
        _movementType.SelectedIndex = 0;
        _fromLocation.ItemsSource = LocationItems(includeEmpty: true);
        _fromLocation.SelectedIndex = 0;
        _toLocation.ItemsSource = LocationItems(includeEmpty: true);
        _toLocation.SelectedIndex = 0;
        _movementReason.Watermark = "Motivo";
        _movementOperator.Watermark = "Operatore";

        AddControl(grid, _movementDevice, 0, 1);
        AddControl(grid, _movementType, 1, 1);
        AddControl(grid, _fromLocation, 2, 1);
        AddControl(grid, _toLocation, 3, 1);
        AddControl(grid, _movementReason, 4, 1);
        AddControl(grid, _movementOperator, 5, 1);

        var create = PrimaryButton("Registra");
        create.Click += (_, _) => CreateMovement();
        AddControl(grid, create, 6, 1);

        return Card("Movimentazione", grid);
    }

    private Control BuildShipmentForm()
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("220,120,160,140,120,130,100"), RowDefinitions = new RowDefinitions("Auto,Auto") };
        AddLabel(grid, "Dispositivo", 0, 0);
        AddLabel(grid, "Spedizione", 1, 0);
        AddLabel(grid, "Destinazione", 2, 0);
        AddLabel(grid, "Tracking", 3, 0);
        AddLabel(grid, "Data", 4, 0);
        AddLabel(grid, "Operatore", 5, 0);

        _shipmentDevice.ItemsSource = DeviceItems();
        _shipmentDevice.SelectedIndex = _shipmentDevice.ItemCount > 0 ? 0 : -1;
        _shipmentCode.Watermark = "SPED001";
        _destination.Watermark = "Cliente / sede";
        _tracking.Watermark = "TRK...";
        _shipDate.Watermark = "2026-01-01";
        _shipmentOperator.Watermark = "Operatore";

        AddControl(grid, _shipmentDevice, 0, 1);
        AddControl(grid, _shipmentCode, 1, 1);
        AddControl(grid, _destination, 2, 1);
        AddControl(grid, _tracking, 3, 1);
        AddControl(grid, _shipDate, 4, 1);
        AddControl(grid, _shipmentOperator, 5, 1);

        var create = PrimaryButton("Spedisci");
        create.Click += (_, _) => CreateShipment();
        AddControl(grid, create, 6, 1);

        return Card("Nuova spedizione", grid);
    }

    private void CreateLocation()
    {
        var location = new WarehouseLocationRecord
        {
            LocationCode = _locationCode.Text ?? "",
            Warehouse = _warehouse.Text ?? "",
            Aisle = _aisle.Text ?? "",
            Shelf = _shelf.Text ?? "",
            Level = _level.Text ?? ""
        };

        var ok = _database.CreateWarehouseLocation(location, _user.Username, out var error);
        _message.Text = ok ? "Ubicazione creata." : error;
        if (ok)
        {
            _locationCode.Text = "";
            _warehouse.Text = "";
            _aisle.Text = "";
            _shelf.Text = "";
            _level.Text = "";
            Refresh();
        }
    }

    private void CreateMovement()
    {
        var movement = new StockMovementRecord
        {
            MedicalDeviceId = SelectedDeviceId(_movementDevice),
            MovementType = _movementType.SelectedItem?.ToString() ?? "",
            Quantity = "1",
            Reason = _movementReason.Text ?? "",
            OperatorName = _movementOperator.Text ?? ""
        };

        var ok = _database.CreateStockMovement(movement, SelectedLocationId(_fromLocation), SelectedLocationId(_toLocation), _user.Username, out var error);
        _message.Text = ok ? "Movimentazione registrata." : error;
        if (ok) Refresh();
    }

    private void CreateShipment()
    {
        var shipment = new ShipmentRecord
        {
            MedicalDeviceId = SelectedDeviceId(_shipmentDevice),
            ShipmentCode = _shipmentCode.Text ?? "",
            Destination = _destination.Text ?? "",
            TrackingCode = _tracking.Text ?? "",
            Status = "Spedito",
            ShipDate = _shipDate.Text ?? "",
            OperatorName = _shipmentOperator.Text ?? ""
        };

        var ok = _database.CreateShipment(shipment, _user.Username, out var error);
        _message.Text = ok ? "Spedizione registrata." : error;
        if (ok) Refresh();
    }

    private void Refresh()
    {
        LoadData();

        _movementDevice.ItemsSource = DeviceItems();
        _shipmentDevice.ItemsSource = DeviceItems();
        _fromLocation.ItemsSource = LocationItems(includeEmpty: true);
        _toLocation.ItemsSource = LocationItems(includeEmpty: true);

        RefreshLocations();
        RefreshMovements();
        RefreshShipments();
    }

    private void RefreshLocations()
    {
        _locationsPanel.Children.Clear();
        _locationsPanel.Spacing = 8;

        _locationsPanel.Children.Add(new TextBlock { Text = $"Ubicazioni ({_locations.Count})", FontSize = 18, FontWeight = FontWeight.Bold });

        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("140,180,100,100,100,200") };
        AddHeader(header, "Codice", 0);
        AddHeader(header, "Magazzino", 1);
        AddHeader(header, "Corsia", 2);
        AddHeader(header, "Scaffale", 3);
        AddHeader(header, "Ripiano", 4);
        AddHeader(header, "Descrizione", 5);
        _locationsPanel.Children.Add(header);

        foreach (var l in _locations)
        {
            var row = new Grid { ColumnDefinitions = new ColumnDefinitions("140,180,100,100,100,200") };
            AddText(row, l.LocationCode, 0);
            AddText(row, l.Warehouse, 1);
            AddText(row, l.Aisle, 2);
            AddText(row, l.Shelf, 3);
            AddText(row, l.Level, 4);
            AddText(row, l.Description, 5);
            _locationsPanel.Children.Add(row);
        }
    }

    private void RefreshMovements()
    {
        _movementsPanel.Children.Clear();
        _movementsPanel.Spacing = 8;

        var rows = _database.GetStockMovements();
        _movementsPanel.Children.Add(new TextBlock { Text = $"Movimentazioni ({rows.Count})", FontSize = 18, FontWeight = FontWeight.Bold });

        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("130,130,120,120,120,180,130,150") };
        AddHeader(header, "Data", 0);
        AddHeader(header, "Dispositivo", 1);
        AddHeader(header, "Tipo", 2);
        AddHeader(header, "Da", 3);
        AddHeader(header, "A", 4);
        AddHeader(header, "Motivo", 5);
        AddHeader(header, "Operatore", 6);
        AddHeader(header, "Q.tà", 7);
        _movementsPanel.Children.Add(header);

        foreach (var m in rows)
        {
            var row = new Grid { ColumnDefinitions = new ColumnDefinitions("130,130,120,120,120,180,130,150") };
            AddText(row, m.CreatedAt, 0);
            AddText(row, m.DeviceCode, 1);
            AddText(row, m.MovementType, 2);
            AddText(row, m.FromLocationCode, 3);
            AddText(row, m.ToLocationCode, 4);
            AddText(row, m.Reason, 5);
            AddText(row, m.OperatorName, 6);
            AddText(row, m.Quantity, 7);
            _movementsPanel.Children.Add(row);
        }
    }

    private void RefreshShipments()
    {
        _shipmentsPanel.Children.Clear();
        _shipmentsPanel.Spacing = 8;

        var rows = _database.GetShipments();
        _shipmentsPanel.Children.Add(new TextBlock { Text = $"Spedizioni ({rows.Count})", FontSize = 18, FontWeight = FontWeight.Bold });

        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("110,120,160,120,120,130,120,100") };
        AddHeader(header, "Codice", 0);
        AddHeader(header, "Dispositivo", 1);
        AddHeader(header, "Destinazione", 2);
        AddHeader(header, "Stato", 3);
        AddHeader(header, "Tracking", 4);
        AddHeader(header, "Operatore", 5);
        AddHeader(header, "Data", 6);
        AddHeader(header, "Rientro", 7);
        _shipmentsPanel.Children.Add(header);

        foreach (var sh in rows)
        {
            var row = new Grid { ColumnDefinitions = new ColumnDefinitions("110,120,160,120,120,130,120,100") };
            AddText(row, sh.ShipmentCode, 0);
            AddText(row, sh.DeviceCode, 1);
            AddText(row, sh.Destination, 2);
            AddText(row, sh.Status, 3);
            AddText(row, sh.TrackingCode, 4);
            AddText(row, sh.OperatorName, 5);
            AddText(row, sh.ShipDate, 6);

            var ret = new Button { Content = "Rientro" };
            ret.Click += (_, _) =>
            {
                _database.MarkShipmentReturned(sh.Id, _user.Username);
                Refresh();
            };
            AddControl(row, ret, 7, 0);

            _shipmentsPanel.Children.Add(row);
        }
    }

    private List<string> DeviceItems() => _devices.Select(d => $"{d.Id}|{d.DeviceCode} - {d.DeviceType}").ToList();

    private List<string> LocationItems(bool includeEmpty)
    {
        var items = new List<string>();
        if (includeEmpty)
            items.Add("Nessuna");
        items.AddRange(_locations.Select(l => $"{l.Id}|{l.LocationCode}"));
        return items;
    }

    private static long SelectedDeviceId(ComboBox combo)
    {
        var text = combo.SelectedItem?.ToString() ?? "";
        var first = text.Split('|')[0];
        return long.TryParse(first, out var id) ? id : 0;
    }

    private static long? SelectedLocationId(ComboBox combo)
    {
        var text = combo.SelectedItem?.ToString() ?? "";
        if (text == "Nessuna" || string.IsNullOrWhiteSpace(text))
            return null;

        var first = text.Split('|')[0];
        return long.TryParse(first, out var id) ? id : null;
    }

    private static Border Card(string title, Control content)
    {
        return new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(18),
            Child = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new TextBlock { Text = title, FontSize = 18, FontWeight = FontWeight.Bold },
                    content
                }
            }
        };
    }

    private static Button PrimaryButton(string text)
    {
        return new Button
        {
            Content = text,
            Background = Brush.Parse("#B5162B"),
            Foreground = Brushes.White,
            FontWeight = FontWeight.Bold
        };
    }

    private static void AddLabel(Grid grid, string text, int column, int row) => AddControl(grid, new TextBlock { Text = text, FontWeight = FontWeight.Bold }, column, row);
    private static void AddHeader(Grid grid, string text, int column) => AddControl(grid, new TextBlock { Text = text, FontWeight = FontWeight.Bold, Foreground = Brush.Parse("#B5162B") }, column, 0);
    private static void AddText(Grid grid, string text, int column) => AddControl(grid, new TextBlock { Text = string.IsNullOrWhiteSpace(text) ? "-" : text }, column, 0);

    private static void AddControl(Grid grid, Control control, int column, int row)
    {
        control.Margin = new Thickness(4);
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
