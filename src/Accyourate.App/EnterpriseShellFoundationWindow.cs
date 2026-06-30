using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Components;
using Accyourate.App.UIFramework.Shell;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App;

public sealed class EnterpriseShellFoundationWindow : Window
{
    public EnterpriseShellFoundationWindow()
    {
        Title = "Accyourate Enterprise X 8.0 - Enterprise UI Framework";
        Width = 1280;
        Height = 820;
        MinWidth = 1100;
        MinHeight = 720;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();
    }

    private Control BuildLayout()
    {
        var root = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("280,*")
        };

        Add(root, BuildSidebar(), 0, 0);
        Add(root, BuildContent(), 1, 0);

        return root;
    }

    private Control BuildSidebar()
    {
        var sidebar = new DockPanel
        {
            Background = UiTokens.Brush(UiTokens.Sidebar)
        };

        var header = new StackPanel
        {
            Margin = new Thickness(22),
            Spacing = 4
        };
        header.Children.Add(new TextBlock
        {
            Text = "Accyourate",
            FontSize = 22,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        header.Children.Add(new TextBlock
        {
            Text = "Enterprise UI Framework",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });
        DockPanel.SetDock(header, Dock.Top);
        sidebar.Children.Add(header);

        var menu = new StackPanel { Margin = new Thickness(14), Spacing = 5 };
        var currentSection = "";

        foreach (var module in ShellRegistry.Modules)
        {
            if (module.Section != currentSection)
            {
                currentSection = module.Section;
                menu.Children.Add(new TextBlock
                {
                    Text = currentSection.ToUpperInvariant(),
                    FontSize = 12,
                    FontWeight = FontWeight.Bold,
                    Foreground = UiTokens.Brush(UiTokens.TextSecondary),
                    Margin = new Thickness(10, 14, 10, 4)
                });
            }

            menu.Children.Add(MenuItem(module.Icon, module.Title));
        }

        sidebar.Children.Add(new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Content = menu
        });

        return new Border
        {
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(0, 0, 1, 0),
            Child = sidebar
        };
    }

    private Control BuildContent()
    {
        var dock = new DockPanel();

        var top = new Grid
        {
            Height = 72,
            Background = UiTokens.Brush(UiTokens.Surface),
            ColumnDefinitions = new ColumnDefinitions("*,140,140,180")
        };

        Add(top, SearchBox(), 0, 0);
        Add(top, TopButton("🔔 Notifiche"), 1, 0);
        Add(top, TopButton("🎨 Tema"), 2, 0);
        Add(top, new TextBlock
        {
            Text = "👤 admin",
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Margin = new Thickness(0, 0, 20, 0),
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        }, 3, 0);

        DockPanel.SetDock(top, Dock.Top);
        dock.Children.Add(new Border
        {
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child = top
        });

        var page = new StackPanel
        {
            Margin = new Thickness(26),
            Spacing = 18
        };

        page.Children.Add(UiComponentFactory.Title("Enterprise UI Framework Foundation"));
        page.Children.Add(UiComponentFactory.Body("Versione 8.0: da questo punto il refactoring dell'interfaccia seguirà un approccio component-based e XAML-first. Le funzionalità esistenti restano validate e verranno migrate progressivamente dentro la shell enterprise."));

        var cards = new WrapPanel { ItemWidth = 300, ItemHeight = 170 };
        cards.Children.Add(InfoCard("XAML-first", "Le nuove UI saranno definite con risorse e componenti riutilizzabili."));
        cards.Children.Add(InfoCard("Shell unica", "I moduli verranno caricati in un'area centrale, non più come finestre isolate."));
        cards.Children.Add(InfoCard("Design System", "Colori, card, pulsanti, form e tabelle saranno standardizzati."));
        cards.Children.Add(InfoCard("Refactoring sicuro", "Ogni modulo sarà migrato uno alla volta mantenendo le funzioni già validate."));
        page.Children.Add(cards);

        var roadmap = new StackPanel { Spacing = 8 };
        roadmap.Children.Add(new TextBlock { Text = "Roadmap UI Framework", FontSize = 20, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary) });
        roadmap.Children.Add(UiComponentFactory.Body("8.1 - Enterprise Shell con area contenuti reale"));
        roadmap.Children.Add(UiComponentFactory.Body("8.2 - Dashboard refactor dentro la shell"));
        roadmap.Children.Add(UiComponentFactory.Body("8.3 - Medical Suite refactor"));
        roadmap.Children.Add(UiComponentFactory.Body("8.4 - Document Management refactor"));
        roadmap.Children.Add(UiComponentFactory.Body("8.5 - Theme/Branding integrati nella shell"));
        page.Children.Add(UiComponentFactory.Card(roadmap));

        dock.Children.Add(new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Content = page
        });

        return dock;
    }

    private static Border MenuItem(string icon, string title)
    {
        var row = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 12
        };

        row.Children.Add(new TextBlock
        {
            Text = icon,
            Width = 24,
            FontSize = 18,
            Foreground = UiTokens.Brush(UiTokens.BrandBlue)
        });

        row.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        return new Border
        {
            Background = Brushes.Transparent,
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(12, 9),
            Child = row
        };
    }

    private static Border InfoCard(string title, string body)
    {
        var stack = new StackPanel { Spacing = 8 };
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        stack.Children.Add(UiComponentFactory.Body(body));
        return UiComponentFactory.Card(stack);
    }

    private static Border SearchBox()
    {
        return new Border
        {
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            CornerRadius = new CornerRadius(18),
            Margin = new Thickness(18, 16),
            Padding = new Thickness(16, 8),
            Child = new TextBlock
            {
                Text = "🔍 Cerca nel gestionale...",
                Foreground = UiTokens.Brush(UiTokens.TextSecondary)
            }
        };
    }

    private static Button TopButton(string text)
    {
        return new Button
        {
            Content = text,
            Background = Brushes.Transparent,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            Padding = new Thickness(8)
        };
    }

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
