using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Accyourate.App.Data;
using Accyourate.App.Models;

namespace Accyourate.App;

public sealed class BrandedHomeWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;
    private readonly BrandingPreferenceRecord _branding;

    private const string Navy = "#061426";
    private const string Blue = "#2F80FF";
    private const string Cyan = "#38BDF8";

    public BrandedHomeWindow(DatabaseService database, CurrentUser user)
    {
        _database = database;
        _user = user;
        _branding = _database.GetBrandingPreferences();

        Title = "Accyourate Enterprise X - Home";
        Width = 1420;
        Height = 900;
        MinWidth = 1180;
        MinHeight = 760;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse(Navy);
        Content = BuildLayout();
    }

    private Control BuildLayout()
    {
        var root = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,420")
        };

        Add(root, BuildHero(), 0, 0);
        Add(root, BuildActionPanel(), 1, 0);

        return root;
    }

    private Control BuildHero()
    {
        var grid = new Grid();
        grid.Children.Add(new Border { Background = Brush.Parse(Navy) });

        var image = TryLoadHeroImage();
        if (image is not null)
        {
            grid.Children.Add(new Image
            {
                Source = image,
                Stretch = Stretch.UniformToFill,
                Opacity = 0.60
            });
        }

        grid.Children.Add(new Border
        {
            Background = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(Color.Parse("#F0051020"), 0),
                    new GradientStop(Color.Parse("#C0061426"), 0.50),
                    new GradientStop(Color.Parse("#700A4C9A"), 1)
                }
            }
        });

        var content = new StackPanel
        {
            Margin = new Thickness(64, 58, 48, 42),
            Spacing = 24
        };

        content.Children.Add(new TextBlock
        {
            Text = _branding.ProductTitle,
            Foreground = Brushes.White,
            FontSize = 18,
            FontWeight = FontWeight.Bold
        });

        content.Children.Add(new TextBlock
        {
            Text = _branding.HeroTitle,
            Foreground = Brushes.White,
            FontSize = 58,
            FontWeight = FontWeight.Bold,
            TextWrapping = TextWrapping.Wrap
        });

        content.Children.Add(new Border
        {
            Width = 58,
            Height = 3,
            Background = Brush.Parse(Blue),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            CornerRadius = new CornerRadius(3)
        });

        content.Children.Add(new TextBlock
        {
            Text = _branding.HeroSubtitle,
            Foreground = Brush.Parse("#F3F4F6"),
            FontSize = 24,
            FontWeight = FontWeight.SemiBold,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 640
        });

        var features = new StackPanel { Spacing = 18, Margin = new Thickness(0, 18, 0, 0) };
        features.Children.Add(Feature("▧", _branding.Feature1Title, _branding.Feature1Text));
        features.Children.Add(Feature("◇", _branding.Feature2Title, _branding.Feature2Text));
        features.Children.Add(Feature("▥", _branding.Feature3Title, _branding.Feature3Text));
        features.Children.Add(Feature("⌘", _branding.Feature4Title, _branding.Feature4Text));
        content.Children.Add(features);

        content.Children.Add(BuildModuleStrip());
        content.Children.Add(BuildStatusBar());

        grid.Children.Add(content);
        return grid;
    }

    private Control BuildActionPanel()
    {
        var outer = new Border
        {
            Background = Brush.Parse("#F8FAFC"),
            Padding = new Thickness(30)
        };

        var stack = new StackPanel
        {
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Spacing = 18
        };

        stack.Children.Add(new TextBlock
        {
            Text = "X",
            FontSize = 76,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse(Blue),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        });

        stack.Children.Add(new TextBlock
        {
            Text = _branding.ProductTitle,
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#111827"),
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        });

        stack.Children.Add(new TextBlock
        {
            Text = $"Benvenuto, {_user.Username}",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#111827"),
            Margin = new Thickness(0, 18, 0, 0)
        });

        var dashboard = PrimaryButton("→ Entra nel gestionale");
        dashboard.Click += (_, _) => Close();
        stack.Children.Add(dashboard);

        var apple = SecondaryButton("🍎 Apri Apple Style Dashboard");
        apple.Click += (_, _) => new AppleStyleDashboardWindow(_database, _user).Show();
        stack.Children.Add(apple);

        var analytics = SecondaryButton("📈 Apri Analytics Dashboard");
        analytics.Click += (_, _) => new AnalyticsDashboardWindow(_database, _user).Show();
        stack.Children.Add(analytics);

        var branding = SecondaryButton("🏷 Personalizza branding");
        branding.Click += (_, _) => new BrandingCenterWindow(_database, _user).Show();
        stack.Children.Add(branding);

        stack.Children.Add(new Separator { Margin = new Thickness(0, 12) });

        stack.Children.Add(KpiMini("Dispositivi medici", _database.CountTable("medical_devices").ToString()));
        stack.Children.Add(KpiMini("Documenti", _database.CountTable("documents").ToString()));
        stack.Children.Add(KpiMini("Asset IT", _database.CountTable("assets").ToString()));

        stack.Children.Add(new TextBlock
        {
            Text = $"© 2026 {_branding.CompanyName}\n{_branding.IndustryLabel}",
            Foreground = Brush.Parse("#6B7280"),
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Thickness(0, 24, 0, 0)
        });

        outer.Child = stack;
        return outer;
    }

    private Bitmap? TryLoadHeroImage()
    {
        var path = _branding.HeroImagePath;
        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            return new Bitmap(path);

        var bundled = Path.Combine(AppContext.BaseDirectory, "Assets", "Branding", "default_splash_hero.png");
        if (File.Exists(bundled))
            return new Bitmap(bundled);

        return null;
    }

    private static Control Feature(string icon, string title, string text)
    {
        var row = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 14
        };

        row.Children.Add(new Border
        {
            Width = 44,
            Height = 44,
            Background = Brush.Parse("#172B4D"),
            CornerRadius = new CornerRadius(10),
            Child = new TextBlock
            {
                Text = icon,
                Foreground = Brush.Parse(Cyan),
                FontSize = 23,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            }
        });

        var col = new StackPanel();
        col.Children.Add(new TextBlock { Text = title, Foreground = Brushes.White, FontWeight = FontWeight.Bold, FontSize = 16 });
        col.Children.Add(new TextBlock { Text = text, Foreground = Brush.Parse("#CBD5E1"), FontSize = 13, TextWrapping = TextWrapping.Wrap });
        row.Children.Add(col);

        return row;
    }

    private static Control BuildModuleStrip()
    {
        var strip = new WrapPanel
        {
            ItemWidth = 128,
            ItemHeight = 64
        };

        strip.Children.Add(Module("ERP", "Gestione risorse"));
        strip.Children.Add(Module("CRM", "Relazioni clienti"));
        strip.Children.Add(Module("HR", "Risorse umane"));
        strip.Children.Add(Module("DOC", "Documenti"));
        strip.Children.Add(Module("BI", "Business Intelligence"));
        strip.Children.Add(Module("MED", "Medical Suite"));

        return new Border
        {
            Background = Brush.Parse("#AA0B1728"),
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(14),
            Margin = new Thickness(0, 22, 0, 0),
            Child = strip
        };
    }

    private static Control BuildStatusBar()
    {
        var row = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 22,
            Margin = new Thickness(0, 10, 0, 0)
        };

        row.Children.Add(Status("●", "Sistema operativo", "#22C55E"));
        row.Children.Add(Status("🔒", "Connessione sicura", "#CBD5E1"));
        row.Children.Add(Status("v7.2.2", "Branded Home", "#CBD5E1"));

        return row;
    }

    private static Control Module(string title, string subtitle)
    {
        var stack = new StackPanel { Spacing = 2 };
        stack.Children.Add(new TextBlock { Text = title, Foreground = Brushes.White, FontWeight = FontWeight.Bold });
        stack.Children.Add(new TextBlock { Text = subtitle, Foreground = Brush.Parse("#CBD5E1"), FontSize = 11 });
        return stack;
    }

    private static Control Status(string icon, string text, string color)
    {
        return new TextBlock
        {
            Text = $"{icon} {text}",
            Foreground = Brush.Parse(color),
            FontSize = 13
        };
    }

    private static Button PrimaryButton(string text)
    {
        return new Button
        {
            Content = text,
            Background = Brush.Parse(Blue),
            Foreground = Brushes.White,
            FontSize = 17,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(16, 12),
            CornerRadius = new CornerRadius(12)
        };
    }

    private static Button SecondaryButton(string text)
    {
        return new Button
        {
            Content = text,
            Background = Brushes.White,
            Foreground = Brush.Parse("#1F2937"),
            BorderBrush = Brush.Parse("#CBD5E1"),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(14, 10),
            CornerRadius = new CornerRadius(10)
        };
    }

    private static Control KpiMini(string title, string value)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,80") };
        Add(grid, new TextBlock { Text = title, Foreground = Brush.Parse("#475569") }, 0, 0);
        Add(grid, new TextBlock
        {
            Text = value,
            Foreground = Brush.Parse("#111827"),
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
        }, 1, 0);

        return new Border
        {
            Background = Brush.Parse("#F1F5F9"),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(12),
            Child = grid
        };
    }

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
