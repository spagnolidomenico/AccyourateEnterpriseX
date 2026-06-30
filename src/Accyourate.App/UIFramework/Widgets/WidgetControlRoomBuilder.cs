using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.Models;
using Accyourate.App.UIFramework.Components;
using Accyourate.App.UIFramework.Tokens;
using Accyourate.App.UIFramework.Icons;

namespace Accyourate.App.UIFramework.Widgets;

public sealed class WidgetControlRoomBuilder
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;
    private WorkspaceWidgetLayout _layout;

    public WidgetControlRoomBuilder(DatabaseService database, CurrentUser user)
    {
        _database = database;
        _user = user;
        _layout = WorkspaceWidgetStorage.Load(_user.Username);
    }

    public Control Build(Action refresh)
    {
        var page = new StackPanel { Margin = new Thickness(30), Spacing = 22 };

        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("*,220,180") };
        var title = new StackPanel { Spacing = 4 };
        title.Children.Add(new TextBlock
        {
            Text = "Enterprise Control Room",
            FontSize = 36,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        title.Children.Add(new TextBlock
        {
            Text = "Dashboard componibile, personale e pronta per layout per ruolo.",
            FontSize = 15,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });
        Add(header, title, 0, 0);

        var customize = UiComponentFactory.PrimaryButton("Personalizza widget");
        customize.Click += (_, _) =>
        {
            var win = new WidgetLayoutEditorWindow(_layout);
            win.Closed += (_, _) =>
            {
                _layout = WorkspaceWidgetStorage.Load(_user.Username);
                refresh();
            };
            win.Show();
        };
        Add(header, customize, 1, 0);

        var reset = new Button
        {
            Content = "Reset layout",
            Background = UiTokens.Brush(UiTokens.PremiumHover),
            Foreground = UiTokens.Brush(UiTokens.BrandBlue),
            FontWeight = FontWeight.SemiBold,
            Padding = new Thickness(14, 10),
            CornerRadius = new CornerRadius(14)
        };
        reset.Click += (_, _) =>
        {
            _layout = new WorkspaceWidgetLayout { UserName = _user.Username };
            WorkspaceWidgetStorage.Save(_layout);
            refresh();
        };
        Add(header, reset, 2, 0);

        page.Children.Add(header);

        var widgets = Accyourate.App.UIFramework.Components.AdaptiveWidgetGrid.Create();

        foreach (var id in _layout.VisibleWidgetIds)
            widgets.Children.Add(BuildWidget(id));

        page.Children.Add(widgets);

        page.Children.Add(UiComponentFactory.Card(new StackPanel
        {
            Spacing = 8,
            Children =
            {
                new TextBlock { Text = "Roadmap Widget Engine", FontSize = 20, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary) },
                UiComponentFactory.Body("8.4.1: ridimensionamento widget."),
                UiComponentFactory.Body("8.4.2: drag & drop ordinamento."),
                UiComponentFactory.Body("8.4.3: layout per ruolo."),
                UiComponentFactory.Body("8.5: migrazione Document Management nella Workspace.")
            }
        }));

        return new ScrollViewer
        {
            Content = page,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
    }

    private Control BuildWidget(string id)
    {
        return id switch
        {
            "kpi-medical" => Kpi(AxIcons.Medical, "Dispositivi medici", Count("medical_devices"), "Medical Suite", UiTokens.BrandBlue),
            "kpi-documents" => Kpi(AxIcons.Documents, "Documenti", Count("documents"), "Archivio", UiTokens.Warning),
            "kpi-assets" => Kpi(AxIcons.Assets, "Asset IT", Count("assets"), "Inventario", UiTokens.Success),
            "kpi-people" => Kpi(AxIcons.People, "Persone", Count("employees"), "HR", UiTokens.BrandAccent),
            "system-status" => Card("Stato sistemi", SystemStatus()),
            "recent-activity" => Card("Attività recenti", RecentActivity()),
            "deadlines" => Card("Scadenze", Deadlines()),
            "quick-actions" => Card("Accessi rapidi", QuickActions()),
            "medical-lifecycle" => Card("Lifecycle Medical", MedicalLifecycle()),
            "analytics-trend" => Card("Trend operativo", Trend()),
            _ => Card("Widget", UiComponentFactory.Body($"Widget '{id}' non disponibile."))
        };
    }

    private Border Kpi(string icon, string title, string value, string subtitle, string color)
    {
        var row = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 14 };
        row.Children.Add(new Border
        {
            Width = 52,
            Height = 52,
            Background = UiTokens.Brush(color),
            CornerRadius = new CornerRadius(14),
            Child = new TextBlock
            {
                Text = icon,
                Foreground = Brushes.White,
                FontSize = 23,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            }
        });

        var col = new StackPanel();
        col.Children.Add(new TextBlock { Text = title, Foreground = UiTokens.Brush(UiTokens.TextPrimary) });
        col.Children.Add(new TextBlock { Text = value, FontSize = 28, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary) });
        col.Children.Add(new TextBlock { Text = subtitle, Foreground = UiTokens.Brush(UiTokens.TextSecondary) });
        row.Children.Add(col);

        return UiComponentFactory.Card(row);
    }

    private Border Card(string title, Control content)
    {
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        stack.Children.Add(new ScrollViewer
        {
            Content = content,
            MaxHeight = 158,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
        });

        return UiComponentFactory.Card(stack);
    }

    private Control SystemStatus()
    {
        var stack = new StackPanel { Spacing = 8 };
        stack.Children.Add(Line("Database", "Connesso", UiTokens.Success));
        stack.Children.Add(Line("Workspace", "Attiva", UiTokens.Success));
        stack.Children.Add(Line("Widget Engine", "8.4", UiTokens.BrandBlue));
        stack.Children.Add(Line("Backup", "Da verificare", UiTokens.Warning));
        return stack;
    }

    private Control RecentActivity()
    {
        var stack = new StackPanel { Spacing = 8 };
        stack.Children.Add(UiComponentFactory.Body("• Workspace avviata"));
        stack.Children.Add(UiComponentFactory.Body("• Dashboard migrata"));
        stack.Children.Add(UiComponentFactory.Body("• Medical Suite integrata"));
        stack.Children.Add(UiComponentFactory.Body("• Widget Engine attivo"));
        return stack;
    }

    private Control Deadlines()
    {
        var stack = new StackPanel { Spacing = 8 };
        stack.Children.Add(Line("Certificazioni", "30 gg", UiTokens.Warning));
        stack.Children.Add(Line("Manutenzione", "15 gg", UiTokens.BrandBlue));
        stack.Children.Add(Line("Documenti", "5 scadenze", UiTokens.Danger));
        return stack;
    }

    private Control QuickActions()
    {
        var stack = new StackPanel { Spacing = 8 };
        stack.Children.Add(UiComponentFactory.Body("• Apri Medical"));
        stack.Children.Add(UiComponentFactory.Body("• Apri Analytics"));
        stack.Children.Add(UiComponentFactory.Body("• Apri Branding"));
        stack.Children.Add(UiComponentFactory.Body("• Apri Design System"));
        return stack;
    }

    private Control MedicalLifecycle()
    {
        var stack = new StackPanel { Spacing = 8 };
        stack.Children.Add(UiComponentFactory.Body("1. Registrazione dispositivo"));
        stack.Children.Add(UiComponentFactory.Body("2. Produzione e collaudo"));
        stack.Children.Add(UiComponentFactory.Body("3. Qualità e conformità"));
        stack.Children.Add(UiComponentFactory.Body("4. Logistica e assegnazione"));
        stack.Children.Add(UiComponentFactory.Body("5. Digital Twin e storico eventi"));
        return stack;
    }

    private Control Trend()
    {
        var stack = new StackPanel { Spacing = 8 };
        var values = new[] { 18, 24, 21, 38, 44 };
        foreach (var v in values)
        {
            stack.Children.Add(new Border
            {
                Width = v * 5,
                Height = 12,
                Background = UiTokens.Brush(UiTokens.BrandBlue),
                CornerRadius = new CornerRadius(6),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            });
        }
        return stack;
    }

    private Control Line(string left, string right, string color)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,90") };
        Add(grid, new TextBlock { Text = left, Foreground = UiTokens.Brush(UiTokens.TextSecondary) }, 0, 0);
        Add(grid, new TextBlock { Text = right, Foreground = UiTokens.Brush(color), FontWeight = FontWeight.Bold, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right }, 1, 0);
        return grid;
    }

    private string Count(string table)
    {
        try { return _database.CountTable(table).ToString(); }
        catch { return "0"; }
    }

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
