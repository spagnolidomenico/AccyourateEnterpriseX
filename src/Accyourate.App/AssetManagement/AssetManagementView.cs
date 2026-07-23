using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Models;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.Tokens;
using Accyourate.App.UIFramework.Controls;
using Accyourate.App.UIFramework.Layout;
using Accyourate.App.UIFramework.EnterpriseTable;
using Accyourate.App.UIFramework.DesignSystem;

namespace Accyourate.App.AssetManagement;

public sealed class AssetManagementView : UserControl
{
    private readonly AssetService _service = new();
    private readonly AssetAssignmentEngine _assignmentEngine = new();

    private readonly TextBox _search = new();
    private readonly ComboBox _category = new();
    private readonly ComboBox _status = new();
    private readonly ComboBox _manufacturer = new();
    private readonly AxEnterpriseTable<Asset> _assetTable = new();
    private readonly StackPanel _kpis = new();
    private readonly ContentControl _details = new();
    private readonly TextBlock _message = new();
    private readonly TextBlock _resultSummary = new();
    private readonly Grid _adaptiveContent = new();
    private Control? _assetList;
    private Control? _detailsHost;

    private IReadOnlyList<Asset> _assets = Array.Empty<Asset>();
    private Asset? _selected;

    public AssetManagementView()
    {
        Background = UiTokens.Brush(UiTokens.Background);
        ConfigureAssetTable();
        Content = BuildLayout();
        Load();
    }

    private Control BuildLayout()
    {
        var root = new DockPanel();

        var headerSurface = new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(24, 20, 24, 18)
        };

