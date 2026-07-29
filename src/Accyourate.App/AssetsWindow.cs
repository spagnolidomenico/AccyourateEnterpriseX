using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.Models;
using Accyourate.App.UIFramework.EnterpriseTable;

namespace Accyourate.App;

public sealed class AssetsWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;

    private readonly StackPanel _rowsPanel = new();
    private readonly TextBlock _message = new();
    private readonly TextBox _search = new();
    private readonly CheckBox _includeArchived = new();

    private readonly TextBox _assetCode = new();
    private readonly ComboBox _category = new();
    private readonly TextBox _brand = new();
    private readonly TextBox _model = new();
    private readonly TextBox _serial = new();
    private readonly TextBox _os = new();
    private readonly ComboBox _assignedEmployee = new();
    private readonly TextBox _purchaseDate = new();
    private readonly TextBox _warrantyEnd = new();
    private readonly TextBox _notes = new();

    private List<EmployeeRecord> _employees = new();

    public AssetsWindow(DatabaseService database, CurrentUser user)
    {
        _database = database;
        _user = user;

        Title = "Accyourate Enterprise X - Asset IT";
        Width = 1320;
        Height = 860;
        
        MinWidth = 1180;
        MinHeight = 760;
WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");

        LoadEmployees();
        Content = BuildLayout();
        RefreshRows();
    }

    private void LoadEmployees()
    {
        _employees = _database.GetEmployees(null, false);
    }

    private Control BuildLayout()
    {
        var root = new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
        var stack = new StackPanel { Margin = new Thickness(24), Spacing = 16 };

        stack.Children.Add(new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Text = "Asset IT",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });

        stack.Children.Add(new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Text = "Developer 2.0: inventario IT, assegnazione a dipendente, ricerca, export e archiviazione."
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

        root.Content = stack;
        return root;
    }

    private Control BuildFormCard()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("110,130,120,140,150,130,180,120,120,160,95"),
            RowDefinitions = new RowDefinitions("Auto,Auto")
        };

        AddLabel(grid, "Codice", 0, 0);
        AddLabel(grid, "Categoria", 1, 0);
        AddLabel(grid, "Marca", 2, 0);
        AddLabel(grid, "Modello", 3, 0);
        AddLabel(grid, "Seriale", 4, 0);
        AddLabel(grid, "OS", 5, 0);
        AddLabel(grid, "Assegnato a", 6, 0);
        AddLabel(grid, "Acquisto", 7, 0);
        AddLabel(grid, "Garanzia", 8, 0);
        AddLabel(grid, "Note", 9, 0);

        _assetCode.Watermark = "IT001";
        _category.ItemsSource = new[] { "PC Desktop", "Notebook", "Mac", "MacBook", "Smartphone", "Tablet", "Monitor", "Stampante", "Server", "Switch", "Firewall", "Access Point", "Altro" };
        _category.SelectedIndex = 1;
        _brand.Watermark = "Dell";
        _model.Watermark = "Latitude";
        _serial.Watermark = "SN...";
        _os.Watermark = "Windows 11";
        _purchaseDate.Watermark = "2026-01-01";
        _warrantyEnd.Watermark = "2029-01-01";
        _notes.Watermark = "Note";

        _assignedEmployee.ItemsSource = BuildEmployeeItems();
        _assignedEmployee.SelectedIndex = 0;

        AddControl(grid, _assetCode, 0, 1);
        AddControl(grid, _category, 1, 1);
        AddControl(grid, _brand, 2, 1);
        AddControl(grid, _model, 3, 1);
        AddControl(grid, _serial, 4, 1);
        AddControl(grid, _os, 5, 1);
        AddControl(grid, _assignedEmployee, 6, 1);
        AddControl(grid, _purchaseDate, 7, 1);
        AddControl(grid, _warrantyEnd, 8, 1);
        AddControl(grid, _notes, 9, 1);

        var create = new Button
        {
            Content = "Crea",
            Background = Brush.Parse("#B5162B"),
            Foreground = Brushes.White,
            FontWeight = FontWeight.Bold,
            VerticalAlignment = VerticalAlignment.Bottom
        };
        create.Click += (_, _) => CreateAsset();
        AddControl(grid, create, 10, 1);

        return new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(18),
            Child = grid
        };
    }

    private List<string> BuildEmployeeItems()
    {
        var items = new List<string> { "Non assegnato" };
        items.AddRange(_employees.Select(e => $"{e.Id}|{e.EmployeeCode} - {e.FullName}"));
        return items;
    }

    private long? SelectedEmployeeId()
    {
        var text = _assignedEmployee.SelectedItem?.ToString() ?? "";
        if (string.IsNullOrWhiteSpace(text) || text == "Non assegnato")
            return null;

        var first = text.Split('|')[0];
        return long.TryParse(first, out var id) ? id : null;
    }

    private Control BuildSearchCard()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,180,100,130")
        };

        _search.Watermark = "Cerca per codice, categoria, marca, modello, seriale, dipendente...";
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

    private void CreateAsset()
    {
        var employeeId = SelectedEmployeeId();

        var asset = new AssetRecord
        {
            AssetCode = _assetCode.Text ?? "",
            Category = _category.SelectedItem?.ToString() ?? "",
            Brand = _brand.Text ?? "",
            Model = _model.Text ?? "",
            SerialNumber = _serial.Text ?? "",
            OperatingSystem = _os.Text ?? "",
            Status = employeeId.HasValue ? "Assegnato" : "Disponibile",
            AssignedEmployeeId = employeeId,
            PurchaseDate = _purchaseDate.Text ?? "",
            WarrantyEnd = _warrantyEnd.Text ?? "",
            Notes = _notes.Text ?? ""
        };

        var ok = _database.CreateAsset(asset, _user.Username, out var error);
        if (!ok)
        {
            _message.Text = error;
            return;
        }

        _message.Text = "Asset creato correttamente.";
        ClearForm();
        RefreshRows();
    }

    private void ClearForm()
    {
        _assetCode.Text = "";
        _brand.Text = "";
        _model.Text = "";
        _serial.Text = "";
        _os.Text = "";
        _purchaseDate.Text = "";
        _warrantyEnd.Text = "";
        _notes.Text = "";
        _assignedEmployee.SelectedIndex = 0;
    }

    private void RefreshRows()
    {
        LoadEmployees();
        _rowsPanel.Children.Clear();
        _rowsPanel.Spacing = 8;

        var rows = _database.GetAssets(_search.Text, _includeArchived.IsChecked == true);

        _rowsPanel.Children.Add(new TextBlock
        {
            Text = $"Asset IT ({rows.Count})",
            FontSize = 18,
            FontWeight = FontWeight.Bold
        });

        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions(AxTableLayout.AssetColumns)
        };

        AddHeader(header, "Codice", 0);
        AddHeader(header, "Categoria", 1);
        AddHeader(header, "Marca", 2);
        AddHeader(header, "Modello", 3);
        AddHeader(header, "Seriale", 4);
        AddHeader(header, "OS", 5);
        AddHeader(header, "Stato", 6, true);
        AddHeader(header, "Assegnato", 7);
        AddHeader(header, "QR", 8, true);
        AddHeader(header, "Archivio", 9, true);
        AddHeader(header, "Rientro", 10, true);
        AddHeader(header, "Workflow", 11, true);
        _rowsPanel.Children.Add(header);

        foreach (var asset in rows)
            _rowsPanel.Children.Add(BuildRow(asset));
    }

    private Control BuildRow(AssetRecord asset)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions(AxTableLayout.AssetColumns),
            Margin = new Thickness(0, 4),
            MinHeight = 44
        };

        AddText(grid, asset.AssetCode, 0);
        AddText(grid, asset.Category, 1);
        AddText(grid, asset.Brand, 2);
        AddText(grid, asset.Model, 3);
        AddText(grid, asset.SerialNumber, 4);
        AddText(grid, asset.OperatingSystem, 5);
        AddText(grid, asset.IsArchived ? "Archiv." : asset.Status, 6, true);
        AddText(grid, string.IsNullOrWhiteSpace(asset.AssignedEmployeeName) ? "-" : asset.AssignedEmployeeName, 7);

        var qr = AxTableLayout.ActionButton("QR");
        qr.Click += (_, _) => CreateQrPayload(asset);
        AddControl(grid, qr, 8, 0);

        var archive = AxTableLayout.ActionButton(asset.IsArchived ? "Ripristina" : "Archivia");
        archive.Click += (_, _) =>
        {
            _database.ArchiveAsset(asset.Id, !asset.IsArchived, _user.Username);
            RefreshRows();
        };
        AddControl(grid, archive, 9, 0);

        var ret = AxTableLayout.ActionButton("Rientro");
        ret.Click += (_, _) =>
        {
            _database.AssignAsset(asset.Id, null, _user.Username);
            RefreshRows();
        };
        AddControl(grid, ret, 10, 0);

        var workflow = AxTableLayout.ActionButton("Manut.");
        workflow.Click += (_, _) =>
        {
            _database.ChangeAssetStatus(asset.Id, "Manutenzione", "Invio in manutenzione da modulo Asset IT", _user.Username);
            RefreshRows();
        };
        AddControl(grid, workflow, 11, 0);

        return grid;
    }

    private void CreateQrPayload(AssetRecord asset)
    {
        var exportsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Accyourate Enterprise X", "exports");
        Directory.CreateDirectory(exportsDir);
        var path = Path.Combine(exportsDir, $"qr_asset_{asset.AssetCode}_{DateTime.Now:yyyyMMdd_HHmmss}.json");

        var json = $$"""
{
  "type": "AssetIT",
  "code": "{{asset.AssetCode}}",
  "category": "{{asset.Category}}",
  "brand": "{{asset.Brand}}",
  "model": "{{asset.Model}}",
  "serial": "{{asset.SerialNumber}}",
  "status": "{{asset.Status}}",
  "assignedTo": "{{asset.AssignedEmployeeName}}",
  "generatedAt": "{{DateTime.UtcNow:O}}"
}
""";
        File.WriteAllText(path, json);
        _message.Text = $"Payload QR creato: {path}";
    }

    private void ExportCsv()
    {
        var exportsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Accyourate Enterprise X", "exports");
        Directory.CreateDirectory(exportsDir);
        var path = Path.Combine(exportsDir, $"asset_it_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

        _database.ExportAssetsCsv(path, _search.Text, _includeArchived.IsChecked == true, _user.Username);
        _message.Text = $"Export creato: {path}";
    }

    private static void AddLabel(Grid grid, string text, int column, int row)
    {
        var label = new TextBlock { Text = text, FontWeight = FontWeight.Bold, Margin = new Thickness(4) };
        AddControl(grid, label, column, row);
    }

    private static void AddHeader(Grid grid, string text, int column, bool centered = false)
    {
        var label = AxTableLayout.Header(text, centered);
        AddControl(grid, label, column, 0);
    }

    private static void AddText(Grid grid, string text, int column, bool centered = false)
    {
        var block = AxTableLayout.CellText(text, centered);
        AddControl(grid, block, column, 0);
    }

    private static void AddControl(Grid grid, Control control, int column, int row)
    {
        control.Margin = new Thickness(4);
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
