using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.DesignSystem;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.HumanResources.Enterprise;

public sealed class HumanResourcesEnterpriseView : UserControl
{
    private readonly HumanResourcesEnterpriseService _service = new();
    private readonly Action<string, string>? _navigate;

    public HumanResourcesEnterpriseView(Action<string, string>? navigate = null)
    {
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

        page.Children.Add(AxPageHeader.Create(
            "Human Resources Enterprise",
            "Fascicolo digitale dipendenti, asset assegnati, documenti e timeline HR.",
            AxButton.Create("+ Nuovo dipendente", () => Navigate("human-resources", "Human Resources"), AxButtonKind.Primary),
            AxButton.Create("Apri HR classico", () => Navigate("human-resources", "Human Resources"), AxButtonKind.Secondary)));

        page.Children.Add(Kpis(snapshot));
        page.Children.Add(Toolbar());

        var main = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,18,420")
        };

        Add(main, EmployeesPanel(), 0, 0);
        Add(main, EmployeeEntityPage(snapshot), 2, 0);
        page.Children.Add(main);

        return new ScrollViewer
        {
            Content = page,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
    }

    private Control Kpis(HumanResourcesEnterpriseSnapshot snapshot)
    {
        var wrap = new WrapPanel
        {
            ItemWidth = 232,
            ItemHeight = 152
        };

        wrap.Children.Add(AxKpiCard.Create("👥", "Dipendenti", snapshot.Employees.ToString(), "Anagrafica HR"));
        wrap.Children.Add(AxKpiCard.Create("✅", "Attivi", snapshot.ActiveEmployees.ToString(), "Dipendenti operativi"));
        wrap.Children.Add(AxKpiCard.Create("💻", "Asset assegnati", snapshot.AssignedAssets.ToString(), "Collegati al personale"));
        wrap.Children.Add(AxKpiCard.Create("📄", "Documenti", snapshot.Documents.ToString(), "Archivio collegato"));

        return wrap;
    }

    private Control Toolbar()
    {
        var toolbar = new AxToolbar()
            .AddLeft(AxSearchBox.Create("Cerca dipendente, matricola, reparto..."))
            .AddLeft(Filter("Reparto"))
            .AddLeft(Filter("Ruolo"))
            .AddLeft(Filter("Stato"))
            .AddRight(AxButton.Create("Aggiorna", () => { }, AxButtonKind.Secondary))
            .AddRight(AxButton.Create("Esporta", () => { }, AxButtonKind.Success));

        return toolbar;
    }

    private static ComboBox Filter(string placeholder)
    {
        return new ComboBox
        {
            PlaceholderText = placeholder,
            MinWidth = 130,
            Padding = new Thickness(10, 8)
        };
    }

    private Control EmployeesPanel()
    {
        var stack = new StackPanel { Spacing = 12 };

        stack.Children.Add(new TextBlock
        {
            Text = "Elenco dipendenti",
            FontSize = AxTypography.SectionTitle,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        stack.Children.Add(AxEmptyState.Create(
            "👥",
            "Fascicolo HR pronto",
            "Questa vista Enterprise prepara elenco dipendenti, profilo, asset e documenti collegati. Usa 'Apri HR classico' per la gestione dati attuale."));

        return AxCard.Create(stack);
    }

    private Control EmployeeEntityPage(HumanResourcesEnterpriseSnapshot snapshot)
    {
        var stack = new StackPanel { Spacing = 14 };

        stack.Children.Add(new TextBlock
        {
            Text = "Entity Page Dipendente",
            FontSize = AxTypography.SectionTitle,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        stack.Children.Add(new TextBlock
        {
            Text = "Prima struttura del fascicolo digitale dipendente. Nei prossimi sprint verrà collegata ai record reali.",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap
        });

        stack.Children.Add(new AxInfoPanel("Anagrafica")
            .AddItem("Nome", "Seleziona un dipendente", "👤")
            .AddItem("Reparto", "Da collegare al database HR", "🏢")
            .AddItem("Ruolo", "Da collegare al database HR", "💼")
            .ToCard());

        stack.Children.Add(new AxInfoPanel("Relazioni")
            .AddItem("Asset assegnati", snapshot.AssignedAssets.ToString(), "💻")
            .AddItem("Documenti collegati", snapshot.Documents.ToString(), "📄")
            .ToCard());

        var timeline = new AxTimeline("Timeline HR");
        foreach (var item in _service.EmployeeTimeline())
            timeline.AddEvent(item, "", "", "•");

        stack.Children.Add(timeline.ToCard());

        return AxCard.Create(stack);
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
