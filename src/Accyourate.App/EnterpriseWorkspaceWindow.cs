using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.Models;
using Accyourate.App.UIFramework.Shell;
using Accyourate.App.UIFramework.Tokens;
using Accyourate.App.UIFramework.Icons;
using Accyourate.App.UIFramework.WorkspaceTabs;

namespace Accyourate.App;

public sealed class EnterpriseWorkspaceWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;
    private readonly WorkspaceModuleFactory _moduleFactory;
    private readonly NavigationState _navigation = new();

    private readonly ContentControl _content = new();
    private readonly TextBlock _breadcrumb = new();
    private readonly TextBlock _status = new();
    private UiThemeMode _themeMode = UiThemeMode.Light;
    private readonly WorkspaceTabManager _dashboardTabManager = new();
    private WorkspaceHost? _dashboardTabHost;

    public EnterpriseWorkspaceWindow(DatabaseService database, CurrentUser user)
    {
        _database = database;
        _user = user;
        _moduleFactory = new WorkspaceModuleFactory(_database, _user);

        Title = "Accyourate Enterprise X 11.0.2 - Dashboard Tab";
        Width = 1480;
        Height = 920;
        MinWidth = 1180;
        MinHeight = 760;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();

        Navigate("workspace-home", "Workspace Home");
    }

    private Control BuildLayout()
    {
        var root = new DockPanel();

        var top = BuildTopBar();
        DockPanel.SetDock(top, Dock.Top);
        root.Children.Add(top);

        var status = BuildStatusBar();
        DockPanel.SetDock(status, Dock.Bottom);
        root.Children.Add(status);

        var main = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("300,*")
        };

        Add(main, BuildSidebar(), 0, 0);
        Add(main, BuildContentArea(), 1, 0);

        root.Children.Add(main);
        return root;
    }

    private Control BuildTopBar()
    {
        var grid = new Grid
        {
            Height = 76,
            Background = UiTokens.Brush(UiTokens.Surface),
            ColumnDefinitions = new ColumnDefinitions("340,*,120,120,130,120,130,170")
        };

        Add(grid, new TextBlock
        {
            Text = "Accyourate Enterprise X",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(24, 0)
        }, 0, 0);

        Add(grid, SearchBox(), 1, 0);
        Add(grid, TopButton(AxIcons.Command + "K Comandi", OpenCommandPalette), 2, 0);
        Add(grid, TopButton("⌕ Command", () => new UniversalCommandBarWindow(_database, _user, Navigate).Show()), 3, 0);
        Add(grid, TopButton("AI", () => new EnterpriseAiAssistantWindow(_database, _user).Show()), 4, 0);
        Add(grid, TopButton(AxIcons.Notifications + " Notifiche", () => new NotificationsWindow().Show()), 5, 0);
        Add(grid, TopButton("◐ Tema", ToggleTheme), 6, 0);
        Add(grid, new TextBlock
        {
            Text = $"👤 {_user.Username}",
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 0, 20, 0)
        }, 7, 0);

        return new Border
        {
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Avalonia.Thickness(0, 0, 0, 1),
            Child = grid
        };
    }

    private Control BuildSidebar()
    {
        var dock = new DockPanel
        {
            Background = UiTokens.Brush(UiTokens.PremiumSurfaceGlass)
        };

        var header = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 5 };
        header.Children.Add(new TextBlock { Text = "Workspace", FontSize = 24, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary) });
        header.Children.Add(new TextBlock { Text = "Moduli e aree", Foreground = UiTokens.Brush(UiTokens.TextSecondary) });
        DockPanel.SetDock(header, Dock.Top);
        dock.Children.Add(header);

        var menu = new StackPanel { Margin = new Avalonia.Thickness(14), Spacing = 5 };

        AddMenu(menu, AxIcons.Home, "Home", "workspace-home", "Workspace Home");
        AddMenu(menu, AxIcons.ControlRoom, "Control Room", "control-room", "Enterprise Control Room");
        menu.Children.Add(ExternalButton("AI  Enterprise AI Assistant", () => new EnterpriseAiAssistantWindow(_database, _user).Show()));
        menu.Children.Add(ExternalButton("AI  AI Intent Catalog", () => new AiIntentCatalogManagerWindow().Show()));
        menu.Children.Add(ExternalButton("AX  Action Engine", () => new ActionEngineWindow(_database, _user).Show()));
        menu.Children.Add(ExternalButton("⌕  Universal Command Bar", () => new UniversalCommandBarWindow(_database, _user, Navigate).Show()));
        AddMenu(menu, AxIcons.Dashboard, "Dashboard", "dashboard", "Dashboard");
        AddMenu(menu, AxIcons.Analytics, "Analytics", "analytics", "Analytics");
        AddMenu(menu, AxIcons.Medical, "Medical", "medical", "Medical Device Suite");
        AddMenu(menu, "DT", "Digital Twin", "digital-twin", "Digital Twin Platform");
        AddMenu(menu, AxIcons.Branding, "Branding", "branding", "Branding Center");
        AddMenu(menu, AxIcons.Design, "Design System", "design-system", "Design System");
        AddMenu(menu, AxIcons.Architecture, "Architecture", "architecture", "Enterprise Architecture");

        menu.Children.Add(new Separator { Margin = new Avalonia.Thickness(8, 14) });
        menu.Children.Add(ExternalButton("🍎 Apple Style Dashboard", () => new AppleStyleDashboardWindow(_database, _user).Show()));
        menu.Children.Add(ExternalButton("🖼 Branded Home", () => new BrandedHomeWindow(_database, _user).Show()));
        menu.Children.Add(ExternalButton("📁 Document Management", () => new DocumentManagementWindow(_database, _user).Show()));
        menu.Children.Add(ExternalButton("⌁ Medical Workspace", () =>
        {
            Navigate("medical", "Medical Device Suite");
        }));

        dock.Children.Add(new ScrollViewer { Content = menu, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto });

        return new Border
        {
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Avalonia.Thickness(0, 0, 1, 0),
            Child = dock
        };
    }

    private Control BuildContentArea()
    {
        var dock = new DockPanel();

        var header = new Grid
        {
            Height = 58,
            Background = UiTokens.Brush(UiTokens.Background),
            ColumnDefinitions = new ColumnDefinitions("*,160")
        };

        _breadcrumb.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        _breadcrumb.Margin = new Avalonia.Thickness(22, 0);
        _breadcrumb.FontWeight = FontWeight.Bold;
        _breadcrumb.Foreground = UiTokens.Brush(UiTokens.TextPrimary);
        Add(header, _breadcrumb, 0, 0);

        Add(header, TopButton("Aggiorna", () => Navigate(_navigation.CurrentModuleId, _navigation.CurrentTitle)), 1, 0);

        DockPanel.SetDock(header, Dock.Top);
        dock.Children.Add(header);

        dock.Children.Add(_content);
        return dock;
    }

    private Control BuildStatusBar()
    {
        var grid = new Grid
        {
            Height = 34,
            Background = UiTokens.Brush(UiTokens.Surface),
            ColumnDefinitions = new ColumnDefinitions("*,180,180,180")
        };

        _status.Margin = new Avalonia.Thickness(14, 0);
        _status.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        _status.Foreground = UiTokens.Brush(UiTokens.TextSecondary);
        Add(grid, _status, 0, 0);

        Add(grid, StatusText("DB: SQLite"), 1, 0);
        Add(grid, StatusText("Versione: 11.0.2"), 2, 0);
        Add(grid, StatusText($"Utente: {_user.Username}"), 3, 0);

        return new Border
        {
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Avalonia.Thickness(0, 1, 0, 0),
            Child = grid
        };
    }

    private void Navigate(string moduleId, string title)
    {
        if (moduleId == "ai-assistant")
        {
            new EnterpriseAiAssistantWindow(_database, _user).Show();
            return;
        }

        if (moduleId == "ai-catalog")
        {
            new AiIntentCatalogManagerWindow().Show();
            return;
        }

        if (moduleId == "action-engine")
        {
            new ActionEngineWindow(_database, _user).Show();
            return;
        }

        if (moduleId == "universal-command-bar")
        {
            new UniversalCommandBarWindow(_database, _user, Navigate).Show();
            return;
        }

        _navigation.CurrentModuleId = moduleId;
        _navigation.CurrentTitle = title;
        _navigation.History.Add(moduleId);

        _breadcrumb.Text = $"Workspace > {title}";
        _status.Text = $"Modulo attivo: {title} | Cronologia: {_navigation.History.Count}";

        if (moduleId == "dashboard")
        {
            _content.Content = BuildDashboardTabHost();
            return;
        }

        if (moduleId == "control-room")
        {
            var builder = new Accyourate.App.UIFramework.Widgets.WidgetControlRoomBuilder(_database, _user);
            _content.Content = builder.Build(() => Navigate("control-room", "Enterprise Control Room"));
        }
        else
        {
            _content.Content = _moduleFactory.Create(moduleId);
        }
    }

    private Control BuildDashboardTabHost()
    {
        _dashboardTabHost ??= new WorkspaceHost(_dashboardTabManager);

        _dashboardTabManager.OpenOrActivate(new WorkspaceTab
        {
            Id = "dashboard",
            Title = "Dashboard",
            Icon = AxIcons.Dashboard,
            Content = _moduleFactory.Create("dashboard"),
            CanClose = false,
            IsPinned = true
        });

        return _dashboardTabHost;
    }

    private void AddMenu(StackPanel menu, string icon, string text, string moduleId, string title)
    {
        var button = MenuButton($"{icon}  {text}");
        button.Click += (_, _) => Navigate(moduleId, title);
        menu.Children.Add(button);
    }

    private void OpenCommandPalette()
    {
        var win = new CommandPaletteWindow(_database, _user, Navigate);
        win.Show();
    }

    private static Button MenuButton(string text)
    {
        return new Button
        {
            Content = text,
            Background = Brushes.Transparent,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Padding = new Avalonia.Thickness(14, 10),
            CornerRadius = new Avalonia.CornerRadius(12)
        };
    }

    private static Button ExternalButton(string text, Action action)
    {
        var button = MenuButton(text);
        button.Foreground = UiTokens.Brush(UiTokens.TextSecondary);
        button.Click += (_, _) => action();
        return button;
    }

    private static Border SearchBox()
    {
        return new Border
        {
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            CornerRadius = new Avalonia.CornerRadius(18),
            Margin = new Avalonia.Thickness(12, 16),
            Padding = new Avalonia.Thickness(16, 8),
            Child = new TextBlock
            {
                Text = "⌕ Cerca nel gestionale o premi ⌘K...",
                Foreground = UiTokens.Brush(UiTokens.TextSecondary)
            }
        };
    }

    private static Button TopButton(string text, Action action)
    {
        var button = new Button
        {
            Content = text,
            Background = Brushes.Transparent,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            Padding = new Avalonia.Thickness(8)
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static TextBlock StatusText(string text)
    {
        return new TextBlock
        {
            Text = text,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
    }


    private void ToggleTheme()
    {
        _themeMode = _themeMode == UiThemeMode.Light ? UiThemeMode.Dark : UiThemeMode.Light;
        Background = UiTokens.Brush(UiTokens.BackgroundFor(_themeMode));
        _status.Text = $"Tema: {_themeMode} | Modulo attivo: {_navigation.CurrentTitle} | Cronologia: {_navigation.History.Count}";
    }

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
