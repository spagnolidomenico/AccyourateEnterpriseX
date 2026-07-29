using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Models;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.Platform.Notifications;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class MaintenanceOperationsView : UserControl
{
    private readonly MaintenanceRepository _repository = new();
    private readonly AssetService _assets = new();
    private readonly AssetAssignmentEngine _assignments = new();
    private readonly MaintenancePdfService _pdf = new();
    private readonly NotificationService _notifications = new();
    private readonly TextBox _search = new();
    private readonly ComboBox _status = new();
    private readonly ComboBox _priority = new();
    private readonly CheckBox _overdueOnly = new() { Content = "Solo scadute" };
    private readonly StackPanel _kpis = new() { Orientation = Orientation.Horizontal, Spacing = 10 };
    private readonly StackPanel _rows = new();
    private readonly TextBlock _summary = new();
    private readonly TextBlock _message = new();

    public MaintenanceOperationsView()
    {
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();
        Load();
    }

    public event Action<int>? AssetRequested;

    private Control BuildLayout()
    {
        var root = new DockPanel();
        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Margin = new Thickness(24, 20, 24, 12)
        };
        var heading = new StackPanel { Spacing = 4 };
        heading.Children.Add(new TextBlock
        {
            Text = "Centro Manutenzioni",
            FontSize = 30,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        heading.Children.Add(new TextBlock
        {
            Text = "Scadenze, interventi, tecnici, costi e verbali del patrimonio aziendale.",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });
        header.Children.Add(heading);
        var refresh = ActionButton("Aggiorna", () => Load(), true);
        Grid.SetColumn(refresh, 1);
        header.Children.Add(refresh);
        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        var kpiScroll = new ScrollViewer
        {
            Content = _kpis,
            Margin = new Thickness(24, 0, 24, 12),
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
        };
        DockPanel.SetDock(kpiScroll, Dock.Top);
        root.Children.Add(kpiScroll);

        var filters = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,150,150,120,110"),
            Margin = new Thickness(24, 0, 24, 10)
        };
        _search.Watermark = "Cerca asset, intervento, tecnico...";
        _search.TextChanged += (_, _) => Load(false);
        Add(filters, _search, 0);
        _status.ItemsSource = new[] { "Tutti gli stati", "Aperto", "In lavorazione", "Completato" };
        _status.SelectedIndex = 0;
        _status.SelectionChanged += (_, _) => Load(false);
        Add(filters, _status, 1);
        _priority.ItemsSource = new[] { "Tutte le priorità", "Bassa", "Media", "Alta", "Urgente" };
        _priority.SelectedIndex = 0;
        _priority.SelectionChanged += (_, _) => Load(false);
        Add(filters, _priority, 2);
        _overdueOnly.VerticalAlignment = VerticalAlignment.Center;
        _overdueOnly.HorizontalAlignment = HorizontalAlignment.Center;
        _overdueOnly.IsCheckedChanged += (_, _) => Load(false);
        Add(filters, _overdueOnly, 3);
        Add(filters, ActionButton("Reimposta", ResetFilters), 4);
        DockPanel.SetDock(filters, Dock.Top);
        root.Children.Add(filters);

        _message.Margin = new Thickness(24, 0, 24, 8);
        _message.TextWrapping = TextWrapping.Wrap;
        DockPanel.SetDock(_message, Dock.Top);
        root.Children.Add(_message);
        _summary.Margin = new Thickness(24, 0, 24, 8);
        _summary.Foreground = UiTokens.Brush(UiTokens.TextSecondary);
        DockPanel.SetDock(_summary, Dock.Top);
        root.Children.Add(_summary);

        var table = new StackPanel { MinWidth = 1190 };
        table.Children.Add(BuildHeader());
        table.Children.Add(_rows);
        root.Children.Add(new ScrollViewer
        {
            Content = table,
            Margin = new Thickness(24, 0, 24, 24),
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        });
        return root;
    }

    private void Load(bool publishOverdueNotifications = true)
    {
        try
        {
            _message.Text = string.Empty;
            var assets = _assets.GetAssets().ToDictionary(asset => asset.Id);
            var all = _repository.GetAll();
            if (publishOverdueNotifications)
                PublishOverdueNotifications(all, assets);

            var visible = all
                .Where(MatchesStatus)
                .Where(MatchesPriority)
                .Where(ticket => _overdueOnly.IsChecked != true || IsOverdue(ticket))
                .Where(ticket => MatchesSearch(ticket, assets))
                .ToList();

            BuildKpis(all);
            _rows.Children.Clear();
            for (var index = 0; index < visible.Count; index++)
            {
                assets.TryGetValue(visible[index].AssetId, out var asset);
                _rows.Children.Add(BuildRow(visible[index], asset, index));
            }
            if (visible.Count == 0)
                _rows.Children.Add(EmptyState());

            var totalCost = all.Where(ticket => ticket.Status == "Completato").Sum(ticket => ticket.Cost);
            _summary.Text = $"{visible.Count} interventi visualizzati · costo storico EUR {totalCost:N2}";
        }
        catch (Exception ex)
        {
            ShowMessage($"Errore caricamento manutenzioni: {ex.Message}", true);
        }
    }

    private void BuildKpis(IReadOnlyList<MaintenanceTicket> tickets)
    {
        _kpis.Children.Clear();
        _kpis.Children.Add(Kpi("Aperti", tickets.Count(ticket => ticket.Status == "Aperto"), UiTokens.BrandBlue));
        _kpis.Children.Add(Kpi("In lavorazione", tickets.Count(ticket => ticket.Status == "In lavorazione"), UiTokens.Warning));
        _kpis.Children.Add(Kpi("Urgenti", tickets.Count(ticket => ticket.Priority == "Urgente" && ticket.Status != "Completato"), UiTokens.Danger));
        _kpis.Children.Add(Kpi("Scaduti", tickets.Count(IsOverdue), UiTokens.Danger));
        _kpis.Children.Add(Kpi("Completati", tickets.Count(ticket => ticket.Status == "Completato"), UiTokens.Success));
    }

    private static Control Kpi(string label, int value, string color) => new Border
    {
        MinWidth = 168,
        Background = UiTokens.Brush(UiTokens.Surface),
        BorderBrush = UiTokens.Brush(UiTokens.Border),
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(12),
        Padding = new Thickness(16, 11),
        Child = new StackPanel
        {
            Children =
            {
                new TextBlock { Text = value.ToString(), FontSize = 25, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(color) },
                new TextBlock { Text = label, Foreground = UiTokens.Brush(UiTokens.TextSecondary) }
            }
        }
    };

    private Control BuildHeader()
    {
        var grid = RowGrid();
        AddHeader(grid, "Asset", 0);
        AddHeader(grid, "Intervento", 1);
        AddHeader(grid, "Stato", 2, true);
        AddHeader(grid, "Priorità", 3, true);
        AddHeader(grid, "Tecnico", 4);
        AddHeader(grid, "Scadenza", 5);
        AddHeader(grid, "Costo", 6);
        AddHeader(grid, "Asset", 7, true);
        AddHeader(grid, "Verbale", 8, true);
        AddHeader(grid, "Azione", 9, true);
        return new Border
        {
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10, 9),
            Child = grid
        };
    }

    private Control BuildRow(MaintenanceTicket ticket, Asset? asset, int index)
    {
        var grid = RowGrid();
        AddText(grid, asset?.AssetCode ?? $"Asset #{ticket.AssetId}", 0, true);
        AddText(grid, ticket.Title, 1, true);
        Add(grid, Badge(ticket.Status, StatusColor(ticket)), 2);
        Add(grid, Badge(ticket.Priority, PriorityColor(ticket.Priority)), 3);
        AddText(grid, ticket.Technician, 4);
        AddText(grid, Date(ticket.ScheduledAt), 5, false, IsOverdue(ticket));
        AddText(grid, ticket.Cost > 0 ? $"EUR {ticket.Cost:N2}" : "—", 6);
        Add(grid, ActionButton("Apri", () => AssetRequested?.Invoke(ticket.AssetId)), 7);

        var pdf = ActionButton("PDF", () => OpenPdf(ticket));
        pdf.IsEnabled = !string.IsNullOrWhiteSpace(ticket.PdfPath) && File.Exists(ticket.PdfPath);
        Add(grid, pdf, 8);

        var action = ticket.Status switch
        {
            "Aperto" => ActionButton("Avvia", () => Start(ticket)),
            "In lavorazione" => ActionButton("Completa", () => Complete(ticket, asset), true),
            _ => ActionButton("Chiusa", () => { })
        };
        action.IsEnabled = ticket.Status != "Completato";
        Add(grid, action, 9);

        return new Border
        {
            Background = UiTokens.Brush(index % 2 == 0 ? UiTokens.Surface : UiTokens.SurfaceAlt),
            BorderBrush = IsOverdue(ticket) ? UiTokens.Brush(UiTokens.Danger) : UiTokens.Brush(UiTokens.Border),
            BorderThickness = IsOverdue(ticket) ? new Thickness(3, 0, 1, 1) : new Thickness(1, 0, 1, 1),
            Padding = new Thickness(10, 7),
            MinHeight = 52,
            Child = grid
        };
    }

    private void Start(MaintenanceTicket ticket)
    {
        try
        {
            _repository.Start(ticket.Id);
            ShowMessage("Intervento avviato.");
            Load(false);
        }
        catch (Exception ex) { ShowMessage($"Errore avvio intervento: {ex.Message}", true); }
    }

    private async void Complete(MaintenanceTicket ticket, Asset? asset)
    {
        try
        {
            asset ??= _assets.GetAssetById(ticket.AssetId)
                ?? throw new InvalidOperationException("Asset non trovato.");
            var owner = TopLevel.GetTopLevel(this) as Window;
            if (owner is null) return;
            var result = await new MaintenanceCompletionDialog(ticket.Title)
                .ShowDialog<MaintenanceCompletionResult?>(owner);
            if (result is null) return;

            ticket.Status = "Completato";
            ticket.ClosedAt = DateTime.Now.ToString("s");
            ticket.ResolutionNotes = result.Resolution;
            ticket.Cost = result.Cost;
            var path = result.GeneratePdf ? _pdf.Generate(ticket, asset, "Centro Manutenzioni") : string.Empty;
            _repository.Complete(ticket.Id, result.Resolution, result.Cost, path);
            asset.Status = _assignments.GetActiveAssignmentForAsset(asset.Id) is null
                ? "Disponibile"
                : "Assegnato";
            _assets.UpdateAsset(asset);
            _notifications.Publish(
                "Manutenzione completata",
                $"{asset.AssetCode}: {ticket.Title}",
                NotificationCategory.Asset,
                NotificationPriority.Info,
                "Centro Manutenzioni",
                "open-asset",
                asset.Id.ToString());
            if (!string.IsNullOrWhiteSpace(path))
                Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
            ShowMessage("Intervento completato.");
            Load(false);
        }
        catch (Exception ex) { ShowMessage($"Errore completamento: {ex.Message}", true); }
    }

    private static void OpenPdf(MaintenanceTicket ticket)
    {
        if (!string.IsNullOrWhiteSpace(ticket.PdfPath) && File.Exists(ticket.PdfPath))
            Process.Start(new ProcessStartInfo { FileName = ticket.PdfPath, UseShellExecute = true });
    }

    private void PublishOverdueNotifications(
        IEnumerable<MaintenanceTicket> tickets,
        IReadOnlyDictionary<int, Asset> assets)
    {
        foreach (var ticket in tickets.Where(IsOverdue).Where(item => string.IsNullOrWhiteSpace(item.OverdueNotifiedAt)))
        {
            assets.TryGetValue(ticket.AssetId, out var asset);
            _notifications.Publish(
                "Manutenzione scaduta",
                $"{asset?.AssetCode ?? $"Asset #{ticket.AssetId}"}: {ticket.Title}",
                NotificationCategory.Asset,
                ticket.Priority == "Urgente" ? NotificationPriority.Critical : NotificationPriority.High,
                "Centro Manutenzioni",
                "open-asset",
                ticket.AssetId.ToString());
            _repository.MarkOverdueNotified(ticket.Id);
        }
    }

    private bool MatchesStatus(MaintenanceTicket ticket) =>
        _status.SelectedIndex <= 0 || string.Equals(ticket.Status, _status.SelectedItem?.ToString(), StringComparison.OrdinalIgnoreCase);

    private bool MatchesPriority(MaintenanceTicket ticket) =>
        _priority.SelectedIndex <= 0 || string.Equals(ticket.Priority, _priority.SelectedItem?.ToString(), StringComparison.OrdinalIgnoreCase);

    private bool MatchesSearch(MaintenanceTicket ticket, IReadOnlyDictionary<int, Asset> assets)
    {
        var query = (_search.Text ?? string.Empty).Trim();
        if (query.Length == 0) return true;
        assets.TryGetValue(ticket.AssetId, out var asset);
        var text = $"{asset?.AssetCode} {asset?.Manufacturer} {asset?.Model} {ticket.Title} {ticket.Description} {ticket.Technician}";
        return text.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    private void ResetFilters()
    {
        _search.Text = string.Empty;
        _status.SelectedIndex = 0;
        _priority.SelectedIndex = 0;
        _overdueOnly.IsChecked = false;
        Load(false);
    }

    private static bool IsOverdue(MaintenanceTicket ticket) =>
        ticket.Status != "Completato" &&
        DateTime.TryParse(ticket.ScheduledAt, out var scheduled) &&
        scheduled.Date < DateTime.Today;

    private static string StatusColor(MaintenanceTicket ticket) => ticket.Status switch
    {
        "Completato" => UiTokens.Success,
        "In lavorazione" => UiTokens.Warning,
        _ when IsOverdue(ticket) => UiTokens.Danger,
        _ => UiTokens.BrandBlue
    };

    private static string PriorityColor(string priority) => priority switch
    {
        "Urgente" => UiTokens.Danger,
        "Alta" => UiTokens.Warning,
        "Bassa" => UiTokens.TextSecondary,
        _ => UiTokens.BrandBlue
    };

    private static Grid RowGrid() => new()
    {
        ColumnDefinitions = new ColumnDefinitions("110,220,120,100,150,110,100,80,90,110")
    };

    private static Control Badge(string text, string color) => new Border
    {
        Background = UiTokens.Brush(UiTokens.Surface),
        BorderBrush = UiTokens.Brush(color),
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(10),
        Padding = new Thickness(7, 4),
        Margin = new Thickness(4),
        Child = new TextBlock
        {
            Text = text,
            Foreground = UiTokens.Brush(color),
            FontSize = 11,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        }
    };

    private static Button ActionButton(string text, Action action, bool primary = false)
    {
        var button = new Button
        {
            Content = text,
            MinHeight = 34,
            Padding = new Thickness(10, 5),
            Margin = new Thickness(4),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Background = UiTokens.Brush(primary ? UiTokens.BrandBlue : UiTokens.SurfaceAlt),
            Foreground = primary ? Brushes.White : UiTokens.Brush(UiTokens.TextPrimary),
            FontWeight = FontWeight.SemiBold
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static void AddHeader(Grid grid, string text, int column, bool centered = false) =>
        Add(grid, new TextBlock
        {
            Text = text,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            HorizontalAlignment = centered ? HorizontalAlignment.Center : HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        }, column);

    private static void AddText(Grid grid, string text, int column, bool strong = false, bool danger = false) =>
        Add(grid, new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(text) ? "—" : text,
            FontWeight = strong ? FontWeight.SemiBold : FontWeight.Normal,
            Foreground = UiTokens.Brush(danger ? UiTokens.Danger : strong ? UiTokens.TextPrimary : UiTokens.TextSecondary),
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(4)
        }, column);

    private static Control EmptyState() => new Border
    {
        Padding = new Thickness(28),
        Child = new TextBlock
        {
            Text = "Nessun intervento corrisponde ai filtri selezionati.",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            HorizontalAlignment = HorizontalAlignment.Center
        }
    };

    private void ShowMessage(string text, bool error = false)
    {
        _message.Text = text;
        _message.Foreground = UiTokens.Brush(error ? UiTokens.Danger : UiTokens.Success);
    }

    private static string Date(string value) =>
        DateTime.TryParse(value, out var date) ? date.ToString("dd/MM/yyyy") : "—";

    private static void Add(Grid grid, Control control, int column)
    {
        Grid.SetColumn(control, column);
        grid.Children.Add(control);
    }
}
