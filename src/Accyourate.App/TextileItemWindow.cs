using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.Models;

namespace Accyourate.App;

public sealed class TextileItemWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;
    private readonly MedicalDeviceRecord _device;
    private readonly TextBlock _message = new();

    private readonly ComboBox _type = new();
    private readonly TextBox _size = new();
    private readonly TextBox _color = new();
    private readonly TextBox _lot = new();
    private readonly TextBox _rfid = new();
    private readonly TextBox _washCount = new();
    private readonly TextBox _testDate = new();
    private readonly ComboBox _testResult = new();
    private readonly ComboBox _conformity = new();
    private readonly TextBox _notes = new();

    public TextileItemWindow(DatabaseService database, CurrentUser user, MedicalDeviceRecord device)
    {
        _database = database;
        _user = user;
        _device = device;

        Title = $"Capo Tessile - {_device.DeviceCode}";
        Width = 620;
        Height = 720;
        
        MinWidth = 1024;
        MinHeight = 680;
WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");

        Content = BuildLayout();
    }

    private Control BuildLayout()
    {
        var stack = new StackPanel { Margin = new Thickness(24), Spacing = 10 };

        stack.Children.Add(new TextBlock { Text = $"Capo Tessile - {_device.DeviceCode}", FontSize = 24, FontWeight = FontWeight.Bold, Foreground = Brush.Parse("#B5162B") });

        _type.ItemsSource = new[] { "Top", "T-Shirt", "Gilet", "Fascia", "Kit" };
        _type.SelectedIndex = 0;
        _size.Watermark = "M";
        _color.Watermark = "Nero";
        _lot.Watermark = "LOT001";
        _rfid.Watermark = "RFID001";
        _washCount.Watermark = "0";
        _testDate.Watermark = "2026-01-01";
        _testResult.ItemsSource = new[] { "Conforme", "Non conforme", "Da testare" };
        _testResult.SelectedIndex = 0;
        _conformity.ItemsSource = new[] { "Conforme", "Non conforme", "In verifica" };
        _conformity.SelectedIndex = 0;

        stack.Children.Add(new TextBlock { Text = "Tipo capo", FontWeight = FontWeight.Bold });
        stack.Children.Add(_type);
        AddField(stack, "Taglia", _size);
        AddField(stack, "Colore", _color);
        AddField(stack, "Lotto", _lot);
        AddField(stack, "RFID", _rfid);
        AddField(stack, "Numero lavaggi", _washCount);
        AddField(stack, "Data test", _testDate);
        stack.Children.Add(new TextBlock { Text = "Esito test", FontWeight = FontWeight.Bold });
        stack.Children.Add(_testResult);
        stack.Children.Add(new TextBlock { Text = "Conformità", FontWeight = FontWeight.Bold });
        stack.Children.Add(_conformity);
        AddField(stack, "Note", _notes);

        _message.Foreground = Brush.Parse("#B5162B");
        stack.Children.Add(_message);

        var save = new Button { Content = "Salva Capo Tessile", Background = Brush.Parse("#B5162B"), Foreground = Brushes.White, FontWeight = FontWeight.Bold, Padding = new Thickness(12) };
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
        var item = new TextileItemRecord
        {
            MedicalDeviceId = _device.Id,
            TextileType = _type.SelectedItem?.ToString() ?? "",
            Size = _size.Text ?? "",
            Color = _color.Text ?? "",
            LotNumber = _lot.Text ?? "",
            RfidCode = _rfid.Text ?? "",
            WashCount = int.TryParse(_washCount.Text, out var wash) ? wash : 0,
            LastFunctionalTestDate = _testDate.Text ?? "",
            LastFunctionalTestResult = _testResult.SelectedItem?.ToString() ?? "",
            ConformityStatus = _conformity.SelectedItem?.ToString() ?? "",
            Notes = _notes.Text ?? ""
        };

        var ok = _database.CreateTextileItem(item, _user.Username, out var error);
        _message.Text = ok ? "Capo tessile salvato." : error;
    }
}
