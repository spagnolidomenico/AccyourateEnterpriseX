using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Input;
using Avalonia.Media;
using System.Collections.Generic;
using Accyourate.App.Models;
using Accyourate.App.Data;
using Accyourate.App.Security;
using Accyourate.App.Framework;
using Accyourate.App.Shared.UI;
using Accyourate.App.Shared.Theme;
using Accyourate.App.UIFramework.Foundation;
using Accyourate.App.AssetManagement;
using Accyourate.App.AssetManagement.Deliveries;

namespace Accyourate.App;

public sealed class MainWindow : Window
{
    private readonly CurrentUser _user;
    private readonly DatabaseService _database;
    private TextBlock? _breadcrumb;
    private StackPanel? _currentMenuGroup;
    private Grid? _rootGrid;
    private Border? _menuBorder;
    private Border? _contextBorder;
    private StackPanel? _contextMenu;
    private TextBlock? _contextTitle;
    private bool _sidebarCollapsed;
    private CommandPaletteWindow? _commandPalette;
    private bool _commandPaletteOpening;
    private ContentControl? _workspaceContent;
    private Window? _embeddedModuleWindow;
    private readonly Dictionary<string, Button> _areaButtons = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Button> _contextButtons = new(StringComparer.OrdinalIgnoreCase);
    private string _activeArea = "Home";
    private string? _activeContextKey;
    private string? _currentWorkspaceKey;

    private const double ExpandedSidebarWidth = AxLayoutTokens.SidebarWidth;
    private const double CollapsedSidebarWidth = 72;
    private const string SidebarTextColor = "#F8FAFC";
    private const string SidebarMutedTextColor = "#CBD5E1";
    private const string SidebarHoverColor = "#1E293B";
    private const string SidebarActiveColor = "#334155";
    private const string ContextTextColor = "#1E293B";
    private const string ContextHoverColor = "#E2E8F0";
    private const string ContextActiveColor = "#DBEAFE";

    public MainWindow(CurrentUser user, DatabaseService database)
    {
        _user = user;
        _database = database;

        Title = "Accyourate Enterprise X — M4.2 Workspace Stabilization";
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
            ColumnDefinitions = new ColumnDefinitions($"{(_sidebarCollapsed ? CollapsedSidebarWidth : ExpandedSidebarWidth)},220,*"),
            RowDefinitions = new RowDefinitions("64,*")
        };

        _rootGrid = root;

        var menu = BuildMenu();
        Grid.SetRowSpan(menu, 2);
        root.Children.Add(menu);

        var header = BuildHeader();
        Grid.SetColumn(header, 1);
        Grid.SetColumnSpan(header, 2);
        root.Children.Add(header);

        var context = BuildContextSidebar();
        Grid.SetRow(context, 1);
        Grid.SetColumn(context, 1);
        root.Children.Add(context);

        _workspaceContent = new ContentControl
        {
            Content = BuildDashboard(),
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch
        };
        Grid.SetRow(_workspaceContent, 1);
        Grid.SetColumn(_workspaceContent, 2);
        root.Children.Add(_workspaceContent);

        SetContextArea("Home");