        var headerGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto")
        };

        var titleBlock = new StackPanel { Spacing = 5 };
        titleBlock.Children.Add(new TextBlock
        {
            Text = "IT Asset Management",
            FontSize = 30,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        titleBlock.Children.Add(new TextBlock
        {
            Text = "Gestione del patrimonio informatico aziendale, disponibilità, assegnazioni e garanzie.",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap
        });

        Grid.SetColumn(titleBlock, 0);
        headerGrid.Children.Add(titleBlock);

        var newAssetButton = ActionButton("+ Nuovo asset", OpenNewAsset, true);
        newAssetButton.MinWidth = 142;
        newAssetButton.Margin = new Thickness(18, 0, 0, 0);
        newAssetButton.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        Grid.SetColumn(newAssetButton, 1);
        headerGrid.Children.Add(newAssetButton);

        headerSurface.Child = headerGrid;
        DockPanel.SetDock(headerSurface, Dock.Top);
        root.Children.Add(headerSurface);

        var page = new Grid
        {
            Margin = new Thickness(24, 18, 24, 24),
            RowDefinitions = new RowDefinitions("Auto,Auto,*")
        };

        var kpiWrap = new WrapPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 16)
        };
        _kpis.Orientation = Avalonia.Layout.Orientation.Horizontal;
        _kpis.Spacing = 12;
        kpiWrap.Children.Add(_kpis);
        Grid.SetRow(kpiWrap, 0);
        page.Children.Add(kpiWrap);

        _message.TextWrapping = TextWrapping.Wrap;
        _message.Foreground = UiTokens.Brush(UiTokens.BrandBlue);
        _message.IsVisible = false;
        _message.Margin = new Thickness(0, 0, 0, 12);
        Grid.SetRow(_message, 1);
        page.Children.Add(_message);

        var master = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,14,*"),
            MinHeight = 0
        };

        var commandSurface = BuildCommandSurface();
        Grid.SetRow(commandSurface, 0);
        master.Children.Add(commandSurface);

        _assetList = new ScrollViewer
        {
            Content = _assetTable,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            MinWidth = 0,
            MinHeight = 0
        };
        Grid.SetRow(_assetList, 2);
        master.Children.Add(_assetList);

        _details.Content = EmptyDetails();
        _detailsHost = new ScrollViewer
        {
            Content = _details,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            MinHeight = 360
        };

        EnterpriseAdaptiveLayout.ArrangeMasterDetails(_adaptiveContent, master, _detailsHost, Bounds.Width);
        SizeChanged += (_, e) =>
        {
            if (_detailsHost is not null)
                EnterpriseAdaptiveLayout.ArrangeMasterDetails(_adaptiveContent, master, _detailsHost, e.NewSize.Width);
        };

        _adaptiveContent.MinHeight = 0;
        Grid.SetRow(_adaptiveContent, 2);
        page.Children.Add(_adaptiveContent);
        root.Children.Add(page);
        return root;
    }

    private Control BuildCommandSurface()
    {
        var surface = new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(14)
        };

        var content = new StackPanel { Spacing = 12 };

        var actions = new WrapPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal
        };
        actions.Children.Add(ActionButton("Modifica", () =>
        {
            if (_selected is not null)
                OpenEditAsset(_selected);
        }));
        actions.Children.Add(ActionButton("Assegna", () =>
        {
            if (_selected is not null)
                _ = OpenAssignAsset(_selected);
        }));
        actions.Children.Add(ActionButton("Restituisci", () =>
        {
            if (_selected is not null)
                ReturnAsset(_selected);
        }));
        actions.Children.Add(ActionButton("Elimina", () =>
        {
            if (_selected is not null)
                DeleteAsset(_selected);
        }, danger: true));
        actions.Children.Add(ActionButton("↻ Aggiorna", Load));
        actions.Children.Add(ActionButton("Reimposta filtri", ResetFilters));
        content.Children.Add(actions);

        var filters = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,12,140,12,140,12,160")
        };

        _search.Watermark = "Cerca per codice, categoria, produttore, modello o seriale...";
        _search.MinWidth = 260;
        _search.TextChanged += (_, _) => RefreshRows();

        _category.ItemsSource = new[] { "Tutte", "Desktop PC", "Notebook", "Mac", "Stampante", "Smartphone", "Tablet", "Monitor", "Accessorio", "Licenza software", "Dispositivo medicale", "Altro" };
        _category.SelectedIndex = 0;
        _category.SelectionChanged += (_, _) => RefreshRows();

        _status.ItemsSource = new[] { "Tutti", "Attivo", "Assegnato", "Disponibile", "In manutenzione", "Dismesso", "Smarrito", "Da verificare" };
        _status.SelectedIndex = 0;
        _status.SelectionChanged += (_, _) => RefreshRows();

        _manufacturer.MinWidth = 150;
        _manufacturer.SelectionChanged += (_, _) => RefreshRows();

        Add(filters, _search, 0, 0);
        Add(filters, LabeledFilter("Categoria", _category), 2, 0);
        Add(filters, LabeledFilter("Stato", _status), 4, 0);
        Add(filters, LabeledFilter("Produttore", _manufacturer), 6, 0);
        content.Children.Add(filters);

        _resultSummary.FontSize = 12;
        _resultSummary.Foreground = UiTokens.Brush(UiTokens.TextSecondary);
        _resultSummary.Margin = new Thickness(2, 0, 0, 0);
        content.Children.Add(_resultSummary);

        surface.Child = content;
        return surface;
    }

    private void Load()
    {
        var keepCode = _selected?.AssetCode;

        _assets = _service.GetAssets();
        RefreshManufacturerFilter();
        RefreshKpis();
        RefreshRows();

        _selected = !string.IsNullOrWhiteSpace(keepCode)
            ? _assets.FirstOrDefault(a => a.AssetCode == keepCode)
            : _assets.FirstOrDefault();

        _assetTable.SetSelectedItem(_selected);
        _details.Content = _selected is not null ? DetailsCard(_selected) : EmptyDetails();
    }

    private void RefreshKpis()
    {
        _kpis.Children.Clear();

        var total = _assets.Count;
        var available = _assets.Count(a =>
            a.Status.Equals("Disponibile", StringComparison.OrdinalIgnoreCase) ||
            a.Status.Equals("Attivo", StringComparison.OrdinalIgnoreCase));
        var assigned = _assets.Count(a => a.Status.Equals("Assegnato", StringComparison.OrdinalIgnoreCase));
        var maintenance = _assets.Count(a => a.Status.Equals("In manutenzione", StringComparison.OrdinalIgnoreCase));
        var expiringWarranty = _assets.Count(a => IsWarrantyExpiring(a.WarrantyEndDate));

        _kpis.Children.Add(new EnterpriseKpiCard("▣", total.ToString(), "Asset totali", "Tutti gli asset registrati"));
        _kpis.Children.Add(new EnterpriseKpiCard("✓", available.ToString(), "Disponibili", "Pronti per l'assegnazione"));
        _kpis.Children.Add(new EnterpriseKpiCard("↗", assigned.ToString(), "Assegnati", "Attualmente in uso"));
        _kpis.Children.Add(new EnterpriseKpiCard("⚙", maintenance.ToString(), "Manutenzione", "In lavorazione"));
        _kpis.Children.Add(new EnterpriseKpiCard("◷", expiringWarranty.ToString(), "Garanzie in scadenza", "Entro i prossimi 90 giorni"));
    }

    private void ConfigureAssetTable()
    {
        _assetTable.ConfigureColumns(new[]
        {
            new AxEnterpriseColumn<Asset>
            {
                Id = "asset-code",
                Header = "Codice",
                Width = 120,
                MinWidth = 120,
                CellFactory = asset => Cell(asset.AssetCode, true)
            },
            new AxEnterpriseColumn<Asset>
            {
                Id = "category",
                Header = "Categoria",
                Width = 150,
                MinWidth = 140,
                TextSelector = asset => asset.Category
            },
            new AxEnterpriseColumn<Asset>
            {
                Id = "manufacturer",
                Header = "Produttore",
                Width = 170,
                MinWidth = 150,
                TextSelector = asset => asset.Manufacturer
            },
            new AxEnterpriseColumn<Asset>
            {
                Id = "model",
                Header = "Modello",
                Width = 210,
                MinWidth = 180,
                TextSelector = asset => asset.Model
            },
            new AxEnterpriseColumn<Asset>
            {
                Id = "serial-number",
                Header = "Seriale",
                Width = 170,
                MinWidth = 150,
                TextSelector = asset => asset.SerialNumber
            },
            new AxEnterpriseColumn<Asset>
            {
                Id = "assigned-to",
                Header = "Assegnato a",
                Width = 190,
                MinWidth = 170,
                TextSelector = asset => _assignmentEngine.GetActiveAssignmentForAsset(asset.Id)?.EmployeeName ?? "—"
            },
            new AxEnterpriseColumn<Asset>
            {
                Id = "status",
                Header = "Stato",
                Width = 160,
                MinWidth = 150,
                Alignment = AxColumnAlignment.Center,
                CellFactory = asset => StatusBadge(asset.Status)
            },
            new AxEnterpriseColumn<Asset>
            {
                Id = "warranty",
                Header = "Garanzia",
                Width = 140,
                MinWidth = 130,
                TextSelector = asset => FormatDate(asset.WarrantyEndDate)
            }
        });

        _assetTable.SelectionChanged += asset =>
        {
            _selected = asset;
            _details.Content = DetailsCard(asset);
        };
        _assetTable.ItemActivated += OpenEditAsset;
    }

    private void RefreshRows()
    {
        var query = (_search.Text ?? string.Empty).Trim().ToLowerInvariant();
        var selectedCategory = _category.SelectedItem?.ToString() ?? "Tutte";
        var selectedStatus = _status.SelectedItem?.ToString() ?? "Tutti";
        var selectedManufacturer = _manufacturer.SelectedItem?.ToString() ?? "Tutti";

        var filtered = _assets.Where(a =>
            (string.IsNullOrWhiteSpace(query) ||
             $"{a.AssetCode} {a.Category} {a.Manufacturer} {a.Model} {a.SerialNumber} {a.Status} {a.OperatingSystem}".ToLowerInvariant().Contains(query)) &&
            (selectedCategory == "Tutte" || a.Category == selectedCategory) &&
            (selectedStatus == "Tutti" || a.Status == selectedStatus) &&
            (selectedManufacturer == "Tutti" || a.Manufacturer == selectedManufacturer))
            .ToList();

        var visibleSelection = filtered.FirstOrDefault(a => a.Id == _selected?.Id)
            ?? filtered.FirstOrDefault();

        _selected = visibleSelection;
        _assetTable.SetItems(filtered);
        _resultSummary.Text = $"{filtered.Count} di {_assets.Count} asset visualizzati";
        _assetTable.SetSelectedItem(visibleSelection);
        _details.Content = visibleSelection is not null ? DetailsCard(visibleSelection) : EmptyDetails();
    }

    private Control DetailsCard(Asset asset)
    {
        var assignment = _assignmentEngine.GetActiveAssignmentForAsset(asset.Id);
        var content = new StackPanel { Spacing = 14 };

        var eyebrow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto")
        };
        var label = new TextBlock
        {
            Text = "DETTAGLIO ASSET",
            FontSize = 11,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        Grid.SetColumn(label, 0);
        eyebrow.Children.Add(label);

        var closeHint = new TextBlock
        {
            Text = "×",
            FontSize = 18,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        Grid.SetColumn(closeHint, 1);
        eyebrow.Children.Add(closeHint);
        content.Children.Add(eyebrow);

        var identity = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("56,14,*")
        };
        var icon = new Border
        {
            Width = 56,
            Height = 56,
            CornerRadius = new CornerRadius(18),
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            Child = new TextBlock
            {
                Text = "▰",
                FontSize = 24,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Foreground = UiTokens.Brush(UiTokens.BrandBlue)
            }
        };
        Grid.SetColumn(icon, 0);
        identity.Children.Add(icon);

        var identityText = new StackPanel
        {
            Spacing = 3,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        identityText.Children.Add(StatusBadge(asset.Status));
        identityText.Children.Add(new TextBlock
        {
            Text = $"{asset.Manufacturer} {asset.Model}".Trim(),
            FontSize = 22,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            TextWrapping = TextWrapping.Wrap
        });
        identityText.Children.Add(new TextBlock
        {
            Text = asset.AssetCode,
            FontSize = 13,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });
        Grid.SetColumn(identityText, 2);
        identity.Children.Add(identityText);
        content.Children.Add(identity);

        var tabs = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*,*"),
            Margin = new Thickness(0, 4, 0, 0)
        };
        Add(tabs, InspectorTab("Dettagli", true), 0, 0);
        Add(tabs, InspectorTab("Assegnazioni"), 1, 0);
        Add(tabs, InspectorTab("Storico"), 2, 0);
        Add(tabs, InspectorTab("Documenti"), 3, 0);
        content.Children.Add(tabs);

        var details = new StackPanel { Spacing = 0 };
        details.Children.Add(InspectorRow("Codice", asset.AssetCode));
        details.Children.Add(InspectorRow("Categoria", asset.Category));
        details.Children.Add(InspectorRow("Produttore", asset.Manufacturer));
        details.Children.Add(InspectorRow("Modello", asset.Model));
        details.Children.Add(InspectorRow("Seriale", asset.SerialNumber));
        details.Children.Add(InspectorRow("Asset Tag", asset.AssetTag));
        details.Children.Add(InspectorRow("Stato", asset.Status));
        details.Children.Add(InspectorRow("Assegnato a", assignment?.EmployeeName ?? "—"));
        details.Children.Add(InspectorRow("Sistema operativo", asset.OperatingSystem));
        details.Children.Add(InspectorRow("BitLocker", asset.BitLockerEnabled ? "Abilitato" : "Non abilitato"));
        details.Children.Add(InspectorRow("Garanzia", FormatDate(asset.WarrantyEndDate)));
        details.Children.Add(InspectorRow("Ultimo aggiornamento", FormatDate(asset.UpdatedAt)));
        details.Children.Add(InspectorRow("Note", asset.Notes));
        content.Children.Add(details);

        var actions = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,10,*"),
            RowDefinitions = new RowDefinitions("Auto,10,Auto"),
            Margin = new Thickness(0, 6, 0, 0)
        };
        Add(actions, SmallButton("Modifica", () => OpenEditAsset(asset)), 0, 0);
        Add(actions, SmallButton("Assegna", () => _ = OpenAssignAsset(asset)), 2, 0);
        Add(actions, SmallButton("Restituisci", () => ReturnAsset(asset)), 0, 2);
        Add(actions, SmallButton("Elimina", () => DeleteAsset(asset), true), 2, 2);
        content.Children.Add(actions);

        return new AxInspectorPanel(content);
    }

    private Control EmptyDetails()
    {
        return new AxInspectorPanel(new TextBlock
        {
            Text = "Seleziona un asset per vedere i dettagli.",
            TextWrapping = TextWrapping.Wrap,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });
    }

    private void RefreshManufacturerFilter()
    {
        var current = _manufacturer.SelectedItem?.ToString() ?? "Tutti";
        var values = new List<string> { "Tutti" };
        values.AddRange(_assets
            .Select(a => a.Manufacturer)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase));

        _manufacturer.ItemsSource = values;
        _manufacturer.SelectedItem = values.Contains(current, StringComparer.OrdinalIgnoreCase)
            ? values.First(value => value.Equals(current, StringComparison.OrdinalIgnoreCase))
            : "Tutti";
    }


    private async void OpenAssignAsset()
    {
        await OpenAssignAsset(null);
    }

    private async Task OpenAssignAsset(Asset? asset)
    {
        try
        {
            _assignmentEngine.SyncEmployeesFromMasterData();

            var dialog = new AssetAssignmentDialog(_assignmentEngine, asset?.Id);
            var result = await ShowAssignmentDialog(dialog);
            if (result is null)
                return;

            _assignmentEngine.AssignAsset(result.AssetId, result.MasterEmployeeId, "Asset Management", result.Notes);
            ShowMessage("Asset assegnato correttamente.");
            Load();
        }
        catch (Exception ex)
        {
            ShowMessage($"Errore assegnazione asset: {ex.Message}", true);
        }
    }

    private void ReturnAsset(Asset asset)
    {
        try
        {
            var assignment = _assignmentEngine.GetActiveAssignmentForAsset(asset.Id);
            if (assignment is null)
            {
                ShowMessage("Questo asset non ha assegnazioni attive.", true);
                return;
            }

            _assignmentEngine.ReturnAssignment(assignment.AssignmentId, "Restituito da Asset Management.");
            ShowMessage("Asset restituito correttamente.");
            Load();
        }
        catch (Exception ex)
        {
            ShowMessage($"Errore restituzione asset: {ex.Message}", true);
        }
    }

    private async Task<AssetAssignmentDialogResult?> ShowAssignmentDialog(AssetAssignmentDialog dialog)
    {
        var owner = TopLevel.GetTopLevel(this) as Window;
        if (owner is not null)
            return await dialog.ShowDialog<AssetAssignmentDialogResult?>(owner);

        dialog.Show();
        return null;
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

    private void ResetFilters()
    {
        _search.Text = string.Empty;
        _category.SelectedIndex = 0;
        _status.SelectedIndex = 0;
        _manufacturer.SelectedIndex = 0;
        RefreshRows();
        ShowMessage("Filtri reimpostati.");
    }

    private static bool IsWarrantyExpiring(string value)
    {
        if (!DateTime.TryParse(value, out var warrantyEnd))
            return false;

        var today = DateTime.Today;
        return warrantyEnd.Date >= today && warrantyEnd.Date <= today.AddDays(90);
    }

    private void ShowMessage(string text, bool isError = false)
    {
        _message.Text = text;
        _message.IsVisible = !string.IsNullOrWhiteSpace(text);
        _message.Foreground = UiTokens.Brush(isError ? UiTokens.Danger : UiTokens.BrandBlue);
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
            TextTrimming = TextTrimming.CharacterEllipsis
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
            MinWidth = 126,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Child = new TextBlock
            {
                Text = status,
                FontWeight = FontWeight.SemiBold,
                Foreground = UiTokens.Brush(UiTokens.BrandBlue),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            }
        };
    }


    private static TextBlock SectionLabel(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 15,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            Margin = new Thickness(0, 10, 0, 0)
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

    private static Button ActionButton(string text, Action action, bool primary = false, bool danger = false)
    {
        var button = new Button
        {
            Content = text,
            Background = UiTokens.Brush(primary ? UiTokens.BrandBlue : UiTokens.SurfaceAlt),
            Foreground = primary
                ? Brushes.White
                : UiTokens.Brush(danger ? UiTokens.Danger : UiTokens.TextPrimary),
            FontWeight = primary ? FontWeight.Bold : FontWeight.SemiBold,
            Padding = new Thickness(14, 9),
            CornerRadius = new CornerRadius(10),
            Margin = new Thickness(0, 0, 8, 0),
            MinWidth = 96
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static Button SmallButton(string text, Action action, bool danger = false)
    {
        var b = new Button
        {
            Content = text,
            Background = UiTokens.Brush(danger ? UiTokens.SurfaceAlt : UiTokens.BrandBlue),
            Foreground = danger ? UiTokens.Brush(UiTokens.Danger) : Brushes.White,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(12, 9),
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(0),
            MinWidth = 118,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
        b.Click += (_, _) => action();
        return b;
    }

    private static Border Card(Control child)
    {
        return new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(18),
            Child = child
        };
    }

    private static Control LabeledFilter(string label, Control control)
    {
        var stack = new StackPanel { Spacing = 4 };
        stack.Children.Add(new TextBlock
        {
            Text = label,
            FontSize = 11,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });
        stack.Children.Add(control);
        return stack;
    }

    private static Border InspectorTab(string text, bool selected = false)
    {
        return new Border
        {
            BorderBrush = UiTokens.Brush(selected ? UiTokens.BrandBlue : UiTokens.Border),
            BorderThickness = new Thickness(0, 0, 0, selected ? 2 : 1),
            Padding = new Thickness(4, 10),
            Child = new TextBlock
            {
                Text = text,
                FontSize = 11,
                FontWeight = selected ? FontWeight.Bold : FontWeight.Normal,
                Foreground = UiTokens.Brush(selected ? UiTokens.BrandBlue : UiTokens.TextSecondary),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            }
        };
    }

    private static Border InspectorRow(string label, string value)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("118,*")
        };
        var labelText = new TextBlock
        {
            Text = label,
            FontSize = 12,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        Grid.SetColumn(labelText, 0);
        grid.Children.Add(labelText);

        var valueText = new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(value) ? "—" : value,
            FontSize = 12,
            FontWeight = FontWeight.SemiBold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        Grid.SetColumn(valueText, 1);
        grid.Children.Add(valueText);

        return new Border
        {
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(0, 10),
            Child = grid
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
