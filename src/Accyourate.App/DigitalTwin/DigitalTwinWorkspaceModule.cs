using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.UIFramework.Components;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.DigitalTwin;

public sealed class DigitalTwinWorkspaceModule
{
    private readonly DigitalTwinService _service;

    public DigitalTwinWorkspaceModule(DatabaseService database)
    {
        _service = new DigitalTwinService(database);
    }

    public Control Build()
    {
        var page = new StackPanel { Margin = new Thickness(24), Spacing = 18 };

        page.Children.Add(UiComponentFactory.Title("Digital Twin Platform"));
        page.Children.Add(UiComponentFactory.Body("Fondazione 9.0 per capi tessili medicali intelligenti con monitoraggio ECG, battito cardiaco, batteria, qualità segnale e lifecycle dispositivo."));

        var kpis = new WrapPanel { ItemWidth = 260, ItemHeight = 135 };
        kpis.Children.Add(Kpi("●", "Online", _service.CountOnline().ToString(), "Dispositivi connessi", UiTokens.Success));
        kpis.Children.Add(Kpi("○", "Offline", _service.CountOffline().ToString(), "Da verificare", UiTokens.Warning));
        kpis.Children.Add(Kpi("!", "Batteria bassa", _service.CountLowBattery().ToString(), "Sotto 20%", UiTokens.Danger));
        kpis.Children.Add(Kpi("♡", "ECG normale", _service.CountEcgNormal().ToString(), "Ultima lettura", UiTokens.BrandBlue));
        page.Children.Add(kpis);

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("1.35*,*") };
        Add(grid, UiComponentFactory.Card(DeviceTable()), 0, 0);
        Add(grid, UiComponentFactory.Card(ClinicalPanel()), 1, 0);
        page.Children.Add(grid);

        var lower = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*") };
        Add(lower, UiComponentFactory.Card(TelemetryFeed()), 0, 0);
        Add(lower, UiComponentFactory.Card(TwinLifecycle()), 1, 0);
        page.Children.Add(lower);

        return new ScrollViewer
        {
            Content = page,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
    }

    private Control DeviceTable()
    {
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(Section("Dispositivi Digital Twin"));

        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("90,130,130,90,90,110") };
        AddHeader(header, "Codice", 0);
        AddHeader(header, "Tipo", 1);
        AddHeader(header, "Modello", 2);
        AddHeader(header, "Stato", 3);
        AddHeader(header, "Batteria", 4);
        AddHeader(header, "ECG", 5);
        stack.Children.Add(header);

        foreach (var d in _service.GetDevices())
        {
            var row = new Grid { ColumnDefinitions = new ColumnDefinitions("90,130,130,90,90,110") };
            AddCell(row, d.Code, 0);
            AddCell(row, d.Type, 1);
            AddCell(row, d.Model, 2);
            AddCell(row, d.Status, 3);
            AddCell(row, $"{d.BatteryLevel}%", 4);
            AddCell(row, d.EcgStatus, 5);
            stack.Children.Add(row);
        }

        return stack;
    }

    private Control ClinicalPanel()
    {
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(Section("Monitoraggio clinico"));
        var first = _service.GetDevices().FirstOrDefault(d => d.Status == "Online");

        if (first is null)
        {
            stack.Children.Add(UiComponentFactory.Body("Nessun dispositivo online."));
            return stack;
        }

        stack.Children.Add(Line("Dispositivo", first.Code, UiTokens.BrandBlue));
        stack.Children.Add(Line("Battito cardiaco", $"{first.HeartRate} bpm", UiTokens.Success));
        stack.Children.Add(Line("ECG", first.EcgStatus, UiTokens.BrandBlue));
        stack.Children.Add(Line("Qualità segnale", first.SignalQuality, UiTokens.Success));
        stack.Children.Add(Line("Batteria", $"{first.BatteryLevel}%", first.BatteryLevel < 20 ? UiTokens.Danger : UiTokens.Success));
        stack.Children.Add(Line("Firmware", first.Firmware, UiTokens.TextSecondary));
        stack.Children.Add(Line("Assegnato a", first.AssignedTo, UiTokens.TextPrimary));

        return stack;
    }

    private Control TelemetryFeed()
    {
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(Section("Telemetry feed"));

        foreach (var t in _service.GetTelemetry())
        {
            stack.Children.Add(UiComponentFactory.Body($"{t.Timestamp:HH:mm:ss} · {t.DeviceCode} · {t.EventType} · HR {t.HeartRate} · BAT {t.BatteryLevel}% · ECG {t.EcgStatus}"));
        }

        return stack;
    }

    private Control TwinLifecycle()
    {
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(Section("Lifecycle Digital Twin"));
        stack.Children.Add(UiComponentFactory.Body("1. Produzione e serializzazione"));
        stack.Children.Add(UiComponentFactory.Body("2. Associazione QR/RFID/NFC"));
        stack.Children.Add(UiComponentFactory.Body("3. Collaudo e certificazione"));
        stack.Children.Add(UiComponentFactory.Body("4. Assegnazione cliente/paziente"));
        stack.Children.Add(UiComponentFactory.Body("5. Telemetria ECG e battito cardiaco"));
        stack.Children.Add(UiComponentFactory.Body("6. Manutenzione, sanificazione e storico eventi"));
        return stack;
    }

    private Border Kpi(string icon, string title, string value, string subtitle, string color)
    {
        var row = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 14 };
        row.Children.Add(new Border
        {
            Width = 52,
            Height = 52,
            Background = UiTokens.Brush(color),
            CornerRadius = new CornerRadius(14),
            Child = new TextBlock { Text = icon, Foreground = Brushes.White, FontSize = 24, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center }
        });

        var col = new StackPanel();
        col.Children.Add(new TextBlock { Text = title, Foreground = UiTokens.Brush(UiTokens.TextPrimary) });
        col.Children.Add(new TextBlock { Text = value, FontSize = 26, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary) });
        col.Children.Add(new TextBlock { Text = subtitle, Foreground = UiTokens.Brush(UiTokens.TextSecondary) });
        row.Children.Add(col);

        return UiComponentFactory.Card(row);
    }

    private static TextBlock Section(string text) => new()
    {
        Text = text,
        FontSize = 18,
        FontWeight = FontWeight.Bold,
        Foreground = UiTokens.Brush(UiTokens.TextPrimary)
    };

    private static Control Line(string label, string value, string color)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,150") };
        Add(grid, new TextBlock { Text = label, Foreground = UiTokens.Brush(UiTokens.TextSecondary) }, 0, 0);
        Add(grid, new TextBlock { Text = value, Foreground = UiTokens.Brush(color), FontWeight = FontWeight.Bold, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right }, 1, 0);
        return grid;
    }

    private static void AddHeader(Grid grid, string text, int col)
    {
        Add(grid, new TextBlock { Text = text, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary) }, col, 0);
    }

    private static void AddCell(Grid grid, string text, int col)
    {
        Add(grid, new TextBlock { Text = text, Foreground = UiTokens.Brush(UiTokens.TextSecondary) }, col, 0);
    }

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
