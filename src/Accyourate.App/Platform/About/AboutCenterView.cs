using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Platform.Settings;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.Platform.About;

public sealed class AboutCenterView : UserControl
{
    private readonly AboutService _aboutService = new();
    private readonly SettingsService _settingsService = new();

    public AboutCenterView()
    {
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();
    }

    private Control BuildLayout()
    {
        var info = _aboutService.GetSystemInfo();
        var settings = _settingsService.Load();
        var modules = _aboutService.GetModules();

        var root = new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Content = BuildContent(info, settings, modules)
        };

        return root;
    }

    private Control BuildContent(AboutSystemInfo info, ApplicationSettings settings, IReadOnlyList<AboutModuleInfo> modules)
    {
        var stack = new StackPanel
        {
            Margin = new Thickness(24),
            Spacing = 16
        };

        stack.Children.Add(Header(info));
        stack.Children.Add(Section("Applicazione", new[]
        {
            Info("Prodotto", info.ProductName),
            Info("Versione", info.Version),
            Info("Build", info.Build),
            Info("Licenza", info.LicenseEdition),
            Info("Framework", info.Framework),
            Info("Sistema operativo", info.OperatingSystem),
            Info("Architettura", info.Architecture)
        }));

        stack.Children.Add(Section("Azienda", new[]
        {
            Info("Nome azienda", settings.Company.CompanyName),
            Info("Ragione sociale", settings.Company.LegalName),
            Info("Partita IVA", settings.Company.VatNumber),
            Info("Codice fiscale", settings.Company.FiscalCode),
            Info("Indirizzo", $"{settings.Company.Address} {settings.Company.City} {settings.Company.Province}".Trim()),
            Info("Email", settings.Company.Email),
            Info("Telefono", settings.Company.Phone),
            Info("Sito web", settings.Company.Website),
            Info("Logo", settings.Company.LogoPath)
        }));

        stack.Children.Add(Section("Sistema e percorsi", new[]
        {
            Info("Cartella dati", info.AppDataFolder),
            Info("Cartella documenti", info.DocumentFolder),
            Info("Database", info.DatabaseSummary)
        }));

        stack.Children.Add(ModuleSection(modules));

        stack.Children.Add(Section("Crediti e roadmap", new[]
        {
            Info("Copyright", $"© {DateTime.Now:yyyy} Accyourate Group"),
            Info("Canale", settings.VersionChannel),
            Info("Prossimi sprint", "UPDATE-001, INSTALLER-001, RC1, Beta 0.9"),
            Info("Nota", "Sistema About predisposto per licenza, changelog e update center.")
        }));

        return stack;
    }

    private static Control Header(AboutSystemInfo info)
    {
        var stack = new StackPanel { Spacing = 8 };

        stack.Children.Add(new TextBlock
        {
            Text = "Accyourate Enterprise X",
            FontSize = 36,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        stack.Children.Add(new TextBlock
        {
            Text = $"Versione {info.Version} · {info.LicenseEdition}",
            FontSize = 16,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });

        stack.Children.Add(new TextBlock
        {
            Text = "Piattaforma enterprise modulare per HR, Asset, Documenti, Ricerca e Dashboard.",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap
        });

        return Card(stack);
    }

    private static Control ModuleSection(IReadOnlyList<AboutModuleInfo> modules)
    {
        var stack = new StackPanel { Spacing = 10 };

        stack.Children.Add(new TextBlock
        {
            Text = "Moduli installati",
            FontSize = 22,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        foreach (var module in modules)
        {
            stack.Children.Add(Info(module.Name, $"{module.Status} · {module.Version} · {module.Description}"));
        }

        return Card(stack);
    }

    private static Control Section(string title, IEnumerable<Control> controls)
    {
        var wrap = new WrapPanel
        {
            ItemWidth = 350,
            ItemHeight = 96
        };

        foreach (var control in controls)
            wrap.Children.Add(control);

        var stack = new StackPanel { Spacing = 12 };
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 22,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        stack.Children.Add(wrap);

        return Card(stack);
    }

    private static Control Info(string label, string value)
    {
        return new Border
        {
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(12),
            Margin = new Thickness(0, 0, 12, 12),
            Child = new StackPanel
            {
                Spacing = 4,
                Children =
                {
                    new TextBlock
                    {
                        Text = label,
                        FontSize = 12,
                        FontWeight = FontWeight.SemiBold,
                        Foreground = UiTokens.Brush(UiTokens.TextSecondary)
                    },
                    new TextBlock
                    {
                        Text = string.IsNullOrWhiteSpace(value) ? "—" : value,
                        FontWeight = FontWeight.Bold,
                        Foreground = UiTokens.Brush(UiTokens.TextPrimary),
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
        };
    }

    private static Border Card(Control child)
    {
        return new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(22),
            Padding = new Thickness(18),
            Child = child
        };
    }
}
