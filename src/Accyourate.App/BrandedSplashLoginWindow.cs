using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Accyourate.App.Data;
using Accyourate.App.Models;
using Accyourate.App.Security;

namespace Accyourate.App;

public sealed class BrandedSplashLoginWindow : Window
{
    private readonly DatabaseService _database;
    private readonly AuthenticationService? _auth;
    private readonly CurrentUser? _previewUser;
    private readonly BrandingPreferenceRecord _branding;

    private readonly TextBox _usernameBox = new();
    private readonly TextBox _passwordBox = new();
    private readonly TextBlock _errorText = new();

    private const string Navy = "#061426";
    private const string Blue = "#2F80FF";
    private const string Cyan = "#38BDF8";

    public event Action<CurrentUser>? LoginSucceeded;

    public BrandedSplashLoginWindow(DatabaseService database, AuthenticationService auth)
    {
        _database = database;
        _auth = auth;
        _branding = _database.GetBrandingPreferences();

        BuildWindow("Accyourate Enterprise X - Login");
    }

    public BrandedSplashLoginWindow(DatabaseService database, CurrentUser user)
    {
        _database = database;
        _previewUser = user;
        _branding = _database.GetBrandingPreferences();

        BuildWindow("Accyourate Enterprise X - Anteprima Splash/Login");
    }

    private void BuildWindow(string title)
    {
        Title = title;
        Width = 1180;
        Height = 760;
        MinWidth = 980;
        MinHeight = 680;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = Brush.Parse(Navy);
        Content = BuildLayout();
    }

    private Control BuildLayout()
    {
        var root = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,380")
        };

