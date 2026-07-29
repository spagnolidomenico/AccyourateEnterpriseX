using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement.Deliveries;

public sealed class AssetReturnDialog : Window
{
    private readonly ComboBox _condition = new();
    private readonly TextBox _notes = new()
    {
        AcceptsReturn = true,
        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
        MinHeight = 110,
        Watermark = "Annotazioni, accessori mancanti, danni o interventi necessari..."
    };
    private readonly CheckBox _generatePdf = new()
    {
        Content = "Genera e apri il verbale PDF",
        IsChecked = true
    };
    private readonly TextBlock _message = new();

    public AssetReturnDialog(string assetCode, string employeeName)
    {
        Title = "Riconsegna asset";
        Width = 560;
        Height = 520;
        MinWidth = 500;
        MinHeight = 470;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = UiTokens.Brush(UiTokens.Background);

        _condition.ItemsSource = new[]
        {
            "Buono",
            "Buono con normale usura",
            "Da verificare",
            "Danneggiato",
            "In manutenzione"
        };
        _condition.SelectedIndex = 0;

        var content = new StackPanel { Spacing = 14, Margin = new Thickness(24) };
        content.Children.Add(new TextBlock
        {
            Text = "Riconsegna attrezzatura",
            FontSize = 25,
            FontWeight = Avalonia.Media.FontWeight.Bold
        });
        content.Children.Add(new TextBlock
        {
            Text = $"{assetCode} · {employeeName}",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        });
        content.Children.Add(Field("Condizioni alla riconsegna", _condition));
        content.Children.Add(Field("Note di riconsegna", _notes));
        content.Children.Add(_generatePdf);
        content.Children.Add(_message);

        var buttons = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,130,150"),
            Margin = new Thickness(0, 8, 0, 0)
        };
        var cancel = Button("Annulla", false);
        cancel.Click += (_, _) => Close(null);
        var confirm = Button("Conferma riconsegna", true);
        confirm.Click += (_, _) => Confirm();
        Add(buttons, cancel, 1);
        Add(buttons, confirm, 2);
        content.Children.Add(buttons);

        Content = content;
    }

    private void Confirm()
    {
        var condition = _condition.SelectedItem?.ToString()?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(condition))
        {
            _message.Text = "Seleziona le condizioni del bene.";
            _message.Foreground = UiTokens.Brush(UiTokens.Danger);
            return;
        }

        Close(new AssetReturnDialogResult
        {
            ReturnDate = DateTime.Now,
            Condition = condition,
            Notes = _notes.Text?.Trim() ?? string.Empty,
            GeneratePdf = _generatePdf.IsChecked == true
        });
    }

    private static Control Field(string label, Control control)
    {
        return new StackPanel
        {
            Spacing = 6,
            Children =
            {
                new TextBlock { Text = label, FontWeight = Avalonia.Media.FontWeight.SemiBold },
                control
            }
        };
    }

    private static Button Button(string text, bool primary)
    {
        return new Button
        {
            Content = text,
            Height = 38,
            Margin = new Thickness(6, 0, 0, 0),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Background = UiTokens.Brush(primary ? UiTokens.BrandBlue : UiTokens.SurfaceAlt),
            Foreground = primary
                ? Avalonia.Media.Brushes.White
                : UiTokens.Brush(UiTokens.TextPrimary)
        };
    }

    private static void Add(Grid grid, Control control, int column)
    {
        Grid.SetColumn(control, column);
        grid.Children.Add(control);
    }
}

public sealed class AssetReturnDialogResult
{
    public DateTime ReturnDate { get; init; }
    public string Condition { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
    public bool GeneratePdf { get; init; }
}
