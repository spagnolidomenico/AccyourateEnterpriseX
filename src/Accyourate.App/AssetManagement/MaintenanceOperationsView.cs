using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Models;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.Platform.Notifications;
using Accyourate.App.UIFramework.Tokens;
using Accyourate.App.UIFramework.DesignSystem;

namespace Accyourate.App.AssetManagement;

public sealed class MaintenanceOperationsView : UserControl
{
    private readonly MaintenanceRepository _repository = new();
    private readonly AssetService _assets = new();
    private readonly AssetAssignmentEngine _assignments = new();
    private readonly MaintenancePdfService _pdf = new();
    private readonly MaintenanceAnalyticsPdfService _analyticsPdf = new();
    private readonly MaintenancePartsRepository _parts = new();
    private readonly NotificationService _notifications = new();
    private readonly TextBox _search = new();
    private readonly ComboBox _status = new();
    private readonly ComboBox _priority = new();
    private readonly CheckBox _overdueOnly = new() { Content = "Solo scadute" };
    private readonly Grid _kpis = new()
    {
        ColumnDefinitions = new ColumnDefinitions("*,*,*,*,*,*")
    };
    private StackPanel _rows = new();
    private readonly TextBlock _summary = new();
    private readonly TextBlock _message = new();
    private readonly ContentControl _contentHost = new();
    private DateTime _calendarMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private bool _calendarMode;
    private bool _analyticsMode;
    private IReadOnlyDictionary<int, decimal> _partTotals = new Dictionary<int, decimal>();

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
        var headerActions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        headerActions.Children.Add(ActionButton("Pianifica intervento", PlanMaintenance, true));
        headerActions.Children.Add(ActionButton("Lista", ShowList));
        headerActions.Children.Add(ActionButton("Calendario", ShowCalendar));
        headerActions.Children.Add(ActionButton("Analisi", ShowAnalytics));
        headerActions.Children.Add(ActionButton("Aggiorna", () => Load()));
        Grid.SetColumn(headerActions, 1);
        header.Children.Add(headerActions);
        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        _kpis.Margin = new Thickness(24, 0, 24, 12);
        DockPanel.SetDock(_kpis, Dock.Top);
        root.Children.Add(_kpis);