        Add(root, BuildHero(), 0, 0);
        Add(root, BuildLoginPanel(), 1, 0);

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
                Stretch = Avalonia.Media.Stretch.UniformToFill,
                Opacity = 0.58
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
                    new GradientStop(Color.Parse("#C0061426"), 0.45),
                    new GradientStop(Color.Parse("#700A4C9A"), 1)
                }
            }
        });

        var content = new StackPanel
        {
            Margin = new Thickness(44, 38, 34, 30),
            Spacing = 24
        };

        content.Children.Add(new TextBlock
        {
            Text = Safe(_branding.ProductTitle, "Accyourate Enterprise X"),
            Foreground = Brushes.White,
            FontSize = 18,
            FontWeight = FontWeight.Bold
        });

        content.Children.Add(new TextBlock
        {
            Text = Safe(_branding.HeroTitle, "Accyourate Enterprise X"),
            Foreground = Brushes.White,
            FontSize = 44,
            FontWeight = FontWeight.Bold,
            TextWrapping = TextWrapping.Wrap
        });

        content.Children.Add(new Border
        {
            Width = 48,
            Height = 3,
            Background = Brush.Parse(Blue),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            CornerRadius = new CornerRadius(3)
        });

        content.Children.Add(new TextBlock
        {
            Text = Safe(_branding.HeroSubtitle, "La piattaforma integrata per aziende che guardano avanti."),
            Foreground = Brush.Parse("#F3F4F6"),
            FontSize = 20,
            FontWeight = FontWeight.SemiBold,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 620
        });

        var features = new StackPanel { Spacing = 18, Margin = new Thickness(0, 18, 0, 0) };
        features.Children.Add(Feature("▧", Safe(_branding.Feature1Title, "Gestione completa"), Safe(_branding.Feature1Text, "Moduli integrati per ogni area aziendale")));
        features.Children.Add(Feature("◇", Safe(_branding.Feature2Title, "Sicurezza e conformità"), Safe(_branding.Feature2Text, "Protezione dei dati e conformità normativa")));
        features.Children.Add(Feature("▥", Safe(_branding.Feature3Title, "Analytics avanzata"), Safe(_branding.Feature3Text, "Dati, KPI e report per decisioni migliori")));
        features.Children.Add(Feature("⌘", Safe(_branding.Feature4Title, "Innovazione continua"), Safe(_branding.Feature4Text, "Tecnologia all'avanguardia per il tuo business")));
        content.Children.Add(features);

        content.Children.Add(new TextBlock
        {
            Text = "“L’innovazione è il ponte tra oggi e il futuro della tua azienda.”",
            Foreground = Brush.Parse("#E5E7EB"),
            FontStyle = FontStyle.Italic,
            FontSize = 20,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 560,
            Margin = new Thickness(0, 24, 0, 0)
        });

        content.Children.Add(BuildModuleStrip());

        grid.Children.Add(content);
        return grid;
    }

    private Control BuildLoginPanel()
    {
        var outer = new Border
        {
            Background = Brush.Parse("#F8FAFC"),
            Padding = new Thickness(24)
        };

        var stack = new StackPanel
        {
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Spacing = 18
        };

        stack.Children.Add(BuildCompanyLogo());

        stack.Children.Add(new TextBlock
        {
            Text = Safe(_branding.CompanyName, "Accyourate Group"),
            FontSize = 22,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#111827"),
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        });

        stack.Children.Add(new TextBlock
        {
            Text = Safe(_branding.ProductTitle, "Accyourate Enterprise X"),
            FontSize = 15,
            FontWeight = FontWeight.SemiBold,
            Foreground = Brush.Parse("#64748B"),
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        });

        stack.Children.Add(new TextBlock
        {
            Text = "Accedi al tuo account",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#111827"),
            Margin = new Thickness(0, 14, 0, 0)
        });

        _usernameBox.Text = "admin";
        _usernameBox.Watermark = "Nome utente";
        stack.Children.Add(InputBox("👤 Nome utente", _usernameBox));

        _passwordBox.Text = "admin123";
        _passwordBox.PasswordChar = '●';
        _passwordBox.Watermark = "Password";
        stack.Children.Add(InputBox("🔒 Password", _passwordBox));

        _errorText.Foreground = Brush.Parse("#DC2626");
        _errorText.MinHeight = 24;
        _errorText.TextWrapping = TextWrapping.Wrap;
        stack.Children.Add(_errorText);

        var login = new Button
        {
            Content = _auth is null ? "Anteprima login" : "→  Accedi",
            Background = Brush.Parse(Blue),
            Foreground = Brushes.White,
            FontSize = 17,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(14, 10),
            CornerRadius = new CornerRadius(12)
        };
        login.Click += (_, _) => Login();
        stack.Children.Add(login);

        stack.Children.Add(new TextBlock
        {
            Text = _auth is null
                ? $"Anteprima branding per {_previewUser?.Username ?? "utente"}"
                : "Credenziali iniziali: admin / admin123",
            Foreground = Brush.Parse("#64748B"),
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        });

        stack.Children.Add(new TextBlock
        {
            Text = $"© 2026 {Safe(_branding.CompanyName, "Accyourate Group")}\n{Safe(_branding.IndustryLabel, "Enterprise Suite")}",
            Foreground = Brush.Parse("#6B7280"),
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Thickness(0, 24, 0, 0)
        });

        outer.Child = stack;
        return outer;
    }

    private void Login()
    {
        if (_auth is null)
        {
            _errorText.Text = "Anteprima branding: il login reale è attivo solo all'avvio dell'app.";
            return;
        }

        var username = _usernameBox.Text?.Trim() ?? string.Empty;
        var password = _passwordBox.Text ?? string.Empty;

        var user = _auth.Login(username, password);

        if (user is not null)
        {
            LoginSucceeded?.Invoke(user);
            return;
        }

        _errorText.Text = "Username o password non validi.";
    }


    private Control BuildCompanyLogo()
    {
        var logo = TryLoadCompanyLogo();

        if (logo is not null)
        {
            return new Border
            {
                Width = 260,
                Height = 118,
                Background = Brushes.White,
                BorderBrush = Brush.Parse("#E2E8F0"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(22),
                Padding = new Thickness(16),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Child = new Image
                {
                    Source = logo,
                    Stretch = Avalonia.Media.Stretch.Uniform,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                }
            };
        }

        var initials = Safe(_branding.CompanyName, "Accyourate Group")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2)
            .Select(x => x[0].ToString().ToUpperInvariant());

        return new Border
        {
            Width = 260,
            Height = 118,
            Background = Brush.Parse(Blue),
            CornerRadius = new CornerRadius(22),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Child = new TextBlock
            {
                Text = string.Join("", initials),
                Foreground = Brushes.White,
                FontSize = 38,
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            }
        };
    }

    private Bitmap? TryLoadCompanyLogo()
    {
        var possiblePaths = new[]
        {
            _branding.LogoPath,
            Path.Combine(AppContext.BaseDirectory, "Assets", "Branding", "company_logo.png"),
            Path.Combine(AppContext.BaseDirectory, "Assets", "Branding", "logo.png")
        };

        foreach (var path in possiblePaths)
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                return new Bitmap(path);
        }

        return null;
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
            Margin = new Thickness(0, 18, 0, 0),
            ItemWidth = 112,
            ItemHeight = 54
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
            Child = strip
        };
    }

    private static Control Module(string title, string subtitle)
    {
        var stack = new StackPanel { Spacing = 2 };
        stack.Children.Add(new TextBlock { Text = title, Foreground = Brushes.White, FontWeight = FontWeight.Bold });
        stack.Children.Add(new TextBlock { Text = subtitle, Foreground = Brush.Parse("#CBD5E1"), FontSize = 11 });
        return stack;
    }

    private static Border InputBox(string label, TextBox textBox)
    {
        var stack = new StackPanel { Spacing = 6 };
        stack.Children.Add(new TextBlock
        {
            Text = label,
            Foreground = Brush.Parse("#334155"),
            FontWeight = FontWeight.SemiBold
        });
        stack.Children.Add(textBox);

        return new Border
        {
            BorderBrush = Brush.Parse("#CBD5E1"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(12),
            Background = Brushes.White,
            Child = stack
        };
    }

    private static string Safe(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
