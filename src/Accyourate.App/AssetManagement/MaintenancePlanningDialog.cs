using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Models;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class MaintenancePlanningDialog : Window
{
    private readonly ComboBox _asset = new();
    private readonly TextBox _title = new();
    private readonly TextBox _description = new() { AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, MinHeight = 75 };
    private readonly ComboBox _priority = new();
    private readonly TextBox _technician = new();
    private readonly TextBox _scheduled = new() { Watermark = "gg/mm/aaaa" };
    private readonly ComboBox _recurrence = new();
    private readonly ComboBox _reminder = new();
    private readonly TextBlock _message = new();

    public MaintenancePlanningDialog(IReadOnlyList<Asset> assets)
    {
        Title = "Pianifica manutenzione";
        Width = 590;
        Height = 720;
        MinWidth = 520;
        MinHeight = 620;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = UiTokens.Brush(UiTokens.Background);

        _asset.ItemsSource = assets.Select(item => new AssetChoice(item)).ToList();
        _asset.SelectedIndex = assets.Count > 0 ? 0 : -1;
        _priority.ItemsSource = new[] { "Bassa", "Media", "Alta", "Urgente" };
        _priority.SelectedIndex = 1;
        _recurrence.ItemsSource = new[]
        {
            new NumberChoice("Nessuna ricorrenza", 0),
            new NumberChoice("Ogni mese", 1),
            new NumberChoice("Ogni 3 mesi", 3),
            new NumberChoice("Ogni 6 mesi", 6),
            new NumberChoice("Ogni 12 mesi", 12)
        };
        _recurrence.SelectedIndex = 0;
        _reminder.ItemsSource = new[]
        {
            new NumberChoice("Nessun promemoria", 0),
            new NumberChoice("3 giorni prima", 3),
            new NumberChoice("7 giorni prima", 7),
            new NumberChoice("14 giorni prima", 14),
            new NumberChoice("30 giorni prima", 30)
        };
        _reminder.SelectedIndex = 2;

        var root = new StackPanel { Spacing = 11, Margin = new Thickness(24) };
        root.Children.Add(new TextBlock { Text = "Pianifica intervento", FontSize = 26, FontWeight = FontWeight.Bold });
        root.Children.Add(new TextBlock
        {
            Text = "Crea un intervento futuro e, se necessario, rendilo ricorrente.",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });
        root.Children.Add(Field("Asset", _asset));
        root.Children.Add(Field("Titolo", _title));
        root.Children.Add(Field("Descrizione", _description));
        root.Children.Add(Field("Priorità", _priority));
        root.Children.Add(Field("Tecnico responsabile", _technician));
        root.Children.Add(Field("Data prevista", _scheduled));
        root.Children.Add(Field("Ricorrenza", _recurrence));
        root.Children.Add(Field("Promemoria", _reminder));
        root.Children.Add(_message);

        var actions = new Grid { ColumnDefinitions = new ColumnDefinitions("*,120,150") };
        var cancel = Button("Annulla", false);
        cancel.Click += (_, _) => Close(null);
        var save = Button("Pianifica", true);
        save.Click += (_, _) => Confirm();
        Add(actions, cancel, 1);
        Add(actions, save, 2);
        root.Children.Add(actions);
        Content = new ScrollViewer { Content = root };
    }

    private void Confirm()
    {
        if (_asset.SelectedItem is not AssetChoice asset)
        {
            Error("Seleziona un asset.");
            return;
        }
        if (string.IsNullOrWhiteSpace(_title.Text))
        {
            Error("Inserisci il titolo dell'intervento.");
            return;
        }
        if (!DateTime.TryParse(_scheduled.Text, out var scheduled))
        {
            Error("Inserisci una data valida.");
            return;
        }

        var recurrence = (_recurrence.SelectedItem as NumberChoice)?.Value ?? 0;
        var reminder = (_reminder.SelectedItem as NumberChoice)?.Value ?? 0;
        Close(new MaintenanceTicket
        {
            AssetId = asset.Asset.Id,
            Title = _title.Text.Trim(),
            Description = _description.Text?.Trim() ?? string.Empty,
            Priority = _priority.SelectedItem?.ToString() ?? "Media",
            Status = "Pianificato",
            Technician = _technician.Text?.Trim() ?? string.Empty,
            ScheduledAt = scheduled.ToString("s"),
            RecurrenceMonths = recurrence,
            ReminderDays = reminder
        });
    }

    private void Error(string text)
    {
        _message.Text = text;
        _message.Foreground = UiTokens.Brush(UiTokens.Danger);
    }

    private static Control Field(string label, Control control) => new StackPanel
    {
        Spacing = 5,
        Children =
        {
            new TextBlock { Text = label, FontWeight = FontWeight.SemiBold },
            control
        }
    };

    private static Button Button(string text, bool primary) => new()
    {
        Content = text,
        Height = 38,
        Margin = new Thickness(6, 0, 0, 0),
        HorizontalContentAlignment = HorizontalAlignment.Center,
        Background = UiTokens.Brush(primary ? UiTokens.BrandBlue : UiTokens.SurfaceAlt),
        Foreground = primary ? Brushes.White : UiTokens.Brush(UiTokens.TextPrimary)
    };

    private static void Add(Grid grid, Control control, int column)
    {
        Grid.SetColumn(control, column);
        grid.Children.Add(control);
    }

    private sealed class AssetChoice
    {
        public AssetChoice(Asset asset) => Asset = asset;
        public Asset Asset { get; }
        public override string ToString() => $"{Asset.AssetCode} · {Asset.Manufacturer} {Asset.Model}".Trim();
    }

    private sealed record NumberChoice(string Label, int Value)
    {
        public override string ToString() => Label;
    }
}
