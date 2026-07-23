using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Input;
using Avalonia.Media;
using Accyourate.App.Models;
using Accyourate.App.Data;
using Accyourate.App.Security;
using Accyourate.App.Framework;
using Accyourate.App.Shared.UI;
using Accyourate.App.Shared.Theme;
using Accyourate.App.UIFramework.Foundation;

namespace Accyourate.App;

public sealed class MainWindow : Window
{
    private readonly CurrentUser _user;
    private readonly DatabaseService _database;
    private TextBlock? _breadcrumb;
    private StackPanel? _currentMenuGroup;
    private Grid? _rootGrid;
    private Border? _menuBorder;
    private bool _sidebarCollapsed;

    private const double ExpandedSidebarWidth = AxLayoutTokens.SidebarWidth;
    private const double CollapsedSidebarWidth = 72;

    public MainWindow(CurrentUser user, DatabaseService database)
    {
        _user = user;
        _database = database;

        Title = "Accyourate Enterprise X — Developer Edition M3.4";
        Width = 1440;
        Height = 900;
        MinWidth = 1120;
        MinHeight = 700;
        Background = Brush.Parse(AxSemanticTokens.Background);

        _sidebarCollapsed = LoadSidebarState();
        Content = BuildLayout();
        KeyDown += OnWindowKeyDown;
    }

    private Control BuildLayout()
    {
        var root = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions($"{(_sidebarCollapsed ? CollapsedSidebarWidth : ExpandedSidebarWidth)},*"),
            RowDefinitions = new RowDefinitions("76,*")
        };

        _rootGrid = root;

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
        var header = new Border
        {
            Background = Brush.Parse(AxSemanticTokens.DarkBackground),
            BorderBrush = Brush.Parse("#243047"),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(24, 0)
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions($"{AxLayoutTokens.SidebarWidth - 24},*,Auto"),
            VerticalAlignment = VerticalAlignment.Stretch
        };

