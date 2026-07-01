using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Models;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class AssetEditDialog : Window
{
    private readonly Asset _asset;

    private readonly TextBox _assetCode = new();
    private readonly ComboBox _category = new();
    private readonly TextBox _manufacturer = new();
    private readonly TextBox _model = new();
    private readonly TextBox _serialNumber = new();
    private readonly TextBox _assetTag = new();
    private readonly ComboBox _status = new();
    private readonly TextBox _purchaseDate = new();
    private readonly TextBox _warrantyEndDate = new();
    private readonly TextBox _operatingSystem = new();
    private readonly CheckBox _bitLocker = new();
    private readonly TextBox _notes = new();
    private readonly TextBlock _validation = new();

    public AssetEditDialog(Asset? asset = null)
    {
        _asset = Clone(asset) ?? new Asset
        {
            Category = "Notebook",
            Status = "Da verificare",
            PurchaseDate = DateTime.Now.ToString("yyyy-MM-dd"),
            WarrantyEndDate = DateTime.Now.AddYears(2).ToString("yyyy-MM-dd")
        };

        Title = _asset.Id == 0 ? "Nuovo Asset" : $"Modifica {_asset.AssetCode}";
        Width = 780;
        Height = 760;
        MinWidth = 680;
        MinHeight = 640;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();

        LoadAsset();
    }

    private Control BuildLayout()
    {
        var root = new DockPanel();

        var header = new StackPanel
        {
            Margin = new Thickness(24, 20, 24, 12),
            Spacing = 4
        };

        header.Children.Add(new TextBlock
        {
            Text = _asset.Id == 0 ? "Nuovo Asset" : "Modifica Asset",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        header.Children.Add(new TextBlock
        {
            Text = "Compila i dati principali del bene aziendale.",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });

        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        var footer = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,120,120"),
            Margin = new Thickness(24, 12, 24, 20)
        };

        _validation.Foreground = UiTokens.Brush(UiTokens.Danger);
        _validation.TextWrapping = TextWrapping.Wrap;
        Add(footer, _validation, 0, 0);

        var cancel = new Button
        {
            Content = "Annulla",
            Background = UiTokens.Brush(UiTokens.Surface),
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            Padding = new Thickness(14, 10),
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(8, 0)
        };
        cancel.Click += (_, _) => Close(null);
        Add(footer, cancel, 1, 0);

        var save = new Button
        {
            Content = "Salva",
            Background = UiTokens.Brush(UiTokens.BrandBlue),
            Foreground = Brushes.White,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(14, 10),
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(8, 0)
        };
        save.Click += (_, _) => Save();
        Add(footer, save, 2, 0);

        DockPanel.SetDock(footer, Dock.Bottom);
        root.Children.Add(footer);

        var form = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto,Auto,Auto,Auto"),
            Margin = new Thickness(24, 0, 24, 12)
        };

        _category.ItemsSource = new[] { "Desktop PC", "Notebook", "Mac", "Stampante", "Smartphone", "Tablet", "Monitor", "Accessorio", "Licenza software", "Dispositivo medicale", "Altro" };
        _status.ItemsSource = new[] { "Attivo", "Assegnato", "Disponibile", "In manutenzione", "Dismesso", "Smarrito", "Da verificare" };

        Add(form, Field("Codice Asset *", _assetCode), 0, 0);
        Add(form, Field("Categoria *", _category), 1, 0);
        Add(form, Field("Produttore *", _manufacturer), 0, 1);
        Add(form, Field("Modello *", _model), 1, 1);
        Add(form, Field("Numero di serie", _serialNumber), 0, 2);
        Add(form, Field("Asset Tag", _assetTag), 1, 2);
        Add(form, Field("Stato *", _status), 0, 3);
        Add(form, Field("Sistema operativo", _operatingSystem), 1, 3);
        Add(form, Field("Data acquisto", _purchaseDate), 0, 4);
        Add(form, Field("Fine garanzia", _warrantyEndDate), 1, 4);
        Add(form, Field("BitLocker", _bitLocker), 0, 5);

        _notes.AcceptsReturn = true;
        _notes.Height = 100;
        _notes.TextWrapping = TextWrapping.Wrap;
        var notesField = Field("Note", _notes);
        Grid.SetColumn(notesField, 0);
        Grid.SetColumnSpan(notesField, 2);
        Grid.SetRow(notesField, 6);
        form.Children.Add(notesField);

        root.Children.Add(new ScrollViewer
        {
            Content = form,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        });

        return root;
    }

    private void LoadAsset()
    {
        _assetCode.Text = _asset.AssetCode;
        _manufacturer.Text = _asset.Manufacturer;
        _model.Text = _asset.Model;
        _serialNumber.Text = _asset.SerialNumber;
        _assetTag.Text = _asset.AssetTag;
        _purchaseDate.Text = _asset.PurchaseDate;
        _warrantyEndDate.Text = _asset.WarrantyEndDate;
        _operatingSystem.Text = _asset.OperatingSystem;
        _bitLocker.IsChecked = _asset.BitLockerEnabled;
        _notes.Text = _asset.Notes;

        _category.SelectedItem = string.IsNullOrWhiteSpace(_asset.Category) ? "Notebook" : _asset.Category;
        _status.SelectedItem = string.IsNullOrWhiteSpace(_asset.Status) ? "Da verificare" : _asset.Status;
    }

    private void Save()
    {
        _validation.Text = "";

        var assetCode = (_assetCode.Text ?? string.Empty).Trim();
        var category = _category.SelectedItem?.ToString() ?? string.Empty;
        var manufacturer = (_manufacturer.Text ?? string.Empty).Trim();
        var model = (_model.Text ?? string.Empty).Trim();
        var status = _status.SelectedItem?.ToString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(assetCode))
        {
            _validation.Text = "Il codice asset è obbligatorio.";
            return;
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            _validation.Text = "La categoria è obbligatoria.";
            return;
        }

        if (string.IsNullOrWhiteSpace(manufacturer))
        {
            _validation.Text = "Il produttore è obbligatorio.";
            return;
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            _validation.Text = "Il modello è obbligatorio.";
            return;
        }

        if (string.IsNullOrWhiteSpace(status))
        {
            _validation.Text = "Lo stato è obbligatorio.";
            return;
        }

        _asset.AssetCode = assetCode;
        _asset.Category = category;
        _asset.Manufacturer = manufacturer;
        _asset.Model = model;
        _asset.SerialNumber = (_serialNumber.Text ?? string.Empty).Trim();
        _asset.AssetTag = (_assetTag.Text ?? string.Empty).Trim();
        _asset.Status = status;
        _asset.PurchaseDate = (_purchaseDate.Text ?? string.Empty).Trim();
        _asset.WarrantyEndDate = (_warrantyEndDate.Text ?? string.Empty).Trim();
        _asset.OperatingSystem = (_operatingSystem.Text ?? string.Empty).Trim();
        _asset.BitLockerEnabled = _bitLocker.IsChecked == true;
        _asset.Notes = (_notes.Text ?? string.Empty).Trim();

        Close(_asset);
    }

    private static StackPanel Field(string label, Control input)
    {
        input.Margin = new Thickness(0, 6, 0, 0);

        return new StackPanel
        {
            Margin = new Thickness(8, 8),
            Spacing = 2,
            Children =
            {
                new TextBlock
                {
                    Text = label,
                    FontSize = 12,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = UiTokens.Brush(UiTokens.TextSecondary)
                },
                input
            }
        };
    }

    private static Asset? Clone(Asset? asset)
    {
        if (asset is null)
            return null;

        return new Asset
        {
            Id = asset.Id,
            AssetCode = asset.AssetCode,
            Category = asset.Category,
            Manufacturer = asset.Manufacturer,
            Model = asset.Model,
            SerialNumber = asset.SerialNumber,
            AssetTag = asset.AssetTag,
            Status = asset.Status,
            PurchaseDate = asset.PurchaseDate,
            WarrantyEndDate = asset.WarrantyEndDate,
            OperatingSystem = asset.OperatingSystem,
            BitLockerEnabled = asset.BitLockerEnabled,
            Notes = asset.Notes,
            CreatedAt = asset.CreatedAt,
            UpdatedAt = asset.UpdatedAt
        };
    }

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
