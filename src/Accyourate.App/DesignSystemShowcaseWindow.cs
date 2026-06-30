using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.DesignSystem;

namespace Accyourate.App;

public sealed class DesignSystemShowcaseWindow : Window
{
    public DesignSystemShowcaseWindow()
    {
        Title = "Accyourate Enterprise X - Design System Foundation";
        Width = 1180;
        Height = 820;
        MinWidth = 1024;
        MinHeight = 700;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = AccyourateDesignTokens.Brush(AccyourateDesignTokens.Background);
        Content = BuildLayout();
    }

    private Control BuildLayout()
    {
        var page = AxLayout.Page();

        page.Children.Add(AxTypography.PageTitle("Design System Foundation"));
        page.Children.Add(AxTypography.Body("Versione 7.2: componenti grafici comuni per rifattorizzare progressivamente tutta l'interfaccia mantenendo le funzionalità già validate."));

        var kpis = new WrapPanel { ItemWidth = 270, ItemHeight = 140 };
        kpis.Children.Add(AxCards.Kpi("⌁", "Dispositivi", "128", "Digital Twin", AccyourateDesignTokens.Purple));
        kpis.Children.Add(AxCards.Kpi("▣", "Asset IT", "42", "Inventario", AccyourateDesignTokens.Info));
        kpis.Children.Add(AxCards.Kpi("✓", "Conformità", "97%", "Qualità", AccyourateDesignTokens.Success));
        kpis.Children.Add(AxCards.Kpi("!", "Scadenze", "5", "Da verificare", AccyourateDesignTokens.Warning));
        page.Children.Add(kpis);

        var buttons = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10 };
        buttons.Children.Add(AxButtons.Primary("Primary"));
        buttons.Children.Add(AxButtons.Secondary("Secondary"));
        buttons.Children.Add(AxButtons.Success("Success"));
        buttons.Children.Add(AxButtons.Warning("Warning"));
        buttons.Children.Add(AxButtons.Danger("Danger"));
        page.Children.Add(AxCards.Card(new StackPanel { Spacing = 12, Children = { AxTypography.SectionTitle("Buttons"), buttons } }));

        var badges = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10 };
        badges.Children.Add(AxBadges.Status("Operativo", "success"));
        badges.Children.Add(AxBadges.Status("Attenzione", "warning"));
        badges.Children.Add(AxBadges.Status("Errore", "danger"));
        badges.Children.Add(AxBadges.Status("Info", "info"));
        page.Children.Add(AxCards.Card(new StackPanel { Spacing = 12, Children = { AxTypography.SectionTitle("Badges"), badges } }));

        var roadmap = new StackPanel { Spacing = 8 };
        roadmap.Children.Add(AxTypography.SectionTitle("Refactoring progressivo"));
        roadmap.Children.Add(AxTypography.Body("7.2.1 Dashboard refactor"));
        roadmap.Children.Add(AxTypography.Body("7.2.2 Menu e top bar refactor"));
        roadmap.Children.Add(AxTypography.Body("7.2.3 Form e tabelle refactor"));
        roadmap.Children.Add(AxTypography.Body("7.2.4 Moduli Medical e Documentale refactor"));
        page.Children.Add(AxCards.Card(roadmap));

        return AxLayout.PageShell(AxLayout.ScrollPage(page));
    }
}