        var brand = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            VerticalAlignment = VerticalAlignment.Center
        };
        brand.Children.Add(new Border
        {
            Width = 36,
            Height = 36,
            CornerRadius = new CornerRadius(10),
            Background = Brush.Parse(AxSemanticTokens.BrandAccent),
            Child = new TextBlock
            {
                Text = "AX",
                Foreground = Brushes.White,
                FontWeight = FontWeight.Bold,
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        });
        brand.Children.Add(new StackPanel
        {
            Spacing = 1,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new TextBlock
                {
                    Text = "Accyourate Enterprise X",
                    Foreground = Brushes.White,
                    FontSize = 16,
                    FontWeight = FontWeight.SemiBold
                },
                new TextBlock
                {
                    Text = "Developer Edition  •  M3 Design System",
                    Foreground = Brush.Parse("#94A3B8"),
                    FontSize = 11
                }
            }
        });
        grid.Children.Add(brand);

        _breadcrumb = new TextBlock
        {
            Text = "Centro Operativo  /  Dashboard",
            Foreground = Brush.Parse("#CBD5E1"),
            FontSize = 14,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(18, 0, 0, 0)
        };
        Grid.SetColumn(_breadcrumb, 1);
        grid.Children.Add(_breadcrumb);

        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            VerticalAlignment = VerticalAlignment.Center
        };
        actions.Children.Add(MakeHeaderAction("☰", "Comprimi o espandi menu", ToggleSidebar));
        actions.Children.Add(MakeHeaderAction("⌕", "Ricerca globale (Ctrl+K)", OpenCommandPalette));
        actions.Children.Add(MakeHeaderAction("◐", "Cambia tema", ToggleTheme));
        actions.Children.Add(MakeHeaderAction("🔔", "Notifiche", () => new NotificationsWindow().Show()));
        actions.Children.Add(new Border
        {
            Width = 1,
            Height = 30,
            Background = Brush.Parse("#334155"),
            Margin = new Thickness(4, 0)
        });

        var avatarText = string.IsNullOrWhiteSpace(_user.DisplayName)
            ? "A"
            : _user.DisplayName.Trim()[0].ToString().ToUpperInvariant();
        actions.Children.Add(new Border
        {
            Width = 36,
            Height = 36,
            CornerRadius = new CornerRadius(18),
            Background = Brush.Parse("#334155"),
            Child = new TextBlock
            {
                Text = avatarText,
                Foreground = Brushes.White,
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        });
        actions.Children.Add(new StackPanel
        {
            Spacing = 0,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new TextBlock
                {
                    Text = _user.DisplayName,
                    Foreground = Brushes.White,
                    FontSize = 13,
                    FontWeight = FontWeight.SemiBold
                },
                new TextBlock
                {
                    Text = _user.Role,
                    Foreground = Brush.Parse("#94A3B8"),
                    FontSize = 11
                }
            }
        });
        Grid.SetColumn(actions, 2);
        grid.Children.Add(actions);

        header.Child = grid;
        return header;
    }

    private static Button MakeHeaderAction(string glyph, string tooltip, Action? action = null)
    {
        var button = new Button
        {
            Content = glyph,
            Width = 38,
            Height = 38,
            Padding = new Thickness(0),
            Background = Brushes.Transparent,
            Foreground = Brush.Parse("#CBD5E1"),
            BorderThickness = new Thickness(0),
            FontSize = 15
        };
        ToolTip.SetTip(button, tooltip);
        if (action is not null)
            button.Click += (_, _) => action();
        return button;
    }

    private Control BuildMenu()
    {
        var border = new Border
        {
            Background = Brush.Parse(AxSemanticTokens.DarkNavigationSurface),
            BorderBrush = Brush.Parse("#243047"),
            BorderThickness = new Thickness(0, 0, 1, 0),
            Padding = new Thickness(14, 18)
        };

        _menuBorder = border;

        var stack = new StackPanel { Spacing = 8 };

        stack.Children.Add(new TextBlock
        {
            Text = "NAVIGAZIONE",
            Foreground = Brush.Parse("#64748B"),
            FontSize = 11,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(10, 0, 0, 8)
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
            _breadcrumb.Text = text.Replace(" > ", "  /  ");
    }

    private void AddMenuButton(StackPanel stack, string text, Action? action = null, string? permission = null)
    {
        if (permission is not null && !_user.Can(permission))
            return;

        var button = new Button
        {
            Content = text,
            Foreground = Brush.Parse("#D7E0EC"),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            FontSize = 13,
            Padding = new Thickness(14, 10),
            Margin = new Thickness(0, 1),
            CornerRadius = new CornerRadius(AxLayoutTokens.RadiusSmall)
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
            Foreground = Brush.Parse("#E2E8F0"),
            Background = Brush.Parse("#172033"),
            BorderThickness = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            FontWeight = FontWeight.SemiBold,
            FontSize = 13,
            Padding = new Thickness(12, 11),
            Margin = new Thickness(0, 10, 0, 2),
            CornerRadius = new CornerRadius(AxLayoutTokens.RadiusSmall)
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
            Background = Brush.Parse(AxSemanticTokens.Background),
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
        };

        var root = new StackPanel
        {
            Margin = new Thickness(32, 28, 32, 36),
            Spacing = 24
        };

        var welcome = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
        welcome.Children.Add(new StackPanel
        {
            Spacing = 6,
            Children =
            {
                new TextBlock
                {
                    Text = $"Buongiorno, {_user.DisplayName}",
                    FontSize = AxTypographyTokens.Display,
                    FontWeight = FontWeight.Bold,
                    Foreground = Brush.Parse(AxSemanticTokens.TextPrimary)
                },
                new TextBlock
                {
                    Text = "Panoramica operativa dell'organizzazione e dei servizi Enterprise X.",
                    FontSize = AxTypographyTokens.BodyLarge,
                    Foreground = Brush.Parse(AxSemanticTokens.TextSecondary)
                }
            }
        });
        var environment = new Border
        {
            Background = Brush.Parse("#E8F1FF"),
            BorderBrush = Brush.Parse("#C7DBFF"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(AxLayoutTokens.RadiusPill),
            Padding = new Thickness(14, 7),
            VerticalAlignment = VerticalAlignment.Top,
            Child = new TextBlock
            {
                Text = "●  Ambiente di sviluppo",
                Foreground = Brush.Parse("#2457A6"),
                FontSize = 12,
                FontWeight = FontWeight.SemiBold
            }
        };
        Grid.SetColumn(environment, 1);
        welcome.Children.Add(environment);
        root.Children.Add(welcome);

        var kpis = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*,*")
        };
        AddDashboardCard(kpis, 0, "Dipendenti", "15", "+2 questo trimestre", "👥", AxSemanticTokens.Info);
        AddDashboardCard(kpis, 1, "Asset IT", "30", "97% operativi", "▣", AxSemanticTokens.Success);
        AddDashboardCard(kpis, 2, "Dispositivi medici", "12", "2 controlli pianificati", "✚", AxSemanticTokens.Highlight);
        AddDashboardCard(kpis, 3, "Attività aperte", "7", "3 ad alta priorità", "✓", AxSemanticTokens.Warning);
        root.Children.Add(kpis);

        var body = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("1.55*,1*")
        };

        var operations = MakeSurface();
        operations.Child = new StackPanel
        {
            Spacing = 18,
            Children =
            {
                MakeSectionHeader("Stato operativo", "Aggiornamento del workspace corrente"),
                MakeStatusRow("Navigazione e permessi", "Operativo", "Menu e moduli filtrati per ruolo", AxSemanticTokens.Success),
                MakeDivider(),
                MakeStatusRow("Master Data", "Operativo", "Employees CRUD e registry disponibili", AxSemanticTokens.Success),
                MakeDivider(),
                MakeStatusRow("Design System M3.1", "Attivo", "Foundation e token canonici caricati", AxSemanticTokens.Info),
                MakeDivider(),
                MakeStatusRow("Controlli programmati", "2 in scadenza", "Verificare dispositivi e strumenti", AxSemanticTokens.Warning)
            }
        };
        operations.Margin = new Thickness(0, 0, 18, 0);
        body.Children.Add(operations);

        var quickPanel = new StackPanel { Spacing = 18 };
        var quickActions = MakeSurface();
        quickActions.Child = new StackPanel
        {
            Spacing = 14,
            Children =
            {
                MakeSectionHeader("Azioni rapide", "Accesso alle attività frequenti"),
                MakeQuickAction("⌕", "Ricerca globale", "Trova asset, persone e documenti", OpenCommandPalette),
                MakeQuickAction("＋", "Nuovo asset", "Apri Asset Management", () => new AssetsWindow(_database, _user).Show()),
                MakeQuickAction("▤", "Analytics", "Consulta indicatori e scadenze", () => new AnalyticsDashboardWindow(_database, _user).Show())
            }
        };
        quickPanel.Children.Add(quickActions);

        var release = MakeSurface();
        release.Child = new StackPanel
        {
            Spacing = 12,
            Children =
            {
                MakeSectionHeader("Release corrente", "Accyourate Enterprise X"),
                new TextBlock
                {
                    Text = "M3.4 • Enterprise Workspace",
                    FontSize = 16,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = Brush.Parse(AxSemanticTokens.TextPrimary)
                },
                new TextBlock
                {
                    Text = "Workspace enterprise con sidebar comprimibile, Command Palette Ctrl+K, azioni rapide operative e navigazione centralizzata.",
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = Brush.Parse(AxSemanticTokens.TextSecondary),
                    FontSize = 13,
                    LineHeight = 19
                },
                new Border
                {
                    Height = 8,
                    CornerRadius = new CornerRadius(4),
                    Background = Brush.Parse("#E2E8F0"),
                    Child = new Border
                    {
                        Width = 210,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        CornerRadius = new CornerRadius(4),
                        Background = Brush.Parse(AxSemanticTokens.BrandPrimary)
                    }
                },
                new TextBlock
                {
                    Text = "Workspace foundation completata • Asset Management prossimo obiettivo",
                    FontSize = 11,
                    Foreground = Brush.Parse(AxSemanticTokens.TextMuted)
                }
            }
        };
        quickPanel.Children.Add(release);
        Grid.SetColumn(quickPanel, 1);
        body.Children.Add(quickPanel);
        root.Children.Add(body);

        scroll.Content = root;
        return scroll;
    }

    private static void AddDashboardCard(Grid grid, int column, string title, string value, string detail, string glyph, string accent)
    {
        var card = MakeSurface();
        card.Padding = new Thickness(20);
        if (column < 3)
            card.Margin = new Thickness(0, 0, 14, 0);
        card.Child = new StackPanel
        {
            Spacing = 14,
            Children =
            {
                new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = title,
                            Foreground = Brush.Parse(AxSemanticTokens.TextSecondary),
                            FontSize = 13,
                            FontWeight = FontWeight.SemiBold,
                            VerticalAlignment = VerticalAlignment.Center
                        },
                        new Border
                        {
                            Width = 36,
                            Height = 36,
                            CornerRadius = new CornerRadius(10),
                            Background = MakeTintBrush(accent),
                            Child = new TextBlock
                            {
                                Text = glyph,
                                Foreground = Brush.Parse(accent),
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center,
                                FontSize = 16,
                                FontWeight = FontWeight.Bold
                            }
                        }.WithColumn(1)
                    }
                },
                new TextBlock
                {
                    Text = value,
                    FontSize = 30,
                    FontWeight = FontWeight.Bold,
                    Foreground = Brush.Parse(AxSemanticTokens.TextPrimary)
                },
                new TextBlock
                {
                    Text = detail,
                    Foreground = Brush.Parse(AxSemanticTokens.TextMuted),
                    FontSize = 12
                }
            }
        };
        Grid.SetColumn(card, column);
        grid.Children.Add(card);
    }

    private static IBrush MakeTintBrush(string color)
    {
        var parsed = Color.Parse(color);
        return new SolidColorBrush(Color.FromArgb(24, parsed.R, parsed.G, parsed.B));
    }

    private static Border MakeSurface() => new()
    {
        Background = Brush.Parse(AxSemanticTokens.Surface),
        BorderBrush = Brush.Parse(AxSemanticTokens.Border),
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(AxLayoutTokens.RadiusLarge),
        Padding = new Thickness(22)
    };

    private static StackPanel MakeSectionHeader(string title, string subtitle) => new()
    {
        Spacing = 4,
        Children =
        {
            new TextBlock
            {
                Text = title,
                FontSize = AxTypographyTokens.TitleSmall,
                FontWeight = FontWeight.SemiBold,
                Foreground = Brush.Parse(AxSemanticTokens.TextPrimary)
            },
            new TextBlock
            {
                Text = subtitle,
                FontSize = 12,
                Foreground = Brush.Parse(AxSemanticTokens.TextMuted)
            }
        }
    };

    private static Grid MakeStatusRow(string title, string status, string detail, string accent)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto") };
        grid.Children.Add(new Border
        {
            Width = 10,
            Height = 10,
            CornerRadius = new CornerRadius(5),
            Background = Brush.Parse(accent),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 14, 0)
        });
        var text = new StackPanel
        {
            Spacing = 3,
            Children =
            {
                new TextBlock { Text = title, FontWeight = FontWeight.SemiBold, Foreground = Brush.Parse(AxSemanticTokens.TextPrimary) },
                new TextBlock { Text = detail, FontSize = 12, Foreground = Brush.Parse(AxSemanticTokens.TextMuted) }
            }
        };
        text.Margin = new Thickness(0, 0, 14, 0);
        Grid.SetColumn(text, 1);
        grid.Children.Add(text);
        var badge = new Border
        {
            Background = MakeTintBrush(accent),
            CornerRadius = new CornerRadius(AxLayoutTokens.RadiusPill),
            Padding = new Thickness(10, 5),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock { Text = status, Foreground = Brush.Parse(accent), FontSize = 11, FontWeight = FontWeight.SemiBold }
        };
        Grid.SetColumn(badge, 2);
        grid.Children.Add(badge);
        return grid;
    }

    private static Border MakeDivider() => new()
    {
        Height = 1,
        Background = Brush.Parse(AxSemanticTokens.Border)
    };

    private static Button MakeQuickAction(string glyph, string title, string detail, Action action)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto") };
        grid.Children.Add(new Border
        {
            Width = 38,
            Height = 38,
            CornerRadius = new CornerRadius(10),
            Background = Brush.Parse(AxSemanticTokens.Selection),
            Margin = new Thickness(0, 0, 12, 0),
            Child = new TextBlock
            {
                Text = glyph,
                Foreground = Brush.Parse(AxSemanticTokens.BrandPrimary),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16,
                FontWeight = FontWeight.Bold
            }
        });
        var text = new StackPanel
        {
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new TextBlock { Text = title, FontWeight = FontWeight.SemiBold, Foreground = Brush.Parse(AxSemanticTokens.TextPrimary) },
                new TextBlock { Text = detail, FontSize = 11, Foreground = Brush.Parse(AxSemanticTokens.TextMuted) }
            }
        };
        text.Margin = new Thickness(0, 0, 12, 0);
        Grid.SetColumn(text, 1);
        grid.Children.Add(text);
        var arrow = new TextBlock
        {
            Text = "›",
            FontSize = 22,
            Foreground = Brush.Parse(AxSemanticTokens.TextMuted),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(arrow, 2);
        grid.Children.Add(arrow);
        var button = new Button
        {
            Background = Brush.Parse(AxSemanticTokens.SurfaceSubtle),
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(AxLayoutTokens.RadiusMedium),
            Padding = new Thickness(12),
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Content = grid
        };
        button.Click += (_, _) => action();
        return button;
    }

    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.K && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true;
            OpenCommandPalette();
        }
    }

    private void OpenCommandPalette()
    {
        var palette = new CommandPaletteWindow(_database, _user, NavigateFromCommandPalette);
        palette.ShowDialog(this);
    }

    private void NavigateFromCommandPalette(string moduleId, string title)
    {
        SetBreadcrumb($"Workspace > {title}");

        switch (moduleId)
        {
            case "workspace-home":
            case "dashboard":
                Activate();
                break;
            case "assets":
                new AssetsWindow(_database, _user).Show();
                break;
            case "employees":
                new EmployeesWindow(_database, _user).Show();
                break;
            case "analytics":
                new AnalyticsDashboardWindow(_database, _user).Show();
                break;
            case "medical":
                new MedicalDevicesWindow(_database, _user).Show();
                break;
            case "branding":
                new BrandingCenterWindow(_database, _user).Show();
                break;
            case "design-system":
                new DesignSystemShowcaseWindow().Show();
                break;
            case "architecture":
                new ArchitectureWindow(_database).Show();
                break;
            case "notifications":
                new NotificationsWindow().Show();
                break;
            case "settings":
                new SettingsWindow(_database).Show();
                break;
            case "ai-assistant":
                new EnterpriseAiAssistantWindow(_database, _user).Show();
                break;
            case "ai-catalog":
                new AiIntentCatalogManagerWindow().Show();
                break;
            case "action-engine":
                new ActionEngineWindow(_database, _user).Show();
                break;
            case "universal-command-bar":
                new UniversalCommandBarWindow(_database, _user).Show();
                break;
            default:
                new EnterpriseWorkspaceWindow(_database, _user).Show();
                break;
        }
    }

    private void ToggleTheme()
    {
        var mode = AxThemeManager.Current.Toggle();
        Title = $"Accyourate Enterprise X — M3.4 — Tema {mode}";
    }

    private void ToggleSidebar()
    {
        _sidebarCollapsed = !_sidebarCollapsed;
        if (_rootGrid is not null)
            _rootGrid.ColumnDefinitions[0].Width = new GridLength(_sidebarCollapsed ? CollapsedSidebarWidth : ExpandedSidebarWidth);

        if (_menuBorder is not null)
            _menuBorder.Padding = _sidebarCollapsed ? new Thickness(8, 18) : new Thickness(14, 18);

        SaveSidebarState();
    }

    private static string SidebarStatePath
    {
        get
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AccyourateEnterpriseX");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "workspace.sidebar");
        }
    }

    private static bool LoadSidebarState()
    {
        try
        {
            return File.Exists(SidebarStatePath) && File.ReadAllText(SidebarStatePath).Trim() == "collapsed";
        }
        catch
        {
            return false;
        }
    }

    private void SaveSidebarState()
    {
        try
        {
            File.WriteAllText(SidebarStatePath, _sidebarCollapsed ? "collapsed" : "expanded");
        }
        catch
        {
            // Lo stato della sidebar non deve mai impedire l'avvio dell'applicazione.
        }
    }
}

internal static class MainWindowGridExtensions
{
    public static T WithColumn<T>(this T control, int column) where T : Control
    {
        Grid.SetColumn(control, column);
        return control;
    }
}
