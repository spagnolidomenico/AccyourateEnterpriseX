using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.DeliveryReports;
using Accyourate.App.AssetManagement.Models;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement.Deliveries;

public sealed class DeliveryRegisterView : UserControl
{
    private readonly DeliveryRecordRepository _repository = new();
    private readonly AssetAssignmentEngine _assignments = new();
    private readonly AssetService _assets = new();
    private readonly DeliveryReportService _reports = new();
    private readonly ReturnReportPdfService _returnReports = new();

    private readonly TextBox _search = new();
    private readonly ComboBox _status = new();
    private readonly TextBox _fromDate = new();
    private readonly TextBox _toDate = new();
    private readonly StackPanel _rows = new();
    private readonly TextBlock _summary = new();
    private readonly TextBlock _message = new();

    public DeliveryRegisterView()
    {
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();
        Load();
    }

    public event Action<int>? AssetRequested;

    private Control BuildLayout()
    {
        var root = new DockPanel();

        var header = new StackPanel
        {
            Margin = new Thickness(24, 20, 24, 14),
            Spacing = 4
        };
        header.Children.Add(new TextBlock
        {
            Text = "Registro Consegne",
            FontSize = 30,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        header.Children.Add(new TextBlock
        {
            Text = "Consegne, riconsegne e verbali collegati agli asset aziendali.",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });
        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        var filters = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,150,130,130,110"),
            Margin = new Thickness(24, 0, 24, 12)
        };
        _search.Watermark = "Cerca asset, dipendente o note...";
        _search.TextChanged += (_, _) => Load();
        Add(filters, _search, 0);

        _status.ItemsSource = new[] { "Tutti", "Attive", "Riconsegnate", "Annullate", "Pianificate" };
        _status.SelectedIndex = 0;
        _status.SelectionChanged += (_, _) => Load();
        Add(filters, _status, 1);

        _fromDate.Watermark = "Dal: AAAA-MM-GG";
        _toDate.Watermark = "Al: AAAA-MM-GG";
        Add(filters, _fromDate, 2);
        Add(filters, _toDate, 3);

        var apply = Button("Applica", Load, true);
        Add(filters, apply, 4);
        DockPanel.SetDock(filters, Dock.Top);
        root.Children.Add(filters);

        _message.Margin = new Thickness(24, 0, 24, 8);
        _message.Foreground = UiTokens.Brush(UiTokens.Danger);
        _message.TextWrapping = TextWrapping.Wrap;
        DockPanel.SetDock(_message, Dock.Top);
        root.Children.Add(_message);

        _summary.Margin = new Thickness(24, 0, 24, 8);
        _summary.Foreground = UiTokens.Brush(UiTokens.TextSecondary);
        DockPanel.SetDock(_summary, Dock.Top);
        root.Children.Add(_summary);

        var table = new StackPanel { Spacing = 0, MinWidth = 1120 };
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

    private void Load()
    {
        try
        {
            _message.Text = string.Empty;
            var assets = _assets.GetAssets().ToDictionary(asset => asset.Id);
            var employees = _assignments.GetEmployees()
                .GroupBy(employee => employee.MasterEmployeeId)
                .ToDictionary(group => group.Key, group => group.First().FullName);

            var records = _repository.GetLatest(500)
                .Where(MatchesStatus)
                .Where(MatchesPeriod)
                .Where(record => MatchesSearch(record, assets, employees))
                .ToList();

            _rows.Children.Clear();
            for (var index = 0; index < records.Count; index++)
            {
                assets.TryGetValue(records[index].AssetId, out var asset);
                employees.TryGetValue(records[index].EmployeeId, out var employee);
                _rows.Children.Add(BuildRow(records[index], asset, employee, index));
            }

            _summary.Text = $"{records.Count} consegne visualizzate";
            if (records.Count == 0)
                _rows.Children.Add(EmptyState());
        }
        catch (Exception ex)
        {
            _message.Text = $"Errore caricamento registro: {ex.Message}";
        }
    }

    private Control BuildHeader()
    {
        var grid = RowGrid();
        AddHeader(grid, "Asset", 0);
        AddHeader(grid, "Dipendente", 1);
        AddHeader(grid, "Consegna", 2);
        AddHeader(grid, "Riconsegna", 3);
        AddHeader(grid, "Stato", 4, true);
        AddHeader(grid, "Asset", 5, true);
        AddHeader(grid, "Verbale", 6, true);
        AddHeader(grid, "Azione", 7, true);
        return new Border
        {
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10, 9),
            Child = grid
        };
    }

    private Control BuildRow(DeliveryRecord record, Asset? asset, string? employee, int index)
    {
        var grid = RowGrid();
        AddText(grid, asset?.AssetCode ?? $"Asset #{record.AssetId}", 0, true);
        AddText(grid, employee ?? $"Dipendente #{record.EmployeeId}", 1);
        AddText(grid, FormatDate(record.DeliveryDate), 2);
        AddText(grid, FormatDate(record.ReturnDate), 3);
        Add(grid, StatusBadge(record.Status), 4);

        var openAsset = Button("Apri", () =>
        {
            _message.Text = string.Empty;
            AssetRequested?.Invoke(record.AssetId);
        });
        Add(grid, openAsset, 5);

        var report = Button(
            record.Status == DeliveryRecordStatus.Returned ? "PDF reso" : "PDF",
            () => OpenReport(record, asset, employee));
        Add(grid, report, 6);

        var returnButton = Button(
            record.IsActive ? "Riconsegna" : "Chiusa",
            () => Return(record, asset, employee));
        returnButton.IsEnabled = record.IsActive;
        Add(grid, returnButton, 7);

        return new Border
        {
            Background = UiTokens.Brush(index % 2 == 0 ? UiTokens.Surface : UiTokens.SurfaceAlt),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(1, 0, 1, 1),
            Padding = new Thickness(10, 7),
            MinHeight = 50,
            Child = grid
        };
    }

    private async void Return(DeliveryRecord record, Asset? asset, string? employee)
    {
        try
        {
            asset ??= _assets.GetAssetById(record.AssetId)
                ?? throw new InvalidOperationException("Asset non trovato.");
            employee ??= _assignments.GetEmployees()
                .FirstOrDefault(item => item.MasterEmployeeId == record.EmployeeId)?.FullName
                ?? $"Dipendente #{record.EmployeeId}";

            var assignment = _assignments.GetActiveAssignmentForAsset(record.AssetId)
                ?? throw new InvalidOperationException("Nessuna assegnazione attiva trovata.");

            var dialog = new AssetReturnDialog(asset.AssetCode, employee);
            var owner = TopLevel.GetTopLevel(this) as Window;
            var result = owner is null
                ? null
                : await dialog.ShowDialog<AssetReturnDialogResult?>(owner);
            if (result is null)
                return;

            record.ReturnDate = result.ReturnDate.ToString("s");
            record.ReturnCondition = result.Condition;
            record.ReturnNotes = result.Notes;
            var pdfPath = result.GeneratePdf
                ? _returnReports.Generate(record, asset, employee, "Registro Consegne")
                : string.Empty;

            _assignments.ReturnAssignment(assignment.AssignmentId, "Riconsegna dal Registro Consegne.");
            _repository.MarkReturned(
                record.Id,
                result.ReturnDate,
                "Riconsegna dal Registro Consegne.",
                result.Condition,
                result.Notes,
                pdfPath);
            _message.Foreground = UiTokens.Brush(UiTokens.Success);
            _message.Text = string.IsNullOrWhiteSpace(pdfPath)
                ? "Riconsegna completata."
                : $"Riconsegna completata e verbale generato: {pdfPath}";
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
            _message.Foreground = UiTokens.Brush(UiTokens.Danger);
            _message.Text = $"Errore riconsegna: {ex.Message}";
        }
    }

    private void OpenReport(DeliveryRecord record, Asset? asset, string? employee)
    {
        try
        {
            if (record.Status == DeliveryRecordStatus.Returned &&
                !string.IsNullOrWhiteSpace(record.ReturnPdfPath) &&
                File.Exists(record.ReturnPdfPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = record.ReturnPdfPath,
                    UseShellExecute = true
                });
                return;
            }

            var report = _reports.GetByAssetId(record.AssetId).FirstOrDefault()
                ?? throw new InvalidOperationException("Nessun verbale collegato a questo asset.");

            // Rigenera il documento per applicare sempre il modello e il branding correnti.
            var path = _reports.GeneratePdf(report.Id, "Registro Consegne");

            Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _message.Foreground = UiTokens.Brush(UiTokens.Danger);
            _message.Text = $"Errore apertura verbale: {ex.Message}";
        }
    }

