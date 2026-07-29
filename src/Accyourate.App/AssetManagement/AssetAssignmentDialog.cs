using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class AssetAssignmentDialog : Window
{
    private readonly AssetAssignmentEngine _engine;
    private readonly ComboBox _employees = new();
    private readonly ComboBox _assets = new();
    private readonly TextBox _deliveryDate = new();
    private readonly TextBox _notes = new();
    private readonly CheckBox _generateReport = new();
    private readonly TextBlock _validation = new();

    public AssetAssignmentDialog(AssetAssignmentEngine engine, int? assetId = null)
    {
        _engine = engine;

        Title = "Assegna Asset";
        Width = 720;
        Height = 620;
        MinWidth = 640;
        MinHeight = 440;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();

        LoadData(assetId);
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
            Text = "Assegna Asset",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        header.Children.Add(new TextBlock
        {
            Text = "Seleziona un dipendente e un asset disponibile.",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap
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

        var cancel = DialogButton("Annulla", UiTokens.Surface, UiTokens.TextPrimary);
        cancel.Click += (_, _) => Close(null);
        Add(footer, cancel, 1, 0);

        var assign = DialogButton("Assegna", UiTokens.BrandBlue, null, true);
        assign.Foreground = Brushes.White;
        assign.Click += (_, _) => Save();
        Add(footer, assign, 2, 0);

        DockPanel.SetDock(footer, Dock.Bottom);
        root.Children.Add(footer);

        _deliveryDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
        _deliveryDate.Watermark = "AAAA-MM-GG";

        _notes.AcceptsReturn = true;
        _notes.Height = 100;
        _notes.TextWrapping = TextWrapping.Wrap;
        _generateReport.Content = "Genera automaticamente il verbale PDF";
        _generateReport.IsChecked = true;

        var form = new StackPanel
        {
            Margin = new Thickness(24, 0, 24, 12),
            Spacing = 14
        };

        form.Children.Add(Field("Dipendente *", _employees));
        form.Children.Add(Field("Asset disponibile *", _assets));
        form.Children.Add(Field("Data consegna *", _deliveryDate));
        form.Children.Add(Field("Note", _notes));
        form.Children.Add(_generateReport);

        root.Children.Add(form);
        return root;
    }

    private void LoadData(int? assetId)
    {
        var employees = _engine.GetEmployees().ToList();
        var assets = _engine.GetAvailableAssets().ToList();

        _employees.ItemsSource = employees;
        _employees.SelectedIndex = employees.Count > 0 ? 0 : -1;

        _assets.ItemsSource = assets;
        if (assetId.HasValue)
        {
            var selected = assets.FirstOrDefault(a => a.AssetId == assetId.Value);
            _assets.SelectedItem = selected;
        }

        if (_assets.SelectedItem is null)
            _assets.SelectedIndex = assets.Count > 0 ? 0 : -1;
    }

    private void Save()
    {
        _validation.Text = string.Empty;

        if (_employees.SelectedItem is not AssignableEmployee employee)
        {
            _validation.Text = "Seleziona un dipendente.";
            return;
        }

        if (_assets.SelectedItem is not AssignableAsset asset)
        {
            _validation.Text = "Seleziona un asset disponibile.";
            return;
        }

        if (!DateTime.TryParse(_deliveryDate.Text, out var deliveryDate))
        {
            _validation.Text = "Inserisci una data di consegna valida.";
            return;
        }

        if (deliveryDate.Date > DateTime.Today)
        {
            _validation.Text = "La data di consegna non può essere futura.";
            return;
        }

        Close(new AssetAssignmentDialogResult
        {
            AssetId = asset.AssetId,
            MasterEmployeeId = employee.MasterEmployeeId,
            DeliveryDate = deliveryDate.Date,
            Notes = (_notes.Text ?? string.Empty).Trim(),
            GenerateReport = _generateReport.IsChecked == true
        });
    }

    private static StackPanel Field(string label, Control input)
    {
        return new StackPanel
        {
            Spacing = 4,
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

    private static Button DialogButton(string text, string backgroundToken, string? foregroundToken, bool bold = false)
    {
        return new Button
        {
            Content = text,
            Background = UiTokens.Brush(backgroundToken),
            Foreground = foregroundToken is null ? UiTokens.Brush(UiTokens.TextPrimary) : UiTokens.Brush(foregroundToken),
            FontWeight = bold ? FontWeight.Bold : FontWeight.Normal,
            Padding = new Thickness(14, 10),
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(8, 0)
        };
    }

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}

public sealed class AssetAssignmentDialogResult
{
    public int AssetId { get; set; }
    public int MasterEmployeeId { get; set; }
    public DateTime DeliveryDate { get; set; } = DateTime.Today;
    public string Notes { get; set; } = string.Empty;
    public bool GenerateReport { get; set; } = true;
}
