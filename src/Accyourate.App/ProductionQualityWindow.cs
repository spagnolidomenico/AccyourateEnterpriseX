using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.Models;

namespace Accyourate.App;

public sealed class ProductionQualityWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;
    private readonly StackPanel _ordersPanel = new();
    private readonly StackPanel _testsPanel = new();
    private readonly TextBlock _message = new();

    private readonly ComboBox _deviceOrder = new();
    private readonly TextBox _orderCode = new();
    private readonly TextBox _lot = new();
    private readonly TextBox _plannedDate = new();
    private readonly TextBox _operator = new();

    private readonly ComboBox _deviceTest = new();
    private readonly TextBox _testCode = new();
    private readonly TextBox _checklist = new();
    private readonly ComboBox _functional = new();
    private readonly ComboBox _electrical = new();
    private readonly ComboBox _conformity = new();
    private readonly ComboBox _final = new();
    private readonly TextBox _testOperator = new();
    private readonly TextBox _testDate = new();

    private List<MedicalDeviceRecord> _devices = new();

    public ProductionQualityWindow(DatabaseService database, CurrentUser user)
    {
        _database = database;
        _user = user;
        Title = "Accyourate Enterprise X - Production & Quality Suite";
        Width = 1320;
        Height = 860;
        
        MinWidth = 1180;
        MinHeight = 760;
WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");
        LoadDevices();
        Content = BuildLayout();
        Refresh();
    }

    private void LoadDevices() => _devices = _database.GetMedicalDevices(null, false);

    private Control BuildLayout()
    {
        var scroll = new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
        var stack = new StackPanel { Margin = new Avalonia.Thickness(24), Spacing = 16 };

        stack.Children.Add(new TextBlock { Text = "Production & Quality Suite", FontSize = 28, FontWeight = FontWeight.Bold, Foreground = Brush.Parse("#B5162B") });
        stack.Children.Add(new TextBlock { Text = "RC 4.1: ordini produzione, avanzamento, test qualità e timeline Digital Twin." });
        stack.Children.Add(BuildProductionForm());
        stack.Children.Add(BuildQualityForm());
        _message.Foreground = Brush.Parse("#B5162B");
        stack.Children.Add(_message);
        stack.Children.Add(new Border { Background = Brushes.White, CornerRadius = new Avalonia.CornerRadius(14), Padding = new Avalonia.Thickness(18), Child = _ordersPanel });
        stack.Children.Add(new Border { Background = Brushes.White, CornerRadius = new Avalonia.CornerRadius(14), Padding = new Avalonia.Thickness(18), Child = _testsPanel });

        scroll.Content = stack;
        return scroll;
    }

    private Control BuildProductionForm()
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("220,120,120,120,140,110"), RowDefinitions = new RowDefinitions("Auto,Auto") };
        AddLabel(grid, "Dispositivo", 0, 0); AddLabel(grid, "Ordine", 1, 0); AddLabel(grid, "Lotto", 2, 0); AddLabel(grid, "Pianificato", 3, 0); AddLabel(grid, "Operatore", 4, 0);
        _deviceOrder.ItemsSource = BuildDeviceItems(); _deviceOrder.SelectedIndex = _deviceOrder.ItemCount > 0 ? 0 : -1;
        _orderCode.Watermark = "PROD001"; _lot.Watermark = "LOT001"; _plannedDate.Watermark = "2026-01-01"; _operator.Watermark = "Operatore";
        AddControl(grid, _deviceOrder, 0, 1); AddControl(grid, _orderCode, 1, 1); AddControl(grid, _lot, 2, 1); AddControl(grid, _plannedDate, 3, 1); AddControl(grid, _operator, 4, 1);
        var create = new Button { Content = "Crea Ordine", Background = Brush.Parse("#B5162B"), Foreground = Brushes.White, FontWeight = FontWeight.Bold };
        create.Click += (_, _) => CreateOrder();
        AddControl(grid, create, 5, 1);
        return Card("Ordine di produzione", grid);
    }

    private Control BuildQualityForm()
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("220,110,140,120,120,120,120,130,120,100"), RowDefinitions = new RowDefinitions("Auto,Auto") };
        AddLabel(grid, "Dispositivo", 0, 0); AddLabel(grid, "Test", 1, 0); AddLabel(grid, "Checklist", 2, 0); AddLabel(grid, "Funz.", 3, 0); AddLabel(grid, "Elettrico", 4, 0); AddLabel(grid, "Conform.", 5, 0); AddLabel(grid, "Finale", 6, 0); AddLabel(grid, "Operatore", 7, 0); AddLabel(grid, "Data", 8, 0);
        _deviceTest.ItemsSource = BuildDeviceItems(); _deviceTest.SelectedIndex = _deviceTest.ItemCount > 0 ? 0 : -1;
        _testCode.Watermark = "QT001"; _checklist.Watermark = "Checklist base"; _testOperator.Watermark = "Operatore"; _testDate.Watermark = "2026-01-01";
        _functional.ItemsSource = new[] { "Conforme", "Non conforme", "Non applicabile" };
        _electrical.ItemsSource = new[] { "Conforme", "Non conforme", "Non applicabile" };
        _conformity.ItemsSource = new[] { "Conforme", "Non conforme", "In verifica" };
        _final.ItemsSource = new[] { "Conforme", "Non conforme" };
        _functional.SelectedIndex = _electrical.SelectedIndex = _conformity.SelectedIndex = _final.SelectedIndex = 0;
        AddControl(grid, _deviceTest, 0, 1); AddControl(grid, _testCode, 1, 1); AddControl(grid, _checklist, 2, 1); AddControl(grid, _functional, 3, 1); AddControl(grid, _electrical, 4, 1); AddControl(grid, _conformity, 5, 1); AddControl(grid, _final, 6, 1); AddControl(grid, _testOperator, 7, 1); AddControl(grid, _testDate, 8, 1);
        var create = new Button { Content = "Salva Test", Background = Brush.Parse("#B5162B"), Foreground = Brushes.White, FontWeight = FontWeight.Bold };
        create.Click += (_, _) => CreateTest();
        AddControl(grid, create, 9, 1);
        return Card("Controllo qualità", grid);
    }

    private Control Card(string title, Control content) => new Border
    {
        Background = Brushes.White,
        CornerRadius = new Avalonia.CornerRadius(14),
        Padding = new Avalonia.Thickness(18),
        Child = new StackPanel { Spacing = 10, Children = { new TextBlock { Text = title, FontSize = 18, FontWeight = FontWeight.Bold }, content } }
    };

    private List<string> BuildDeviceItems() => _devices.Select(d => $"{d.Id}|{d.DeviceCode} - {d.DeviceType}").ToList();

    private long SelectedDeviceId(ComboBox combo)
    {
        var text = combo.SelectedItem?.ToString() ?? "";
        var first = text.Split('|')[0];
        return long.TryParse(first, out var id) ? id : 0;
    }

    private void CreateOrder()
    {
        var order = new ProductionOrderRecord { MedicalDeviceId = SelectedDeviceId(_deviceOrder), OrderCode = _orderCode.Text ?? "", LotNumber = _lot.Text ?? "", Status = "Pianificato", PlannedDate = _plannedDate.Text ?? "", OperatorName = _operator.Text ?? "" };
        var ok = _database.CreateProductionOrder(order, _user.Username, out var error);
        _message.Text = ok ? "Ordine di produzione creato." : error;
        if (ok) Refresh();
    }

    private void CreateTest()
    {
        var test = new QualityTestRecord { MedicalDeviceId = SelectedDeviceId(_deviceTest), TestCode = _testCode.Text ?? "", ChecklistName = _checklist.Text ?? "", FunctionalResult = _functional.SelectedItem?.ToString() ?? "", ElectricalResult = _electrical.SelectedItem?.ToString() ?? "", ConformityResult = _conformity.SelectedItem?.ToString() ?? "", FinalResult = _final.SelectedItem?.ToString() ?? "", OperatorName = _testOperator.Text ?? "", TestDate = _testDate.Text ?? "" };
        var ok = _database.CreateQualityTest(test, _user.Username, out var error);
        _message.Text = ok ? "Test qualità salvato." : error;
        if (ok) Refresh();
    }

    private void Refresh()
    {
        LoadDevices();
        _deviceOrder.ItemsSource = BuildDeviceItems();
        _deviceTest.ItemsSource = BuildDeviceItems();
        RefreshOrders();
        RefreshTests();
    }

    private void RefreshOrders()
    {
        _ordersPanel.Children.Clear(); _ordersPanel.Spacing = 8;
        var rows = _database.GetProductionOrders();
        _ordersPanel.Children.Add(new TextBlock { Text = $"Ordini produzione ({rows.Count})", FontSize = 18, FontWeight = FontWeight.Bold });
        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("120,130,130,110,130,130,90,100") };
        AddHeader(header, "Ordine", 0); AddHeader(header, "Dispositivo", 1); AddHeader(header, "Tipo", 2); AddHeader(header, "Lotto", 3); AddHeader(header, "Stato", 4); AddHeader(header, "Operatore", 5); AddHeader(header, "Avvia", 6); AddHeader(header, "Completa", 7);
        _ordersPanel.Children.Add(header);
        foreach (var o in rows)
        {
            var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("120,130,130,110,130,130,90,100") };
            AddText(grid, o.OrderCode, 0); AddText(grid, o.DeviceCode, 1); AddText(grid, o.DeviceType, 2); AddText(grid, o.LotNumber, 3); AddText(grid, o.Status, 4); AddText(grid, o.OperatorName, 5);
            var start = new Button { Content = "Avvia" }; start.Click += (_, _) => { _database.AdvanceProductionOrder(o.Id, "In produzione", _user.Username); Refresh(); }; AddControl(grid, start, 6, 0);
            var complete = new Button { Content = "Completa" }; complete.Click += (_, _) => { _database.AdvanceProductionOrder(o.Id, "Completato", _user.Username); Refresh(); }; AddControl(grid, complete, 7, 0);
            _ordersPanel.Children.Add(grid);
        }
    }

    private void RefreshTests()
    {
        _testsPanel.Children.Clear(); _testsPanel.Spacing = 8;
        var rows = _database.GetQualityTests();
        _testsPanel.Children.Add(new TextBlock { Text = $"Test qualità ({rows.Count})", FontSize = 18, FontWeight = FontWeight.Bold });
        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("110,120,150,120,120,120,120,130,120") };
        AddHeader(header, "Test", 0); AddHeader(header, "Dispositivo", 1); AddHeader(header, "Checklist", 2); AddHeader(header, "Funz.", 3); AddHeader(header, "Elettrico", 4); AddHeader(header, "Conform.", 5); AddHeader(header, "Finale", 6); AddHeader(header, "Operatore", 7); AddHeader(header, "Data", 8);
        _testsPanel.Children.Add(header);
        foreach (var t in rows)
        {
            var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("110,120,150,120,120,120,120,130,120") };
            AddText(grid, t.TestCode, 0); AddText(grid, t.DeviceCode, 1); AddText(grid, t.ChecklistName, 2); AddText(grid, t.FunctionalResult, 3); AddText(grid, t.ElectricalResult, 4); AddText(grid, t.ConformityResult, 5); AddText(grid, t.FinalResult, 6); AddText(grid, t.OperatorName, 7); AddText(grid, t.TestDate, 8);
            _testsPanel.Children.Add(grid);
        }
    }

    private static void AddLabel(Grid grid, string text, int column, int row) => AddControl(grid, new TextBlock { Text = text, FontWeight = FontWeight.Bold }, column, row);
    private static void AddHeader(Grid grid, string text, int column) => AddControl(grid, new TextBlock { Text = text, FontWeight = FontWeight.Bold, Foreground = Brush.Parse("#B5162B") }, column, 0);
    private static void AddText(Grid grid, string text, int column) => AddControl(grid, new TextBlock { Text = string.IsNullOrWhiteSpace(text) ? "-" : text }, column, 0);

    private static void AddControl(Grid grid, Control control, int column, int row)
    {
        control.Margin = new Avalonia.Thickness(4);
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
