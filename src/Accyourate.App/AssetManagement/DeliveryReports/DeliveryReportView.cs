using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;
using System.Diagnostics;

namespace Accyourate.App.AssetManagement.DeliveryReports;

public sealed class DeliveryReportView : UserControl
{
    private readonly DeliveryReportService _service = new();
    private readonly StackPanel _rows = new();
    private readonly ContentControl _details = new();
    private readonly TextBox _search = new();
    private readonly ComboBox _status = new();
    private readonly TextBlock _message = new();

    private IReadOnlyList<DeliveryReport> _reports = Array.Empty<DeliveryReport>();
    private DeliveryReport? _selected;

    public DeliveryReportView()
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
            Text = "Verbali di consegna",
            FontSize = 32,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        header.Children.Add(new TextBlock
        {
            Text = "Gestione dei verbali generati dalle assegnazioni asset.",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap
        });

        _message.Foreground = UiTokens.Brush(UiTokens.BrandBlue);
        _message.TextWrapping = TextWrapping.Wrap;
        header.Children.Add(_message);

        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        var toolbar = new Grid
        {
            Margin = new Thickness(24, 0, 24, 16),
            ColumnDefinitions = new ColumnDefinitions("*,170,Auto,Auto")
        };

        _search.Watermark = "Cerca numero, dipendente, asset...";
        _search.TextChanged += (_, _) => RefreshRows();

        _status.ItemsSource = new[] { "Tutti", DeliveryReportStatus.Draft, DeliveryReportStatus.Generated, DeliveryReportStatus.Signed, DeliveryReportStatus.Archived, DeliveryReportStatus.Cancelled };
        _status.SelectedIndex = 0;
        _status.SelectionChanged += (_, _) => RefreshRows();

        Add(toolbar, _search, 0, 0);
        Add(toolbar, _status, 1, 0);
        Add(toolbar, ToolbarButton("↻ Aggiorna", Load), 2, 0);
        Add(toolbar, ToolbarButton("📂 Cartella PDF", OpenPdfFolder), 3, 0);

        DockPanel.SetDock(toolbar, Dock.Top);
        root.Children.Add(toolbar);

