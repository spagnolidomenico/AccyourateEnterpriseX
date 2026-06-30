using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.Models;

namespace Accyourate.App;

public sealed class LaundryMaintenanceWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;

    private readonly TextBlock _message = new();
    private readonly StackPanel _laundryPanel = new();
    private readonly StackPanel _maintenancePanel = new();

    private readonly ComboBox _laundryDevice = new();
    private readonly TextBox _cycleCode = new();
    private readonly ComboBox _program = new();
    private readonly TextBox _temperature = new();
    private readonly TextBox _washDate = new();
    private readonly TextBox _laundryOperator = new();
    private readonly ComboBox _laundryResult = new();

    private readonly ComboBox _maintenanceDevice = new();
    private readonly TextBox _maintenanceCode = new();
    private readonly ComboBox _maintenanceType = new();
    private readonly TextBox _fault = new();
    private readonly TextBox _action = new();
    private readonly TextBox _parts = new();
    private readonly ComboBox _maintenanceResult = new();
    private readonly TextBox _maintenanceOperator = new();
    private readonly TextBox _maintenanceDate = new();

    private List<MedicalDeviceRecord> _devices = new();

    public LaundryMaintenanceWindow(DatabaseService database, CurrentUser user)
    {
        _database = database;
        _user = user;

        Title = "Accyourate Enterprise X - Laundry & Maintenance";
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
            Text = "Laundry & Maintenance",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });

        stack.Children.Add(new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Text = "RC 4.3: cicli lavaggio, contatore lavaggi, manutenzioni, fuori servizio e rientro in servizio."
        });

        stack.Children.Add(BuildLaundryForm());
        stack.Children.Add(BuildMaintenanceForm());

        _message.Foreground = Brush.Parse("#B5162B");
        stack.Children.Add(_message);

        stack.Children.Add(Card("Cicli di lavaggio", _laundryPanel));
        stack.Children.Add(Card("Manutenzioni e riparazioni", _maintenancePanel));

        scroll.Content = stack;
        return scroll;
    }

    private Control BuildLaundryForm()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("220,120,160,110,120,130,130,100"),
            RowDefinitions = new RowDefinitions("Auto,Auto")
        };

        AddLabel(grid, "Dispositivo", 0, 0);
        AddLabel(grid, "Ciclo", 1, 0);
        AddLabel(grid, "Programma", 2, 0);
        AddLabel(grid, "Temp.", 3, 0);
        AddLabel(grid, "Data", 4, 0);
        AddLabel(grid, "Operatore", 5, 0);
        AddLabel(grid, "Esito", 6, 0);

        _laundryDevice.ItemsSource = DeviceItems();
        _laundryDevice.SelectedIndex = _laundryDevice.ItemCount > 0 ? 0 : -1;
        _cycleCode.Watermark = "LAV001";
        _program.ItemsSource = new[] { "Delicato", "Sanificazione", "Standard", "Ricondizionamento" };
        _program.SelectedIndex = 0;
        _temperature.Watermark = "30°C";
        _washDate.Watermark = "2026-01-01";
        _laundryOperator.Watermark = "Operatore";
        _laundryResult.ItemsSource = new[] { "Conforme", "Non conforme", "Da ritestare" };
        _laundryResult.SelectedIndex = 0;

        AddControl(grid, _laundryDevice, 0, 1);
        AddControl(grid, _cycleCode, 1, 1);
        AddControl(grid, _program, 2, 1);
        AddControl(grid, _temperature, 3, 1);
        AddControl(grid, _washDate, 4, 1);
        AddControl(grid, _laundryOperator, 5, 1);
        AddControl(grid, _laundryResult, 6, 1);

        var save = PrimaryButton("Registra");
        save.Click += (_, _) => CreateLaundry();
        AddControl(grid, save, 7, 1);

        return Card("Nuovo ciclo di lavaggio", grid);
    }

    private Control BuildMaintenanceForm()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("220,120,140,160,160,130,140,130,120,100"),
            RowDefinitions = new RowDefinitions("Auto,Auto")
        };

        AddLabel(grid, "Dispositivo", 0, 0);
        AddLabel(grid, "Codice", 1, 0);
        AddLabel(grid, "Tipo", 2, 0);
        AddLabel(grid, "Guasto", 3, 0);
        AddLabel(grid, "Intervento", 4, 0);
        AddLabel(grid, "Ricambi", 5, 0);
        AddLabel(grid, "Esito", 6, 0);
        AddLabel(grid, "Operatore", 7, 0);
        AddLabel(grid, "Data", 8, 0);

        _maintenanceDevice.ItemsSource = DeviceItems();
        _maintenanceDevice.SelectedIndex = _maintenanceDevice.ItemCount > 0 ? 0 : -1;
        _maintenanceCode.Watermark = "MAN001";
        _maintenanceType.ItemsSource = new[] { "Preventiva", "Correttiva", "Riparazione", "Ricondizionamento" };
        _maintenanceType.SelectedIndex = 0;
        _fault.Watermark = "Descrizione guasto";
        _action.Watermark = "Intervento";
        _parts.Watermark = "Ricambi";
        _maintenanceResult.ItemsSource = new[] { "Rientro in servizio", "Fuori servizio", "Riparato", "Da verificare" };
        _maintenanceResult.SelectedIndex = 0;
        _maintenanceOperator.Watermark = "Operatore";
        _maintenanceDate.Watermark = "2026-01-01";

        AddControl(grid, _maintenanceDevice, 0, 1);
        AddControl(grid, _maintenanceCode, 1, 1);
        AddControl(grid, _maintenanceType, 2, 1);
        AddControl(grid, _fault, 3, 1);
        AddControl(grid, _action, 4, 1);
        AddControl(grid, _parts, 5, 1);
        AddControl(grid, _maintenanceResult, 6, 1);
        AddControl(grid, _maintenanceOperator, 7, 1);
        AddControl(grid, _maintenanceDate, 8, 1);

        var save = PrimaryButton("Registra");
        save.Click += (_, _) => CreateMaintenance();
        AddControl(grid, save, 9, 1);

        return Card("Nuova manutenzione / riparazione", grid);
    }

    private void CreateLaundry()
    {
        var cycle = new LaundryCycleRecord
        {
            MedicalDeviceId = SelectedDeviceId(_laundryDevice),
            CycleCode = _cycleCode.Text ?? "",
            ProgramName = _program.SelectedItem?.ToString() ?? "",
            Temperature = _temperature.Text ?? "",
            WashDate = _washDate.Text ?? "",
            OperatorName = _laundryOperator.Text ?? "",
            Result = _laundryResult.SelectedItem?.ToString() ?? ""
        };

        var ok = _database.CreateLaundryCycle(cycle, _user.Username, out var error);
        _message.Text = ok ? "Ciclo di lavaggio registrato." : error;
        if (ok) Refresh();
    }

    private void CreateMaintenance()
    {
        var record = new MaintenanceRecord
        {
            MedicalDeviceId = SelectedDeviceId(_maintenanceDevice),
            MaintenanceCode = _maintenanceCode.Text ?? "",
            MaintenanceType = _maintenanceType.SelectedItem?.ToString() ?? "",
            FaultDescription = _fault.Text ?? "",
            ActionTaken = _action.Text ?? "",
            PartsReplaced = _parts.Text ?? "",
            Result = _maintenanceResult.SelectedItem?.ToString() ?? "",
            OperatorName = _maintenanceOperator.Text ?? "",
            MaintenanceDate = _maintenanceDate.Text ?? ""
        };

        var ok = _database.CreateMaintenanceRecord(record, _user.Username, out var error);
        _message.Text = ok ? "Manutenzione registrata." : error;
        if (ok) Refresh();
    }

    private void Refresh()
    {
        LoadData();
        _laundryDevice.ItemsSource = DeviceItems();
        _maintenanceDevice.ItemsSource = DeviceItems();
        RefreshLaundry();
        RefreshMaintenance();
    }

    private void RefreshLaundry()
    {
        _laundryPanel.Children.Clear();
        _laundryPanel.Spacing = 8;

        var rows = _database.GetLaundryCycles();
        _laundryPanel.Children.Add(new TextBlock { Text = $"Lavaggi ({rows.Count})", FontSize = 18, FontWeight = FontWeight.Bold });

        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("120,130,160,100,120,130,120,110") };
        AddHeader(header, "Ciclo", 0);
        AddHeader(header, "Dispositivo", 1);
        AddHeader(header, "Programma", 2);
        AddHeader(header, "Temp.", 3);
        AddHeader(header, "Data", 4);
        AddHeader(header, "Operatore", 5);
        AddHeader(header, "Esito", 6);
        AddHeader(header, "Lavaggi", 7);
        _laundryPanel.Children.Add(header);

        foreach (var l in rows)
        {
            var row = new Grid { ColumnDefinitions = new ColumnDefinitions("120,130,160,100,120,130,120,110") };
            AddText(row, l.CycleCode, 0);
            AddText(row, l.DeviceCode, 1);
            AddText(row, l.ProgramName, 2);
            AddText(row, l.Temperature, 3);
            AddText(row, l.WashDate, 4);
            AddText(row, l.OperatorName, 5);
            AddText(row, l.Result, 6);
            AddText(row, l.WashCountAfter.ToString(), 7);
            _laundryPanel.Children.Add(row);
        }
    }

    private void RefreshMaintenance()
    {
        _maintenancePanel.Children.Clear();
        _maintenancePanel.Spacing = 8;

        var rows = _database.GetMaintenanceRecords();
        _maintenancePanel.Children.Add(new TextBlock { Text = $"Manutenzioni ({rows.Count})", FontSize = 18, FontWeight = FontWeight.Bold });

        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("120,130,120,170,170,130,140,120") };
        AddHeader(header, "Codice", 0);
        AddHeader(header, "Dispositivo", 1);
        AddHeader(header, "Tipo", 2);
        AddHeader(header, "Guasto", 3);
        AddHeader(header, "Intervento", 4);
        AddHeader(header, "Ricambi", 5);
        AddHeader(header, "Esito", 6);
        AddHeader(header, "Data", 7);
        _maintenancePanel.Children.Add(header);

        foreach (var m in rows)
        {
            var row = new Grid { ColumnDefinitions = new ColumnDefinitions("120,130,120,170,170,130,140,120") };
            AddText(row, m.MaintenanceCode, 0);
            AddText(row, m.DeviceCode, 1);
            AddText(row, m.MaintenanceType, 2);
            AddText(row, m.FaultDescription, 3);
            AddText(row, m.ActionTaken, 4);
            AddText(row, m.PartsReplaced, 5);
            AddText(row, m.Result, 6);
            AddText(row, m.MaintenanceDate, 7);
            _maintenancePanel.Children.Add(row);
        }
    }

    private List<string> DeviceItems() => _devices.Select(d => $"{d.Id}|{d.DeviceCode} - {d.DeviceType}").ToList();

    private static long SelectedDeviceId(ComboBox combo)
    {
        var text = combo.SelectedItem?.ToString() ?? "";
        var first = text.Split('|')[0];
        return long.TryParse(first, out var id) ? id : 0;
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
