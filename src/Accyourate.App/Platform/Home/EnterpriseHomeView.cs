using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Models;
using Accyourate.App.UIFramework.DesignSystem;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.Platform.Home;

public sealed class EnterpriseHomeView : UserControl
{
    private readonly CurrentUser _user;
    private readonly Action<string, string>? _navigate;
    private readonly EnterpriseHomeService _service = new();

    public EnterpriseHomeView(CurrentUser user, Action<string, string>? navigate = null)
    {
        _user = user;
        _navigate = navigate;
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();
    }

    private Control BuildLayout()
    {
        var snapshot = _service.Load();

        var page = new StackPanel
        {
            Margin = new Thickness(24),
            Spacing = 18
        };

        page.Children.Add(Header(snapshot));
        page.Children.Add(KpiCards(snapshot));

        var center = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("370,18,*"),
            
        };

        Add(center, QuickActions(), 0, 0);
        Add(center, RecentActivity(), 2, 0);
        page.Children.Add(center);

        var lower = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,18,*"),
            
        };

        Add(lower, SystemStatus(snapshot), 0, 0);
        Add(lower, AiPanel(snapshot), 2, 0);
        page.Children.Add(lower);

        return new ScrollViewer
        {
            Content = page,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
    }

    private Control Header(EnterpriseHomeSnapshot snapshot)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,420")
        };

        var left = new StackPanel { Spacing = 8 };

        left.Children.Add(new TextBlock
        {
            Text = $"Buongiorno {_user.Username}",
            FontSize = 36,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        left.Children.Add(new TextBlock
        {
            Text = "Questa è la tua centrale operativa: KPI, azioni rapide, attività recenti e stato sistema.",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap
        });

        var right = new StackPanel
        {
            Spacing = 8,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        var search = new TextBox
        {
            Watermark = "Cerca in Accyourate...",
            FontSize = 16,
            Padding = new Thickness(14, 10)
        };

        search.KeyDown += (_, e) =>
        {
            if (e.Key == Avalonia.Input.Key.Enter)
                Navigate("enterprise-search", "Ricerca Enterprise");
        };

        right.Children.Add(search);
        right.Children.Add(new TextBlock
        {
            Text = $"Versione {snapshot.Version} · {_user.Role}",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });

        Add(grid, left, 0, 0);
        Add(grid, right, 1, 0);

        return AxCard.Create(grid);
    }

    private Control KpiCards(EnterpriseHomeSnapshot snapshot)
    {
        var wrap = new WrapPanel
        {
            ItemWidth = 255,
            ItemHeight = 148
        };

        wrap.Children.Add(Kpi("👥", "Dipendenti", snapshot.Employees.ToString(), "Apri Human Resources", "human-resources", "Human Resources"));
        wrap.Children.Add(Kpi("💻", "Asset IT", snapshot.Assets.ToString(), "Apri Asset Management", "asset-management", "Asset Management"));
        wrap.Children.Add(Kpi("📄", "Documenti", snapshot.Documents.ToString(), "Apri Centro Documenti", "document-center", "Centro Documenti"));
        wrap.Children.Add(Kpi("📦", "Verbali", snapshot.DeliveryReports.ToString(), "Apri Verbali consegna", "delivery-reports", "Verbali consegna"));

        return wrap;
    }

    private Control Kpi(string icon, string title, string value, string subtitle, string moduleId, string moduleTitle)
    {
        var stack = new StackPanel { Spacing = 6 };

        stack.Children.Add(new TextBlock { Text = icon, FontSize = 28 });
        stack.Children.Add(new TextBlock
        {
            Text = value,
            FontSize = 30,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        stack.Children.Add(new TextBlock
        {
            Text = subtitle,
            FontSize = 12,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });

        var button = new Button
        {
            Content = AxCard.Create(stack),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            Margin = new Thickness(0, 0, 12, 12)
        };

        button.Click += (_, _) => Navigate(moduleId, moduleTitle);
        return button;
    }

    private Control QuickActions()
    {
        var stack = new StackPanel { Spacing = 10 };

        stack.Children.Add(SectionTitle("Azioni rapide"));
        stack.Children.Add(ActionButton("➕", "Nuovo dipendente", "Apri Human Resources", "human-resources", "Human Resources"));
        stack.Children.Add(ActionButton("💻", "Nuovo asset", "Apri Asset Management", "asset-management", "Asset Management"));
        stack.Children.Add(ActionButton("📄", "Nuovo documento", "Apri Centro Documenti", "document-center", "Centro Documenti"));
        stack.Children.Add(ActionButton("📦", "Nuova consegna", "Apri Verbali consegna", "delivery-reports", "Verbali consegna"));
        stack.Children.Add(ActionButton("🔎", "Ricerca globale", "Cerca in tutta la piattaforma", "enterprise-search", "Ricerca Enterprise"));

        return AxSection.Create("Centro operativo", stack);
    }

    private Control ActionButton(string icon, string title, string subtitle, string moduleId, string moduleTitle)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("42,*")
        };

        Add(grid, new TextBlock
        {
            Text = icon,
            FontSize = 24,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        }, 0, 0);

        var text = new StackPanel { Spacing = 2 };
        text.Children.Add(new TextBlock
        {
            Text = title,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        text.Children.Add(new TextBlock
        {
            Text = subtitle,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            FontSize = 12
        });

        Add(grid, text, 1, 0);

        var button = new Button
        {
            Content = grid,
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            Padding = new Thickness(12),
            CornerRadius = new CornerRadius(14),
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };

        button.Click += (_, _) => Navigate(moduleId, moduleTitle);
        return button;
    }

    private Control RecentActivity()
    {
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(SectionTitle("Attività recenti"));

        foreach (var activity in _service.RecentActivities())
        {
            stack.Children.Add(new Border
            {
                Background = UiTokens.Brush(UiTokens.SurfaceAlt),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(12),
                Child = new TextBlock
                {
                    Text = activity,
                    Foreground = UiTokens.Brush(UiTokens.TextPrimary),
                    TextWrapping = TextWrapping.Wrap
                }
            });
        }

        return AxSection.Create("Timeline operativa", stack);
    }

    private Control SystemStatus(EnterpriseHomeSnapshot snapshot)
    {
        var stack = new StackPanel { Spacing = 10 };

        stack.Children.Add(SectionTitle("Stato sistema"));
        stack.Children.Add(StatusLine("Database", snapshot.DatabaseStatus));
        stack.Children.Add(StatusLine("Ultimo backup", snapshot.LastBackup));
        stack.Children.Add(StatusLine("Backup registrati", snapshot.BackupCount.ToString()));
        stack.Children.Add(StatusLine("Notifiche non lette", snapshot.UnreadNotifications.ToString()));
        stack.Children.Add(StatusLine("Aggiornamenti", snapshot.UpdateStatus));

        return AxSection.Create("Sistema", stack);
    }

    private Control AiPanel(EnterpriseHomeSnapshot snapshot)
    {
        var stack = new StackPanel { Spacing = 10 };

        stack.Children.Add(SectionTitle("Accyourate AI"));
        stack.Children.Add(new TextBlock
        {
            Text = $"Oggi hai {snapshot.Assets} asset censiti, {snapshot.Documents} documenti archiviati e {snapshot.UnreadNotifications} notifiche non lette.",
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            TextWrapping = TextWrapping.Wrap
        });
        stack.Children.Add(new TextBlock
        {
            Text = "In futuro questo pannello suggerirà attività, anomalie e workflow da avviare.",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap
        });
        stack.Children.Add(AxButton.Create("Apri AI Assistant", () => Navigate("ai-assistant", "AI Assistant"), AxButtonKind.Primary));

        return AxSection.Create("Assistente operativo", stack);
    }

    private static TextBlock SectionTitle(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        };
    }

    private static Control StatusLine(string label, string value)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto")
        };

        Add(grid, new TextBlock
        {
            Text = label,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        }, 0, 0);

        Add(grid, new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(value) ? "—" : value,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        }, 1, 0);

        return new Border
        {
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(12),
            Child = grid
        };
    }

    private void Navigate(string moduleId, string title)
    {
        _navigate?.Invoke(moduleId, title);
    }

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
