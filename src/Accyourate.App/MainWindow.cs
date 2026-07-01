using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.Models;
using Accyourate.App.Data;
using Accyourate.App.Security;
using Accyourate.App.Framework;
using Accyourate.App.Shared.UI;
using Accyourate.App.Shared.Theme;

namespace Accyourate.App;

public sealed class MainWindow : Window
{
    private readonly CurrentUser _user;
    private readonly DatabaseService _database;
    private TextBlock? _breadcrumb;
    private StackPanel? _currentMenuGroup;

    public MainWindow(CurrentUser user, DatabaseService database)
    {
        _user = user;
        _database = database;

        Title = "Accyourate Enterprise X - Developer Edition 1.2";
        Width = 1280;
        Height = 820;
        MinWidth = 1100;
        MinHeight = 720;
        MinWidth = 1024;
        MinHeight = 650;
        Background = Brush.Parse("#F7F7F6");

        Content = BuildLayout();
    }

    private Control BuildLayout()
    {
        var root = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("290,*"),
            RowDefinitions = new RowDefinitions("64,*")
        };

        var header = BuildHeader();
        Grid.SetColumnSpan(header, 2);
        root.Children.Add(header);

        var menu = BuildMenu();
        Grid.SetRow(menu, 1);
        root.Children.Add(menu);

        var content = BuildDashboard();
        Grid.SetRow(content, 1);
        Grid.SetColumn(content, 1);
        root.Children.Add(content);