    private bool MatchesStatus(DeliveryRecord record)
    {
        return _status.SelectedIndex switch
        {
            1 => record.Status == DeliveryRecordStatus.Active,
            2 => record.Status == DeliveryRecordStatus.Returned,
            3 => record.Status == DeliveryRecordStatus.Cancelled,
            4 => record.Status == DeliveryRecordStatus.Planned,
            _ => true
        };
    }

    private bool MatchesPeriod(DeliveryRecord record)
    {
        if (!DateTime.TryParse(record.DeliveryDate, out var date))
            return true;
        if (DateTime.TryParse(_fromDate.Text, out var from) && date.Date < from.Date)
            return false;
        if (DateTime.TryParse(_toDate.Text, out var to) && date.Date > to.Date)
            return false;
        return true;
    }

    private bool MatchesSearch(
        DeliveryRecord record,
        IReadOnlyDictionary<int, Asset> assets,
        IReadOnlyDictionary<int, string> employees)
    {
        var query = (_search.Text ?? string.Empty).Trim();
        if (query.Length == 0)
            return true;

        assets.TryGetValue(record.AssetId, out var asset);
        employees.TryGetValue(record.EmployeeId, out var employee);
        var text = $"{asset?.AssetCode} {asset?.Manufacturer} {asset?.Model} {employee} {record.Notes}";
        return text.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    private static Grid RowGrid() => new()
    {
        ColumnDefinitions = new ColumnDefinitions("150,220,110,110,120,90,90,120")
    };

    private static Control StatusBadge(string status)
    {
        var (label, color) = status switch
        {
            DeliveryRecordStatus.Active => ("Attiva", UiTokens.BrandBlue),
            DeliveryRecordStatus.Returned => ("Riconsegnata", UiTokens.Success),
            DeliveryRecordStatus.Cancelled => ("Annullata", UiTokens.Danger),
            DeliveryRecordStatus.Planned => ("Pianificata", UiTokens.Warning),
            _ => (status, UiTokens.TextSecondary)
        };

        return new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            BorderBrush = UiTokens.Brush(color),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(8, 5),
            Margin = new Thickness(4),
            Child = new TextBlock
            {
                Text = label,
                Foreground = UiTokens.Brush(color),
                FontSize = 11,
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            }
        };
    }

    private static Button Button(string text, Action action, bool primary = false)
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

    private static void AddHeader(Grid grid, string text, int column, bool centered = false)
    {
        Add(grid, new TextBlock
        {
            Text = text,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            HorizontalAlignment = centered ? HorizontalAlignment.Center : HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        }, column);
    }

    private static void AddText(Grid grid, string text, int column, bool strong = false)
    {
        Add(grid, new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(text) ? "—" : text,
            FontWeight = strong ? FontWeight.SemiBold : FontWeight.Normal,
            Foreground = UiTokens.Brush(strong ? UiTokens.TextPrimary : UiTokens.TextSecondary),
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(4)
        }, column);
    }

    private static Control EmptyState() => new Border
    {
        Padding = new Thickness(24),
        Child = new TextBlock
        {
            Text = "Nessuna consegna corrisponde ai filtri selezionati.",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            HorizontalAlignment = HorizontalAlignment.Center
        }
    };

    private static string FormatDate(string value) =>
        DateTime.TryParse(value, out var date) ? date.ToString("dd/MM/yyyy") : "—";

    private static void Add(Grid grid, Control control, int column)
    {
        Grid.SetColumn(control, column);
        grid.Children.Add(control);
    }
}