        var filters = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,150,150,120,110"),
            Margin = new Thickness(24, 0, 24, 10)
        };
        _search.Watermark = "Cerca asset, intervento, tecnico...";
        _search.TextChanged += (_, _) => Load(false);
        Add(filters, _search, 0);
        _status.ItemsSource = new[] { "Tutti gli stati", "Pianificato", "Aperto", "In lavorazione", "Completato" };
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

        root.Children.Add(_contentHost);
        return root;
    }

    private void Load(bool publishOverdueNotifications = true)
    {
        try
        {
            _message.Text = string.Empty;
            var assets = _assets.GetAssets().ToDictionary(asset => asset.Id);
            var all = _repository.GetAll();
            _partTotals = _parts.GetTotalsByTicket();
            if (publishOverdueNotifications)
            {
                PublishOverdueNotifications(all, assets);
                PublishUpcomingReminders(all, assets);
                PublishSlaBreaches(all, assets);
            }

            var visible = all
                .Where(MatchesStatus)
                .Where(MatchesPriority)
                .Where(ticket => _overdueOnly.IsChecked != true || IsOverdue(ticket))
                .Where(ticket => MatchesSearch(ticket, assets))
                .ToList();

            BuildKpis(all);
            // Ogni cambio Lista/Calendario usa un nuovo contenitore:
            // Avalonia non consente di assegnare lo stesso controllo a due genitori visuali.
            _rows = new StackPanel();
            for (var index = 0; index < visible.Count; index++)
            {
                assets.TryGetValue(visible[index].AssetId, out var asset);
                _rows.Children.Add(BuildRow(visible[index], asset, index));
            }
            if (visible.Count == 0)
                _rows.Children.Add(EmptyState());

            var totalCost = all.Where(ticket => ticket.Status == "Completato")
                .Sum(ticket => ticket.Cost + PartCost(ticket.Id));
            _summary.Text = $"{visible.Count} interventi visualizzati · costo storico EUR {totalCost:N2}";
            _contentHost.Content = _analyticsMode
                ? BuildAnalytics(all, assets)
                : _calendarMode
                    ? BuildCalendar(all, assets)
                    : BuildList();
        }
        catch (Exception ex)
        {
            ShowMessage($"Errore caricamento manutenzioni: {ex.Message}", true);
        }
    }

    private Control BuildList()
    {
        return new ScrollViewer
        {
            Content = _rows,
            Margin = new Thickness(24, 0, 24, 24),
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
    }

    private Control BuildCalendar(
        IReadOnlyList<MaintenanceTicket> tickets,
        IReadOnlyDictionary<int, Asset> assets)
    {
        var root = new StackPanel { Margin = new Thickness(24, 0, 24, 24), Spacing = 12 };
        var toolbar = new Grid { ColumnDefinitions = new ColumnDefinitions("120,*,120") };
        Add(toolbar, ActionButton("← Mese", PreviousMonth), 0);
        var month = new TextBlock
        {
            Text = _calendarMonth.ToString("MMMM yyyy"),
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Add(toolbar, month, 1);
        Add(toolbar, ActionButton("Mese →", NextMonth), 2);
        root.Children.Add(toolbar);

        var calendar = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*,*,*,*,*"),
            RowDefinitions = new RowDefinitions("34,116,116,116,116,116,116")
        };
        var dayNames = new[] { "Lun", "Mar", "Mer", "Gio", "Ven", "Sab", "Dom" };
        for (var column = 0; column < dayNames.Length; column++)
        {
            var label = new TextBlock
            {
                Text = dayNames[column],
                FontWeight = FontWeight.Bold,
                Foreground = UiTokens.Brush(UiTokens.TextSecondary),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(label, column);
            calendar.Children.Add(label);
        }

        var firstOffset = ((int)_calendarMonth.DayOfWeek + 6) % 7;
        var firstCellDate = _calendarMonth.AddDays(-firstOffset);
        for (var cell = 0; cell < 42; cell++)
        {
            var date = firstCellDate.AddDays(cell);
            var dayTickets = tickets
                .Where(ticket => DateTime.TryParse(ticket.ScheduledAt, out var scheduled) && scheduled.Date == date.Date)
                .OrderBy(ticket => ticket.Status == "Completato")
                .ThenBy(ticket => ticket.Priority == "Urgente" ? 0 : 1)
                .ToList();
            var day = BuildCalendarDay(date, dayTickets, assets);
            Grid.SetColumn(day, cell % 7);
            Grid.SetRow(day, 1 + cell / 7);
            calendar.Children.Add(day);
        }
        root.Children.Add(calendar);
        root.Children.Add(BuildTechnicianWorkload(tickets));
        return new ScrollViewer
        {
            Content = root,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
        };
    }

    private Control BuildCalendarDay(
        DateTime date,
        IReadOnlyList<MaintenanceTicket> tickets,
        IReadOnlyDictionary<int, Asset> assets)
    {
        var panel = new StackPanel { Spacing = 3 };
        panel.Children.Add(new TextBlock
        {
            Text = date.Day.ToString(),
            FontWeight = date.Date == DateTime.Today ? FontWeight.Bold : FontWeight.Normal,
            Foreground = UiTokens.Brush(
                date.Month == _calendarMonth.Month ? UiTokens.TextPrimary : UiTokens.TextSecondary)
        });
        foreach (var ticket in tickets.Take(3))
        {
            assets.TryGetValue(ticket.AssetId, out var asset);
            var button = new Button
            {
                Content = $"{asset?.AssetCode ?? ticket.AssetId.ToString()} · {ticket.Title}",
                FontSize = 10,
                Padding = new Thickness(5, 3),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Background = UiTokens.Brush(StatusColor(ticket)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };
            var assetId = ticket.AssetId;
            button.Click += (_, _) => AssetRequested?.Invoke(assetId);
            panel.Children.Add(button);
        }
        if (tickets.Count > 3)
            panel.Children.Add(new TextBlock
            {
                Text = $"+{tickets.Count - 3} altri",
                FontSize = 10,
                Foreground = UiTokens.Brush(UiTokens.TextSecondary)
            });
        return new Border
        {
            Background = UiTokens.Brush(date.Date == DateTime.Today ? UiTokens.SurfaceAlt : UiTokens.Surface),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(6),
            Margin = new Thickness(2),
            Child = panel
        };
    }

    private Control BuildTechnicianWorkload(IReadOnlyList<MaintenanceTicket> tickets)
    {
        var monthEnd = _calendarMonth.AddMonths(1);
        var groups = tickets
            .Where(ticket => ticket.Status != "Completato")
            .Where(ticket => DateTime.TryParse(ticket.ScheduledAt, out var scheduled) &&
                             scheduled >= _calendarMonth && scheduled < monthEnd)
            .GroupBy(ticket => string.IsNullOrWhiteSpace(ticket.Technician) ? "Non assegnato" : ticket.Technician)
            .OrderByDescending(group => group.Count())
            .ToList();
        var panel = new StackPanel { Spacing = 6 };
        panel.Children.Add(new TextBlock
        {
            Text = "Carico per tecnico",
            FontSize = 18,
            FontWeight = FontWeight.Bold
        });
        if (groups.Count == 0)
            panel.Children.Add(new TextBlock
            {
                Text = "Nessun intervento pianificato nel mese.",
                Foreground = UiTokens.Brush(UiTokens.TextSecondary)
            });
        foreach (var group in groups)
        {
            panel.Children.Add(new Border
            {
                Background = UiTokens.Brush(UiTokens.SurfaceAlt),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 8),
                Child = new TextBlock
                {
                    Text = $"{group.Key}: {group.Count()} interventi",
                    FontWeight = FontWeight.SemiBold
                }
            });
        }
        return panel;
    }

    private Control BuildAnalytics(
        IReadOnlyList<MaintenanceTicket> tickets,
        IReadOnlyDictionary<int, Asset> assets)
    {
        var root = new StackPanel { Margin = new Thickness(24, 0, 24, 24), Spacing = 14 };
        var completed = tickets.Where(ticket => ticket.Status == "Completato").ToList();
        var totalCost = completed.Sum(ticket => ticket.Cost + PartCost(ticket.Id));
        var averageHours = MaintenanceAnalyticsPdfService.AverageResolutionHours(completed);
        var recurring = tickets.Count(ticket => ticket.RecurrenceMonths > 0);

        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("*,170") };
        header.Children.Add(new TextBlock
        {
            Text = "Analisi manutenzioni",
            FontSize = 21,
            FontWeight = FontWeight.Bold,
            VerticalAlignment = VerticalAlignment.Center
        });
        var export = ActionButton("Esporta report PDF", () => ExportAnalytics(tickets, assets), true);
        Grid.SetColumn(export, 1);
        header.Children.Add(export);
        root.Children.Add(header);

        var slaEligible = completed.Where(ticket =>
            DateTime.TryParse(ticket.SlaDeadline, out _) &&
            DateTime.TryParse(ticket.ClosedAt, out _)).ToList();
        var slaCompliant = slaEligible.Count == 0
            ? 0
            : 100d * slaEligible.Count(ticket =>
                DateTime.Parse(ticket.ClosedAt) <= DateTime.Parse(ticket.SlaDeadline)) / slaEligible.Count;
        var downtimeHours = completed.Sum(ticket => ticket.DowntimeMinutes) / 60d;

        var metrics = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*,*,*,*") };
        AddMetric(metrics, 0, "Costo complessivo", $"EUR {totalCost:N2}", UiTokens.BrandBlue);
        AddMetric(metrics, 1, "Costo medio", completed.Count > 0 ? $"EUR {totalCost / completed.Count:N2}" : "—", UiTokens.Warning);
        AddMetric(metrics, 2, "Risoluzione media", averageHours > 0 ? $"{averageHours:N1} ore" : "—", UiTokens.Success);
        AddMetric(metrics, 3, "Conformità SLA", slaEligible.Count > 0 ? $"{slaCompliant:N0}%" : "—", slaCompliant >= 90 ? UiTokens.Success : UiTokens.Warning);
        AddMetric(metrics, 4, "Fermo complessivo", $"{downtimeHours:N1} ore", recurring > 0 ? UiTokens.Warning : UiTokens.TextSecondary);
        root.Children.Add(metrics);

        var columns = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*") };
        var assetPanel = AnalyticsPanel("Costi per asset");
        var assetGroups = completed
            .GroupBy(ticket => ticket.AssetId)
            .Select(group => new { AssetId = group.Key, Cost = group.Sum(item => item.Cost), Count = group.Count() })
            .OrderByDescending(item => item.Cost)
            .Take(10)
            .ToList();
        foreach (var group in assetGroups)
        {
            assets.TryGetValue(group.AssetId, out var asset);
            assetPanel.Children.Add(AnalyticsRow(
                asset?.AssetCode ?? $"Asset #{group.AssetId}",
                $"{group.Count} interventi · EUR {group.Cost:N2}"));
        }
        if (assetGroups.Count == 0)
            assetPanel.Children.Add(AnalyticsEmpty());
        Add(columns, Card(assetPanel), 0);

        var technicianPanel = AnalyticsPanel("Carico per tecnico");
        var technicianGroups = tickets
            .GroupBy(ticket => string.IsNullOrWhiteSpace(ticket.Technician) ? "Non assegnato" : ticket.Technician)
            .OrderByDescending(group => group.Count())
            .Take(10)
            .ToList();
        foreach (var group in technicianGroups)
            technicianPanel.Children.Add(AnalyticsRow(
                group.Key,
                $"{group.Count()} totali · {group.Count(item => item.Status != "Completato")} attivi"));
        if (technicianGroups.Count == 0)
            technicianPanel.Children.Add(AnalyticsEmpty());
        var technicianCard = Card(technicianPanel);
        technicianCard.Margin = new Thickness(10, 0, 0, 0);
        Add(columns, technicianCard, 1);
        root.Children.Add(columns);

        var trend = AnalyticsPanel("Interventi per mese");
        foreach (var group in tickets
                     .Where(ticket => DateTime.TryParse(ticket.OpenedAt, out _))
                     .GroupBy(ticket => DateTime.Parse(ticket.OpenedAt).ToString("yyyy-MM"))
                     .OrderByDescending(group => group.Key)
                     .Take(12)
                     .OrderBy(group => group.Key))
        {
            var label = DateTime.TryParse($"{group.Key}-01", out var month)
                ? month.ToString("MMMM yyyy")
                : group.Key;
            trend.Children.Add(AnalyticsRow(label, $"{group.Count()} interventi"));
        }
        root.Children.Add(Card(trend));

        return new ScrollViewer
        {
            Content = root,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
        };
    }

    private static void AddMetric(Grid grid, int column, string label, string value, string color)
    {
        var card = new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(14, 10),
            Margin = new Thickness(0, 0, 10, 0),
            Child = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = value, FontSize = 22, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(color) },
                    new TextBlock { Text = label, Foreground = UiTokens.Brush(UiTokens.TextSecondary) }
                }
            }
        };
        Add(grid, card, column);
    }

    private static StackPanel AnalyticsPanel(string title)
    {
        var panel = new StackPanel { Spacing = 0 };
        panel.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 17,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 8)
        });
        return panel;
    }

    private static Border Card(Control content) => new()
    {
        Background = UiTokens.Brush(UiTokens.Surface),
        BorderBrush = UiTokens.Brush(UiTokens.Border),
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(10),
        Padding = new Thickness(14),
        Child = content
    };

    private static Control AnalyticsRow(string label, string value)
    {
        var row = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Margin = new Thickness(0, 4)
        };
        row.Children.Add(new TextBlock
        {
            Text = label,
            FontWeight = FontWeight.SemiBold,
            TextTrimming = TextTrimming.CharacterEllipsis
        });
        var right = new TextBlock { Text = value, Foreground = UiTokens.Brush(UiTokens.TextSecondary) };
        Grid.SetColumn(right, 1);
        row.Children.Add(right);
        return row;
    }

    private static Control AnalyticsEmpty() => new TextBlock
    {
        Text = "Nessun dato disponibile.",
        Foreground = UiTokens.Brush(UiTokens.TextSecondary),
        Margin = new Thickness(0, 8)
    };

    private void ExportAnalytics(
        IReadOnlyList<MaintenanceTicket> tickets,
        IReadOnlyDictionary<int, Asset> assets)
    {
        try
        {
            var path = _analyticsPdf.Generate(tickets, assets, "Centro Manutenzioni");
            Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
            ShowMessage($"Report generato: {path}");
        }
        catch (Exception ex) { ShowMessage($"Errore generazione report: {ex.Message}", true); }
    }

    private void ShowList()
    {
        _calendarMode = false;
        _analyticsMode = false;
        Load(false);
    }

    private void ShowCalendar()
    {
        _calendarMode = true;
        _analyticsMode = false;
        Load(false);
    }

    private void ShowAnalytics()
    {
        _calendarMode = false;
        _analyticsMode = true;
        Load(false);
    }

    private void PreviousMonth()
    {
        _calendarMonth = _calendarMonth.AddMonths(-1);
        Load(false);
    }

    private void NextMonth()
    {
        _calendarMonth = _calendarMonth.AddMonths(1);
        Load(false);
    }

    private void BuildKpis(IReadOnlyList<MaintenanceTicket> tickets)
    {
        _kpis.Children.Clear();
        AddKpi(0, "Pianificati", tickets.Count(ticket => ticket.Status == "Pianificato"), UiTokens.BrandBlue);
        AddKpi(1, "Aperti", tickets.Count(ticket => ticket.Status == "Aperto"), UiTokens.BrandBlue);
        AddKpi(2, "In lavorazione", tickets.Count(ticket => ticket.Status == "In lavorazione"), UiTokens.Warning);
        AddKpi(3, "Urgenti", tickets.Count(ticket => ticket.Priority == "Urgente" && ticket.Status != "Completato"), UiTokens.Danger);
        AddKpi(4, "Scaduti", tickets.Count(IsOverdue), UiTokens.Danger);
        AddKpi(5, "Completati", tickets.Count(ticket => ticket.Status == "Completato"), UiTokens.Success);
    }

    private void AddKpi(int column, string label, int value, string color)
    {
        var card = Kpi(label, value, color);
        Grid.SetColumn(card, column);
        _kpis.Children.Add(card);
    }

    private static Control Kpi(string label, int value, string color) => new Border
    {
        Background = UiTokens.Brush(UiTokens.Surface),
        BorderBrush = UiTokens.Brush(UiTokens.Border),
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(12),
        Padding = new Thickness(16, 11),
        Margin = new Thickness(0, 0, 10, 0),
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
        AddHeader(grid, "SLA", 6, true);
        AddHeader(grid, "Costo totale", 7);
        AddHeader(grid, "Asset", 8, true);
        AddHeader(grid, "Verbale", 9, true);
        AddHeader(grid, "Ricambi", 10, true);
        AddHeader(grid, "Azione", 11, true);
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
        var combinedCost = ticket.Cost + PartCost(ticket.Id);
        var open = ActionButton("Apri", () => AssetRequested?.Invoke(ticket.AssetId));

        var pdf = ActionButton("PDF", () => OpenPdf(ticket));
        pdf.IsEnabled = !string.IsNullOrWhiteSpace(ticket.PdfPath) && File.Exists(ticket.PdfPath);
        var parts = ActionButton("Ricambi", () => OpenParts(ticket));

        var action = ticket.Status switch
        {
            "Aperto" or "Pianificato" => ActionButton("Avvia", () => Start(ticket)),
            "In lavorazione" => ActionButton("Completa", () => Complete(ticket, asset), true),
            _ => ActionButton("Chiusa", () => { })
        };
        action.IsEnabled = ticket.Status != "Completato";
        return AxResponsiveRecordCard.Create($"{asset?.AssetCode ?? $"Asset #{ticket.AssetId}"} · {ticket.Title}", new[]
        {
            new AxResponsiveRecordField("Stato", ticket.Status, 130, StatusColor(ticket)),
            new AxResponsiveRecordField("Priorità", ticket.Priority, 120, PriorityColor(ticket.Priority)),
            new AxResponsiveRecordField("Tecnico", ticket.Technician, 180),
            new AxResponsiveRecordField("Scadenza", Date(ticket.ScheduledAt), 130, IsOverdue(ticket) ? UiTokens.Danger : null),
            new AxResponsiveRecordField("SLA", SlaLabel(ticket), 120, SlaColor(ticket)),
            new AxResponsiveRecordField("Costo", combinedCost > 0 ? $"EUR {combinedCost:N2}" : "—", 120)
        }, open, pdf, parts, action);
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

    private async void OpenParts(MaintenanceTicket ticket)
    {
        try
        {
            var owner = TopLevel.GetTopLevel(this) as Window;
            if (owner is null) return;
            await new MaintenancePartsDialog(ticket).ShowDialog<bool?>(owner);
            ShowMessage("Ricambi aggiornati.");
            Load(false);
        }
        catch (Exception ex) { ShowMessage($"Errore gestione ricambi: {ex.Message}", true); }
    }

    private decimal PartCost(int ticketId) =>
        _partTotals.TryGetValue(ticketId, out var cost) ? cost : 0;

    private async void PlanMaintenance()
    {
        try
        {
            var owner = TopLevel.GetTopLevel(this) as Window;
            if (owner is null) return;
            var ticket = await new MaintenancePlanningDialog(_assets.GetAssets())
                .ShowDialog<MaintenanceTicket?>(owner);
            if (ticket is null) return;
            _repository.Create(ticket);
            _notifications.Publish(
                "Manutenzione pianificata",
                $"{_assets.GetAssetById(ticket.AssetId)?.AssetCode}: {ticket.Title} · {Date(ticket.ScheduledAt)}",
                NotificationCategory.Asset,
                NotificationPriority.Info,
                "Centro Manutenzioni",
                "open-asset",
                ticket.AssetId.ToString());
            ShowMessage("Intervento pianificato.");
            Load(false);
        }
        catch (Exception ex) { ShowMessage($"Errore pianificazione: {ex.Message}", true); }
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

    private void PublishUpcomingReminders(
        IEnumerable<MaintenanceTicket> tickets,
        IReadOnlyDictionary<int, Asset> assets)
    {
        foreach (var ticket in tickets.Where(IsReminderDue)
                     .Where(item => string.IsNullOrWhiteSpace(item.ReminderNotifiedAt)))
        {
            assets.TryGetValue(ticket.AssetId, out var asset);
            _notifications.Publish(
                "Manutenzione in scadenza",
                $"{asset?.AssetCode ?? $"Asset #{ticket.AssetId}"}: {ticket.Title} · {Date(ticket.ScheduledAt)}",
                NotificationCategory.Asset,
                ticket.Priority is "Urgente" or "Alta" ? NotificationPriority.High : NotificationPriority.Normal,
                "Centro Manutenzioni",
                "open-asset",
                ticket.AssetId.ToString());
            _repository.MarkReminderNotified(ticket.Id);
        }
    }

    private void PublishSlaBreaches(
        IEnumerable<MaintenanceTicket> tickets,
        IReadOnlyDictionary<int, Asset> assets)
    {
        foreach (var ticket in tickets.Where(IsSlaBreached)
                     .Where(item => string.IsNullOrWhiteSpace(item.SlaBreachedNotifiedAt)))
        {
            assets.TryGetValue(ticket.AssetId, out var asset);
            _notifications.Publish(
                "SLA manutenzione superato",
                $"{asset?.AssetCode ?? $"Asset #{ticket.AssetId}"}: {ticket.Title}",
                NotificationCategory.Asset,
                ticket.Priority == "Urgente" ? NotificationPriority.Critical : NotificationPriority.High,
                "Centro Manutenzioni",
                "open-asset",
                ticket.AssetId.ToString());
            _repository.MarkSlaBreachedNotified(ticket.Id);
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

    private static bool IsReminderDue(MaintenanceTicket ticket) =>
        ticket.Status != "Completato" &&
        ticket.ReminderDays > 0 &&
        DateTime.TryParse(ticket.ScheduledAt, out var scheduled) &&
        scheduled.Date >= DateTime.Today &&
        scheduled.Date <= DateTime.Today.AddDays(ticket.ReminderDays);

    private static bool IsSlaBreached(MaintenanceTicket ticket) =>
        ticket.Status != "Completato" &&
        DateTime.TryParse(ticket.SlaDeadline, out var deadline) &&
        deadline < DateTime.Now;

    private static string SlaLabel(MaintenanceTicket ticket)
    {
        if (string.IsNullOrWhiteSpace(ticket.SlaDeadline)) return "Da avviare";
        if (ticket.Status == "Completato" &&
            DateTime.TryParse(ticket.ClosedAt, out var closed) &&
            DateTime.TryParse(ticket.SlaDeadline, out var completedDeadline))
            return closed <= completedDeadline ? "Rispettato" : "Superato";
        if (IsSlaBreached(ticket)) return "Superato";
        if (DateTime.TryParse(ticket.SlaDeadline, out var deadline))
            return $"entro {deadline:dd/MM HH:mm}";
        return "—";
    }

    private static string SlaColor(MaintenanceTicket ticket) => SlaLabel(ticket) switch
    {
        "Rispettato" => UiTokens.Success,
        "Superato" => UiTokens.Danger,
        "Da avviare" => UiTokens.TextSecondary,
        _ => UiTokens.BrandBlue
    };

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
        ColumnDefinitions = new ColumnDefinitions("110,200,120,100,135,105,130,100,80,90,95,110")
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
