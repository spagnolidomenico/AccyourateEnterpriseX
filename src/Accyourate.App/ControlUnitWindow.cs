using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.Models;

namespace Accyourate.App;

public sealed class ControlUnitWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;
    private readonly MedicalDeviceRecord _device;
    private readonly TextBlock _message = new();

    private readonly TextBox _firmware = new();
    private readonly TextBox _hardware = new();
    private readonly TextBox _mac = new();
    private readonly TextBox _battery = new();
    private readonly TextBox _testDate = new();
    private readonly ComboBox _testResult = new();
    private readonly TextBox _notes = new();

    public ControlUnitWindow(DatabaseService database, CurrentUser user, MedicalDeviceRecord device)
    {
        _database = database;
        _user = user;
        _device = device;

        Title = $"Control Unit - {_device.DeviceCode}";
        Width = 620;
        Height = 620;
        
        MinWidth = 1024;
        MinHeight = 680;
WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");

        Content = BuildLayout();
    }

    private Control BuildLayout()
    {
        var stack = new StackPanel { Margin = new Thickness(24), Spacing = 10 };

        stack.Children.Add(new TextBlock { Text = $"Control Unit - {_device.DeviceCode}", FontSize = 24, FontWeight = FontWeight.Bold, Foreground = Brush.Parse("#B5162B") });

        _firmware.Watermark = "1.0.0";
        _hardware.Watermark = "HW-A";
        _mac.Watermark = "00:11:22:33:44:55";
        _battery.Watermark = "OK";
        _testDate.Watermark = "2026-01-01";
        _testResult.ItemsSource = new[] { "Conforme", "Non conforme", "Da testare" };
        _testResult.SelectedIndex = 0;
        _notes.Watermark = "Note";

        AddField(stack, "Firmware", _firmware);
        AddField(stack, "Hardware revision", _hardware);
        AddField(stack, "MAC Address", _mac);
        AddField(stack, "Batteria", _battery);
        AddField(stack, "Data test", _testDate);
        stack.Children.Add(new TextBlock { Text = "Esito test", FontWeight = FontWeight.Bold });
        stack.Children.Add(_testResult);
        AddField(stack, "Note", _notes);

        _message.Foreground = Brush.Parse("#B5162B");
        stack.Children.Add(_message);

        var save = new Button { Content = "Salva Control Unit", Background = Brush.Parse("#B5162B"), Foreground = Brushes.White, FontWeight = FontWeight.Bold, Padding = new Thickness(12) };
        save.Click += (_, _) => Save();
        stack.Children.Add(save);

        return new ScrollViewer { Content = new Border { Background = Brushes.White, CornerRadius = new CornerRadius(16), Padding = new Thickness(18), Margin = new Thickness(20), Child = stack } };
    }

    private static void AddField(StackPanel stack, string label, TextBox box)
    {
        stack.Children.Add(new TextBlock { Text = label, FontWeight = FontWeight.Bold });
        stack.Children.Add(box);
    }

    private void Save()
    {
        var cu = new ControlUnitRecord
        {
            MedicalDeviceId = _device.Id,
            FirmwareVersion = _firmware.Text ?? "",
            HardwareRevision = _hardware.Text ?? "",
            MacAddress = _mac.Text ?? "",
            BatteryStatus = _battery.Text ?? "",
            LastFunctionalTestDate = _testDate.Text ?? "",
            LastFunctionalTestResult = _testResult.SelectedItem?.ToString() ?? "",
            Notes = _notes.Text ?? ""
        };

        var ok = _database.CreateControlUnit(cu, _user.Username, out var error);
        _message.Text = ok ? "Control Unit salvata." : error;
    }
}
