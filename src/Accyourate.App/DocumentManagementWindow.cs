using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.Models;

namespace Accyourate.App;

public sealed class DocumentManagementWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;

    private readonly TextBlock _message = new();
    private readonly StackPanel _documentsPanel = new();

    private readonly TextBox _search = new();
    private readonly TextBox _documentCode = new();
    private readonly TextBox _title = new();
    private readonly ComboBox _category = new();
    private readonly ComboBox _entityType = new();
    private readonly ComboBox _entity = new();
    private readonly TextBox _fileName = new();
    private readonly TextBox _filePath = new();
    private readonly TextBox _version = new();
    private readonly TextBox _notes = new();

    private List<MedicalDeviceRecord> _medicalDevices = new();
    private List<EmployeeRecord> _employees = new();
    private List<AssetRecord> _assets = new();

    public DocumentManagementWindow(DatabaseService database, CurrentUser user)
    {
        _database = database;
        _user = user;

        Title = "Accyourate Enterprise X - Document Management";
        Width = 1320;
        Height = 860;
        
        MinWidth = 1180;
        MinHeight = 760;
WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");

        LoadData();
        Content = BuildLayout();
        RefreshDocuments();
    }

    private void LoadData()
    {
        _medicalDevices = _database.GetMedicalDevices(null, true);
        _employees = _database.GetEmployees(null, true);
        _assets = _database.GetAssets(null, true);
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
            Text = "Document Management",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });

        stack.Children.Add(new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Text = "Versione 5.0: archivio documentale, allegati, versionamento base e collegamento al Digital Twin."
        });

        stack.Children.Add(BuildCreateForm());
        stack.Children.Add(BuildSearchCard());

        _message.Foreground = Brush.Parse("#B5162B");
        stack.Children.Add(_message);

        stack.Children.Add(Card("Documenti", _documentsPanel));

        scroll.Content = stack;
        return scroll;
    }

    private Control BuildCreateForm()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("110,180,150,150,210,160,160,90,180,100"),
            RowDefinitions = new RowDefinitions("Auto,Auto")
        };

        AddLabel(grid, "Codice", 0, 0);
        AddLabel(grid, "Titolo", 1, 0);
        AddLabel(grid, "Categoria", 2, 0);
        AddLabel(grid, "Tipo colleg.", 3, 0);
        AddLabel(grid, "Elemento", 4, 0);
        AddLabel(grid, "File", 5, 0);
        AddLabel(grid, "Percorso", 6, 0);
        AddLabel(grid, "Ver.", 7, 0);
        AddLabel(grid, "Note", 8, 0);

        _documentCode.Watermark = "DOC001";
        _title.Watermark = "Titolo";
        _category.ItemsSource = new[] { "Certificato", "Manuale", "Scheda tecnica", "Verbale", "Collaudo", "Manutenzione", "Foto", "Altro" };
        _category.SelectedIndex = 0;
        _entityType.ItemsSource = new[] { "DispositivoMedico", "Persona", "AssetIT", "Generico" };
        _entityType.SelectedIndex = 0;
        _entityType.SelectionChanged += (_, _) => RefreshEntityItems();
        _fileName.Watermark = "file.pdf";
        _filePath.Watermark = "percorso file";
        _version.Text = "1.0";
        _notes.Watermark = "Note";

        RefreshEntityItems();

        AddControl(grid, _documentCode, 0, 1);
        AddControl(grid, _title, 1, 1);
        AddControl(grid, _category, 2, 1);
        AddControl(grid, _entityType, 3, 1);
        AddControl(grid, _entity, 4, 1);
        AddControl(grid, _fileName, 5, 1);
        AddControl(grid, _filePath, 6, 1);
        AddControl(grid, _version, 7, 1);
        AddControl(grid, _notes, 8, 1);

        var create = PrimaryButton("Crea");
        create.Click += (_, _) => CreateDocument();
        AddControl(grid, create, 9, 1);

        return Card("Nuovo documento", grid);
    }

    private Control BuildSearchCard()
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,100") };
        _search.Watermark = "Cerca per codice, titolo, categoria, collegamento, file...";
        AddControl(grid, _search, 0, 0);

        var button = new Button { Content = "Cerca" };
        button.Click += (_, _) => RefreshDocuments();
        AddControl(grid, button, 1, 0);

        return Card("Ricerca", grid);
    }

    private void RefreshEntityItems()
    {
        var type = _entityType.SelectedItem?.ToString() ?? "DispositivoMedico";
        if (type == "DispositivoMedico")
            _entity.ItemsSource = _medicalDevices.Select(x => $"{x.Id}|{x.DeviceCode} - {x.DeviceType}").ToList();
        else if (type == "Persona")
            _entity.ItemsSource = _employees.Select(x => $"{x.Id}|{x.EmployeeCode} - {x.FullName}").ToList();
        else if (type == "AssetIT")
            _entity.ItemsSource = _assets.Select(x => $"{x.Id}|{x.AssetCode} - {x.Category} {x.Brand}").ToList();
        else
            _entity.ItemsSource = new[] { "0|Generico" };

        _entity.SelectedIndex = _entity.ItemCount > 0 ? 0 : -1;
    }

    private void CreateDocument()
    {
        var (entityId, entityCode) = SelectedEntity();
        var doc = new DocumentRecord
        {
            DocumentCode = _documentCode.Text ?? "",
            Title = _title.Text ?? "",
            Category = _category.SelectedItem?.ToString() ?? "",
            EntityType = _entityType.SelectedItem?.ToString() ?? "",
            EntityId = entityId == 0 ? null : entityId,
            EntityCode = entityCode,
            FileName = _fileName.Text ?? "",
            FilePath = _filePath.Text ?? "",
            Version = _version.Text ?? "1.0",
            Status = "Attivo",
            Notes = _notes.Text ?? ""
        };

        var ok = _database.CreateDocument(doc, _user.Username, out var error);
        if (!ok)
        {
            _message.Text = error;
            return;
        }

        _message.Text = "Documento creato.";
        RefreshDocuments();
    }

    private (long id, string code) SelectedEntity()
    {
        var text = _entity.SelectedItem?.ToString() ?? "";
        var parts = text.Split('|');
        if (parts.Length == 0 || !long.TryParse(parts[0], out var id))
            return (0, "");

        var code = parts.Length > 1 ? parts[1].Split(" - ")[0].Trim() : "";
        return (id, code);
    }

    private void RefreshDocuments()
    {
        _documentsPanel.Children.Clear();
        _documentsPanel.Spacing = 8;

        var rows = _database.GetDocuments(_search.Text);
        _documentsPanel.Children.Add(new TextBlock { Text = $"Documenti ({rows.Count})", FontSize = 18, FontWeight = FontWeight.Bold });

        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("100,180,130,140,130,160,80,90,90,100") };
        AddHeader(header, "Codice", 0);
        AddHeader(header, "Titolo", 1);
        AddHeader(header, "Categoria", 2);
        AddHeader(header, "Tipo", 3);
        AddHeader(header, "Elemento", 4);
        AddHeader(header, "File", 5);
        AddHeader(header, "Ver.", 6);
        AddHeader(header, "Stato", 7);
        AddHeader(header, "Genera", 8);
        AddHeader(header, "Archivia", 9);
        _documentsPanel.Children.Add(header);

        foreach (var d in rows)
        {
            var row = new Grid { ColumnDefinitions = new ColumnDefinitions("100,180,130,140,130,160,80,90,90,100") };
            AddText(row, d.DocumentCode, 0);
            AddText(row, d.Title, 1);
            AddText(row, d.Category, 2);
            AddText(row, d.EntityType, 3);
            AddText(row, d.EntityCode, 4);
            AddText(row, d.FileName, 5);
            AddText(row, d.Version, 6);
            AddText(row, d.Status, 7);

            var gen = new Button { Content = "TXT" };
            gen.Click += (_, _) =>
            {
                var path = _database.GenerateDocumentTxt(d, _user.Username);
                _message.Text = $"Documento generato: {path}";
            };
            AddControl(row, gen, 8, 0);

            var archive = new Button { Content = "Archivia" };
            archive.Click += (_, _) =>
            {
                _database.ArchiveDocument(d.Id, _user.Username);
                RefreshDocuments();
            };
            AddControl(row, archive, 9, 0);

            _documentsPanel.Children.Add(row);
        }
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
