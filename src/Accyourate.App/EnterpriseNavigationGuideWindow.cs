using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Accyourate.App;

public sealed class EnterpriseNavigationGuideWindow : Window
{
    public EnterpriseNavigationGuideWindow()
    {
        Title = "Accyourate Enterprise X - Enterprise Navigation";
        Width = 920;
        Height = 720;
        MinWidth = 860;
        MinHeight = 620;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");
        Content = BuildLayout();
    }

    private Control BuildLayout()
    {
        var stack = new StackPanel { Margin = new Avalonia.Thickness(24), Spacing = 16 };

        stack.Children.Add(new TextBlock
        {
            Text = "Enterprise Navigation",
            FontSize = 30,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });

        stack.Children.Add(new TextBlock
        {
            Text = "RC 6.1.4.2: menu con icone, sezioni più leggibili e base per menu ad albero futuro.",
            TextWrapping = TextWrapping.Wrap
        });

        var content = new StackPanel { Spacing = 10 };
        content.Children.Add(Section("🏠 Centro Operativo", "Dashboard, Enterprise Dashboard, Analytics Dashboard, Ricerca Globale"));
        content.Children.Add(Section("👥 HR", "Persone, utenti e ruoli"));
        content.Children.Add(Section("💻 IT", "Asset IT"));
        content.Children.Add(Section("🏥 Medical Suite", "Dispositivi, produzione, qualità e Digital Twin"));
        content.Children.Add(Section("📦 Logistica", "Magazzino, movimentazioni, spedizioni e rientri"));
        content.Children.Add(Section("🧺 Assistenza", "Lavaggi, manutenzioni e riparazioni"));
        content.Children.Add(Section("📁 Documentale", "Archivio documenti e allegati"));
        content.Children.Add(Section("⚙️ Amministrazione", "Audit, backup, workflow, impostazioni e architettura"));

        stack.Children.Add(Card(content));

        return new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            Content = stack
        };
    }

    private static Border Section(string title, string description)
    {
        return new Border
        {
            Background = Brushes.White,
            CornerRadius = new Avalonia.CornerRadius(12),
            Padding = new Avalonia.Thickness(14),
            Child = new StackPanel
            {
                Spacing = 4,
                Children =
                {
                    new TextBlock { Text = title, FontSize = 18, FontWeight = FontWeight.Bold, Foreground = Brush.Parse("#B5162B") },
                    new TextBlock { Text = description, TextWrapping = TextWrapping.Wrap, Foreground = Brush.Parse("#555555") }
                }
            }
        };
    }

    private static Border Card(Control content)
    {
        return new Border
        {
            Background = Brushes.White,
            CornerRadius = new Avalonia.CornerRadius(14),
            Padding = new Avalonia.Thickness(18),
            Child = content
        };
    }
}
