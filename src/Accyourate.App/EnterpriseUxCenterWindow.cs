using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Accyourate.App;

public sealed class EnterpriseUxCenterWindow : Window
{
    public EnterpriseUxCenterWindow()
    {
        Title = "Accyourate Enterprise X - Enterprise UX Center";
        Width = 980;
        Height = 720;
        MinWidth = 900;
        MinHeight = 640;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");
        Content = BuildLayout();
    }

    private Control BuildLayout()
    {
        var stack = new StackPanel { Margin = new Avalonia.Thickness(24), Spacing = 16 };

        stack.Children.Add(new TextBlock
        {
            Text = "Enterprise UX Center",
            FontSize = 30,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });

        stack.Children.Add(new TextBlock
        {
            Text = "Versione 7.0: base per top bar, navigazione avanzata, preferiti, recenti, temi live e dashboard personalizzabili.",
            TextWrapping = TextWrapping.Wrap
        });

        var cards = new WrapPanel { ItemWidth = 280, ItemHeight = 170 };
        cards.Children.Add(Card("🔝 Top bar", "Accesso rapido a ricerca, notifiche, tema, impostazioni e utente."));
        cards.Children.Add(Card("⭐ Preferiti", "Foundation per fissare i moduli più usati."));
        cards.Children.Add(Card("🕒 Recenti", "Foundation per moduli aperti di recente."));
        cards.Children.Add(Card("🎨 Temi live", "Base per applicare colori e tema senza riavvio."));
        cards.Children.Add(Card("📊 Dashboard layout", "Base per widget configurabili e dashboard per ruolo."));
        cards.Children.Add(Card("🌳 Menu evoluto", "Base per menu ad albero con ricerca interna."));
        stack.Children.Add(cards);

        var roadmap = new StackPanel { Spacing = 8 };
        roadmap.Children.Add(new TextBlock { Text = "Roadmap UX", FontSize = 20, FontWeight = FontWeight.Bold, Foreground = Brush.Parse("#B5162B") });
        roadmap.Children.Add(new TextBlock { Text = "7.1 - Preferiti e recenti" });
        roadmap.Children.Add(new TextBlock { Text = "7.2 - Ricerca nel menu" });
        roadmap.Children.Add(new TextBlock { Text = "7.3 - Temi applicati in tempo reale" });
        roadmap.Children.Add(new TextBlock { Text = "7.4 - Dashboard personalizzabile" });
        stack.Children.Add(CardLarge("Prossimi step", roadmap));

        return new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            Content = stack
        };
    }

    private static Border Card(string title, string description)
    {
        return new Border
        {
            Background = Brushes.White,
            CornerRadius = new Avalonia.CornerRadius(14),
            Padding = new Avalonia.Thickness(16),
            Margin = new Avalonia.Thickness(8),
            Child = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new TextBlock { Text = title, FontSize = 18, FontWeight = FontWeight.Bold, Foreground = Brush.Parse("#B5162B") },
                    new TextBlock { Text = description, TextWrapping = TextWrapping.Wrap }
                }
            }
        };
    }

    private static Border CardLarge(string title, Control content)
    {
        return new Border
        {
            Background = Brushes.White,
            CornerRadius = new Avalonia.CornerRadius(14),
            Padding = new Avalonia.Thickness(18),
            Child = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new TextBlock { Text = title, FontSize = 20, FontWeight = FontWeight.Bold, Foreground = Brush.Parse("#B5162B") },
                    content
                }
            }
        };
    }
}
