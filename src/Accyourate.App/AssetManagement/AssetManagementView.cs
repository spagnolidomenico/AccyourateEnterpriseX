using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Models;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.Tokens;
using Accyourate.App.UIFramework.Controls;

namespace Accyourate.App.AssetManagement;

public sealed class AssetManagementView : UserControl
{
    private readonly AssetService _service = new();

    private readonly TextBox _search = new();
    private readonly ComboBox _category = new();
    private readonly ComboBox _status = new();
    private readonly StackPanel _rows = new();
    private readonly StackPanel _kpis = new();
    private readonly ContentControl _details = new();
    private readonly TextBlock _message = new();

    private IReadOnlyList<Asset> _assets = Array.Empty<Asset>();
    private Asset? _selected;

    public AssetManagementView()
    {
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();
        Load();
    }

    private Control BuildLayout()
    {
        var root = new DockPanel();

        var header = new StackPanel
        {
            Margin = new Thickness(24, 20, 24, 12),
            Spacing = 8
        };

        header.Children.Add(new TextBlock
        {
            Text = "Asset Management",
            FontSize = 32,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        header.Children.Add(new TextBlock
        {
            Text = "Inventario dispositivi, assegnazioni, stato operativo e garanzie.",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap
        });

        _kpis.Orientation = Avalonia.Layout.Orientation.Horizontal;
        _kpis.Spacing = 12;
        header.Children.Add(_kpis);

        _message.TextWrapping = TextWrapping.Wrap;
        _message.Foreground = UiTokens.Brush(UiTokens.BrandBlue);
        header.Children.Add(_message);

        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        var toolbar = new Grid
        {
            Margin = new Thickness(24, 0, 24, 16),
            ColumnDefinitions = new ColumnDefinitions("*,170,170,120,120,130,130")
        };

        _search.Watermark = "Cerca per codice, categoria, modello, seriale, stato...";
        _search.TextChanged += (_, _) => RefreshRows();

        _category.ItemsSource = new[] { "Tutte", "Desktop PC", "Notebook", "Mac", "Stampante", "Smartphone", "Tablet", "Monitor", "Accessorio", "Licenza software", "Dispositivo medicale", "Altro" };
        _category.SelectedIndex = 0;
        _category.SelectionChanged += (_, _) => RefreshRows();

        _status.ItemsSource = new[] { "Tutti", "Attivo", "Assegnato", "Disponibile", "In manutenzione", "Dismesso", "Smarrito", "Da verificare" };
        _status.SelectedIndex = 0;
        _status.SelectionChanged += (_, _) => RefreshRows();

        Add(toolbar, _search, 0, 0);
        Add(toolbar, _category, 1, 0);
        Add(toolbar, _status, 2, 0);
        Add(toolbar, ToolbarButton("↻ Aggiorna", "Ricarica asset", Load), 3, 0);
        Add(toolbar, ToolbarButton("+ Nuovo", "Crea un nuovo asset", OpenNewAsset), 4, 0);
        Add(toolbar, ToolbarButton("Importa Excel", "Prossimo sprint: importazione Excel"), 5, 0);
        Add(toolbar, ToolbarButton("Esporta Excel", "Prossimo sprint: esportazione Excel"), 6, 0);

        DockPanel.SetDock(toolbar, Dock.Top);
        root.Children.Add(toolbar);

        var content = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,380"),
            RowDefinitions = new RowDefinitions("*"),
            Margin = new Thickness(24, 0, 24, 24)
        };

        var list = new DockPanel();

        var tableHeader = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("100,130,160,*,130,120"),
            Margin = new Thickness(0, 0, 0, 8)
        };

        Add(tableHeader, Header("Codice"), 0, 0);
        Add(tableHeader, Header("Categoria"), 1, 0);
        Add(tableHeader, Header("Produttore"), 2, 0);
        Add(tableHeader, Header("Modello"), 3, 0);
        Add(tableHeader, Header("Stato"), 4, 0);
        Add(tableHeader, Header("Garanzia"), 5, 0);

        DockPanel.SetDock(tableHeader, Dock.Top);
        list.Children.Add(tableHeader);

        list.Children.Add(new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            CornerRadius = new CornerRadius(18),
            Padding = new Thickness(8),
            Child = new ScrollViewer
            {
                Content = _rows,
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
            }
        });