        var content = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,390"),
            Margin = new Thickness(24, 0, 24, 24)
        };

        var list = new DockPanel();

        var tableHeader = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("130,*,120,110,140"),
            Margin = new Thickness(0, 0, 0, 8)
        };

        Add(tableHeader, Header("Numero"), 0, 0);
        Add(tableHeader, Header("Dipendente"), 1, 0);
        Add(tableHeader, Header("Asset"), 2, 0);
        Add(tableHeader, Header("Stato"), 3, 0);
        Add(tableHeader, Header("Data"), 4, 0);

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
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
            }
        });

        Add(content, list, 0, 0);

        _details.Content = EmptyDetails();
        Add(content, new ScrollViewer
        {
            Content = _details,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        }, 1, 0);

        root.Children.Add(content);
        return root;
    }

    private void Load()
    {
        try
        {
            var keep = _selected?.Id;
            _reports = _service.GetLatest(200);

            _selected = keep.HasValue
                ? _reports.FirstOrDefault(x => x.Id == keep.Value)
                : _reports.FirstOrDefault();

            RefreshRows();
            _details.Content = _selected is not null ? DetailsCard(_selected) : EmptyDetails();
            ShowMessage($"Caricati {_reports.Count} verbali.");
        }
        catch (Exception ex)
        {
            ShowMessage($"Errore caricamento verbali: {ex.Message}", true);
        }
    }

    private void RefreshRows()
    {
        _rows.Children.Clear();
        _rows.Spacing = 6;

        var query = (_search.Text ?? string.Empty).Trim().ToLowerInvariant();
        var selectedStatus = _status.SelectedItem?.ToString() ?? "Tutti";

        var filtered = _reports.Where(r =>
            (string.IsNullOrWhiteSpace(query) ||
             $"{r.ReportNumber} {r.EmployeeName} {r.AssetCode} {r.Status}".ToLowerInvariant().Contains(query)) &&
            (selectedStatus == "Tutti" || r.Status == selectedStatus))
            .ToList();

        if (filtered.Count == 0)
        {
            _rows.Children.Add(new TextBlock
            {
                Text = "Nessun verbale trovato.",
                Margin = new Thickness(12),
                Foreground = UiTokens.Brush(UiTokens.TextSecondary)
            });
            return;
        }

        foreach (var report in filtered)
            _rows.Children.Add(Row(report));
    }

    private Button Row(DeliveryReport report)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("130,*,120,110,140")
        };

        Add(grid, Cell(report.ReportNumber, true), 0, 0);
        Add(grid, Cell(report.EmployeeName, true), 1, 0);
        Add(grid, Cell(report.AssetCode), 2, 0);
        Add(grid, StatusBadge(report.Status), 3, 0);
        Add(grid, Cell(FormatDate(report.ReportDate)), 4, 0);

        var button = new Button
        {
            Content = grid,
            Background = _selected?.Id == report.Id ? UiTokens.Brush(UiTokens.PremiumSelected) : Brushes.Transparent,
            Padding = new Thickness(8),
            CornerRadius = new CornerRadius(12)
        };

        button.Click += (_, _) =>
        {
            _selected = report;
            _details.Content = DetailsCard(report);
            RefreshRows();
        };

        button.DoubleTapped += (_, _) => GeneratePdf(report);

        return button;
    }

    private Control DetailsCard(DeliveryReport report)
    {
        var stack = new StackPanel { Spacing = 12 };

        stack.Children.Add(new TextBlock
        {
            Text = report.ReportNumber,
            FontSize = 26,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        stack.Children.Add(new TextBlock
        {
            Text = $"{report.EmployeeName} · {report.AssetCode}",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap
        });

        var actions = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            Margin = new Thickness(0, 4, 0, 8)
        };

        Add(actions, SmallButton("Genera PDF", () => GeneratePdf(report)), 0, 0);
        Add(actions, SmallButton("Apri PDF", () => OpenPdf(report), false), 1, 0);
        stack.Children.Add(actions);

        stack.Children.Add(Info("Stato", report.Status));
        stack.Children.Add(Info("Data verbale", FormatDate(report.ReportDate)));
        stack.Children.Add(Info("Dipendente", report.EmployeeName));
        stack.Children.Add(Info("Asset", report.AssetCode));
        stack.Children.Add(Info("Percorso PDF", report.PdfPath));
        stack.Children.Add(Info("Creato da", report.CreatedBy));
        stack.Children.Add(Info("Note", report.Notes));

        stack.Children.Add(new TextBlock
        {
            Text = "Beni associati",
            FontSize = 17,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            Margin = new Thickness(0, 8, 0, 0)
        });

        var items = _service.GetItems(report.Id);
        if (items.Count == 0)
        {
            stack.Children.Add(Info("Beni", "Nessun bene associato."));
        }
        else
        {
            foreach (var item in items)
                stack.Children.Add(Info(item.AssetCode, $"{item.Description} · Stato: {item.Condition}"));
        }

        return Card(stack);
    }

    private Control EmptyDetails()
    {
        return Card(new TextBlock
        {
            Text = "Seleziona un verbale per visualizzare dettagli e azioni.",
            TextWrapping = TextWrapping.Wrap,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });
    }

    private void GeneratePdf(DeliveryReport report)
    {
        try
        {
            var path = _service.GeneratePdf(report.Id, "Delivery Report UI");
            ShowMessage($"PDF generato: {path}");
            Load();
        }
        catch (Exception ex)
        {
            ShowMessage($"Errore generazione PDF: {ex.Message}", true);
        }
    }

    private void OpenPdf(DeliveryReport report)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(report.PdfPath) || !File.Exists(report.PdfPath))
            {
                ShowMessage("PDF non ancora generato o file non trovato.", true);
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = report.PdfPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            ShowMessage($"Errore apertura PDF: {ex.Message}", true);
        }
    }

    private void OpenPdfFolder()
    {
        try
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Accyourate Enterprise X",
                "Verbali Consegna");

            Directory.CreateDirectory(folder);

            Process.Start(new ProcessStartInfo
            {
                FileName = folder,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            ShowMessage($"Errore apertura cartella: {ex.Message}", true);
        }
    }

    private void ShowMessage(string text, bool isError = false)
    {
        _message.Text = text;
        _message.Foreground = UiTokens.Brush(isError ? UiTokens.Danger : UiTokens.BrandBlue);
    }

    private static TextBlock Header(string text) => new()
    {
        Text = text,
        FontWeight = FontWeight.Bold,
        Foreground = UiTokens.Brush(UiTokens.TextSecondary),
        Margin = new Thickness(10, 0)
    };

    private static TextBlock Cell(string text, bool strong = false) => new()
    {
        Text = string.IsNullOrWhiteSpace(text) ? "—" : text,
        FontWeight = strong ? FontWeight.Bold : FontWeight.Normal,
        Foreground = UiTokens.Brush(strong ? UiTokens.TextPrimary : UiTokens.TextSecondary),
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        TextWrapping = TextWrapping.NoWrap,
        Margin = new Thickness(10, 0)
    };

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
                Foreground = UiTokens.Brush(status == DeliveryReportStatus.Generated ? UiTokens.Success : UiTokens.BrandBlue),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            }
        };
    }

    private static Button ToolbarButton(string text, Action action)
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
        b.Click += (_, _) => action();
        return b;
    }

    private static Button SmallButton(string text, Action action, bool primary = true)
    {
        var b = new Button
        {
            Content = text,
            Background = UiTokens.Brush(primary ? UiTokens.BrandBlue : UiTokens.SurfaceAlt),
            Foreground = primary ? Brushes.White : UiTokens.Brush(UiTokens.TextPrimary),
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(10, 8),
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(4)
        };
        b.Click += (_, _) => action();
        return b;
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

    private static Border Card(Control child)
    {
        return new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            CornerRadius = new CornerRadius(22),
            Padding = new Thickness(18),
            Margin = new Thickness(16, 0, 0, 0),
            Child = child
        };
    }

    private static string FormatDate(string value)
    {
        return DateTime.TryParse(value, out var date)
            ? date.ToString("dd/MM/yyyy HH:mm")
            : value;
    }

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
