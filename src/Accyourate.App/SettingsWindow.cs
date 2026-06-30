using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.Shared.UI;

namespace Accyourate.App;

public sealed class SettingsWindow : Window
{
    private readonly DatabaseService _database;
    private readonly StackPanel _content = new();

    public SettingsWindow(DatabaseService database)
    {
        _database = database;

        Title = "Accyourate Enterprise X - Impostazioni";
        Width = 840;
        Height = 680;
        
        MinWidth = 1024;
        MinHeight = 680;
WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");

        Content = BuildLayout();
        Refresh();
    }

    private Control BuildLayout()
    {
        var scroll = new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
        var stack = new StackPanel { Margin = new Thickness(24), Spacing = 16 };

        stack.Children.Add(UiFactory.PageTitle("Impostazioni"));
        stack.Children.Add(new TextBlock
        {
            Text = "Foundation 4.1.5: base configurazione centralizzata per azienda, tema, backup, notifiche e moduli."
        });

        stack.Children.Add(UiFactory.Card(_content));

        scroll.Content = stack;
        return scroll;
    }

    private void Refresh()
    {
        _content.Children.Clear();
        _content.Spacing = 10;

        _content.Children.Add(UiFactory.SectionTitle("Configurazioni disponibili"));

        foreach (var item in _database.GetSettings())
        {
            _content.Children.Add(new TextBlock
            {
                Text = $"[{item.GroupName}] {item.Key} = {item.Value}"
            });
        }

        _content.Children.Add(new Separator());
        _content.Children.Add(UiFactory.SectionTitle("Prossimi parametri configurabili"));
        _content.Children.Add(new TextBlock { Text = "• Logo aziendale" });
        _content.Children.Add(new TextBlock { Text = "• Numerazioni automatiche" });
        _content.Children.Add(new TextBlock { Text = "• Soglie lavaggio" });
        _content.Children.Add(new TextBlock { Text = "• Soglie manutenzione" });
        _content.Children.Add(new TextBlock { Text = "• Percorsi backup" });
        _content.Children.Add(new TextBlock { Text = "• Moduli attivi/disattivi" });
    }
}
