using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.AI;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App;

public sealed class AiIntentCatalogManagerWindow : Window
{
    private readonly ListBox _intentList = new();
    private readonly TextBox _category = new();
    private readonly TextBox _moduleId = new();
    private readonly TextBox _suggestedAction = new();
    private readonly TextBox _strongKeywords = new();
    private readonly TextBox _keywords = new();
    private readonly TextBlock _message = new();

    private List<AiIntentDefinition> _intents = new();
    private AiIntentDefinition? _selected;

    public AiIntentCatalogManagerWindow()
    {
        Title = "Accyourate Enterprise X - AI Intent Catalog Manager";
        Width = 1180;
        Height = 820;
        MinWidth = 1040;
        MinHeight = 720;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();
        LoadCatalog();
    }

    private Control BuildLayout()
    {
        var root = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("330,*")
        };

        var left = new StackPanel
        {
            Margin = new Thickness(22),
            Spacing = 12
        };

        left.Children.Add(new TextBlock
        {
            Text = "Intent Catalog",
            FontSize = 26,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        left.Children.Add(new TextBlock
        {
            Text = "Gestisci categorie, moduli e sinonimi riconosciuti dall'AI Assistant.",
            TextWrapping = TextWrapping.Wrap,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });

        _intentList.Height = 520;
        _intentList.SelectionChanged += (_, _) => SelectIntent();
        left.Children.Add(_intentList);

        var reset = new Button
        {
            Content = "Ripristina catalogo default",
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            Padding = new Thickness(12, 9),
            CornerRadius = new CornerRadius(12)
        };
        reset.Click += (_, _) =>
        {
            AiIntentCatalogStorage.Reset();
            LoadCatalog();
            _message.Text = "Catalogo ripristinato.";
        };
        left.Children.Add(reset);

        Add(root, Card(left), 0, 0);

        var form = new StackPanel
        {
            Margin = new Thickness(22),
            Spacing = 12
        };

        form.Children.Add(new TextBlock
        {
            Text = "Modifica intento",
            FontSize = 26,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        form.Children.Add(Label("Categoria"));
        form.Children.Add(_category);

        form.Children.Add(Label("Modulo Workspace"));
        form.Children.Add(_moduleId);

        form.Children.Add(Label("Azione suggerita"));
        form.Children.Add(_suggestedAction);

        form.Children.Add(Label("Parole chiave forti"));
        _strongKeywords.AcceptsReturn = true;
        _strongKeywords.Height = 120;
        form.Children.Add(_strongKeywords);

        form.Children.Add(Label("Parole chiave normali"));
        _keywords.AcceptsReturn = true;
        _keywords.Height = 150;
        form.Children.Add(_keywords);

        var buttons = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 10
        };

        var save = PrimaryButton("Salva intento");
        save.Click += (_, _) => SaveSelected();
        buttons.Children.Add(save);

        var addDigital = SecondaryButton("Aggiungi sinonimo Digital Twin");
        addDigital.Click += (_, _) => AddDigitalTwinTerm();
        buttons.Children.Add(addDigital);

        form.Children.Add(buttons);

        _message.Foreground = UiTokens.Brush(UiTokens.BrandBlue);
        form.Children.Add(_message);

        form.Children.Add(new TextBlock
        {
            Text = $"File catalogo: {AiIntentCatalogStorage.GetPath()}",
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });

        Add(root, Card(new ScrollViewer
        {
            Content = form,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        }), 1, 0);

        return root;
    }

    private void LoadCatalog()
    {
        _intents = AiIntentCatalogStorage.Load().ToList();
        _intentList.ItemsSource = _intents.Select(i => $"{i.Category}  →  {i.ModuleId}").ToList();

        if (_intents.Count > 0)
        {
            _intentList.SelectedIndex = 0;
            SelectIntent();
        }
    }

    private void SelectIntent()
    {
        var index = _intentList.SelectedIndex;
        if (index < 0 || index >= _intents.Count)
            return;

        _selected = _intents[index];

        _category.Text = _selected.Category;
        _moduleId.Text = _selected.ModuleId;
        _suggestedAction.Text = _selected.SuggestedAction;
        _strongKeywords.Text = string.Join(Environment.NewLine, _selected.StrongKeywords);
        _keywords.Text = string.Join(Environment.NewLine, _selected.Keywords);
        _message.Text = "";
    }

    private void SaveSelected()
    {
        if (_selected is null)
            return;

        var index = _intents.FindIndex(i => i.Id == _selected.Id);
        if (index < 0)
            return;

        _intents[index] = new AiIntentDefinition
        {
            Id = _selected.Id,
            Category = _category.Text ?? string.Empty,
            ModuleId = _moduleId.Text ?? string.Empty,
            SuggestedAction = _suggestedAction.Text ?? string.Empty,
            StrongKeywords = Lines(_strongKeywords.Text),
            Keywords = Lines(_keywords.Text)
        };

        AiIntentCatalogStorage.Save(_intents);
        LoadCatalog();
        _message.Text = "Intento salvato.";
    }

    private void AddDigitalTwinTerm()
    {
        var index = _intents.FindIndex(i => i.Id == "digital-twin");
        if (index < 0)
            return;

        _intentList.SelectedIndex = index;
        SelectIntent();

        var current = _strongKeywords.Text ?? string.Empty;
        if (!current.Contains("holter", StringComparison.OrdinalIgnoreCase))
        {
            _strongKeywords.Text = current + Environment.NewLine + "holter" + Environment.NewLine + "fascia cardiaca" + Environment.NewLine + "sensore ecg";
        }

        _message.Text = "Sinonimi Digital Twin aggiunti. Premi Salva intento.";
    }

    private static string[] Lines(string? value)
    {
        return (value ?? string.Empty)
            .Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static TextBlock Label(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        };
    }

    private static Button PrimaryButton(string text)
    {
        return new Button
        {
            Content = text,
            Background = UiTokens.Brush(UiTokens.BrandBlue),
            Foreground = Brushes.White,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(14, 10),
            CornerRadius = new CornerRadius(12)
        };
    }

    private static Button SecondaryButton(string text)
    {
        return new Button
        {
            Content = text,
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            Padding = new Thickness(14, 10),
            CornerRadius = new CornerRadius(12)
        };
    }

    private static Border Card(Control child)
    {
        return new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            CornerRadius = new CornerRadius(22),
            Padding = new Thickness(18),
            Margin = new Thickness(8),
            Child = child
        };
    }

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
