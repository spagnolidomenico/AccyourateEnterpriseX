using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System.Diagnostics;
using Accyourate.App.Platform.Pdf;
using Accyourate.App.Platform.Qr;
using Accyourate.App.Platform.Settings;
using Accyourate.App.AssetManagement.Models;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.AssetManagement.Deliveries;
using Accyourate.App.AssetManagement.DeliveryReports;
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
    private readonly DeliveryRecordRepository _deliveryRecords = new();
    private readonly DeliveryReportService _deliveryReports = new();
    private readonly ReturnReportPdfService _returnReports = new();

    private readonly TextBox _search = new();
    private readonly ComboBox _category = new();
    private readonly ComboBox _status = new();
    private readonly ComboBox _manufacturer = new();
    private readonly AxEnterpriseTable<Asset> _assetTable = new();
    private readonly StackPanel _kpis = new();
    private readonly ContentControl _enterpriseOverview = new();
    private readonly Border _enterpriseOverviewBody = new();
    private readonly ContentControl _details = new();
    private readonly TextBlock _message = new();
    private readonly TextBlock _resultSummary = new();
    private readonly Grid _adaptiveContent = new();
    private readonly ContentControl _viewHost = new();
    private Control? _detailsHost;
    private Grid? _masterLayout;
    private Button? _listViewButton;
    private Button? _cardViewButton;
    private Button? _overviewToggleButton;
    private Border? _selectedAssetCard;
    private bool _detailsVisible;
    private bool _cardMode;
    private bool _overviewExpanded;
    private string? _kpiFilter;

    private IReadOnlyList<Asset> _assets = Array.Empty<Asset>();
    private Asset? _selected;
    private string _sortColumnId = "asset-code";
    private bool _sortAscending = true;

    public AssetManagementView(string? initialCategory = null)
    {
        Background = UiTokens.Brush(UiTokens.Background);
        ConfigureAssetTable();
        Content = BuildLayout();
        ApplyInitialCategoryFilter(initialCategory);
        Load();
    }

    public void OpenAssetDetails(int assetId)
    {
        var asset = _service.GetAssetById(assetId);
        if (asset is null)
        {
            ShowMessage($"Asset #{assetId} non trovato.", true);
            return;
        }

        _selected = asset;
        _assetTable.SetSelectedItem(asset);
        _detailsVisible = true;
        _details.Content = DetailsCard(asset);
        RefreshRows();
        ArrangeAssetWorkspace(Bounds.Width);
    }

    private void ApplyInitialCategoryFilter(string? initialCategory)
    {
        if (string.IsNullOrWhiteSpace(initialCategory))
        {
            _category.SelectedIndex = 0;
            return;
        }

        var matchingItem = _category.ItemsSource?
            .Cast<object>()
            .FirstOrDefault(item => string.Equals(
                item?.ToString(),
                initialCategory,
                StringComparison.OrdinalIgnoreCase));

        _category.SelectedItem = matchingItem;
        if (matchingItem is null)
            _category.SelectedIndex = 0;
    }

    private Control BuildLayout()
    {
        var root = new DockPanel();

        var headerSurface = new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(24, 10, 24, 10)
        };

        var headerGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto")
        };

        var titleBlock = new StackPanel { Spacing = 5 };
        titleBlock.Children.Add(new TextBlock
        {
            Text = "IT Asset Management",
            FontSize = 25,
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
            Margin = new Thickness(24, 8, 24, 12),
            RowDefinitions = new RowDefinitions("Auto,Auto,*")
        };

        var kpiWrap = new WrapPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 8)
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
            RowDefinitions = new RowDefinitions("Auto,8,Auto,8,*"),
            MinHeight = 0
        };
        _masterLayout = master;

        _enterpriseOverview.Content = BuildCollapsibleEnterpriseOverview();
        Grid.SetRow(_enterpriseOverview, 0);
        master.Children.Add(_enterpriseOverview);

        var commandSurface = BuildCommandSurface();
        Grid.SetRow(commandSurface, 2);
        master.Children.Add(commandSurface);

        _assetTable.MinWidth = 0;
        _assetTable.MinHeight = 0;
        _assetTable.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
        _assetTable.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        _viewHost.Content = _assetTable;
        _viewHost.MinHeight = 0;
        Grid.SetRow(_viewHost, 4);
        master.Children.Add(_viewHost);

        _details.Content = EmptyDetails();
        _detailsHost = new ScrollViewer
        {
            Content = _details,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            MinHeight = 360
        };

        ArrangeAssetWorkspace(Bounds.Width);
        SizeChanged += (_, e) => ArrangeAssetWorkspace(e.NewSize.Width);

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
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(10)
        };

        var content = new StackPanel { Spacing = 8 };

        var topRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,8,Auto,8,Auto,8,Auto,*,8,Auto,8,Auto,8,Auto")
        };

        Add(topRow, ToolbarButton("✎", "Modifica", () =>
        {
            if (_selected is not null)
                OpenEditAsset(_selected);
        }), 0, 0);
        Add(topRow, ToolbarButton("↗", "Assegna", () =>
        {
            if (_selected is not null)
                _ = OpenAssignAsset(_selected);
        }), 2, 0);
        Add(topRow, ToolbarButton("↙", "Restituisci", () =>
        {
            if (_selected is not null)
                ReturnAsset(_selected);
        }), 4, 0);
        Add(topRow, ToolbarButton("⧉", "Duplica", () =>
        {
            if (_selected is not null)
                DuplicateAsset(_selected);
        }), 6, 0);

        _listViewButton = ToolbarButton("☰", "Lista", () => SetViewMode(false));
        _cardViewButton = ToolbarButton("▦", "Card", () => SetViewMode(true));
        Add(topRow, _listViewButton, 9, 0);
        Add(topRow, _cardViewButton, 11, 0);
        Add(topRow, ToolbarButton("↻", string.Empty, Load, iconOnly: true), 13, 0);
        UpdateViewButtons();
        content.Children.Add(topRow);

        var filters = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,8,118,8,118,8,145,8,Auto"),
            RowDefinitions = new RowDefinitions("Auto")
        };

        _search.Watermark = "Cerca asset, seriale, modello...";
        _search.MinWidth = 240;
        _search.Height = 38;
        _search.TextChanged += (_, _) => RefreshRows();

        _category.ItemsSource = new[] { "Tutte", "Desktop PC", "Notebook", "Mac", "Stampante", "Smartphone", "Tablet", "Monitor", "Accessorio", "Licenza software", "Dispositivo medicale", "Altro" };
        _category.SelectedIndex = 0;
        _category.SelectionChanged += (_, _) => RefreshRows();

        _status.ItemsSource = new[] { "Tutti", "Attivo", "Assegnato", "Disponibile", "In manutenzione", "Dismesso", "Smarrito", "Da verificare" };
        _status.SelectedIndex = 0;
        _status.SelectionChanged += (_, _) => RefreshRows();

        _manufacturer.MinWidth = 145;
        _manufacturer.SelectionChanged += (_, _) => RefreshRows();

        var categoryFilter = LabeledFilter("Categoria", _category);
        var statusFilter = LabeledFilter("Stato", _status);
        var manufacturerFilter = LabeledFilter("Produttore", _manufacturer);
        var resetButton = ToolbarButton("↺", "Reimposta", ResetFilters);

        void ApplyResponsiveFilters(double availableWidth)
        {
            var compact = availableWidth > 0 && availableWidth < 920;

            filters.Children.Clear();

            if (compact)
            {
                filters.ColumnDefinitions = new ColumnDefinitions("*,8,*,8,*,8,Auto");
                filters.RowDefinitions = new RowDefinitions("Auto,8,Auto");

                Grid.SetColumn(_search, 0);
                Grid.SetColumnSpan(_search, 7);
                Grid.SetRow(_search, 0);
                filters.Children.Add(_search);

                Grid.SetColumn(categoryFilter, 0);
                Grid.SetColumnSpan(categoryFilter, 1);
                Grid.SetRow(categoryFilter, 2);
                filters.Children.Add(categoryFilter);

                Grid.SetColumn(statusFilter, 2);
                Grid.SetColumnSpan(statusFilter, 1);
                Grid.SetRow(statusFilter, 2);
                filters.Children.Add(statusFilter);

                Grid.SetColumn(manufacturerFilter, 4);
                Grid.SetColumnSpan(manufacturerFilter, 1);
                Grid.SetRow(manufacturerFilter, 2);
                filters.Children.Add(manufacturerFilter);

                Grid.SetColumn(resetButton, 6);
                Grid.SetColumnSpan(resetButton, 1);
                Grid.SetRow(resetButton, 2);
                resetButton.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom;
                filters.Children.Add(resetButton);
            }
            else
            {
                filters.ColumnDefinitions = new ColumnDefinitions("*,8,118,8,118,8,145,8,Auto");
                filters.RowDefinitions = new RowDefinitions("Auto");

                Grid.SetColumnSpan(_search, 1);
                Add(filters, _search, 0, 0);
                Add(filters, categoryFilter, 2, 0);
                Add(filters, statusFilter, 4, 0);
                Add(filters, manufacturerFilter, 6, 0);
                resetButton.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
                Add(filters, resetButton, 8, 0);
            }
        }

        filters.SizeChanged += (_, e) => ApplyResponsiveFilters(e.NewSize.Width);
        ApplyResponsiveFilters(Bounds.Width);
        content.Children.Add(filters);

        var footer = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
        _resultSummary.FontSize = 12;
        _resultSummary.Foreground = UiTokens.Brush(UiTokens.TextSecondary);
        _resultSummary.Margin = new Thickness(2, 0, 0, 0);
        footer.Children.Add(_resultSummary);

        var hint = new TextBlock
        {
            Text = "Doppio clic per aprire | clic singolo per selezionare",
            FontSize = 11,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        Grid.SetColumn(hint, 1);
        footer.Children.Add(hint);
        content.Children.Add(footer);

        surface.Child = content;
        return surface;
    }

    private static Button ToolbarButton(string icon, string text, Action action, bool iconOnly = false)
    {
        return AxCommandButton.Create(
            icon,
            text,
            action,
            iconOnly,
            toolTip: iconOnly && string.IsNullOrWhiteSpace(text) ? "Aggiorna elenco" : text);
    }

    private void Load()
    {
        var keepCode = _selected?.AssetCode;

        _assets = _service.GetAssets();
        RefreshManufacturerFilter();
        RefreshKpis();
        RefreshEnterpriseOverview();
        RefreshRows();

        _selected = !string.IsNullOrWhiteSpace(keepCode)
            ? _assets.FirstOrDefault(a => a.AssetCode == keepCode)
            : _assets.FirstOrDefault();

        _assetTable.SetSelectedItem(_selected);
        _details.Content = _selected is not null && _detailsVisible ? DetailsCard(_selected) : EmptyDetails();
    }


    private Control BuildCollapsibleEnterpriseOverview()
    {
        var root = new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12)
        };

        var content = new StackPanel();
        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            MinHeight = 42,
            Margin = new Thickness(14, 0)
        };

        var icon = new TextBlock
        {
            Text = "▦",
            FontSize = 16,
            Foreground = UiTokens.Brush(UiTokens.BrandBlue),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        Grid.SetColumn(icon, 0);
        header.Children.Add(icon);

        var title = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0)
        };
        title.Children.Add(new TextBlock
        {
            Text = "Dashboard operativa",
            FontWeight = FontWeight.SemiBold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        title.Children.Add(new TextBlock
        {
            Text = "Stato patrimonio, scadenze e attività recenti",
            FontSize = 12,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        });
        Grid.SetColumn(title, 1);
        header.Children.Add(title);

        _overviewToggleButton = ActionButton("Mostra ▼", ToggleEnterpriseOverview);
        _overviewToggleButton.Padding = new Thickness(12, 6);
        _overviewToggleButton.MinWidth = 92;
        _overviewToggleButton.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        Grid.SetColumn(_overviewToggleButton, 2);
        header.Children.Add(_overviewToggleButton);

        content.Children.Add(header);

        _enterpriseOverviewBody.Padding = new Thickness(14, 0, 14, 14);
        _enterpriseOverviewBody.Child = BuildEnterpriseOverview();
        _enterpriseOverviewBody.IsVisible = _overviewExpanded;
        content.Children.Add(_enterpriseOverviewBody);

        root.Child = content;
        return root;
    }

    private void ToggleEnterpriseOverview()
    {
        _overviewExpanded = !_overviewExpanded;
        _enterpriseOverviewBody.IsVisible = _overviewExpanded;
        if (_overviewToggleButton is not null)
            _overviewToggleButton.Content = _overviewExpanded ? "Nascondi ▲" : "Mostra ▼";
    }

    private void RefreshEnterpriseOverview()
    {
        _enterpriseOverviewBody.Child = BuildEnterpriseOverview();
        _enterpriseOverviewBody.IsVisible = _overviewExpanded;
    }

    private Control BuildEnterpriseOverview()
    {
        var active = _assets.Count(a =>
            a.Status.Equals("Disponibile", StringComparison.OrdinalIgnoreCase) ||
            a.Status.Equals("Attivo", StringComparison.OrdinalIgnoreCase) ||
            a.Status.Equals("Assegnato", StringComparison.OrdinalIgnoreCase));
        var attention = _assets.Count(a =>
            a.Status.Equals("In manutenzione", StringComparison.OrdinalIgnoreCase) ||
            a.Status.Equals("Da verificare", StringComparison.OrdinalIgnoreCase));
        var expiring = _assets.Where(a => IsWarrantyExpiring(a.WarrantyEndDate))
            .OrderBy(a => DateTime.TryParse(a.WarrantyEndDate, out var d) ? d : DateTime.MaxValue)
            .Take(4)
            .ToList();
        var recent = _assets
            .OrderByDescending(a => DateTime.TryParse(a.UpdatedAt, out var d) ? d : DateTime.MinValue)
            .Take(4)
            .ToList();

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("1.15*,12,1*,12,1*"),
            Margin = new Thickness(0, 0, 0, 2)
        };

        var health = new StackPanel { Spacing = 10 };
        health.Children.Add(OverviewTitle("Stato del patrimonio", "Panoramica operativa aggiornata"));
        health.Children.Add(ProgressRow("Operativi", active, Math.Max(1, _assets.Count)));
        health.Children.Add(ProgressRow("Richiedono attenzione", attention, Math.Max(1, _assets.Count)));
        health.Children.Add(new TextBlock
        {
            Text = attention == 0
                ? "Nessuna criticità rilevata nel patrimonio IT."
                : $"{attention} asset richiedono una verifica o un intervento.",
            Foreground = UiTokens.Brush(attention == 0 ? UiTokens.Success : UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12
        });
        AddOverviewPanel(grid, health, 0);

        var alerts = new StackPanel { Spacing = 8 };
        alerts.Children.Add(OverviewTitle("Scadenze e avvisi", "Garanzie nei prossimi 90 giorni"));
        if (expiring.Count == 0)
        {
            alerts.Children.Add(OverviewEmpty("Nessuna garanzia in scadenza."));
        }
        else
        {
            foreach (var asset in expiring)
                alerts.Children.Add(OverviewAssetRow(asset, FormatDate(asset.WarrantyEndDate)));
        }
        AddOverviewPanel(grid, alerts, 2);

        var activity = new StackPanel { Spacing = 8 };
        activity.Children.Add(OverviewTitle("Attività recente", "Ultimi asset aggiornati"));
        if (recent.Count == 0)
        {
            activity.Children.Add(OverviewEmpty("Nessuna attività disponibile."));
        }
        else
        {
            foreach (var asset in recent)
            {
                var when = DateTime.TryParse(asset.UpdatedAt, out var date)
                    ? date.ToString("dd/MM/yyyy HH:mm")
                    : "Data non disponibile";
                activity.Children.Add(OverviewAssetRow(asset, when));
            }
        }
        AddOverviewPanel(grid, activity, 4);

        return grid;
    }

    private static void AddOverviewPanel(Grid grid, Control content, int column)
    {
        var panel = new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(16),
            MinHeight = 138,
            Child = content
        };
        Grid.SetColumn(panel, column);
        grid.Children.Add(panel);
    }

    private static Control OverviewTitle(string title, string subtitle)
    {
        var stack = new StackPanel { Spacing = 2 };
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 15,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        stack.Children.Add(new TextBlock
        {
            Text = subtitle,
            FontSize = 11,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });
        return stack;
    }

    private static Control ProgressRow(string label, int value, int total)
    {
        var stack = new StackPanel { Spacing = 4 };
        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
        header.Children.Add(new TextBlock
        {
            Text = label,
            FontSize = 12,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });
        var valueText = new TextBlock
        {
            Text = $"{value} / {total}",
            FontSize = 12,
            FontWeight = FontWeight.SemiBold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        };
        Grid.SetColumn(valueText, 1);
        header.Children.Add(valueText);
        stack.Children.Add(header);
        stack.Children.Add(new ProgressBar
        {
            Minimum = 0,
            Maximum = total,
            Value = value,
            Height = 6
        });
        return stack;
    }

    private static Control OverviewEmpty(string text) => new TextBlock
    {
        Text = text,
        FontSize = 12,
        Foreground = UiTokens.Brush(UiTokens.TextSecondary),
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(0, 8, 0, 0)
    };

    private Control OverviewAssetRow(Asset asset, string trailingText)
    {
        var row = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Margin = new Thickness(0, 3)
        };
        var identity = new StackPanel { Spacing = 1 };
        identity.Children.Add(new TextBlock
        {
            Text = asset.AssetCode,
            FontSize = 12,
            FontWeight = FontWeight.SemiBold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        identity.Children.Add(new TextBlock
        {
            Text = $"{asset.Manufacturer} {asset.Model}".Trim(),
            FontSize = 11,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextTrimming = TextTrimming.CharacterEllipsis
        });
        row.Children.Add(identity);
        var trailing = new TextBlock
        {
            Text = trailingText,
            FontSize = 11,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0)
        };
        Grid.SetColumn(trailing, 1);
        row.Children.Add(trailing);
        return row;
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

        _kpis.Children.Add(CompactKpi("▣", total.ToString(), "Asset totali", null));
        _kpis.Children.Add(CompactKpi("✓", available.ToString(), "Disponibili", "available"));
        _kpis.Children.Add(CompactKpi("↗", assigned.ToString(), "Assegnati", "assigned"));
        _kpis.Children.Add(CompactKpi("⚙", maintenance.ToString(), "Manutenzione", "maintenance"));
        _kpis.Children.Add(CompactKpi("◷", expiringWarranty.ToString(), "Garanzie 90 gg", "warranty"));
    }

    private EnterpriseKpiCard CompactKpi(string icon, string value, string label, string? filter)
    {
        var card = new EnterpriseKpiCard(icon, value, label)
        {
            MinWidth = 158,
            MinHeight = 62,
            Padding = new Thickness(11, 8),
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
        };
        card.PointerPressed += (_, _) => ApplyKpiFilter(filter, label);
        return card;
    }

    private void ConfigureAssetTable()
    {
        _assetTable.CompactRows = true;
        _assetTable.AlternatingRows = true;
        _assetTable.ConfigureColumns(new[]
        {
            new AxEnterpriseColumn<Asset>
            {
                Id = "asset-code",
                IsSortable = true,
                Header = "Codice",
                Width = 118,
                MinWidth = 110,
                CellFactory = asset => Cell(asset.AssetCode, true)
            },
            new AxEnterpriseColumn<Asset>
            {
                Id = "asset",
                IsSortable = true,
                Header = "Asset",
                Width = 285,
                MinWidth = 240,
                CellFactory = AssetIdentityCell
            },
            new AxEnterpriseColumn<Asset>
            {
                Id = "serial-number",
                IsSortable = true,
                Header = "Seriale",
                Width = 155,
                MinWidth = 140,
                TextSelector = asset => string.IsNullOrWhiteSpace(asset.SerialNumber) ? "—" : asset.SerialNumber
            },
            new AxEnterpriseColumn<Asset>
            {
                Id = "assigned-to",
                IsSortable = true,
                Header = "Assegnato a",
                Width = 185,
                MinWidth = 165,
                TextSelector = asset => _assignmentEngine.GetActiveAssignmentForAsset(asset.Id)?.EmployeeName ?? "—"
            },
            new AxEnterpriseColumn<Asset>
            {
                Id = "status",
                IsSortable = true,
                Header = "Stato",
                Width = 145,
                MinWidth = 135,
                Alignment = AxColumnAlignment.Center,
                CellFactory = asset => StatusBadge(asset.Status)
            },
            new AxEnterpriseColumn<Asset>
            {
                Id = "warranty",
                IsSortable = true,
                Header = "Garanzia",
                Width = 135,
                MinWidth = 125,
                TextSelector = asset => FormatDate(asset.WarrantyEndDate)
            }
        });

        _assetTable.SelectionChanged += asset =>
        {
            _selected = asset;
            if (_detailsVisible)
                _details.Content = DetailsCard(asset);
        };
        _assetTable.ItemActivated += asset =>
        {
            _selected = asset;
            ShowDetails();
        };
        _assetTable.SortRequested += (columnId, ascending) =>
        {
            _sortColumnId = columnId;
            _sortAscending = ascending;
            RefreshRows();
        };
    }

    private void RefreshRows()
    {
        var query = (_search.Text ?? string.Empty).Trim().ToLowerInvariant();
        var selectedCategory = _category.SelectedItem?.ToString() ?? "Tutte";
        var selectedStatus = _status.SelectedItem?.ToString() ?? "Tutti";
        var selectedManufacturer = _manufacturer.SelectedItem?.ToString() ?? "Tutti";

        var filteredQuery = _assets.Where(a =>
            (string.IsNullOrWhiteSpace(query) ||
             $"{a.AssetCode} {a.Category} {a.Manufacturer} {a.Model} {a.SerialNumber} {a.Status} {a.OperatingSystem}".ToLowerInvariant().Contains(query)) &&
            (selectedCategory == "Tutte" || a.Category == selectedCategory) &&
            (selectedStatus == "Tutti" || a.Status == selectedStatus) &&
            (selectedManufacturer == "Tutti" || a.Manufacturer == selectedManufacturer) &&
            MatchesKpiFilter(a));

        var filtered = ApplySort(filteredQuery).ToList();

        var visibleSelection = filtered.FirstOrDefault(a => a.Id == _selected?.Id)
            ?? filtered.FirstOrDefault();

        _selected = visibleSelection;
        _assetTable.SetItems(filtered);
        _viewHost.Content = _cardMode ? BuildCardView(filtered) : _assetTable;
        _resultSummary.Text = $"{filtered.Count} di {_assets.Count} asset visualizzati" +
            (_kpiFilter is null ? string.Empty : " | filtro KPI attivo");
        _assetTable.SetSelectedItem(visibleSelection);
        _details.Content = visibleSelection is not null && _detailsVisible ? DetailsCard(visibleSelection) : EmptyDetails();
    }

    private void SetViewMode(bool cardMode)
    {
        if (_cardMode == cardMode)
            return;

        _cardMode = cardMode;
        UpdateViewButtons();
        RefreshRows();
    }

    private void UpdateViewButtons()
    {
        if (_listViewButton is not null)
        {
            _listViewButton.Background = UiTokens.Brush(_cardMode ? UiTokens.SurfaceAlt : UiTokens.BrandBlue);
            _listViewButton.Foreground = _cardMode ? UiTokens.Brush(UiTokens.TextPrimary) : Brushes.White;
        }

        if (_cardViewButton is not null)
        {
            _cardViewButton.Background = UiTokens.Brush(_cardMode ? UiTokens.BrandBlue : UiTokens.SurfaceAlt);
            _cardViewButton.Foreground = _cardMode ? Brushes.White : UiTokens.Brush(UiTokens.TextPrimary);
        }
    }

    private void ApplyKpiFilter(string? filter, string label)
    {
        _kpiFilter = _kpiFilter == filter ? null : filter;
        _status.SelectedIndex = 0;
        RefreshRows();
        ShowMessage(_kpiFilter is null ? "Filtro KPI rimosso." : $"Filtro KPI: {label}.");
    }

    private bool MatchesKpiFilter(Asset asset)
    {
        return _kpiFilter switch
        {
            "available" => asset.Status.Equals("Disponibile", StringComparison.OrdinalIgnoreCase) ||
                           asset.Status.Equals("Attivo", StringComparison.OrdinalIgnoreCase),
            "assigned" => asset.Status.Equals("Assegnato", StringComparison.OrdinalIgnoreCase),
            "maintenance" => asset.Status.Equals("In manutenzione", StringComparison.OrdinalIgnoreCase),
            "warranty" => IsWarrantyExpiring(asset.WarrantyEndDate),
            _ => true
        };
    }

    private Control BuildCardView(IReadOnlyList<Asset> assets)
    {
        _selectedAssetCard = null;

        if (assets.Count == 0)
        {
            return new Border
            {
                Background = UiTokens.Brush(UiTokens.Surface),
                BorderBrush = UiTokens.Brush(UiTokens.Border),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(14),
                Padding = new Thickness(28),
                Child = new TextBlock
                {
                    Text = "Nessun asset corrisponde ai filtri selezionati.",
                    Foreground = UiTokens.Brush(UiTokens.TextSecondary),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                }
            };
        }

        var cards = new WrapPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            ItemWidth = 300,
            ItemHeight = 208
        };

        foreach (var asset in assets)
            cards.Children.Add(BuildAssetCard(asset));

        return new ScrollViewer
        {
            Content = cards,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
        };
    }

    private Control BuildAssetCard(Asset asset)
    {
        var assignment = _assignmentEngine.GetActiveAssignmentForAsset(asset.Id);
        var stack = new StackPanel { Spacing = 10 };

        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
        var identity = new StackPanel { Spacing = 2 };
        identity.Children.Add(new TextBlock
        {
            Text = $"{asset.Manufacturer} {asset.Model}".Trim(),
            FontSize = 17,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            TextTrimming = TextTrimming.CharacterEllipsis
        });
        identity.Children.Add(new TextBlock
        {
            Text = $"{asset.AssetCode} | {asset.Category}",
            FontSize = 12,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextTrimming = TextTrimming.CharacterEllipsis
        });
        Grid.SetColumn(identity, 0);
        header.Children.Add(identity);
        var badge = StatusBadge(asset.Status);
        badge.MinWidth = 0;
        badge.Margin = new Thickness(8, 0, 0, 0);
        Grid.SetColumn(badge, 1);
        header.Children.Add(badge);
        stack.Children.Add(header);

        stack.Children.Add(InspectorRow("Seriale", string.IsNullOrWhiteSpace(asset.SerialNumber) ? "—" : asset.SerialNumber));
        stack.Children.Add(InspectorRow("Assegnato a", assignment?.EmployeeName ?? "—"));
        stack.Children.Add(InspectorRow("Garanzia", FormatDate(asset.WarrantyEndDate)));

        var card = new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            BorderBrush = UiTokens.Brush(_selected?.Id == asset.Id ? UiTokens.BrandBlue : UiTokens.Border),
            BorderThickness = new Thickness(_selected?.Id == asset.Id ? 2 : 1),
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(16),
            Margin = new Thickness(0, 0, 12, 12),
            Width = 288,
            Height = 184,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
            Child = stack
        };
        if (_selected?.Id == asset.Id)
            _selectedAssetCard = card;

        card.PointerPressed += (_, e) =>
        {
            if (_selectedAssetCard is not null && !ReferenceEquals(_selectedAssetCard, card))
            {
                _selectedAssetCard.BorderBrush = UiTokens.Brush(UiTokens.Border);
                _selectedAssetCard.BorderThickness = new Thickness(1);
            }

            _selected = asset;
            _selectedAssetCard = card;
            card.BorderBrush = UiTokens.Brush(UiTokens.BrandBlue);
            card.BorderThickness = new Thickness(2);
            _assetTable.SetSelectedItem(asset);

            if (e.ClickCount >= 2)
            {
                ShowDetails();
                return;
            }

            _details.Content = _detailsVisible ? DetailsCard(asset) : EmptyDetails();
        };
        return card;
    }

    private IEnumerable<Asset> ApplySort(IEnumerable<Asset> assets)
    {
        Func<Asset, string> selector = _sortColumnId switch
        {
            "asset" => asset => $"{asset.Manufacturer} {asset.Model} {asset.Category}",
            "serial-number" => asset => asset.SerialNumber,
            "assigned-to" => asset => _assignmentEngine.GetActiveAssignmentForAsset(asset.Id)?.EmployeeName ?? string.Empty,
            "status" => asset => asset.Status,
            "warranty" => asset => NormalizeDateForSort(asset.WarrantyEndDate),
            _ => asset => asset.AssetCode
        };

        return _sortAscending
            ? assets.OrderBy(selector, StringComparer.OrdinalIgnoreCase)
            : assets.OrderByDescending(selector, StringComparer.OrdinalIgnoreCase);
    }

    private static string NormalizeDateForSort(string value)
    {
        return DateTime.TryParse(value, out var date)
            ? date.ToString("yyyyMMddHHmmss")
            : string.Empty;
    }

    private void ArrangeAssetWorkspace(double width)
    {
        if (_masterLayout is null || _detailsHost is null)
            return;

        _adaptiveContent.Children.Clear();
        _adaptiveContent.RowDefinitions = new RowDefinitions("*");

        if (!_detailsVisible)
        {
            _adaptiveContent.ColumnDefinitions = new ColumnDefinitions("*");
            Grid.SetColumn(_masterLayout, 0);
            Grid.SetRow(_masterLayout, 0);
            _adaptiveContent.Children.Add(_masterLayout);
            return;
        }

        var detailsWidth = width >= 1500 ? 360 : 330;
        _adaptiveContent.ColumnDefinitions = new ColumnDefinitions($"*,12,{detailsWidth}");
        Grid.SetColumn(_masterLayout, 0);
        Grid.SetRow(_masterLayout, 0);
        _adaptiveContent.Children.Add(_masterLayout);
        Grid.SetColumn(_detailsHost, 2);
        Grid.SetRow(_detailsHost, 0);
        _adaptiveContent.Children.Add(_detailsHost);
    }

    private void ToggleDetails()
    {
        if (_detailsVisible)
            HideDetails();
        else
            ShowDetails();
    }

    private void ShowDetails()
    {
        _detailsVisible = true;
        _details.Content = _selected is not null ? DetailsCard(_selected) : EmptyDetails();
        ArrangeAssetWorkspace(Bounds.Width);
    }

    private void HideDetails()
    {
        _detailsVisible = false;
        ArrangeAssetWorkspace(Bounds.Width);
    }

    private Control AssetIdentityCell(Asset asset)
    {
        var panel = new StackPanel { Spacing = 1 };
        panel.Children.Add(new TextBlock
        {
            Text = $"{asset.Manufacturer} {asset.Model}".Trim(),
            FontWeight = FontWeight.SemiBold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 270
        });
        panel.Children.Add(new TextBlock
        {
            Text = asset.Category,
            FontSize = 11,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 270
        });
        return panel;
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
            Text = "CENTRO DI COMANDO ASSET",
            FontSize = 11,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        Grid.SetColumn(label, 0);
        eyebrow.Children.Add(label);

        var closeHint = new Button
        {
            Content = "×",
            FontSize = 18,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(6, 0),
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        closeHint.Click += (_, _) => HideDetails();
        Grid.SetColumn(closeHint, 1);
        eyebrow.Children.Add(closeHint);
        content.Children.Add(eyebrow);

        var identity = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("52,12,*"),
            Margin = new Thickness(0, 0, 0, 2)
        };
        var icon = new Border
        {
            Width = 52,
            Height = 52,
            CornerRadius = new CornerRadius(16),
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            Child = new TextBlock
            {
                Text = CategoryIcon(asset.Category),
                FontSize = 22,
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
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            TextWrapping = TextWrapping.Wrap
        });
        identityText.Children.Add(new TextBlock
        {
            Text = $"{asset.AssetCode} | {asset.Category}",
            FontSize = 12,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });
        Grid.SetColumn(identityText, 2);
        identity.Children.Add(identityText);
        content.Children.Add(identity);

        var quickActions = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,8,*"),
            RowDefinitions = new RowDefinitions("Auto,8,Auto")
        };
        Add(quickActions, InspectorActionButton("✎", "Modifica", () => OpenEditAsset(asset)), 0, 0);
        Add(quickActions, InspectorActionButton("↗", "Assegna", () => _ = OpenAssignAsset(asset)), 2, 0);
        Add(quickActions, InspectorActionButton("↙", "Restituisci", () => ReturnAsset(asset)), 0, 2);
        Add(quickActions, InspectorActionButton("▣", "Duplica", () => DuplicateAsset(asset)), 2, 2);
        content.Children.Add(quickActions);

        var sections = new StackPanel { Spacing = 10 };
        sections.Children.Add(InspectorSection(
            "Informazioni generali",
            "Anagrafica, inventario e sicurezza",
            InspectorRow("Codice", asset.AssetCode),
            InspectorRow("Categoria", asset.Category),
            InspectorRow("Produttore", asset.Manufacturer),
            InspectorRow("Modello", asset.Model),
            InspectorRow("Seriale", asset.SerialNumber),
            InspectorRow("Asset Tag", asset.AssetTag),
            InspectorRow("Sistema operativo", asset.OperatingSystem),
            InspectorRow("BitLocker", asset.BitLockerEnabled ? "Abilitato" : "Non abilitato")));

        sections.Children.Add(InspectorSection(
            "Assegnazione",
            assignment is null ? "Asset disponibile per una nuova consegna" : "Assegnazione attiva",
            InspectorRow("Assegnato a", assignment?.EmployeeName ?? "—"),
            InspectorRow("Data assegnazione", assignment is null ? "—" : FormatDate(assignment.AssignedAt)),
            InspectorRow("Stato", assignment?.Status ?? "Disponibile"),
            InspectorPlaceholder(assignment is null
                ? "Usa il comando Assegna per avviare la procedura di consegna."
                : "Usa il comando Restituisci per chiudere l'assegnazione corrente.")));

        sections.Children.Add(WarrantySection(asset));

        sections.Children.Add(InspectorSection(
            "Manutenzioni",
            "Interventi, verifiche e assistenza tecnica",
            InspectorPlaceholder("Nessun intervento di manutenzione collegato a questo asset.")));

        sections.Children.Add(InspectorSection(
            "Documenti",
            "Schede, verbali e allegati associati",
            InspectorPlaceholder("La scheda asset è disponibile tramite il comando Stampa scheda. I verbali di consegna saranno collegati nel prossimo sprint.")));

        if (!string.IsNullOrWhiteSpace(asset.Notes))
        {
            sections.Children.Add(InspectorSection(
                "Note",
                "Informazioni operative",
                new TextBlock
                {
                    Text = asset.Notes,
                    FontSize = 12,
                    Foreground = UiTokens.Brush(UiTokens.TextPrimary),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 6, 0, 4)
                }));
        }

        var deliveryHistory = _deliveryRecords.GetByAsset(asset.Id);
        var timelineRows = BuildDeliveryTimeline(asset, deliveryHistory);
        sections.Children.Add(InspectorSection(
            "Timeline",
            deliveryHistory.Count == 0
                ? "Cronologia dell'asset"
                : $"{deliveryHistory.Count} movimenti di consegna registrati",
            timelineRows));

        var scroll = new ScrollViewer
        {
            Content = sections,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            MaxHeight = 560
        };
        content.Children.Add(scroll);

        var footerActions = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,8,*"),
            Margin = new Thickness(0, 2, 0, 0)
        };
        Add(footerActions, InspectorSecondaryButton("Stampa scheda", () => PrintAssetSheet(asset)), 0, 0);
        Add(footerActions, InspectorSecondaryButton("Elimina", () => DeleteAsset(asset), true), 2, 0);
        content.Children.Add(footerActions);

        return new AxInspectorPanel(content);
    }

    private Control[] BuildDeliveryTimeline(Asset asset, IReadOnlyList<DeliveryRecord> records)
    {
        var employees = _assignmentEngine.GetEmployees()
            .GroupBy(employee => employee.MasterEmployeeId)
            .ToDictionary(group => group.Key, group => group.First().FullName);

        var rows = new List<Control>
        {
            TimelineEntry(
                "Asset creato",
                FormatDate(asset.CreatedAt),
                string.IsNullOrWhiteSpace(asset.AssetCode) ? "Nuovo asset" : asset.AssetCode,
                UiTokens.BrandBlue)
        };

        foreach (var record in records.OrderByDescending(record => ParseTimelineDate(record.DeliveryDate)))
        {
            var employeeName = employees.TryGetValue(record.EmployeeId, out var name)
                ? name
                : $"Dipendente #{record.EmployeeId}";

            rows.Add(TimelineEntry(
                DeliveryStatusLabel(record.Status),
                FormatDate(record.DeliveryDate),
                employeeName,
                DeliveryStatusColor(record.Status)));

            if (!string.IsNullOrWhiteSpace(record.ReturnDate))
            {
                rows.Add(TimelineEntry(
                    "Riconsegnato",
                    FormatDate(record.ReturnDate),
                    BuildReturnTimelineDescription(record, employeeName),
                    UiTokens.Success,
                    record.ReturnPdfPath));
            }
            else if (!string.IsNullOrWhiteSpace(record.Notes))
            {
                rows.Add(InspectorPlaceholder(record.Notes));
            }
        }

        if (records.Count == 0)
            rows.Add(InspectorPlaceholder("Nessuna consegna registrata per questo asset."));

        return rows.ToArray();
    }

    private static Control TimelineEntry(
        string title,
        string date,
        string description,
        string colorToken,
        string pdfPath = "")
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("16,*"),
            Margin = new Thickness(0, 4)
        };

        var marker = new Border
        {
            Width = 9,
            Height = 9,
            CornerRadius = new CornerRadius(5),
            Background = UiTokens.Brush(colorToken),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            Margin = new Thickness(0, 5, 0, 0)
        };
        Grid.SetColumn(marker, 0);
        grid.Children.Add(marker);

        var content = new StackPanel { Spacing = 2 };
        content.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 12,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        content.Children.Add(new TextBlock
        {
            Text = date,
            FontSize = 11,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });
        content.Children.Add(new TextBlock
        {
            Text = description,
            FontSize = 11,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap
        });
        if (!string.IsNullOrWhiteSpace(pdfPath) && File.Exists(pdfPath))
        {
            content.Children.Add(InspectorSecondaryButton(
                "Apri verbale di riconsegna",
                () => Process.Start(new ProcessStartInfo
                {
                    FileName = pdfPath,
                    UseShellExecute = true
                })));
        }
        Grid.SetColumn(content, 1);
        grid.Children.Add(content);

        return new Border
        {
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(0, 5, 0, 9),
            Child = grid
        };
    }

    private static string BuildReturnTimelineDescription(DeliveryRecord record, string employeeName)
    {
        var details = new List<string> { employeeName };
        if (!string.IsNullOrWhiteSpace(record.ReturnCondition))
            details.Add($"Condizioni: {record.ReturnCondition}");
        if (!string.IsNullOrWhiteSpace(record.ReturnNotes))
            details.Add(record.ReturnNotes);
        return string.Join(" · ", details);
    }

    private static DateTime ParseTimelineDate(string value) =>
        DateTime.TryParse(value, out var parsed) ? parsed : DateTime.MinValue;

    private static string DeliveryStatusLabel(string status) => status switch
    {
        DeliveryRecordStatus.Active => "Consegnato",
        DeliveryRecordStatus.Returned => "Consegna chiusa",
        DeliveryRecordStatus.Cancelled => "Consegna annullata",
        DeliveryRecordStatus.Planned => "Consegna pianificata",
        _ => status
    };

    private static string DeliveryStatusColor(string status) => status switch
    {
        DeliveryRecordStatus.Active => UiTokens.BrandBlue,
        DeliveryRecordStatus.Returned => UiTokens.Success,
        DeliveryRecordStatus.Cancelled => UiTokens.Danger,
        DeliveryRecordStatus.Planned => UiTokens.Warning,
        _ => UiTokens.TextSecondary
    };

    private static IReadOnlyList<string> BuildCompanyDetailLines(CompanySettings company, string? layout)
    {
        var location = string.Join(" | ", new[]
        {
            company.Address,
            string.Join(" ", new[] { company.City, company.Province }.Where(value => !string.IsNullOrWhiteSpace(value))),
            company.Country
        }.Where(value => !string.IsNullOrWhiteSpace(value)));
        var tax = string.Join(" | ", new[]
        {
            PrefixCompanyValue("P.IVA", company.VatNumber), PrefixCompanyValue("C.F.", company.FiscalCode)
        }.Where(value => !string.IsNullOrWhiteSpace(value)));
        var contacts = string.Join(" | ", new[]
        {
            PrefixCompanyValue("Tel.", company.Phone), company.Email
        }.Where(value => !string.IsNullOrWhiteSpace(value)));
        var digital = string.Join(" | ", new[]
        {
            PrefixCompanyValue("PEC", company.Pec), company.Website
        }.Where(value => !string.IsNullOrWhiteSpace(value)));
        var lines = new[] { location, tax, contacts, digital }.Where(value => !string.IsNullOrWhiteSpace(value)).ToList();
        return string.Equals(layout, "Compatta", StringComparison.OrdinalIgnoreCase) ? lines.Take(1).ToList() : lines;
    }

    private static string BuildCompanyDetails(CompanySettings company) =>
        string.Join(" | ", BuildCompanyDetailLines(company, "Corporate"));

    private static string PrefixCompanyValue(string label, string value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : $"{label} {value.Trim()}";

    private void PrintAssetSheet(Asset asset)
    {
        try
        {
            var settingsService = new SettingsService();
            var settings = settingsService.Load();
            var assignment = _assignmentEngine.GetActiveAssignmentForAsset(asset.Id);

            var document = new SimplePdfDocument
            {
                Title = $"Scheda asset {asset.AssetCode}"
            };

            var companyName = string.IsNullOrWhiteSpace(settings.Company.LegalName)
                ? settings.Company.CompanyName
                : settings.Company.LegalName;
            var template = settings.DocumentTemplate ?? new DocumentTemplateSettings();

            document.Author = string.IsNullOrWhiteSpace(companyName) ? "Accyourate Enterprise X" : companyName;
            document.Branding.CompanyName = document.Author;
            document.Branding.CompanyDetails = BuildCompanyDetails(settings.Company);
            document.Branding.CompanyDetailLines.AddRange(BuildCompanyDetailLines(settings.Company, template.HeaderLayout));
            document.Branding.HeaderLayout = template.HeaderLayout;
            document.Branding.LogoPath = settings.Company.LogoPath;
            document.Branding.LogoSize = template.LogoSize;
            document.Branding.LogoPosition = template.LogoPosition;
            document.Branding.PrimaryColor = template.PrimaryColor;
            document.Branding.DocumentLabel = "SCHEDA PATRIMONIALE ASSET";
            document.Branding.DocumentCode = $"AST-{ValueOrDash(asset.AssetCode)}";
            document.Branding.FooterText = template.FooterText;
            document.Branding.ShowLogo = template.ShowLogo;
            document.Branding.ShowCompanyDetails = template.ShowCompanyDetails;
            document.Branding.ShowDocumentMetadata = template.ShowDocumentMetadata;
            document.Branding.ShowFooter = template.ShowFooter;
            document.Branding.DocumentVersion = template.DocumentVersion;
            document.Branding.ConfidentialityText = template.ConfidentialityText;
            document.Branding.ShowPageNumber = template.ShowPageNumber;
            document.Branding.ShowPrintTimestamp = template.ShowPrintTimestamp;

            document.AddTitle("Scheda identificativa asset");
            document.AddStatus("Stato asset", ValueOrDash(asset.Status));

            document.AddHeading("Identità");
            document.AddKeyValue("Codice asset", ValueOrDash(asset.AssetCode));
            document.AddKeyValue("Categoria", ValueOrDash(asset.Category));
            document.AddKeyValue("Produttore", ValueOrDash(asset.Manufacturer));
            document.AddKeyValue("Modello", ValueOrDash(asset.Model));

            document.AddHeading("Inventario e sicurezza");
            document.AddKeyValue("Numero seriale", ValueOrDash(asset.SerialNumber));
            document.AddKeyValue("Asset tag", ValueOrDash(asset.AssetTag));
            document.AddKeyValue("Sistema operativo", ValueOrDash(asset.OperatingSystem));
            document.AddKeyValue("BitLocker", asset.BitLockerEnabled ? "Attivo" : "Non attivo");

            document.AddHeading("Assegnazione");
            document.AddKeyValue("Assegnato a", ValueOrDash(assignment?.EmployeeName));

            document.AddHeading("Acquisto e garanzia");
            document.AddKeyValue("Data acquisto", FormatDate(asset.PurchaseDate));
            document.AddKeyValue("Scadenza garanzia", FormatDate(asset.WarrantyEndDate));
            document.AddStatus("Stato garanzia", WarrantyLabel(asset.WarrantyEndDate));

            if (!string.IsNullOrWhiteSpace(asset.Notes))
            {
                document.AddBlank();
                document.AddHeading("Note");
                document.AddText(asset.Notes);
            }

            document.AddBlank();
            document.AddHeading("Tracciamento");
            document.AddKeyValue("Creato", FormatDate(asset.CreatedAt));
            document.AddKeyValue("Ultimo aggiornamento", FormatDate(asset.UpdatedAt));
            if (template.ShowQrCodePlaceholder)
                document.AddQrCode(
                    QrDestinationBuilder.Build(
                        template,
                        "assets",
                        ValueOrDash(asset.AssetCode),
                        new[]
                        {
                            "Accyourate Enterprise X",
                            "Scheda asset",
                            $"Codice: {ValueOrDash(asset.AssetCode)}",
                            $"Seriale: {ValueOrDash(asset.SerialNumber)}",
                            $"Modello: {ValueOrDash(asset.Manufacturer)} {ValueOrDash(asset.Model)}"
                        }),
                    $"QR {ValueOrDash(asset.AssetCode)}");

            if (template.ShowSignatures)
            {
                document.AddBlank(18);
                document.AddSignaturePair(
                    string.IsNullOrWhiteSpace(template.LeftSignatureLabel) ? "Consegnato da" : template.LeftSignatureLabel,
                    string.IsNullOrWhiteSpace(template.RightSignatureLabel) ? "Ricevuto da" : template.RightSignatureLabel);
            }

            var root = string.IsNullOrWhiteSpace(settings.Documents.DocumentRootPath)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Accyourate Enterprise X")
                : settings.Documents.DocumentRootPath;
            var folder = Path.Combine(root, "Schede Asset");
            var fileName = $"Scheda_Asset_{ValueOrDash(asset.AssetCode)}_{DateTime.Now:yyyyMMdd_HHmmss}";
            var path = new PdfExportService().Export(document, folder, fileName);

            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });

            ShowMessage($"Scheda PDF generata e aperta: {path}");
        }
        catch (Exception ex)
        {
            ShowMessage($"Impossibile stampare la scheda: {ex.Message}", true);
        }
    }

    private static string ValueOrDash(string? value) => string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();

    private static string WarrantyLabel(string value)
    {
        if (!DateTime.TryParse(value, out var expiry)) return "Non disponibile";
        var days = (expiry.Date - DateTime.Today).Days;
        if (days < 0) return $"Scaduta da {Math.Abs(days)} giorni";
        if (days == 0) return "Scade oggi";
        if (days <= 90) return $"In scadenza tra {days} giorni";
        return $"Valida ({days} giorni residui)";
    }

    private static Control InspectorSection(string title, string subtitle, params Control[] rows)
    {
        var stack = new StackPanel { Spacing = 0 };
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 13,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        stack.Children.Add(new TextBlock
        {
            Text = subtitle,
            FontSize = 11,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            Margin = new Thickness(0, 2, 0, 6),
            TextWrapping = TextWrapping.Wrap
        });
        foreach (var row in rows)
            stack.Children.Add(row);

        return new Border
        {
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(12, 10),
            Child = stack
        };
    }

    private static Control WarrantySection(Asset asset)
    {
        var statusText = "Data non disponibile";
        var statusColor = UiTokens.TextSecondary;
        var remainingText = "—";

        if (DateTime.TryParse(asset.WarrantyEndDate, out var warrantyEnd))
        {
            var days = (warrantyEnd.Date - DateTime.Today).Days;
            remainingText = days >= 0 ? $"{days} giorni" : $"Scaduta da {Math.Abs(days)} giorni";
            if (days < 0)
            {
                statusText = "Garanzia scaduta";
                statusColor = UiTokens.Danger;
            }
            else if (days <= 90)
            {
                statusText = "In scadenza";
                statusColor = UiTokens.Warning;
            }
            else
            {
                statusText = "Garanzia valida";
                statusColor = UiTokens.Success;
            }
        }

        var badge = new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            BorderBrush = UiTokens.Brush(statusColor),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(9, 6),
            Margin = new Thickness(0, 3, 0, 7),
            Child = new TextBlock
            {
                Text = statusText,
                FontSize = 11,
                FontWeight = FontWeight.Bold,
                Foreground = UiTokens.Brush(statusColor)
            }
        };

        return InspectorSection(
            "Garanzia",
            "Copertura e scadenze",
            badge,
            InspectorRow("Data acquisto", FormatDate(asset.PurchaseDate)),
            InspectorRow("Scadenza", FormatDate(asset.WarrantyEndDate)),
            InspectorRow("Tempo residuo", remainingText));
    }

    private static Button InspectorActionButton(string icon, string text, Action action)
    {
        var button = new Button
        {
            Content = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Spacing = 7,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Children =
                {
                    new TextBlock { Text = icon, FontSize = 14 },
                    new TextBlock { Text = text, FontSize = 12, FontWeight = FontWeight.SemiBold }
                }
            },
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(10, 8)
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static Button InspectorSecondaryButton(string text, Action action, bool danger = false)
    {
        var button = new Button
        {
            Content = text,
            Background = Brushes.Transparent,
            Foreground = UiTokens.Brush(danger ? UiTokens.Danger : UiTokens.TextSecondary),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(10, 8),
            FontSize = 12,
            FontWeight = FontWeight.SemiBold
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static Control InspectorPlaceholder(string text)
    {
        return new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            CornerRadius = new CornerRadius(9),
            Padding = new Thickness(10, 8),
            Margin = new Thickness(0, 7, 0, 2),
            Child = new TextBlock
            {
                Text = text,
                FontSize = 11,
                Foreground = UiTokens.Brush(UiTokens.TextSecondary),
                TextWrapping = TextWrapping.Wrap
            }
        };
    }

    private static string CategoryIcon(string category)
    {
        var value = category?.Trim().ToLowerInvariant() ?? string.Empty;
        return value switch
        {
            var text when text.Contains("notebook") || text.Contains("laptop") || text.Contains("mac") => "▰",
            var text when text.Contains("desktop") || text.Contains("pc") => "▣",
            var text when text.Contains("smartphone") || text.Contains("phone") => "▯",
            var text when text.Contains("stamp") || text.Contains("printer") => "▤",
            var text when text.Contains("server") => "▥",
            _ => "◆"
        };
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

            if (_deliveryRecords.HasActiveForAsset(result.AssetId))
            {
                ShowMessage("L'asset ha già una consegna attiva nel registro.", true);
                return;
            }

            _assignmentEngine.AssignAsset(
                result.AssetId,
                result.MasterEmployeeId,
                "Asset Management",
                result.Notes);

            _deliveryRecords.Create(new DeliveryRecord
            {
                AssetId = result.AssetId,
                EmployeeId = result.MasterEmployeeId,
                DeliveryDate = result.DeliveryDate.ToString("s"),
                Notes = result.Notes,
                Status = DeliveryRecordStatus.Active
            });

            var message = "Asset assegnato e consegna registrata.";
            if (result.GenerateReport)
            {
                var assignment = _assignmentEngine.GetActiveAssignmentForAsset(result.AssetId)
                    ?? throw new InvalidOperationException("Assegnazione attiva non trovata dopo il salvataggio.");
                var reportId = _deliveryReports.CreateFromAssignment(
                    assignment,
                    "Asset Management",
                    result.Notes);
                var pdfPath = _deliveryReports.GeneratePdf(reportId, "Asset Management");
                message = $"Consegna registrata e verbale creato: {pdfPath}";
            }

            ShowMessage(message);
            Load();
        }
        catch (Exception ex)
        {
            ShowMessage($"Errore assegnazione asset: {ex.Message}", true);
        }
    }

    private async void ReturnAsset(Asset asset)
    {
        try
        {
            var assignment = _assignmentEngine.GetActiveAssignmentForAsset(asset.Id);
            if (assignment is null)
            {
                ShowMessage("Questo asset non ha assegnazioni attive.", true);
                return;
            }

            var delivery = _deliveryRecords.GetActiveByAsset(asset.Id);
            if (delivery is null)
            {
                ShowMessage("Non è presente una consegna attiva nel registro.", true);
                return;
            }

            var dialog = new AssetReturnDialog(asset.AssetCode, assignment.EmployeeName);
            var result = await ShowReturnDialog(dialog);
            if (result is null)
                return;

            delivery.ReturnDate = result.ReturnDate.ToString("s");
            delivery.ReturnCondition = result.Condition;
            delivery.ReturnNotes = result.Notes;
            var pdfPath = result.GeneratePdf
                ? _returnReports.Generate(delivery, asset, assignment.EmployeeName, "Asset Management")
                : string.Empty;

            _assignmentEngine.ReturnAssignment(assignment.AssignmentId, "Restituito da Asset Management.");
            _deliveryRecords.MarkReturned(
                delivery.Id,
                result.ReturnDate,
                "Restituito da Asset Management.",
                result.Condition,
                result.Notes,
                pdfPath);

            ShowMessage(string.IsNullOrWhiteSpace(pdfPath)
                ? "Asset restituito correttamente."
                : $"Asset restituito e verbale generato: {pdfPath}");
            if (!string.IsNullOrWhiteSpace(pdfPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = pdfPath,
                    UseShellExecute = true
                });
            }
            Load();
        }
        catch (Exception ex)
        {
            ShowMessage($"Errore restituzione asset: {ex.Message}", true);
        }
    }

    private async Task<AssetReturnDialogResult?> ShowReturnDialog(AssetReturnDialog dialog)
    {
        var owner = TopLevel.GetTopLevel(this) as Window;
        if (owner is not null)
            return await dialog.ShowDialog<AssetReturnDialogResult?>(owner);

        dialog.Show();
        return null;
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

    private void DuplicateAsset(Asset source)
    {
        var copy = new Asset
        {
            AssetCode = BuildDuplicateAssetCode(source.AssetCode),
            Category = source.Category,
            Manufacturer = source.Manufacturer,
            Model = source.Model,
            SerialNumber = string.Empty,
            AssetTag = string.Empty,
            Status = "Da verificare",
            PurchaseDate = source.PurchaseDate,
            WarrantyEndDate = source.WarrantyEndDate,
            OperatingSystem = source.OperatingSystem,
            BitLockerEnabled = source.BitLockerEnabled,
            Notes = string.IsNullOrWhiteSpace(source.Notes)
                ? $"Duplicato da {source.AssetCode}."
                : $"{source.Notes}\nDuplicato da {source.AssetCode}."
        };

        OpenEditAsset(copy);
    }

    private string BuildDuplicateAssetCode(string sourceCode)
    {
        var baseCode = string.IsNullOrWhiteSpace(sourceCode) ? "ASSET" : sourceCode.Trim();
        var candidate = $"{baseCode}-COPY";
        var suffix = 2;

        while (_service.AssetCodeExists(candidate))
            candidate = $"{baseCode}-COPY-{suffix++}";

        return candidate;
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
        _kpiFilter = null;
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
        var badge = AxStatusBadge.FromStatus(status);
        badge.Margin = new Thickness(8, 0);
        badge.MinWidth = 126;
        badge.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
        return badge;
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

    private static Control CreateToolbarLabel(string icon, string text)
    {
        var label = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        label.Children.Add(new TextBlock { Text = icon, FontSize = 15, FontWeight = FontWeight.SemiBold });
        label.Children.Add(new TextBlock { Text = text, FontSize = 12, FontWeight = FontWeight.SemiBold });
        return label;
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
