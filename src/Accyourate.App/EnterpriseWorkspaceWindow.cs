using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.Models;
using Accyourate.App.UIFramework.Shell;
using Accyourate.App.UIFramework.Tokens;
using Accyourate.App.UIFramework.Icons;
using Accyourate.App.UIFramework.DesignSystem;
using Accyourate.App.UIFramework.WorkspaceTabs;
using Accyourate.App.Platform.Notifications;
using WorkspaceModuleRegistryCore = Accyourate.App.UIFramework.WorkspaceModules.WorkspaceModuleRegistry;
using WorkspaceModuleCore = Accyourate.App.UIFramework.WorkspaceModules.IWorkspaceModule;
using DashboardWorkspaceModuleCore = Accyourate.App.UIFramework.WorkspaceModules.DashboardWorkspaceModule;
using DigitalTwinWorkspaceModuleCore = Accyourate.App.UIFramework.WorkspaceModules.DigitalTwinWorkspaceModule;
using AiAssistantWorkspaceModuleCore = Accyourate.App.UIFramework.WorkspaceModules.AiAssistantWorkspaceModule;
using ActionEngineWorkspaceModuleCore = Accyourate.App.UIFramework.WorkspaceModules.ActionEngineWorkspaceModule;
using UniversalCommandBarWorkspaceModuleCore = Accyourate.App.UIFramework.WorkspaceModules.UniversalCommandBarWorkspaceModule;
using AssetManagementWorkspaceModuleCore = Accyourate.App.UIFramework.WorkspaceModules.AssetManagementWorkspaceModule;
using MasterDataWorkspaceModuleCore = Accyourate.App.UIFramework.WorkspaceModules.MasterDataWorkspaceModule;
using HumanResourcesWorkspaceModuleCore = Accyourate.App.UIFramework.WorkspaceModules.HumanResourcesWorkspaceModule;
using DeliveryReportWorkspaceModuleCore = Accyourate.App.UIFramework.WorkspaceModules.DeliveryReportWorkspaceModule;
using SettingsWorkspaceModuleCore = Accyourate.App.UIFramework.WorkspaceModules.SettingsWorkspaceModule;
using DocumentCenterWorkspaceModuleCore = Accyourate.App.UIFramework.WorkspaceModules.DocumentCenterWorkspaceModule;
using EnterpriseSearchWorkspaceModuleCore = Accyourate.App.UIFramework.WorkspaceModules.EnterpriseSearchWorkspaceModule;
using AboutWorkspaceModuleCore = Accyourate.App.UIFramework.WorkspaceModules.AboutWorkspaceModule;
using BackupWorkspaceModuleCore = Accyourate.App.UIFramework.WorkspaceModules.BackupWorkspaceModule;
using UpdateWorkspaceModuleCore = Accyourate.App.UIFramework.WorkspaceModules.UpdateWorkspaceModule;

namespace Accyourate.App;

public sealed class EnterpriseWorkspaceWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;
    private readonly WorkspaceModuleFactory _moduleFactory;
    private readonly WorkspaceModuleRegistryCore _moduleRegistry = new();
    private readonly NavigationState _navigation = new();
    private readonly NotificationService _notificationService = new();

    private readonly ContentControl _content = new();
    private readonly TextBlock _breadcrumb = new();
    private readonly TextBlock _status = new();
    private UiThemeMode _themeMode = UiThemeMode.Light;
    private readonly WorkspaceTabManager _dashboardTabManager = new();
    private WorkspaceHost? _dashboardTabHost;
    private readonly WorkspaceTabManager _digitalTwinTabManager = new();
    private WorkspaceHost? _digitalTwinTabHost;
    private readonly WorkspaceTabManager _workspaceTabManager = new();
    private WorkspaceHost? _workspaceTabHost;

    public EnterpriseWorkspaceWindow(DatabaseService database, CurrentUser user)
    {
        _database = database;
        _user = user;
        _moduleFactory = new WorkspaceModuleFactory(_database, _user, Navigate);
        RegisterWorkspaceModules();

        Title = "Accyourate Enterprise X 15.0.1C2 - Notification Center";
        Width = 1480;
        Height = 920;
        MinWidth = 1180;
        MinHeight = 760;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();

        Navigate("workspace-home", "Workspace Home");
    }


    private void RegisterWorkspaceModules()
    {
        _moduleRegistry.Register(new DashboardWorkspaceModuleCore(_moduleFactory));
        _moduleRegistry.Register(new DigitalTwinWorkspaceModuleCore(_moduleFactory));
        _moduleRegistry.Register(new AiAssistantWorkspaceModuleCore(_database, _user));
        _moduleRegistry.Register(new ActionEngineWorkspaceModuleCore(_database, _user));
        _moduleRegistry.Register(new UniversalCommandBarWorkspaceModuleCore(_database, _user, Navigate));
        _moduleRegistry.Register(new AssetManagementWorkspaceModuleCore());
        _moduleRegistry.Register(new MasterDataWorkspaceModuleCore());
        _moduleRegistry.Register(new HumanResourcesWorkspaceModuleCore());
        _moduleRegistry.Register(new DeliveryReportWorkspaceModuleCore());
        _moduleRegistry.Register(new SettingsWorkspaceModuleCore());
        _moduleRegistry.Register(new DocumentCenterWorkspaceModuleCore());
        _moduleRegistry.Register(new EnterpriseSearchWorkspaceModuleCore(Navigate));
        _moduleRegistry.Register(new AboutWorkspaceModuleCore());
        _moduleRegistry.Register(new BackupWorkspaceModuleCore());
        _moduleRegistry.Register(new UpdateWorkspaceModuleCore());
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


    private string NotificationButtonText()
    {
        var unread = _notificationService.CountUnread();
        return unread > 0
            ? $"{AxIcons.Notifications} Notifiche ({unread})"
            : $"{AxIcons.Notifications} Notifiche";
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
        Add(grid, TopButton("⌕ Command", () => Navigate("universal-command-bar", "Universal Command Bar")), 3, 0);
        Add(grid, TopButton("AI", () => Navigate("ai-assistant", "AI Assistant")), 4, 0);
        Add(grid, TopButton(NotificationButtonText(), () => Navigate("notifications", "Notification Center")), 5, 0);
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
        AddMenu(menu, AxIcons.Notifications, "Notifiche", "notifications", "Notification Center");
        AddMenu(menu, AxIcons.ControlRoom, "Control Room", "control-room", "Enterprise Control Room");
        AddMenu(menu, "AI", "AI Assistant", "ai-assistant", "AI Assistant");
        AddMenu(menu, "AI", "AI Intent Catalog", "ai-catalog", "AI Intent Catalog");
        AddMenu(menu, "AX", "Action Engine", "action-engine", "Action Engine");
        AddMenu(menu, "⌕", "Universal Command Bar", "universal-command-bar", "Universal Command Bar");
        AddMenu(menu, "🔎", "Ricerca Enterprise", "enterprise-search", "Ricerca Enterprise");
        AddMenu(menu, AxIcons.Dashboard, "Dashboard", "dashboard", "Dashboard");
        AddMenu(menu, "IT", "Asset Management", "asset-management", "Asset Management");
        AddMenu(menu, "📄", "Verbali consegna", "delivery-reports", "Verbali di consegna");
        AddMenu(menu, "📁", "Centro Documenti", "document-center", "Centro Documenti");
        AddMenu(menu, "🏢", "Anagrafica Aziendale", "master-data", "Anagrafica Aziendale");
        AddMenu(menu, "👥", "Human Resources", "human-resources", "Human Resources");
        AddMenu(menu, AxIcons.Analytics, "Analytics", "analytics", "Analytics");
        AddMenu(menu, AxIcons.Medical, "Medical", "medical", "Medical Device Suite");
        AddMenu(menu, "DT", "Digital Twin", "digital-twin", "Digital Twin Platform");
        AddMenu(menu, AxIcons.Branding, "Branding", "branding", "Branding Center");
        AddMenu(menu, "⚙️", "Impostazioni", "settings-center", "Impostazioni");
        AddMenu(menu, "ℹ️", "Informazioni", "about-center", "Informazioni");
        AddMenu(menu, "💾", "Backup Center", "backup-center", "Backup Center");
        AddMenu(menu, "🔄", "Update Center", "update-center", "Update Center");
        AddMenu(menu, AxIcons.Design, "Design System", "design-system", "Design System");
        AddMenu(menu, AxIcons.Architecture, "Architecture", "architecture", "Enterprise Architecture");

        menu.Children.Add(new Separator { Margin = new Avalonia.Thickness(8, 14) });
        menu.Children.Add(new TextBlock
        {
            Text = "Azioni Workspace",
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            Margin = new Avalonia.Thickness(12, 6, 0, 4)
        });
        menu.Children.Add(ExternalButton("🧹 Chiudi tutte le schede", CloseAllWorkspaceTabs));
        menu.Children.Add(ExternalButton("📌 Dashboard", () => Navigate("dashboard", "Dashboard")));
        menu.Children.Add(ExternalButton("🔎 Ricerca Enterprise", () => Navigate("enterprise-search", "Ricerca Enterprise")));

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
            Height = 38,
            Background = UiTokens.Brush(UiTokens.Surface),
            ColumnDefinitions = new ColumnDefinitions("*,150,170,170,170,170")
        };

        _status.Margin = new Avalonia.Thickness(14, 0);
        _status.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        _status.Foreground = UiTokens.Brush(UiTokens.TextSecondary);
        Add(grid, _status, 0, 0);

        Add(grid, StatusText("v0.9 RC1"), 1, 0);
        Add(grid, StatusText("DB: SQLite ✓"), 2, 0);
        Add(grid, StatusText("Backup: disponibile"), 3, 0);
        Add(grid, StatusText("Update: pronto"), 4, 0);
        Add(grid, StatusText($"Utente: {_user.Username}"), 5, 0);

        return new Border
        {
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Avalonia.Thickness(0, 1, 0, 0),
            Child = grid
        };
    }

    private void CloseAllWorkspaceTabs()
    {
        _workspaceTabManager.CloseAllClosable();
        _status.Text = $"Schede chiuse | Modulo attivo: {_workspaceTabManager.ActiveTab?.Title ?? _navigation.CurrentTitle}";
    }

    private void Navigate(string moduleId, string title)
    {
        _navigation.CurrentModuleId = moduleId;
        _navigation.CurrentTitle = title;
        _navigation.History.Add(moduleId);

        _breadcrumb.Text = $"Workspace > {title}";
        _status.Text = $"Modulo attivo: {title} | Cronologia: {_navigation.History.Count}";

        if (OpenRegisteredWorkspaceModule(moduleId))
            return;

        if (moduleId == "digital-twin")
        {
            _content.Content = OpenWorkspaceModuleTab("digital-twin", "Digital Twin", "DT", true, false);
            return;
        }


        if (IsWorkspaceTabModule(moduleId))
        {
            _content.Content = OpenWorkspaceModuleTab(moduleId, title, WorkspaceTabIcon(moduleId), true, false);
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

    private Control BuildDigitalTwinTabHost()
    {
        _digitalTwinTabHost ??= new WorkspaceHost(_digitalTwinTabManager);

        _digitalTwinTabManager.OpenOrActivate(new WorkspaceTab
        {
            Id = "digital-twin",
            Title = "Digital Twin",
            Icon = "DT",
            Content = _moduleFactory.Create("digital-twin"),
            CanClose = true,
            IsPinned = false
        });

        return _digitalTwinTabHost;
    }



    private static bool IsWorkspaceTabModule(string moduleId)
    {
        return moduleId is
            "ai-catalog" or
            "analytics" or
            "medical" or
            "branding" or
            "design-system" or
            "architecture";
    }

    private static string WorkspaceTabIcon(string moduleId)
    {
        return moduleId switch
        {
            "ai-catalog" => "AI",
            "analytics" => AxIcons.Analytics,
            "medical" => AxIcons.Medical,
            "branding" => AxIcons.Branding,
            "design-system" => AxIcons.Design,
            "architecture" => AxIcons.Architecture,
            _ => "•"
        };
    }

    private bool OpenRegisteredWorkspaceModule(string moduleId)
    {
        var module = _moduleRegistry.Find(moduleId);
        if (module is null)
            return false;

        _content.Content = OpenWorkspaceRegisteredModuleTab(module);
        return true;
    }

    private Control OpenWorkspaceModuleTab(string moduleId, string title, string icon, bool canClose, bool isPinned)
    {
        _workspaceTabHost ??= new WorkspaceHost(_workspaceTabManager);

        try
        {
            _workspaceTabManager.OpenOrActivate(new WorkspaceTab
            {
                Id = moduleId,
                Title = title,
                Icon = icon,
                Content = _moduleFactory.Create(moduleId),
                CanClose = canClose,
                IsPinned = isPinned
            });

            _status.Text = $"Modulo attivo: {title} | Schede Workspace: {_workspaceTabManager.Tabs.Count}";
        }
        catch (Exception ex)
        {
            _workspaceTabManager.OpenOrActivate(new WorkspaceTab
            {
                Id = $"error-{moduleId}",
                Title = "Errore modulo",
                Icon = "!",
                Content = new Border
                {
                    Margin = new Avalonia.Thickness(24),
                    Padding = new Avalonia.Thickness(18),
                    Background = UiTokens.Brush(UiTokens.Surface),
                    CornerRadius = new Avalonia.CornerRadius(18),
                    Child = new TextBlock
                    {
                        Text = $"Impossibile aprire il modulo {title}: {ex.Message}",
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = UiTokens.Brush(UiTokens.Danger)
                    }
                },
                CanClose = true,
                IsPinned = false
            });
        }

        return _workspaceTabHost;
    }

    private Control OpenWorkspaceRegisteredModuleTab(WorkspaceModuleCore module)
    {
        _workspaceTabHost ??= new WorkspaceHost(_workspaceTabManager);

        try
        {
            _workspaceTabManager.OpenOrActivate(new WorkspaceTab
            {
                Id = module.Id,
                Title = module.Title,
                Icon = module.Icon,
                Content = module.CreateView(),
                CanClose = module.CanClose,
                IsPinned = module.IsPinned
            });

            _breadcrumb.Text = $"Workspace > {module.Title}";
            _status.Text = $"Modulo attivo: {module.Title} | Schede Workspace: {_workspaceTabManager.Tabs.Count}";
        }
        catch (Exception ex)
        {
            _workspaceTabManager.OpenOrActivate(new WorkspaceTab
            {
                Id = $"error-{module.Id}",
                Title = "Errore modulo",
                Icon = "!",
                Content = new Border
                {
                    Margin = new Avalonia.Thickness(24),
                    Padding = new Avalonia.Thickness(18),
                    Background = UiTokens.Brush(UiTokens.Surface),
                    CornerRadius = new Avalonia.CornerRadius(18),
                    Child = new TextBlock
                    {
                        Text = $"Impossibile aprire il modulo {module.Title}: {ex.Message}",
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = UiTokens.Brush(UiTokens.Danger)
                    }
                },
                CanClose = true,
                IsPinned = false
            });
        }

        return _workspaceTabHost;
    }

    private Control OpenWorkspaceCustomTab(string id, string title, string icon, Control content, bool canClose, bool isPinned)
    {
        _workspaceTabHost ??= new WorkspaceHost(_workspaceTabManager);

        _workspaceTabManager.OpenOrActivate(new WorkspaceTab
        {
            Id = id,
            Title = title,
            Icon = icon,
            Content = content,
            CanClose = canClose,
            IsPinned = isPinned
        });

        _breadcrumb.Text = $"Workspace > {title}";
        _status.Text = $"Modulo attivo: {title} | Schede Workspace: {_workspaceTabManager.Tabs.Count}";
        return _workspaceTabHost;
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
