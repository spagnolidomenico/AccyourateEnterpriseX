using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.Models;

namespace Accyourate.App;

public sealed class ThemePersonalizationWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;

    private readonly TextBlock _message = new();

    private readonly TextBox _companyName = new();
    private readonly ComboBox _themeMode = new();
    private readonly ComboBox _primaryColor = new();
    private readonly ComboBox _sidebarColor = new();
    private readonly ComboBox _workspaceColor = new();
    private readonly ComboBox _menuStyle = new();
    private readonly TextBox _logoPath = new();
    private readonly ComboBox _menuItemColor = new();
    private readonly ComboBox _menuItemTextColor = new();
    private readonly ComboBox _menuHoverColor = new();
    private readonly ComboBox _menuHoverTextColor = new();
    private readonly ComboBox _menuSelectedColor = new();
    private readonly ComboBox _menuSelectedTextColor = new();

    private readonly Border _previewSidebar = new();
    private readonly Border _previewWorkspace = new();
    private readonly TextBlock _previewTitle = new();
    private readonly Button _previewButton = new();
    private readonly Border _previewNormalItem = new();
    private readonly Border _previewHoverItem = new();
    private readonly Border _previewSelectedItem = new();

    public ThemePersonalizationWindow(DatabaseService database, CurrentUser user)
    {
        _database = database;
        _user = user;

        Title = "Accyourate Enterprise X - Personalizzazione Tema";
        Width = 1080;
        Height = 760;
        MinWidth = 980;
        MinHeight = 680;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");

        Content = BuildLayout();
        LoadPreferences();
        UpdatePreview();
    }

    private Control BuildLayout()
    {
        var scroll = new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
        };

        var stack = new StackPanel { Margin = new Thickness(24), Spacing = 16 };

        stack.Children.Add(new TextBlock
        {
            Text = "Theme & Personalization Center",
            FontSize = 30,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });

        stack.Children.Add(new TextBlock
        {
            Text = "RC 6.1.7: personalizzazione tema, colori menu, hover e selezione.",
            TextWrapping = TextWrapping.Wrap
        });

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*")
        };

        AddControl(grid, BuildForm(), 0, 0);
        AddControl(grid, BuildPreview(), 1, 0);

        stack.Children.Add(grid);

        _message.Foreground = Brush.Parse("#B5162B");
        stack.Children.Add(_message);

        scroll.Content = stack;
        return scroll;
    }

    private Control BuildForm()
    {
        var form = new StackPanel { Spacing = 10 };

        form.Children.Add(Label("Nome azienda"));
        _companyName.Watermark = "Accyourate Group";
        form.Children.Add(_companyName);

        form.Children.Add(Label("Tema"));
        _themeMode.ItemsSource = new[] { "Chiaro", "Scuro", "Sistema" };
        _themeMode.SelectionChanged += (_, _) => UpdatePreview();
        form.Children.Add(_themeMode);

        form.Children.Add(Label("Colore primario"));
        _primaryColor.ItemsSource = new[]
        {
            "#B5162B - Bordeaux Accyourate",
            "#2563EB - Blu Enterprise",
            "#16A34A - Verde",
            "#7C3AED - Viola",
            "#D97706 - Ambra",
            "#DC2626 - Rosso"
        };
        _primaryColor.SelectionChanged += (_, _) => UpdatePreview();
        form.Children.Add(_primaryColor);

        form.Children.Add(Label("Colore menu laterale"));
        _sidebarColor.ItemsSource = new[]
        {
            "#111827 - Antracite",
            "#1F2937 - Grigio scuro",
            "#0F172A - Navy",
            "#2B2926 - Caldo scuro",
            "#000000 - Nero"
        };
        _sidebarColor.SelectionChanged += (_, _) => UpdatePreview();
        form.Children.Add(_sidebarColor);

        form.Children.Add(Label("Colore area lavoro"));
        _workspaceColor.ItemsSource = new[]
        {
            "#F7F7F6 - Grigio chiaro",
            "#FFFFFF - Bianco",
            "#F5EFEA - Beige chiaro",
            "#F3F4F6 - Neutro",
            "#EEF2FF - Blu chiaro"
        };
        _workspaceColor.SelectionChanged += (_, _) => UpdatePreview();
        form.Children.Add(_workspaceColor);

        form.Children.Add(Label("Stile menu"));
        _menuStyle.ItemsSource = new[] { "Collassabile", "Sempre aperto", "Compatto icone" };
        form.Children.Add(_menuStyle);

        form.Children.Add(Label("Percorso logo"));
        _logoPath.Watermark = "C:\\Logo\\logo.png";
        form.Children.Add(_logoPath);

        form.Children.Add(new Separator());
        form.Children.Add(new TextBlock { Text = "Personalizzazione voci menu", FontWeight = FontWeight.Bold, Foreground = Brush.Parse("#B5162B") });

        form.Children.Add(Label("Colore voce menu"));
        _menuItemColor.ItemsSource = MenuColorOptions();
        _menuItemColor.SelectionChanged += (_, _) => UpdatePreview();
        form.Children.Add(_menuItemColor);

        form.Children.Add(Label("Colore testo voce menu"));
        _menuItemTextColor.ItemsSource = TextColorOptions();
        _menuItemTextColor.SelectionChanged += (_, _) => UpdatePreview();
        form.Children.Add(_menuItemTextColor);

        form.Children.Add(Label("Colore voce al passaggio mouse"));
        _menuHoverColor.ItemsSource = MenuColorOptions();
        _menuHoverColor.SelectionChanged += (_, _) => UpdatePreview();
        form.Children.Add(_menuHoverColor);

        form.Children.Add(Label("Colore testo hover"));
        _menuHoverTextColor.ItemsSource = TextColorOptions();
        _menuHoverTextColor.SelectionChanged += (_, _) => UpdatePreview();
        form.Children.Add(_menuHoverTextColor);

        form.Children.Add(Label("Colore voce selezionata"));
        _menuSelectedColor.ItemsSource = PrimaryColorOptions();
        _menuSelectedColor.SelectionChanged += (_, _) => UpdatePreview();
        form.Children.Add(_menuSelectedColor);

        form.Children.Add(Label("Colore testo selezionato"));
        _menuSelectedTextColor.ItemsSource = TextColorOptions();
        _menuSelectedTextColor.SelectionChanged += (_, _) => UpdatePreview();
        form.Children.Add(_menuSelectedTextColor);

        var buttons = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 10
        };

        var save = PrimaryButton("Salva preferenze");
        save.Click += (_, _) => Save();

        var defaults = new Button { Content = "Ripristina default" };
        defaults.Click += (_, _) => ResetDefaults();

        buttons.Children.Add(save);
        buttons.Children.Add(defaults);
        form.Children.Add(buttons);

        return Card("Preferenze tema", form);
    }

    private Control BuildPreview()
    {
        _previewNormalItem.CornerRadius = new CornerRadius(8);
        _previewNormalItem.Padding = new Thickness(10);
        _previewNormalItem.Child = new TextBlock { Text = "🏠 Dashboard" };

        _previewHoverItem.CornerRadius = new CornerRadius(8);
        _previewHoverItem.Padding = new Thickness(10);
        _previewHoverItem.Child = new TextBlock { Text = "📈 Analytics" };

        _previewSelectedItem.CornerRadius = new CornerRadius(8);
        _previewSelectedItem.Padding = new Thickness(10);
        _previewSelectedItem.Child = new TextBlock { Text = "🏥 Medical" };

        var layout = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("180,*"),
            Height = 430
        };

        _previewSidebar.Child = new StackPanel
        {
            Margin = new Thickness(12),
            Spacing = 10,
            Children =
            {
                new TextBlock { Text = "Normale", Foreground = Brush.Parse("#D1D5DB"), FontWeight = FontWeight.Bold },
                _previewNormalItem,
                new TextBlock { Text = "Hover", Foreground = Brush.Parse("#D1D5DB"), FontWeight = FontWeight.Bold },
                _previewHoverItem,
                new TextBlock { Text = "Selezionato", Foreground = Brush.Parse("#D1D5DB"), FontWeight = FontWeight.Bold },
                _previewSelectedItem
            }
        };

        var workspace = new StackPanel { Margin = new Thickness(18), Spacing = 12 };
        _previewTitle.Text = "Anteprima area lavoro";
        _previewTitle.FontSize = 22;
        _previewTitle.FontWeight = FontWeight.Bold;
        workspace.Children.Add(_previewTitle);

        workspace.Children.Add(new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(14),
            Child = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new TextBlock { Text = "KPI esempio", FontWeight = FontWeight.Bold },
                    new TextBlock { Text = "128", FontSize = 30 },
                    new TextBlock { Text = "Dispositivi monitorati" }
                }
            }
        });

        _previewButton.Content = "Azione primaria";
        _previewButton.Foreground = Brushes.White;
        workspace.Children.Add(_previewButton);

        _previewWorkspace.Child = workspace;

        Grid.SetColumn(_previewSidebar, 0);
        Grid.SetColumn(_previewWorkspace, 1);
        layout.Children.Add(_previewSidebar);
        layout.Children.Add(_previewWorkspace);

        return Card("Anteprima", layout);
    }

    private void LoadPreferences()
    {
        var p = _database.GetThemePreferences();

        _companyName.Text = p.CompanyName;
        SelectByPrefix(_themeMode, p.ThemeMode);
        SelectByPrefix(_primaryColor, p.PrimaryColor);
        SelectByPrefix(_sidebarColor, p.SidebarColor);
        SelectByPrefix(_workspaceColor, p.WorkspaceColor);
        SelectByPrefix(_menuStyle, p.MenuStyle);
        _logoPath.Text = p.LogoPath;
        SelectByPrefix(_menuItemColor, p.MenuItemColor);
        SelectByPrefix(_menuItemTextColor, p.MenuItemTextColor);
        SelectByPrefix(_menuHoverColor, p.MenuHoverColor);
        SelectByPrefix(_menuHoverTextColor, p.MenuHoverTextColor);
        SelectByPrefix(_menuSelectedColor, p.MenuSelectedColor);
        SelectByPrefix(_menuSelectedTextColor, p.MenuSelectedTextColor);
    }

    private void Save()
    {
        var preferences = new ThemePreferenceRecord
        {
            CompanyName = _companyName.Text ?? "Accyourate Group",
            ThemeMode = _themeMode.SelectedItem?.ToString() ?? "Chiaro",
            PrimaryColor = ExtractColor(_primaryColor.SelectedItem?.ToString(), "#B5162B"),
            SidebarColor = ExtractColor(_sidebarColor.SelectedItem?.ToString(), "#111827"),
            WorkspaceColor = ExtractColor(_workspaceColor.SelectedItem?.ToString(), "#F7F7F6"),
            MenuStyle = _menuStyle.SelectedItem?.ToString() ?? "Collassabile",
            LogoPath = _logoPath.Text ?? "",
            MenuItemColor = ExtractColor(_menuItemColor.SelectedItem?.ToString(), "#111827"),
            MenuItemTextColor = ExtractColor(_menuItemTextColor.SelectedItem?.ToString(), "#FFFFFF"),
            MenuHoverColor = ExtractColor(_menuHoverColor.SelectedItem?.ToString(), "#374151"),
            MenuHoverTextColor = ExtractColor(_menuHoverTextColor.SelectedItem?.ToString(), "#FFFFFF"),
            MenuSelectedColor = ExtractColor(_menuSelectedColor.SelectedItem?.ToString(), "#B5162B"),
            MenuSelectedTextColor = ExtractColor(_menuSelectedTextColor.SelectedItem?.ToString(), "#FFFFFF")
        };

        _database.SaveThemePreferences(preferences, _user.Username);
        _message.Text = "Preferenze salvate. Alcune modifiche saranno applicate al prossimo riavvio.";
        UpdatePreview();
    }

    private void ResetDefaults()
    {
        _companyName.Text = "Accyourate Group";
        SelectByPrefix(_themeMode, "Chiaro");
        SelectByPrefix(_primaryColor, "#B5162B");
        SelectByPrefix(_sidebarColor, "#111827");
        SelectByPrefix(_workspaceColor, "#F7F7F6");
        SelectByPrefix(_menuStyle, "Collassabile");
        _logoPath.Text = "";
        SelectByPrefix(_menuItemColor, "#111827");
        SelectByPrefix(_menuItemTextColor, "#FFFFFF");
        SelectByPrefix(_menuHoverColor, "#374151");
        SelectByPrefix(_menuHoverTextColor, "#FFFFFF");
        SelectByPrefix(_menuSelectedColor, "#B5162B");
        SelectByPrefix(_menuSelectedTextColor, "#FFFFFF");
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        var primary = ExtractColor(_primaryColor.SelectedItem?.ToString(), "#B5162B");
        var sidebar = ExtractColor(_sidebarColor.SelectedItem?.ToString(), "#111827");
        var workspace = ExtractColor(_workspaceColor.SelectedItem?.ToString(), "#F7F7F6");
        var menuItem = ExtractColor(_menuItemColor.SelectedItem?.ToString(), "#111827");
        var menuItemText = ExtractColor(_menuItemTextColor.SelectedItem?.ToString(), "#FFFFFF");
        var menuHover = ExtractColor(_menuHoverColor.SelectedItem?.ToString(), "#374151");
        var menuHoverText = ExtractColor(_menuHoverTextColor.SelectedItem?.ToString(), "#FFFFFF");
        var menuSelected = ExtractColor(_menuSelectedColor.SelectedItem?.ToString(), "#B5162B");
        var menuSelectedText = ExtractColor(_menuSelectedTextColor.SelectedItem?.ToString(), "#FFFFFF");

        _previewSidebar.Background = Brush.Parse(sidebar);
        _previewWorkspace.Background = Brush.Parse(workspace);
        _previewTitle.Foreground = Brush.Parse(primary);
        _previewButton.Background = Brush.Parse(primary);

        _previewNormalItem.Background = Brush.Parse(menuItem);
        if (_previewNormalItem.Child is TextBlock normalText)
            normalText.Foreground = Brush.Parse(menuItemText);

        _previewHoverItem.Background = Brush.Parse(menuHover);
        if (_previewHoverItem.Child is TextBlock hoverText)
            hoverText.Foreground = Brush.Parse(menuHoverText);

        _previewSelectedItem.Background = Brush.Parse(menuSelected);
        if (_previewSelectedItem.Child is TextBlock selectedText)
            selectedText.Foreground = Brush.Parse(menuSelectedText);
    }

    private static string[] PrimaryColorOptions()
    {
        return new[]
        {
            "#B5162B - Bordeaux Accyourate",
            "#2563EB - Blu Enterprise",
            "#16A34A - Verde",
            "#7C3AED - Viola",
            "#D97706 - Ambra",
            "#DC2626 - Rosso",
            "#111827 - Antracite"
        };
    }

    private static string[] MenuColorOptions()
    {
        return new[]
        {
            "#111827 - Antracite",
            "#1F2937 - Grigio scuro",
            "#374151 - Hover grigio",
            "#0F172A - Navy",
            "#2B2926 - Caldo scuro",
            "#000000 - Nero",
            "#B5162B - Bordeaux"
        };
    }

    private static string[] TextColorOptions()
    {
        return new[]
        {
            "#FFFFFF - Bianco",
            "#D1D5DB - Grigio chiaro",
            "#F9FAFB - Quasi bianco",
            "#111827 - Antracite",
            "#B5162B - Bordeaux"
        };
    }

    private static void SelectByPrefix(ComboBox combo, string value)
    {
        for (var i = 0; i < combo.ItemCount; i++)
        {
            var item = combo.Items[i]?.ToString() ?? "";
            if (item.StartsWith(value, StringComparison.OrdinalIgnoreCase) || item.Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                combo.SelectedIndex = i;
                return;
            }
        }

        combo.SelectedIndex = combo.ItemCount > 0 ? 0 : -1;
    }

    private static string ExtractColor(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        var first = value.Split(' ')[0].Trim();
        return first.StartsWith("#") ? first : fallback;
    }

    private static TextBlock Label(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontWeight = FontWeight.Bold
        };
    }

    private static Button PrimaryButton(string text)
    {
        return new Button
        {
            Content = text,
            Background = Brush.Parse("#B5162B"),
            Foreground = Brushes.White,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(12, 8)
        };
    }

    private static Border Card(string title, Control content)
    {
        return new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(18),
            Child = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    new TextBlock { Text = title, FontSize = 20, FontWeight = FontWeight.Bold, Foreground = Brush.Parse("#B5162B") },
                    content
                }
            }
        };
    }

    private static void AddControl(Grid grid, Control control, int column, int row)
    {
        control.Margin = new Thickness(4);
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