        return root;
    }

    private Control BuildHeader()
    {
        var grid = new Grid
        {
            Background = Brush.Parse("#111827"),
            ColumnDefinitions = new ColumnDefinitions("290,*,340")
        };

        grid.Children.Add(new TextBlock
        {
            Text = "ACCYOURATE ENTERPRISE X",
            Foreground = Brushes.White,
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(24, 0)
        });

        _breadcrumb = new TextBlock
        {
            Text = "Centro Operativo > Dashboard",
            Foreground = Brushes.White,
            FontSize = 16,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(_breadcrumb, 1);
        grid.Children.Add(_breadcrumb);

        var user = new TextBlock
        {
            Text = $"{_user.DisplayName} • {_user.Role}",
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 0, 24, 0)
        };
        Grid.SetColumn(user, 2);
        grid.Children.Add(user);

        return grid;
    }

    private Control BuildMenu()
    {
        var border = new Border
        {
            Background = Brush.Parse("#111827"),
            Padding = new Thickness(18)
        };

        var stack = new StackPanel { Spacing = 8 };

        stack.Children.Add(new TextBlock
        {
            Text = "Navigazione",
            Foreground = Brushes.White,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 8)
        });

        AddMenuSeparator(stack, "🏠 Centro Operativo");

        AddMenuButton(stack, "🏠 Dashboard", () => SetBreadcrumb("Centro Operativo > Dashboard"), PermissionCodes.DashboardView);

        AddMenuButton(stack, "🧭 Enterprise Navigation", () =>
        {
            SetBreadcrumb("Centro Operativo > Enterprise Navigation");
            var win = new EnterpriseNavigationGuideWindow();
            win.Show();
        }, PermissionCodes.DashboardView);

        AddMenuButton(stack, "✨ Enterprise UX Center", () =>
        {
            SetBreadcrumb("Centro Operativo > Enterprise UX Center");
            var win = new EnterpriseUxCenterWindow();
            win.Show();
        }, PermissionCodes.DashboardView);

        AddMenuButton(stack, "🎛 Design System", () =>
        {
            SetBreadcrumb("Centro Operativo > Design System");
            var win = new DesignSystemShowcaseWindow();
            win.Show();
        }, PermissionCodes.DashboardView);

        AddMenuButton(stack, "🧩 Enterprise Shell Foundation", () =>
        {
            SetBreadcrumb("Centro Operativo > Enterprise Shell Foundation");
            var win = new EnterpriseShellFoundationWindow();
            win.Show();
        }, PermissionCodes.DashboardView);

        AddMenuButton(stack, "🖥 Enterprise Workspace", () =>
        {
            SetBreadcrumb("Centro Operativo > Enterprise Workspace");
            var win = new EnterpriseWorkspaceWindow(_database, _user);
            win.Show();
        }, PermissionCodes.DashboardView);

        AddMenuButton(stack, "AI Enterprise AI Assistant", () =>
        {
            SetBreadcrumb("Centro Operativo > Enterprise AI Assistant");
            var win = new EnterpriseAiAssistantWindow(_database, _user);
            win.Show();
        }, PermissionCodes.DashboardView);

        AddMenuButton(stack, "AI Intent Catalog", () =>
        {
            SetBreadcrumb("Centro Operativo > AI Intent Catalog");
            var win = new AiIntentCatalogManagerWindow();
            win.Show();
        }, PermissionCodes.DashboardView);

        AddMenuButton(stack, "AX Action Engine", () =>
        {
            SetBreadcrumb("Centro Operativo > Action Engine");
            var win = new ActionEngineWindow(_database, _user);
            win.Show();
        }, PermissionCodes.DashboardView);

        AddMenuButton(stack, "⌕ Universal Command Bar", () =>
        {
            SetBreadcrumb("Centro Operativo > Universal Command Bar");
            var win = new UniversalCommandBarWindow(_database, _user);
            win.Show();
        }, PermissionCodes.DashboardView);







        AddMenuButton(stack, "🖼 Splash/Login Branding", () =>
        {
            SetBreadcrumb("Centro Operativo > Splash/Login Branding");
            var win = new BrandedSplashLoginWindow(_database, _user);
            win.Show();
        }, PermissionCodes.DashboardView);

        AddMenuButton(stack, "🏠 Branded Home", () =>
        {
            SetBreadcrumb("Centro Operativo > Branded Home");
            var win = new BrandedHomeWindow(_database, _user);
            win.Show();
        }, PermissionCodes.DashboardView);


        AddMenuButton(stack, "🏷 Branding Center", () =>
        {
            SetBreadcrumb("Amministrazione > Branding Center");
            var win = new BrandingCenterWindow(_database, _user);
            win.Show();
        }, PermissionCodes.DiagnosticsView);



        AddMenuButton(stack, "🍎 Apple Style Dashboard", () =>
        {
            SetBreadcrumb("Centro Operativo > Apple Style Dashboard");
            var win = new AppleStyleDashboardWindow(_database, _user);
            win.Show();
        }, PermissionCodes.DashboardView);


        AddMenuButton(stack, "🔝 Top Bar Preview", () =>
        {
            SetBreadcrumb("Centro Operativo > Top Bar Preview");
            var win = new EnterpriseTopBarWindow(_database, _user);
            win.Show();
        }, PermissionCodes.DashboardView);


        AddMenuButton(stack, "📊 Enterprise Dashboard", () =>
        {
            SetBreadcrumb("Centro Operativo > Enterprise Dashboard");
            var win = new EnterpriseDashboardWindow(_database);
            win.Show();
        }, PermissionCodes.DashboardView);

        AddMenuButton(stack, "📈 Analytics Dashboard", () =>
        {
            SetBreadcrumb("Centro Operativo > Analytics Dashboard");
            var win = new AnalyticsDashboardWindow(_database, _user);
            win.Show();
        }, PermissionCodes.DashboardView);

        AddMenuButton(stack, "🔎 Ricerca Globale", () =>
        {
            SetBreadcrumb("Centro Operativo > Ricerca Globale");
            var win = new GlobalSearchWindow(_database);
            win.Show();
        }, PermissionCodes.DashboardView);

        AddMenuSeparator(stack, "📋 Moduli Base");

        foreach (var module in ModuleRegistry.Modules)
        {
            AddMenuButton(stack, module.Title, () =>
            {
                SetBreadcrumb($"Moduli > {module.Title}");

                if (module.Code == "people")
                {
                    var people = new EmployeesWindow(_database, _user);
                    people.Show();
                    return;
                }

                if (module.Code == "assets")
                {
                    var assets = new AssetsWindow(_database, _user);
                    assets.Show();
                    return;
                }

                if (module.Code == "medical")
                {
                    var medical = new MedicalDevicesWindow(_database, _user);
                    medical.Show();
                    return;
                }

                var win = new CrudPlaceholderWindow(module, _user);
                win.Show();
            }, module.Permission);
        }

        AddMenuSeparator(stack, "🏥 Medical Suite");

        AddMenuButton(stack, "🏭 Produzione & Qualità", () =>
        {
            SetBreadcrumb("Medical Suite > Production & Quality");
            var win = new ProductionQualityWindow(_database, _user);
            win.Show();
        }, PermissionCodes.MedicalView);

        AddMenuSeparator(stack, "📦 Logistica");

        AddMenuButton(stack, "📦 Magazzino & Logistica", () =>
        {
            SetBreadcrumb("Logistica > Warehouse & Logistics");
            var win = new WarehouseLogisticsWindow(_database, _user);
            win.Show();
        }, PermissionCodes.MedicalView);

        AddMenuSeparator(stack, "🧺 Assistenza");

        AddMenuButton(stack, "🧺 Lavaggi & Manutenzione", () =>
        {
            SetBreadcrumb("Assistenza > Laundry & Maintenance");
            var win = new LaundryMaintenanceWindow(_database, _user);
            win.Show();
        }, PermissionCodes.MedicalView);

        AddMenuSeparator(stack, "📁 Documentale");

        AddMenuButton(stack, "📁 Document Management", () =>
        {
            SetBreadcrumb("Documentale > Document Management");
            var win = new DocumentManagementWindow(_database, _user);
            win.Show();
        }, PermissionCodes.DocumentsView);

        AddMenuSeparator(stack, "⚙️ Amministrazione");

        AddMenuButton(stack, "Gestione Utenti", () =>
        {
            SetBreadcrumb("Amministrazione > Gestione Utenti");
            var win = new UsersWindow(_database, _user);
            win.Show();
        }, PermissionCodes.UsersManage);

        AddMenuButton(stack, "🛠 Diagnostica", () =>
        {
            SetBreadcrumb("Amministrazione > Diagnostica");
            var win = new DiagnosticsWindow(_database);
            win.Show();
        }, PermissionCodes.DiagnosticsView);

        AddMenuButton(stack, "🧬 Workflow", () =>
        {
            SetBreadcrumb("Amministrazione > Workflow");
            var win = new WorkflowWindow(_database);
            win.Show();
        }, PermissionCodes.DiagnosticsView);

        AddMenuButton(stack, "💾 Infrastruttura", () =>
        {
            SetBreadcrumb("Amministrazione > Infrastruttura");
            var win = new InfrastructureWindow(_database, _user);
            win.Show();
        }, PermissionCodes.DiagnosticsView);

        AddMenuButton(stack, "⚙️ Impostazioni", () =>
        {
            SetBreadcrumb("Amministrazione > Impostazioni");
            var win = new SettingsWindow(_database);
            win.Show();
        }, PermissionCodes.DiagnosticsView);

        AddMenuButton(stack, "🎨 Personalizzazione Tema", () =>
        {
            SetBreadcrumb("Amministrazione > Personalizzazione Tema");
            var win = new ThemePersonalizationWindow(_database, _user);
            win.Show();
        }, PermissionCodes.DiagnosticsView);

        AddMenuButton(stack, "🏗 Enterprise Architecture", () =>
        {
            SetBreadcrumb("Amministrazione > Enterprise Architecture");
            var win = new ArchitectureWindow(_database);
            win.Show();
        }, PermissionCodes.DiagnosticsView);

        AddMenuButton(stack, "🔔 Notifiche", () =>
        {
            SetBreadcrumb("Core > Notifiche");
            var win = new NotificationsWindow();
            win.Show();
        }, PermissionCodes.DiagnosticsView);

        AddMenuButton(stack, "🔑 Cambio Password", () =>
        {
            SetBreadcrumb("Account > Cambio Password");
            var win = new ChangePasswordWindow(_database, _user);
            win.Show();
        }, PermissionCodes.PasswordChange);

        border.Child = new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            Content = stack
        };

        return border;
    }

    private void SetBreadcrumb(string text)
    {
        if (_breadcrumb is not null)
            _breadcrumb.Text = text;
    }

    private void AddMenuButton(StackPanel stack, string text, Action? action = null, string? permission = null)
    {
        if (permission is not null && !_user.Can(permission))
            return;

        var button = new Button
        {
            Content = text,
            Foreground = Brushes.White,
            Background = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Padding = new Thickness(18, 9, 12, 9),
            Margin = new Thickness(0, 2)
        };

        if (action is not null)
            button.Click += (_, _) => action();

        if (_currentMenuGroup is not null)
            _currentMenuGroup.Children.Add(button);
        else
            stack.Children.Add(button);
    }

    private void AddMenuSeparator(StackPanel stack, string title)
    {
        var group = new StackPanel
        {
            Spacing = 2,
            IsVisible = false,
            Margin = new Thickness(0, 2, 0, 8)
        };

        var header = new Button
        {
            Content = $"▶ {title}",
            Foreground = Brush.Parse("#D1D5DB"),
            Background = Brush.Parse("#1F2937"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(10, 9),
            Margin = new Thickness(0, 10, 0, 2)
        };

        header.Click += (_, _) =>
        {
            group.IsVisible = !group.IsVisible;
            header.Content = $"{(group.IsVisible ? "▼" : "▶")} {title}";
        };

        stack.Children.Add(header);
        stack.Children.Add(group);
        _currentMenuGroup = group;
    }

    private Control BuildDashboard()
    {
        var scroll = new ScrollViewer
        {
            Background = Brush.Parse("#F7F7F6"),
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };

        var stack = new StackPanel
        {
            Margin = new Thickness(28),
            Spacing = 20
        };

        stack.Children.Add(new TextBlock
        {
            Text = "Centro Operativo",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });

        stack.Children.Add(new TextBlock
        {
            Text = "Versione 13.0.2: UI Framework Adoption KPI."
        });

        var cards = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*,*")
        };

        var c1 = MakeCard("Navigazione", "OK");
        var c2 = MakeCard("Permessi", "OK");
        var c3 = MakeCard("CRUD Base", "OK");
        var c4 = MakeCard("Analytics", "OK");

        Grid.SetColumn(c1, 0);
        Grid.SetColumn(c2, 1);
        Grid.SetColumn(c3, 2);
        Grid.SetColumn(c4, 3);

        cards.Children.Add(c1);
        cards.Children.Add(c2);
        cards.Children.Add(c3);
        cards.Children.Add(c4);

        stack.Children.Add(cards);

        stack.Children.Add(new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(22),
            Child = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new TextBlock { Text = "Foundation 1.2 completata", FontSize = 20, FontWeight = FontWeight.Bold },
                    new TextBlock { Text = "• Menu filtrato per permessi" },
                    new TextBlock { Text = "• Breadcrumb superiore" },
                    new TextBlock { Text = "• Moduli registrati in ModuleRegistry" },
                    new TextBlock { Text = "• Finestra CRUD standard riutilizzabile" },
                    new TextBlock { Text = "• Dashboard registrata come IWorkspaceModule" }
                }
            }
        });

        scroll.Content = stack;
        return scroll;
    }

    private static Control MakeCard(string title, string value)
    {
        return new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(18),
            Margin = new Thickness(6),
            Child = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = title, FontWeight = FontWeight.Bold },
                    new TextBlock { Text = value, FontSize = 28, Foreground = Brush.Parse("#B5162B") }
                }
            }
        };
    }
}
