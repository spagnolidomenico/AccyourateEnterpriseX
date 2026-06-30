using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.Models;

namespace Accyourate.App;

public sealed class AppleStyleDashboardWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;

    private const string BackgroundColor = "#F5F5F7";
    private const string SidebarColor = "#FBFBFD";
    private const string CardColor = "#FFFFFF";
    private const string TextColor = "#1D1D1F";
    private const string MutedTextColor = "#6E6E73";
    private const string BorderColor = "#E5E5EA";
    private const string BlueColor = "#0A84FF";
    private const string GreenColor = "#34C759";
    private const string OrangeColor = "#FF9F0A";
    private const string RedColor = "#FF3B30";
    private const string PurpleColor = "#8E5CF7";

    public AppleStyleDashboardWindow(DatabaseService database, CurrentUser user)
    {
        _database = database;
        _user = user;

        Title = "Accyourate Enterprise X 7.1.2 - Apple Style UX";
        Width = 1420;
        Height = 900;
        MinWidth = 1180;
        MinHeight = 760;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse(BackgroundColor);
        Content = BuildLayout();
    }

    private Control BuildLayout()
    {
        var root = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("280,*")
        };

        Add(root, BuildSidebar(), 0, 0);
        Add(root, BuildMainArea(), 1, 0);

        return root;
    }

    private Control BuildSidebar()
    {
        var sidebar = new DockPanel
        {
            Background = Brush.Parse(SidebarColor)
        };

        var brand = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Margin = new Thickness(20, 22, 20, 16),
            Spacing = 12
        };

        var logo = new Border
        {
            Width = 44,
            Height = 44,
            Background = Brush.Parse("#0B1220"),
            CornerRadius = new CornerRadius(10)
        };
        logo.Child = new TextBlock
        {
            Text = "A",
            Foreground = Brushes.White,
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        var brandText = new StackPanel();
        brandText.Children.Add(new TextBlock { Text = "Accyourate", FontWeight = FontWeight.Bold, FontSize = 17, Foreground = Brush.Parse(TextColor) });
        brandText.Children.Add(new TextBlock { Text = "Enterprise X", Foreground = Brush.Parse(MutedTextColor) });

        brand.Children.Add(logo);
        brand.Children.Add(brandText);

        DockPanel.SetDock(brand, Dock.Top);
        sidebar.Children.Add(brand);

        var menu = new StackPanel
        {
            Margin = new Thickness(16, 8),
            Spacing = 6
        };

        menu.Children.Add(MenuItem("⌂", "Dashboard", true));
        menu.Children.Add(MenuItem("▥", "Analytics Dashboard", false));
        menu.Children.Add(MenuItem("⌘", "Medical Device Suite", false));
        menu.Children.Add(MenuItem("▣", "Asset Management", false));
        menu.Children.Add(MenuItem("▤", "Produzione", false));
        menu.Children.Add(MenuItem("□", "Magazzino & Logistica", false));
        menu.Children.Add(MenuItem("✓", "Qualità", false));
        menu.Children.Add(MenuItem("▧", "Document Management", false));
        menu.Children.Add(MenuItem("⌬", "Enterprise Architecture", false));
        menu.Children.Add(MenuItem("⚙", "Amministrazione", false));

        menu.Children.Add(new Separator { Margin = new Thickness(8, 14) });
        menu.Children.Add(MenuItem("☆", "Preferiti", false));
        menu.Children.Add(MenuItem("◷", "Recenti", false));
        menu.Children.Add(MenuItem("⌫", "Cestino", false));

        var scroller = new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Content = menu
        };

        sidebar.Children.Add(scroller);

        var border = new Border
        {
            BorderBrush = Brush.Parse(BorderColor),
            BorderThickness = new Thickness(0, 0, 1, 0),
            Child = sidebar
        };

        return border;
    }

    private Control BuildMainArea()
    {
        var dock = new DockPanel();

        var top = BuildTopBar();
        DockPanel.SetDock(top, Dock.Top);
        dock.Children.Add(top);

        var body = new StackPanel
        {
            Margin = new Thickness(26),
            Spacing = 18
        };

        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("*,220") };

        var title = new StackPanel { Spacing = 6 };
        title.Children.Add(new TextBlock
        {
            Text = $"Benvenuto, {_user.Username} 👋",
            FontSize = 26,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse(TextColor)
        });
        title.Children.Add(new TextBlock
        {
            Text = "Ecco cosa sta succedendo nella tua azienda oggi.",
            FontSize = 15,
            Foreground = Brush.Parse(MutedTextColor)
        });

        Add(header, title, 0, 0);
        Add(header, new TextBlock
        {
            Text = DateTime.Now.ToString("dd MMMM yyyy"),
            FontSize = 16,
            FontWeight = FontWeight.SemiBold,
            Foreground = Brush.Parse(TextColor),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        }, 1, 0);

        body.Children.Add(header);

        var kpis = new WrapPanel { ItemWidth = 275, ItemHeight = 140 };
        kpis.Children.Add(KpiCard("⌁", "Dispositivi attivi", _database.CountTable("medical_devices").ToString(), "Digital Twin", PurpleColor));
        kpis.Children.Add(KpiCard("▣", "Asset IT", _database.CountTable("assets").ToString(), "Inventario", BlueColor));
        kpis.Children.Add(KpiCard("✓", "Interventi", _database.CountTable("maintenance_records").ToString(), "Manutenzioni", GreenColor));
        kpis.Children.Add(KpiCard("▧", "Documenti", _database.CountTable("documents").ToString(), "Archivio", OrangeColor));
        body.Children.Add(kpis);

        var charts = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*") };
        Add(charts, LargeCard("Andamento interventi", BuildBarTrend()), 0, 0);
        Add(charts, LargeCard("Stato dispositivi", BuildStatusPanel()), 1, 0);
        body.Children.Add(charts);

        var lower = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*") };
        Add(lower, LargeCard("Attività recenti", RecentActivity()), 0, 0);
        Add(lower, LargeCard("Promemoria e scadenze", Reminders()), 1, 0);
        body.Children.Add(lower);

        dock.Children.Add(new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Content = body
        });

        return dock;
    }

    private Control BuildTopBar()
    {
        var top = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("440,*,130,110,150,180"),
            Height = 78,
            Background = Brush.Parse(CardColor)
        };

        Add(top, SearchBox(), 0, 0);
        Add(top, new Border(), 1, 0);
        Add(top, TopButton("🔔 Notifiche 5"), 2, 0);
        Add(top, TopButton("🎨 Tema"), 3, 0);
        Add(top, TopButton("⚙ Impostazioni"), 4, 0);
        Add(top, UserBox(), 5, 0);

        return new Border
        {
            BorderBrush = Brush.Parse(BorderColor),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child = top
        };
    }

    private static Border MenuItem(string icon, string text, bool selected)
    {
        var row = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 12
        };

        row.Children.Add(new TextBlock
        {
            Text = icon,
            Width = 22,
            FontSize = 18,
            Foreground = selected ? Brush.Parse(BlueColor) : Brush.Parse("#4B5563")
        });

        row.Children.Add(new TextBlock
        {
            Text = text,
            FontSize = 15,
            FontWeight = selected ? FontWeight.SemiBold : FontWeight.Normal,
            Foreground = selected ? Brush.Parse(BlueColor) : Brush.Parse(TextColor)
        });

        return new Border
        {
            Background = selected ? Brush.Parse("#E8F1FF") : Brushes.Transparent,
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(12, 10),
            Child = row
        };
    }

    private static Border SearchBox()
    {
        return new Border
        {
            Background = Brush.Parse("#F2F2F7"),
            CornerRadius = new CornerRadius(18),
            Margin = new Thickness(24, 18, 10, 18),
            Padding = new Thickness(16, 8),
            Child = new TextBlock
            {
                Text = "🔍   Cerca globale...",
                Foreground = Brush.Parse(MutedTextColor),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            }
        };
    }

    private static Button TopButton(string text)
    {
        return new Button
        {
            Content = text,
            Background = Brushes.Transparent,
            Foreground = Brush.Parse(TextColor),
            Padding = new Thickness(8),
            Margin = new Thickness(4)
        };
    }

    private static Border UserBox()
    {
        var row = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 10
        };

        row.Children.Add(new Border
        {
            Width = 38,
            Height = 38,
            CornerRadius = new CornerRadius(19),
            Background = Brush.Parse("#D1E9FF"),
            Child = new TextBlock
            {
                Text = "👤",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            }
        });

        var text = new StackPanel();
        text.Children.Add(new TextBlock { Text = "Administrator", FontWeight = FontWeight.Bold, Foreground = Brush.Parse(TextColor) });
        text.Children.Add(new TextBlock { Text = "admin", Foreground = Brush.Parse(MutedTextColor) });
        row.Children.Add(text);

        return new Border
        {
            Margin = new Thickness(6, 12),
            Child = row
        };
    }

    private static Border KpiCard(string icon, string title, string value, string subtitle, string color)
    {
        var row = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 16
        };

        row.Children.Add(new Border
        {
            Width = 58,
            Height = 58,
            Background = Brush.Parse(color),
            CornerRadius = new CornerRadius(14),
            Child = new TextBlock
            {
                Text = icon,
                Foreground = Brushes.White,
                FontSize = 28,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            }
        });

        var text = new StackPanel();
        text.Children.Add(new TextBlock { Text = title, Foreground = Brush.Parse(TextColor) });
        text.Children.Add(new TextBlock { Text = value, FontSize = 28, FontWeight = FontWeight.Bold, Foreground = Brush.Parse(TextColor) });
        text.Children.Add(new TextBlock { Text = subtitle, Foreground = Brush.Parse(MutedTextColor) });
        row.Children.Add(text);

        return Card(row, 18);
    }

    private static Border LargeCard(string title, Control child)
    {
        var stack = new StackPanel { Spacing = 12 };
        stack.Children.Add(new TextBlock { Text = title, FontSize = 17, FontWeight = FontWeight.Bold, Foreground = Brush.Parse(TextColor) });
        stack.Children.Add(child);
        return Card(stack, 18);
    }

    private static Border Card(Control child, double padding)
    {
        return new Border
        {
            Background = Brush.Parse(CardColor),
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(padding),
            Margin = new Thickness(6),
            BoxShadow = new BoxShadows(new BoxShadow { Blur = 16, Spread = 0, OffsetY = 4, Color = Color.Parse("#14000000") }),
            Child = child
        };
    }

    private static Control BuildBarTrend()
    {
        var panel = new StackPanel { Spacing = 10, Height = 180 };
        var values = new[] { 22, 30, 20, 34, 46, 30, 39 };
        var labels = new[] { "10 Mag", "11 Mag", "12 Mag", "13 Mag", "14 Mag", "15 Mag", "16 Mag" };

        for (var i = 0; i < values.Length; i++)
        {
            var row = new Grid { ColumnDefinitions = new ColumnDefinitions("70,*,45") };
            Add(row, new TextBlock { Text = labels[i], Foreground = Brush.Parse(MutedTextColor) }, 0, 0);
            Add(row, new Border
            {
                Height = 12,
                Width = values[i] * 8,
                Background = Brush.Parse(BlueColor),
                CornerRadius = new CornerRadius(6),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            }, 1, 0);
            Add(row, new TextBlock { Text = values[i].ToString(), Foreground = Brush.Parse(TextColor) }, 2, 0);
            panel.Children.Add(row);
        }

        return panel;
    }

    private static Control BuildStatusPanel()
    {
        var row = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 26
        };

        row.Children.Add(new Border
        {
            Width = 150,
            Height = 150,
            CornerRadius = new CornerRadius(75),
            BorderBrush = Brush.Parse(GreenColor),
            BorderThickness = new Thickness(22),
            Child = new TextBlock
            {
                Text = "Totale",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            }
        });

        var legend = new StackPanel { Spacing = 10 };
        legend.Children.Add(new TextBlock { Text = "● Operativi", Foreground = Brush.Parse(GreenColor) });
        legend.Children.Add(new TextBlock { Text = "● In manutenzione", Foreground = Brush.Parse(OrangeColor) });
        legend.Children.Add(new TextBlock { Text = "● Fuori servizio", Foreground = Brush.Parse(RedColor) });
        legend.Children.Add(new TextBlock { Text = "● Non assegnati", Foreground = Brush.Parse("#A1A1AA") });
        row.Children.Add(legend);

        return row;
    }

    private static Control RecentActivity()
    {
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(Activity("⌘", "Intervento completato", "ECG Philips - Sala 1", "10:24"));
        stack.Children.Add(Activity("✓", "Manutenzione programmata", "Ventilatore Dräger - Sala 3", "09:15"));
        stack.Children.Add(Activity("▧", "Nuovo documento caricato", "Manuale uso - Defibrillatore X1", "08:42"));
        stack.Children.Add(Activity("□", "Stock aggiornato", "Elettrodi - Magazzino Centrale", "08:05"));
        return stack;
    }

    private static Control Reminders()
    {
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(Activity("!", "Certificazione ISO 13485", "Scade il 30/06/2025", "45 giorni"));
        stack.Children.Add(Activity("◷", "Manutenzione annuale TAC", "Scade il 25/05/2025", "10 giorni"));
        stack.Children.Add(Activity("▧", "Verifica sicurezza elettrica", "Scade il 20/05/2025", "5 giorni"));
        stack.Children.Add(Activity("▤", "Formazione DPI", "Scade il 31/05/2025", "16 giorni"));
        return stack;
    }

    private static Control Activity(string icon, string title, string subtitle, string time)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("40,*,90") };

        Add(grid, new TextBlock
        {
            Text = icon,
            FontSize = 22,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        }, 0, 0);

        var text = new StackPanel();
        text.Children.Add(new TextBlock { Text = title, FontWeight = FontWeight.SemiBold, Foreground = Brush.Parse(TextColor) });
        text.Children.Add(new TextBlock { Text = subtitle, Foreground = Brush.Parse(MutedTextColor) });
        Add(grid, text, 1, 0);

        Add(grid, new TextBlock
        {
            Text = time,
            Foreground = Brush.Parse(MutedTextColor),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        }, 2, 0);

        return grid;
    }

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
