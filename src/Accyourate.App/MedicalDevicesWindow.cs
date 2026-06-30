using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.Models;

namespace Accyourate.App;

public sealed class MedicalDevicesWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;

    private readonly StackPanel _rowsPanel = new();
    private readonly TextBlock _message = new();
    private readonly TextBox _search = new();
    private readonly CheckBox _includeArchived = new();

    private readonly TextBox _code = new();
    private readonly ComboBox _type = new();
    private readonly TextBox _model = new();
    private readonly TextBox _serial = new();
    private readonly TextBox _lot = new();
    private readonly TextBox _rfid = new();
    private readonly TextBox _productionDate = new();
    private readonly TextBox _testDate = new();
    private readonly TextBox _notes = new();

    public MedicalDevicesWindow(DatabaseService database, CurrentUser user)
    {
        _database = database;
        _user = user;

        Title = "Accyourate Enterprise X - Medical Device Suite";
        Width = 1320;
        Height = 860;
        
        MinWidth = 1180;
        MinHeight = 760;
WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");

        Content = BuildLayout();
        RefreshRows();
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
            Text = "Medical Device Suite",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });

        stack.Children.Add(new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Text = "RC 4.0: Digital Twin base per Dispositivi Medici, Control Unit e Capi Tessili."
        });

        stack.Children.Add(BuildFormCard());
        stack.Children.Add(BuildSearchCard());

        _message.Foreground = Brush.Parse("#B5162B");
        stack.Children.Add(_message);

        stack.Children.Add(new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(18),
            Child = _rowsPanel
        });

        scroll.Content = stack;
        return scroll;
    }

    private Control BuildFormCard()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("110,150,130,140,120,130,120,120,160,95"),
            RowDefinitions = new RowDefinitions("Auto,Auto")
        };

        AddLabel(grid, "Codice", 0, 0);
        AddLabel(grid, "Tipo", 1, 0);
        AddLabel(grid, "Modello", 2, 0);
        AddLabel(grid, "Seriale", 3, 0);
        AddLabel(grid, "Lotto", 4, 0);
        AddLabel(grid, "RFID", 5, 0);
        AddLabel(grid, "Produzione", 6, 0);
        AddLabel(grid, "Collaudo", 7, 0);
        AddLabel(grid, "Note", 8, 0);

        _code.Watermark = "MED001";
        _type.ItemsSource = new[] { "Control Unit", "Top", "T-Shirt", "Gilet", "Fascia", "Kit", "Accessorio" };
        _type.SelectedIndex = 0;
        _model.Watermark = "Modello";
        _serial.Watermark = "Seriale";
        _lot.Watermark = "Lotto";
        _rfid.Watermark = "RFID";
        _productionDate.Watermark = "2026-01-01";
        _testDate.Watermark = "2026-01-02";
        _notes.Watermark = "Note";

        AddControl(grid, _code, 0, 1);
        AddControl(grid, _type, 1, 1);
        AddControl(grid, _model, 2, 1);
        AddControl(grid, _serial, 3, 1);
        AddControl(grid, _lot, 4, 1);
        AddControl(grid, _rfid, 5, 1);
        AddControl(grid, _productionDate, 6, 1);
        AddControl(grid, _testDate, 7, 1);
        AddControl(grid, _notes, 8, 1);

        var create = new Button
        {
            Content = "Crea",
            Background = Brush.Parse("#B5162B"),
            Foreground = Brushes.White,
            FontWeight = FontWeight.Bold,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom
        };
        create.Click += (_, _) => CreateDevice();
        AddControl(grid, create, 9, 1);

        return new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(18),
            Child = grid
        };
    }

    private Control BuildSearchCard()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,180,100,130")
        };

        _search.Watermark = "Cerca per codice, tipo, seriale, lotto, RFID, stato...";
        AddControl(grid, _search, 0, 0);

        _includeArchived.Content = "Includi archiviati";
        AddControl(grid, _includeArchived, 1, 0);

        var searchButton = new Button { Content = "Cerca" };
        searchButton.Click += (_, _) => RefreshRows();
        AddControl(grid, searchButton, 2, 0);

        var exportButton = new Button { Content = "Esporta CSV" };
        exportButton.Click += (_, _) => ExportCsv();
        AddControl(grid, exportButton, 3, 0);

        return new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(14),
            Child = grid
        };
    }

    private void CreateDevice()
    {
        var device = new MedicalDeviceRecord
        {
            DeviceCode = _code.Text ?? "",
            DeviceType = _type.SelectedItem?.ToString() ?? "",
            Model = _model.Text ?? "",
            SerialNumber = _serial.Text ?? "",
            LotNumber = _lot.Text ?? "",
            RfidCode = _rfid.Text ?? "",
            QrCode = _code.Text ?? "",
            Status = "Produzione",
            ProductionDate = _productionDate.Text ?? "",
            TestDate = _testDate.Text ?? "",
            Notes = _notes.Text ?? ""
        };

        var ok = _database.CreateMedicalDevice(device, _user.Username, out var error);
        if (!ok)
        {
            _message.Text = error;
            return;
        }

        _message.Text = "Dispositivo medico creato correttamente.";
        ClearForm();
        RefreshRows();
    }

    private void ClearForm()
    {
        _code.Text = "";
        _model.Text = "";
        _serial.Text = "";
        _lot.Text = "";
        _rfid.Text = "";
        _productionDate.Text = "";
        _testDate.Text = "";
        _notes.Text = "";
    }

    private void RefreshRows()
    {
        _rowsPanel.Children.Clear();
        _rowsPanel.Spacing = 8;

        var rows = _database.GetMedicalDevices(_search.Text, _includeArchived.IsChecked == true);

        _rowsPanel.Children.Add(new TextBlock
        {
            Text = $"Dispositivi Medici ({rows.Count})",
            FontSize = 18,
            FontWeight = FontWeight.Bold
        });

        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("90,115,105,120,95,110,90,90,90,90,90,90")
        };

        AddHeader(header, "Codice", 0);
        AddHeader(header, "Tipo", 1);
        AddHeader(header, "Modello", 2);
        AddHeader(header, "Seriale", 3);
        AddHeader(header, "Lotto", 4);
        AddHeader(header, "RFID", 5);
        AddHeader(header, "Stato", 6);
        AddHeader(header, "Twin", 7);
        AddHeader(header, "CU", 8);
        AddHeader(header, "Tessile", 9);
        AddHeader(header, "Workflow", 10);
        AddHeader(header, "Archivio", 11);

        _rowsPanel.Children.Add(header);

        foreach (var d in rows)
            _rowsPanel.Children.Add(BuildRow(d));
    }

    private Control BuildRow(MedicalDeviceRecord d)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("90,115,105,120,95,110,90,90,90,90,90,90"),
            Margin = new Thickness(0, 4)
        };

        AddText(grid, d.DeviceCode, 0);
        AddText(grid, d.DeviceType, 1);
        AddText(grid, d.Model, 2);
        AddText(grid, d.SerialNumber, 3);
        AddText(grid, d.LotNumber, 4);
        AddText(grid, d.RfidCode, 5);
        AddText(grid, d.IsArchived ? "Archiv." : d.Status, 6);

        var twin = new Button { Content = "Apri" };
        twin.Click += (_, _) => new MedicalDeviceTwinWindow(_database, d).Show();
        AddControl(grid, twin, 7, 0);

        var cu = new Button { Content = "CU" };
        cu.Click += (_, _) => new ControlUnitWindow(_database, _user, d).Show();
        AddControl(grid, cu, 8, 0);

        var textile = new Button { Content = "Capo" };
        textile.Click += (_, _) => new TextileItemWindow(_database, _user, d).Show();
        AddControl(grid, textile, 9, 0);

        var wf = new Button { Content = "Collaudo" };
        wf.Click += (_, _) =>
        {
            _database.ChangeMedicalDeviceStatus(d.Id, "Collaudato", "Collaudo eseguito da Medical Device Suite", _user.Username);
            RefreshRows();
        };
        AddControl(grid, wf, 10, 0);

        var archive = new Button { Content = d.IsArchived ? "Ripristina" : "Archivia" };
        archive.Click += (_, _) =>
        {
            _database.ArchiveMedicalDevice(d.Id, !d.IsArchived, _user.Username);
            RefreshRows();
        };
        AddControl(grid, archive, 11, 0);

        return grid;
    }

    private void ExportCsv()
    {
        var exportsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Accyourate Enterprise X", "exports");
        Directory.CreateDirectory(exportsDir);
        var path = Path.Combine(exportsDir, $"medical_devices_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

        _database.ExportMedicalDevicesCsv(path, _search.Text, _includeArchived.IsChecked == true, _user.Username);
        _message.Text = $"Export creato: {path}";
    }

    private static void AddLabel(Grid grid, string text, int column, int row)
    {
        AddControl(grid, new TextBlock { Text = text, FontWeight = FontWeight.Bold, Margin = new Thickness(4) }, column, row);
    }

    private static void AddHeader(Grid grid, string text, int column)
    {
        AddControl(grid, new TextBlock { Text = text, FontWeight = FontWeight.Bold, Foreground = Brush.Parse("#B5162B"), Margin = new Thickness(4) }, column, 0);
    }

    private static void AddText(Grid grid, string text, int column)
    {
        AddControl(grid, new TextBlock { Text = string.IsNullOrWhiteSpace(text) ? "-" : text, Margin = new Thickness(4) }, column, 0);
    }

    private static void AddControl(Grid grid, Control control, int column, int row)
    {
        control.Margin = new Thickness(4);
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