        Add(content, list, 0, 0);

        _details.Content = EmptyDetails();

        var detailsHost = new ScrollViewer
        {
            Content = _details,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
        };

        Add(content, detailsHost, 1, 0);

        root.Children.Add(content);
        return root;
    }

    private void Load()
    {
        var keepCode = _selected?.AssetCode;

        _assets = _service.GetAssets();
        RefreshKpis();
        RefreshRows();

        _selected = !string.IsNullOrWhiteSpace(keepCode)
            ? _assets.FirstOrDefault(a => a.AssetCode == keepCode)
            : _assets.FirstOrDefault();

        _details.Content = _selected is not null ? DetailsCard(_selected) : EmptyDetails();
    }

    private void RefreshKpis()
    {
        _kpis.Children.Clear();

        var total = _assets.Count;
        var assigned = _assets.Count(a => a.Status.Equals("Assegnato", StringComparison.OrdinalIgnoreCase));
        var maintenance = _assets.Count(a => a.Status.Equals("In manutenzione", StringComparison.OrdinalIgnoreCase));
        var expiring = _assets.Count(IsWarrantyExpiring);

        _kpis.Children.Add(new EnterpriseKpiCard("💻", total.ToString(), "Asset totali"));
        _kpis.Children.Add(new EnterpriseKpiCard("👤", assigned.ToString(), "Assegnati"));
        _kpis.Children.Add(new EnterpriseKpiCard("🔧", maintenance.ToString(), "In manutenzione"));
        _kpis.Children.Add(new EnterpriseKpiCard("⚠", expiring.ToString(), "Garanzie < 90gg"));
    }

    private void RefreshRows()
    {
        _rows.Children.Clear();
        _rows.Spacing = 6;

        var query = (_search.Text ?? string.Empty).Trim().ToLowerInvariant();
        var selectedCategory = _category.SelectedItem?.ToString() ?? "Tutte";
        var selectedStatus = _status.SelectedItem?.ToString() ?? "Tutti";

        var filtered = _assets.Where(a =>
            (string.IsNullOrWhiteSpace(query) ||
             $"{a.AssetCode} {a.Category} {a.Manufacturer} {a.Model} {a.SerialNumber} {a.Status} {a.OperatingSystem}".ToLowerInvariant().Contains(query)) &&
            (selectedCategory == "Tutte" || a.Category == selectedCategory) &&
            (selectedStatus == "Tutti" || a.Status == selectedStatus))
            .ToList();

        if (filtered.Count == 0)
        {
            _rows.Children.Add(new TextBlock
            {
                Text = "Nessun asset trovato.",
                Margin = new Thickness(12),
                Foreground = UiTokens.Brush(UiTokens.TextSecondary)
            });
            return;
        }

        foreach (var asset in filtered)
            _rows.Children.Add(Row(asset));
    }

    private Button Row(Asset asset)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("100,130,160,*,130,120")
        };

        Add(grid, Cell(asset.AssetCode, true), 0, 0);
        Add(grid, Cell(asset.Category), 1, 0);
        Add(grid, Cell(asset.Manufacturer), 2, 0);
        Add(grid, Cell(asset.Model), 3, 0);
        Add(grid, StatusBadge(asset.Status), 4, 0);
        Add(grid, Cell(FormatDate(asset.WarrantyEndDate)), 5, 0);

        var button = new Button
        {
            Content = grid,
            Background = _selected?.Id == asset.Id ? UiTokens.Brush(UiTokens.PremiumSelected) : Brushes.Transparent,
            Padding = new Thickness(8),
            CornerRadius = new CornerRadius(12)
        };

        button.Click += (_, _) =>
        {
            _selected = asset;
            _details.Content = DetailsCard(asset);
            RefreshRows();
        };

        button.DoubleTapped += (_, _) => OpenEditAsset(asset);

        return button;
    }

    private Control DetailsCard(Asset asset)
    {
        var stack = new StackPanel { Spacing = 12 };

        stack.Children.Add(new TextBlock
        {
            Text = asset.AssetCode,
            FontSize = 26,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        stack.Children.Add(new TextBlock
        {
            Text = $"{asset.Manufacturer} {asset.Model}",
            FontSize = 16,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap
        });

        var actions = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            Margin = new Thickness(0, 4, 0, 8)
        };
        Add(actions, SmallButton("Modifica", () => OpenEditAsset(asset)), 0, 0);
        Add(actions, SmallButton("Elimina", () => DeleteAsset(asset), true), 1, 0);
        stack.Children.Add(actions);

        stack.Children.Add(Info("Categoria", asset.Category));
        stack.Children.Add(Info("Stato", asset.Status));
        stack.Children.Add(Info("Seriale", asset.SerialNumber));
        stack.Children.Add(Info("Asset Tag", asset.AssetTag));
        stack.Children.Add(Info("Sistema operativo", asset.OperatingSystem));
        stack.Children.Add(Info("BitLocker", asset.BitLockerEnabled ? "Abilitato" : "Non abilitato"));
        stack.Children.Add(Info("Garanzia", FormatDate(asset.WarrantyEndDate)));
        stack.Children.Add(Info("Note", asset.Notes));

        stack.Children.Add(new Separator { Margin = new Thickness(0, 8) });

        stack.Children.Add(Info("Ultimo aggiornamento", FormatDate(asset.UpdatedAt)));

        stack.Children.Add(new TextBlock
        {
            Text = "Prossimi step",
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        stack.Children.Add(new TextBlock
        {
            Text = "Assegnazioni, manutenzioni, documenti, QR Code e import/export Excel.",
            TextWrapping = TextWrapping.Wrap,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });

        return Card(stack);
    }

    private Control EmptyDetails()
    {
        return Card(new TextBlock
        {
            Text = "Seleziona un asset per vedere i dettagli.",
            TextWrapping = TextWrapping.Wrap,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });
    }

    private async void OpenNewAsset()
    {
        try
        {
            var dialog = new AssetEditDialog();
            var result = await ShowAssetDialog(dialog);
            if (result is null)
                return;

            SaveAsset(result);
        }
        catch (Exception ex)
        {
            ShowMessage($"Errore apertura Nuovo Asset: {ex.Message}", true);
        }
    }

    private async void OpenEditAsset(Asset asset)
    {
        try
        {
            var dialog = new AssetEditDialog(asset);
            var result = await ShowAssetDialog(dialog);
            if (result is null)
                return;

            SaveAsset(result);
        }
        catch (Exception ex)
        {
            ShowMessage($"Errore apertura Modifica Asset: {ex.Message}", true);
        }
    }

    private async Task<Asset?> ShowAssetDialog(AssetEditDialog dialog)
    {
        var owner = TopLevel.GetTopLevel(this) as Window;
        if (owner is not null)
            return await dialog.ShowDialog<Asset?>(owner);

        dialog.Show();
        return null;
    }

    private void SaveAsset(Asset asset)
    {
        try
        {
            if (_service.AssetCodeExists(asset.AssetCode, asset.Id))
            {
                ShowMessage($"Esiste già un asset con codice {asset.AssetCode}.", true);
                return;
            }

            if (asset.Id == 0)
            {
                var id = _service.CreateAsset(asset);
                _selected = _service.GetAssetById(id);
                ShowMessage($"Asset {asset.AssetCode} creato.");
            }
            else
            {
                _service.UpdateAsset(asset);
                _selected = _service.GetAssetById(asset.Id);
                ShowMessage($"Asset {asset.AssetCode} aggiornato.");
            }

            Load();
        }
        catch (Exception ex)
        {
            ShowMessage($"Errore salvataggio asset: {ex.Message}", true);
        }
    }

    private void DeleteAsset(Asset asset)
    {
        try
        {
            _service.DeleteAsset(asset.Id);
            _selected = null;
            ShowMessage($"Asset {asset.AssetCode} eliminato.");
            Load();
        }
        catch (Exception ex)
        {
            ShowMessage($"Errore eliminazione asset: {ex.Message}", true);
        }
    }

    private void ShowMessage(string text, bool isError = false)
    {
        _message.Text = text;
        _message.Foreground = UiTokens.Brush(isError ? UiTokens.Danger : UiTokens.BrandBlue);
    }

    private static Border Kpi(string icon, string value, string label)
    {
        var stack = new StackPanel { Spacing = 2 };
        stack.Children.Add(new TextBlock
        {
            Text = $"{icon} {value}",
            FontSize = 22,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        stack.Children.Add(new TextBlock
        {
            Text = label,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            FontSize = 12
        });

        return new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            CornerRadius = new CornerRadius(18),
            Padding = new Thickness(16, 12),
            MinWidth = 160,
            Child = stack
        };
    }

    private static TextBlock Header(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            Margin = new Thickness(10, 0)
        };
    }

    private static TextBlock Cell(string text, bool strong = false)
    {
        return new TextBlock
        {
            Text = text,
            FontWeight = strong ? FontWeight.Bold : FontWeight.Normal,
            Foreground = UiTokens.Brush(strong ? UiTokens.TextPrimary : UiTokens.TextSecondary),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            TextWrapping = TextWrapping.NoWrap,
            Margin = new Thickness(10, 0)
        };
    }

    private static Border StatusBadge(string status)
    {
        return new Border
        {
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(10, 5),
            Margin = new Thickness(8, 0),
            Child = new TextBlock
            {
                Text = status,
                FontWeight = FontWeight.SemiBold,
                Foreground = UiTokens.Brush(UiTokens.BrandBlue),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            }
        };
    }

    private static Border Info(string label, string value)
    {
        var stack = new StackPanel { Spacing = 2 };
        stack.Children.Add(new TextBlock
        {
            Text = label,
            FontSize = 12,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });
        stack.Children.Add(new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(value) ? "—" : value,
            FontWeight = FontWeight.SemiBold,
            TextWrapping = TextWrapping.Wrap,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        return new Border
        {
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(12),
            Child = stack
        };
    }

    private static Button ToolbarButton(string text, string tooltip, Action? action = null)
    {
        var b = new Button
        {
            Content = text,
            Background = UiTokens.Brush(UiTokens.Surface),
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            Padding = new Thickness(10, 8),
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(8, 0, 0, 0)
        };

        if (action is not null)
            b.Click += (_, _) => action();

        ToolTip.SetTip(b, tooltip);
        return b;
    }

    private static Button SmallButton(string text, Action action, bool danger = false)
    {
        var b = new Button
        {
            Content = text,
            Background = UiTokens.Brush(danger ? UiTokens.SurfaceAlt : UiTokens.BrandBlue),
            Foreground = danger ? UiTokens.Brush(UiTokens.Danger) : Brushes.White,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(10, 8),
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(4)
        };
        b.Click += (_, _) => action();
        return b;
    }

    private static Border Card(Control child)
    {
        return new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            CornerRadius = new CornerRadius(22),
            Padding = new Thickness(18),
            Margin = new Thickness(18, 0, 0, 0),
            Child = child
        };
    }

    private static string FormatDate(string value)
    {
        if (DateTime.TryParse(value, out var date))
            return date.ToString("dd/MM/yyyy");

        return string.IsNullOrWhiteSpace(value) ? "—" : value;
    }

    private static bool IsWarrantyExpiring(Asset asset)
    {
        if (!DateTime.TryParse(asset.WarrantyEndDate, out var warranty))
            return false;

        var now = DateTime.Now.Date;
        return warranty >= now && warranty <= now.AddDays(90);
    }

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
