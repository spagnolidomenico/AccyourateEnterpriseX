using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;

namespace Accyourate.App;

public sealed class MedicalDeviceTwinWindow : Window
{
    private readonly DatabaseService _database;
    private readonly MedicalDeviceRecord _device;

    public MedicalDeviceTwinWindow(DatabaseService database, MedicalDeviceRecord device)
    {
        _database = database;
        _device = device;

        Title = $"Digital Twin - {_device.DeviceCode}";
        Width = 900;
        Height = 760;
        
        MinWidth = 1024;
        MinHeight = 680;
WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");

        Content = BuildLayout();
    }

    private Control BuildLayout()
    {
        var scroll = new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
        var stack = new StackPanel { Margin = new Thickness(24), Spacing = 14 };

        stack.Children.Add(new TextBlock
        {
            Text = $"Digital Twin - {_device.DeviceCode}",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });

        AddInfo(stack, "Tipo", _device.DeviceType);
        AddInfo(stack, "Modello", _device.Model);
        AddInfo(stack, "Seriale", _device.SerialNumber);
        AddInfo(stack, "Lotto", _device.LotNumber);
        AddInfo(stack, "RFID", _device.RfidCode);
        AddInfo(stack, "QR", _device.QrCode);
        AddInfo(stack, "Stato", _device.Status);
        AddInfo(stack, "Produzione", _device.ProductionDate);
        AddInfo(stack, "Collaudo", _device.TestDate);

        stack.Children.Add(new Separator());

        stack.Children.Add(new TextBlock
        {
            Text = "Timeline Workflow",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });

        var events = _database.GetWorkflowEvents(null, _device.DeviceCode, 200);
        foreach (var e in events)
        {
            stack.Children.Add(new TextBlock
            {
                Text = $"{e.CreatedAt} | {e.EntityType} | {e.EventType} | {e.FromStatus} → {e.ToStatus} | {e.Notes} | {e.CreatedBy}"
            });
        }

        if (events.Count == 0)
            stack.Children.Add(new TextBlock { Text = "Nessun evento ancora registrato." });

        scroll.Content = new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(20),
            Margin = new Thickness(20),
            Child = stack
        };

        return scroll;
    }

    private static void AddInfo(StackPanel stack, string label, string value)
    {
        stack.Children.Add(new TextBlock { Text = label, FontWeight = FontWeight.Bold, Foreground = Brush.Parse("#555555") });
        stack.Children.Add(new TextBlock { Text = string.IsNullOrWhiteSpace(value) ? "-" : value });
    }
}