        return root;
    }

    private Control BuildHeader()
    {
        var header = new Border
        {
            Background = Brush.Parse("#FFFFFF"),
            BorderBrush = Brush.Parse("#E5E7EB"),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(24, 0)
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            VerticalAlignment = VerticalAlignment.Stretch
        };

        var left = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16,
            VerticalAlignment = VerticalAlignment.Center
        };

        var menuButton = MakeHeaderAction("☰", "Comprimi o espandi menu", ToggleSidebar);
        menuButton.Foreground = Brush.Parse("#334155");
        left.Children.Add(menuButton);

        var search = new TextBox
        {
            Width = 420,
            Height = 40,
            Watermark = "Cerca nel gestionale o premi Ctrl+K…",
            Background = Brush.Parse("#F4F5F8"),
            Foreground = Brush.Parse("#111827"),
            BorderBrush = Brush.Parse("#E5E7EB"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(14, 8),
            VerticalContentAlignment = VerticalAlignment.Center
        };
        search.IsReadOnly = true;
        search.PointerPressed += (_, e) =>
        {
            e.Handled = true;
            OpenCommandPalette();
        };
        left.Children.Add(search);

        _breadcrumb = new TextBlock
        {
            Text = "Workspace  /  Dashboard",
            Foreground = Brush.Parse("#64748B"),
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center
        };
        left.Children.Add(_breadcrumb);
        grid.Children.Add(left);

        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center
        };
        actions.Children.Add(MakeLightHeaderAction("⌘K", "Apri Command Palette", OpenCommandPalette));
        actions.Children.Add(MakeLightHeaderAction("◐", "Cambia tema", ToggleTheme));
        actions.Children.Add(MakeLightHeaderAction("🔔", "Notifiche", () => OpenModuleInWorkspace(new NotificationsWindow(), "Home > Notifiche")));
        actions.Children.Add(new Border
        {
            Width = 1,
            Height = 28,
            Background = Brush.Parse("#E5E7EB"),
            Margin = new Thickness(5, 0)
        });

        var avatarText = string.IsNullOrWhiteSpace(_user.DisplayName)
            ? "A"
            : _user.DisplayName.Trim()[0].ToString().ToUpperInvariant();
        actions.Children.Add(new Border
        {
            Width = 34,
            Height = 34,
            CornerRadius = new CornerRadius(17),
            Background = Brush.Parse(AxSemanticTokens.BrandAccent),
            Child = new TextBlock
            {
                Text = avatarText,
                Foreground = Brushes.White,
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        });
        actions.Children.Add(new TextBlock
        {
            Text = _user.DisplayName,
            Foreground = Brush.Parse("#111827"),
            FontSize = 13,
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        });
        Grid.SetColumn(actions, 1);
        grid.Children.Add(actions);

        header.Child = grid;
        return header;
    }

    private static Button MakeLightHeaderAction(string glyph, string tooltip, Action? action = null)
    {
        var button = new Button
        {
            Content = glyph,
            MinWidth = 38,
            Height = 38,
            Padding = new Thickness(10, 0),
            Background = Brushes.Transparent,
            Foreground = Brush.Parse("#334155"),
            BorderThickness = new Thickness(0),
            FontSize = 13,
            CornerRadius = new CornerRadius(10)
        };
        ToolTip.SetTip(button, tooltip);
        if (action is not null)
            button.Click += (_, _) => action();
        return button;
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
            Padding = new Thickness(12, 16)
        };

        _menuBorder = border;
        var stack = new StackPanel { Spacing = 6 };

        var brand = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 11,
            Margin = new Thickness(8, 2, 8, 22),
            VerticalAlignment = VerticalAlignment.Center
        };
        brand.Children.Add(new Border
        {
            Width = 38,
            Height = 38,
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
        if (!_sidebarCollapsed)
        {
            brand.Children.Add(new StackPanel
            {
                Spacing = 0,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new TextBlock { Text = "Accyourate", Foreground = Brushes.White, FontSize = 15, FontWeight = FontWeight.SemiBold },
                    new TextBlock { Text = "Enterprise X", Foreground = Brush.Parse("#CBD5E1"), FontSize = 12 }
                }
            });
        }
        stack.Children.Add(brand);

        if (!_sidebarCollapsed)
        {
            stack.Children.Add(new TextBlock
            {
                Text = "AREE",
                Foreground = Brush.Parse("#94A3B8"),
                FontSize = 11,
                FontWeight = FontWeight.SemiBold,
                Margin = new Thickness(10, 0, 0, 8)
            });
        }

        AddAreaButton(stack, "🏠", "Home", "Home");
        AddAreaButton(stack, "💻", "Asset", "Asset");
        AddAreaButton(stack, "👥", "Persone", "Persone");
        AddAreaButton(stack, "✚", "Medical", "Medical");
        AddAreaButton(stack, "🌐", "Infrastruttura", "Infrastruttura");
        AddAreaButton(stack, "📁", "Documenti", "Documenti");
        AddAreaButton(stack, "AI", "Intelligenza artificiale", "AI");
        AddAreaButton(stack, "⚙", "Amministrazione", "Amministrazione");

        border.Child = stack;
        return border;
    }

    private void AddAreaButton(StackPanel stack, string glyph, string label, string area)
    {
        var content = _sidebarCollapsed ? glyph : $"{glyph}  {label}";
        var labelControl = new TextBlock
        {
            Text = content,
            Foreground = Brush.Parse(SidebarTextColor),
            FontSize = 13,
            TextWrapping = TextWrapping.NoWrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
            HorizontalAlignment = _sidebarCollapsed ? HorizontalAlignment.Center : HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        var button = new Button
        {
            Content = labelControl,
            Foreground = Brush.Parse(SidebarTextColor),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = _sidebarCollapsed ? HorizontalAlignment.Center : HorizontalAlignment.Left,
            FontSize = 13,
            Padding = new Thickness(12, 10),
            CornerRadius = new CornerRadius(AxLayoutTokens.RadiusSmall)
        };
        ToolTip.SetTip(button, label);
        button.Click += (_, _) => SetContextArea(area);
        button.PointerEntered += (_, _) =>
        {
            if (!string.Equals(area, _activeArea, StringComparison.OrdinalIgnoreCase))
            {
                button.Background = Brush.Parse(SidebarHoverColor);
                labelControl.Foreground = Brushes.White;
            }
        };
        button.PointerExited += (_, _) =>
            ApplyAreaButtonState(button, string.Equals(area, _activeArea, StringComparison.OrdinalIgnoreCase));
        _areaButtons[area] = button;
        ApplyAreaButtonState(button, string.Equals(area, _activeArea, StringComparison.OrdinalIgnoreCase));
        stack.Children.Add(button);
    }

    private static void ApplyAreaButtonState(Button button, bool isActive)
    {
        var foreground = isActive ? Brushes.White : Brush.Parse(SidebarTextColor);
        button.Background = isActive ? Brush.Parse(SidebarActiveColor) : Brushes.Transparent;
        button.Foreground = foreground;
        button.FontWeight = isActive ? FontWeight.SemiBold : FontWeight.Normal;
        if (button.Content is TextBlock label)
        {
            label.Foreground = foreground;
            label.FontWeight = button.FontWeight;
        }
    }

    private static void ApplyContextButtonState(Button button, bool isActive)
    {
        var foreground = isActive ? Brush.Parse("#0F4C9A") : Brush.Parse(ContextTextColor);
        button.Background = isActive ? Brush.Parse(ContextActiveColor) : Brushes.Transparent;
        button.Foreground = foreground;
        button.FontWeight = isActive ? FontWeight.SemiBold : FontWeight.Normal;
        if (button.Content is TextBlock label)
        {
            label.Foreground = foreground;
            label.FontWeight = button.FontWeight;
        }
    }

    private Control BuildContextSidebar()
    {
        _contextTitle = new TextBlock
        {
            Text = "Home",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse(AxSemanticTokens.TextPrimary),
            Margin = new Thickness(18, 20, 18, 14)
        };
        _contextMenu = new StackPanel { Spacing = 4, Margin = new Thickness(10, 0, 10, 16) };

        var panel = new DockPanel();
        DockPanel.SetDock(_contextTitle, Dock.Top);
        panel.Children.Add(_contextTitle);
        panel.Children.Add(new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            Content = _contextMenu
        });

        _contextBorder = new Border
        {
            Background = Brush.Parse("#F8FAFC"),
            BorderBrush = Brush.Parse("#E5E7EB"),
            BorderThickness = new Thickness(0, 0, 1, 0),
            Child = panel
        };
        return _contextBorder;
    }

    private void SetContextArea(string area)
    {
        if (_contextMenu is null || _contextTitle is null)
            return;

        _activeArea = area;
        foreach (var pair in _areaButtons)
            ApplyAreaButtonState(pair.Value, string.Equals(pair.Key, area, StringComparison.OrdinalIgnoreCase));

        _contextTitle.Text = area;
        _contextMenu.Children.Clear();
        _contextButtons.Clear();
        _activeContextKey = null;
        SetBreadcrumb($"Workspace > {area}");

        switch (area)
        {
            case "Home":
                OpenOperationalCenter();
                AddContextButton("🏠 Centro Operativo", OpenOperationalCenter);
                AddContextButton("🔔 Notifiche", () => OpenModuleInWorkspace(new NotificationsWindow(), "Home > Notifiche"));
                AddContextButton("📊 Analytics", () => OpenModuleInWorkspace(new AnalyticsDashboardWindow(_database, _user), "Analytics"));
                AddContextButton("⌕ Ricerca globale", OpenCommandPalette);
                break;
            case "Asset":
                AddContextButton("📋 Tutti gli asset", () => OpenAssetWorkspace(null, "Tutti gli asset"));
                AddContextButton("📦 Registro consegne", OpenDeliveryRegister);
                AddContextButton("🛠 Centro manutenzioni", OpenMaintenanceOperations);
                AddContextButton("💻 Notebook", () => OpenAssetWorkspace("Notebook", "Notebook"));
                AddContextButton("🖥 Desktop", () => OpenAssetWorkspace("Desktop PC", "Desktop"));
                AddContextButton("🖨 Stampanti", () => OpenAssetWorkspace("Stampante", "Stampanti"));
                AddContextButton("📱 Smartphone", () => OpenAssetWorkspace("Smartphone", "Smartphone"));
                AddContextButton("🏷 Etichette e QR", () => OpenAssetWorkspace(null, "Etichette e QR"));
                AddContextButton("📈 Report asset", () => OpenModuleInWorkspace(new AnalyticsDashboardWindow(_database, _user), "Analytics"));
                break;
            case "Persone":
                AddContextButton("👥 Dipendenti", () => OpenModuleInWorkspace(new EmployeesWindow(_database, _user), "Persone > Dipendenti"));
                AddContextButton("📄 Verbali consegna", OpenDeliveryRegister);
                AddContextButton("🏢 Anagrafica aziendale", () => OpenModuleInWorkspace(new EmployeesWindow(_database, _user), "Persone > Dipendenti"));
                break;
            case "Medical":
                AddContextButton("✚ Dispositivi medici", () => OpenModuleInWorkspace(new MedicalDevicesWindow(_database, _user), "Medical > Dispositivi medici"));
                AddContextButton("🏭 Produzione e qualità", () => OpenModuleInWorkspace(new ProductionQualityWindow(_database, _user), "Medical > Produzione e qualità"));
                AddContextButton("🧺 Lavaggi e manutenzione", () => OpenModuleInWorkspace(new LaundryMaintenanceWindow(_database, _user), "Medical > Lavaggi e manutenzione"));
                break;
            case "Infrastruttura":
                AddContextButton("🌐 Rete aziendale", () => OpenModuleInWorkspace(new InfrastructureWindow(_database, _user), "Infrastruttura > Rete aziendale"));
                AddContextButton("📦 Magazzino e logistica", () => OpenModuleInWorkspace(new WarehouseLogisticsWindow(_database, _user), "Infrastruttura > Magazzino e logistica"));
                AddContextButton("💾 Backup", () => OpenModuleInWorkspace(new SettingsWindow(_database), "Amministrazione > Impostazioni"));
                AddContextButton("🛠 Diagnostica", () => OpenModuleInWorkspace(new DiagnosticsWindow(_database), "Infrastruttura > Diagnostica"));
                break;
            case "Documenti":
                AddContextButton("📁 Centro documenti", () => OpenModuleInWorkspace(new DocumentManagementWindow(_database, _user), "Documenti"));
                AddContextButton("📄 Verbali", () => OpenModuleInWorkspace(new DocumentManagementWindow(_database, _user), "Documenti"));
                break;
            case "AI":
                AddContextButton("AI Assistant", () => OpenModuleInWorkspace(new EnterpriseAiAssistantWindow(_database, _user), "AI > Assistant"));
                AddContextButton("AI Intent Catalog", () => OpenModuleInWorkspace(new AiIntentCatalogManagerWindow(), "AI > Intent Catalog"));
                AddContextButton("AX Action Engine", () => OpenModuleInWorkspace(new ActionEngineWindow(_database, _user), "AI > Action Engine"));
                AddContextButton("⌕ Universal Command Bar", () => OpenModuleInWorkspace(new UniversalCommandBarWindow(_database, _user), "AI > Universal Command Bar"));
                break;
            case "Amministrazione":
                AddContextButton("👤 Gestione utenti", () => OpenModuleInWorkspace(new UsersWindow(_database, _user), "Amministrazione > Gestione utenti"));
                AddContextButton("⚙ Impostazioni", () => OpenModuleInWorkspace(new SettingsWindow(_database), "Amministrazione > Impostazioni"));
                AddContextButton("🎨 Tema", () => OpenModuleInWorkspace(new ThemePersonalizationWindow(_database, _user), "Amministrazione > Tema"));
                AddContextButton("🏷 Branding Center", () => OpenModuleInWorkspace(new BrandingCenterWindow(_database, _user), "Amministrazione > Branding Center"));
                AddContextButton("🏗 Architettura", () => OpenModuleInWorkspace(new ArchitectureWindow(_database), "Amministrazione > Architettura"));
                break;
        }
    }


    private void OpenOperationalCenter()
    {
        if (_workspaceContent is null)
            return;

        const string key = "home:operational-center";
        if (string.Equals(_currentWorkspaceKey, key, StringComparison.Ordinal))
            return;

        _workspaceContent.Content = BuildDashboard();
        _currentWorkspaceKey = key;
        SetBreadcrumb("Workspace > Centro Operativo");
    }

    private void OpenAssetWorkspace(string? category, string label)
    {
        if (_workspaceContent is null)
            return;

        var key = $"asset:{category ?? "all"}:{label}";
        if (string.Equals(_currentWorkspaceKey, key, StringComparison.Ordinal))
            return;

        _workspaceContent.Content = new AssetManagementView(category);
        _currentWorkspaceKey = key;
        SetBreadcrumb($"Workspace > Asset > {label}");
    }

    private void OpenDeliveryRegister()
    {
        if (_workspaceContent is null)
            return;

        const string key = "asset:delivery-register";
        if (string.Equals(_currentWorkspaceKey, key, StringComparison.Ordinal))
            return;

        var view = new DeliveryRegisterView();
        view.AssetRequested += OpenAssetDetails;
        _workspaceContent.Content = view;
        _currentWorkspaceKey = key;
        SetBreadcrumb("Workspace > Asset > Registro consegne");
    }

    private void OpenMaintenanceOperations()
    {
        if (_workspaceContent is null)
            return;

        const string key = "asset:maintenance-operations";
        if (string.Equals(_currentWorkspaceKey, key, StringComparison.Ordinal))
            return;

        var view = new MaintenanceOperationsView();
        view.AssetRequested += OpenAssetDetails;
        _workspaceContent.Content = view;
        _currentWorkspaceKey = key;
        SetBreadcrumb("Workspace > Asset > Centro manutenzioni");
    }

    private void OpenAssetDetails(int assetId)
    {
        if (_workspaceContent is null)
            return;

        var view = new AssetManagementView();
        view.OpenAssetDetails(assetId);
        _workspaceContent.Content = view;
        _currentWorkspaceKey = $"asset:details:{assetId}";
        SetBreadcrumb("Workspace > Asset > Dettaglio asset");
    }


    private void OpenModuleInWorkspace(Window moduleWindow, string breadcrumb)
    {
        if (_workspaceContent is null)
            return;

        var key = $"module:{breadcrumb}";
        if (string.Equals(_currentWorkspaceKey, key, StringComparison.Ordinal))
            return;

        var moduleContent = moduleWindow.Content;
        moduleWindow.Content = null;

        _workspaceContent.Content = null;
        _embeddedModuleWindow = moduleWindow;
        _workspaceContent.Content = moduleContent ?? new TextBlock
        {
            Text = "Il modulo non contiene una vista incorporabile.",
            Margin = new Thickness(24),
            FontSize = 16
        };

        _currentWorkspaceKey = key;
        SetBreadcrumb($"Workspace > {breadcrumb}");
    }

    private void AddContextButton(string text, Action action)
    {
        if (_contextMenu is null)
            return;

        var label = new TextBlock
        {
            Text = text,
            Foreground = Brush.Parse(ContextTextColor),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center
        };
        var button = new Button
        {
            Content = label,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Background = Brushes.Transparent,
            Foreground = Brush.Parse(ContextTextColor),
            BorderThickness = new Thickness(0),
            FontSize = 13,
            Padding = new Thickness(12, 10),
            CornerRadius = new CornerRadius(8)
        };
        var key = $"{_activeArea}:{text}";
        button.Click += (_, _) =>
        {
            _activeContextKey = key;
            foreach (var pair in _contextButtons)
                ApplyContextButtonState(pair.Value, string.Equals(pair.Key, key, StringComparison.Ordinal));
            action();
        };
        button.PointerEntered += (_, _) =>
        {
            if (!string.Equals(key, _activeContextKey, StringComparison.Ordinal))
            {
                button.Background = Brush.Parse(ContextHoverColor);
                label.Foreground = Brush.Parse("#0F172A");
            }
        };
        button.PointerExited += (_, _) =>
            ApplyContextButtonState(button, string.Equals(key, _activeContextKey, StringComparison.Ordinal));
        _contextButtons[key] = button;
        ApplyContextButtonState(button, string.Equals(_activeContextKey, key, StringComparison.Ordinal));
        _contextMenu.Children.Add(button);
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
            Foreground = Brush.Parse("#E2E8F0"),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            FontSize = 13,
            Padding = new Thickness(12, 9),
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
                MakeQuickAction("＋", "Nuovo asset", "Apri Asset Management", () => OpenAssetWorkspace(null, "Tutti gli asset")),
                MakeQuickAction("▤", "Analytics", "Consulta indicatori e scadenze", () => OpenModuleInWorkspace(new AnalyticsDashboardWindow(_database, _user), "Analytics"))
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
                    Text = "M4.2 • Workspace Stabilization",
                    FontSize = 16,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = Brush.Parse(AxSemanticTokens.TextPrimary)
                },
                new TextBlock
                {
                    Text = "Stato attivo della navigazione, prevenzione dei caricamenti duplicati e workspace più coerente.",
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
        if (_commandPalette is not null)
        {
            _commandPalette.Activate();
            return;
        }

        if (_commandPaletteOpening)
        {
            return;
        }

        _commandPaletteOpening = true;

        try
        {
            var palette = new CommandPaletteWindow(_database, _user, NavigateFromCommandPalette);
            _commandPalette = palette;

            palette.Closed += (_, _) =>
            {
                _commandPalette = null;
                _commandPaletteOpening = false;
            };

            palette.Show(this);
        }
        finally
        {
            _commandPaletteOpening = false;
        }
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
                OpenAssetWorkspace(null, "Tutti gli asset");
                break;
            case "employees":
                OpenModuleInWorkspace(new EmployeesWindow(_database, _user), "Persone > Dipendenti");
                break;
            case "analytics":
                OpenModuleInWorkspace(new AnalyticsDashboardWindow(_database, _user), "Analytics");
                break;
            case "medical":
                OpenModuleInWorkspace(new MedicalDevicesWindow(_database, _user), "Medical > Dispositivi medici");
                break;
            case "branding":
                OpenModuleInWorkspace(new BrandingCenterWindow(_database, _user), "Amministrazione > Branding Center");
                break;
            case "design-system":
                OpenModuleInWorkspace(new DesignSystemShowcaseWindow(), "Amministrazione > Design System");
                break;
            case "architecture":
                OpenModuleInWorkspace(new ArchitectureWindow(_database), "Amministrazione > Architettura");
                break;
            case "notifications":
                OpenModuleInWorkspace(new NotificationsWindow(), "Home > Notifiche");
                break;
            case "settings":
                OpenModuleInWorkspace(new SettingsWindow(_database), "Amministrazione > Impostazioni");
                break;
            case "ai-assistant":
                OpenModuleInWorkspace(new EnterpriseAiAssistantWindow(_database, _user), "AI > Assistant");
                break;
            case "ai-catalog":
                OpenModuleInWorkspace(new AiIntentCatalogManagerWindow(), "AI > Intent Catalog");
                break;
            case "action-engine":
                OpenModuleInWorkspace(new ActionEngineWindow(_database, _user), "AI > Action Engine");
                break;
            case "universal-command-bar":
                OpenModuleInWorkspace(new UniversalCommandBarWindow(_database, _user), "AI > Universal Command Bar");
                break;
            default:
                OpenOperationalCenter();
                break;
        }
    }

    private void ToggleTheme()
    {
        var mode = AxThemeManager.Current.Toggle();
        Title = $"Accyourate Enterprise X — M4.1 — Tema {mode}";
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
