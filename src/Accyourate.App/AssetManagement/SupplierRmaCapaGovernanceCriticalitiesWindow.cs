using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class SupplierRmaCapaGovernanceCriticalitiesWindow : Window
{
    private readonly SupplierRmaCapaGovernanceDashboardService _service = new();
    private readonly StackPanel _rows = new();
    private readonly TextBlock _summary = new();

    public SupplierRmaCapaGovernanceCriticalitiesWindow()
    {
        Title = "Criticita Governance CAPA";
        Width = 1050;
        Height = 720;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Content = Build();
        Load();
    }

    private Control Build()
    {
        var root = new DockPanel { Margin = new Thickness(24) };
        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Margin = new Thickness(0, 0, 0, 16) };
        var title = new StackPanel { Spacing = 3, Children = { new TextBlock { Text = "Registro criticita Governance CAPA", FontSize = 28, FontWeight = FontWeight.Bold }, new TextBlock { Text = "Anomalie consolidate e collegamenti alle funzioni di risoluzione.", Foreground = UiTokens.Brush(UiTokens.TextSecondary) } } };
        Grid.SetColumn(title, 0); header.Children.Add(title);
        var refresh = SupplierRmaCorrectiveActionsWindow.Button("Aggiorna", Load, true);
        Grid.SetColumn(refresh, 1); header.Children.Add(refresh);
        DockPanel.SetDock(header, Dock.Top); root.Children.Add(header);
        _summary.Margin = new Thickness(0, 0, 0, 12); _summary.FontWeight = FontWeight.SemiBold;
        DockPanel.SetDock(_summary, Dock.Top); root.Children.Add(_summary);
        root.Children.Add(new ScrollViewer { Content = _rows, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto });
        return root;
    }

    private void Load()
    {
        try
        {
            var x = _service.Load();
            _rows.Children.Clear(); _rows.Spacing = 8;
            Add("Documenti fascicolo mancanti", x.MissingDocuments, "Critica", "Completa archivio e verbale del fascicolo.", () => new SupplierRmaCapaDossierRegistryWindow().Show(this));
            Add("Riesami fascicolo scaduti", x.ReviewsOverdue, "Critica", "Registra il riesame documentale scaduto.", () => new SupplierRmaCapaDossierRegistryWindow().Show(this));
            Add("Attestazioni fascicolo non valide", x.InvalidAttestations, "Critica", "Verifica integrita e rigenera l'attestazione.", () => new SupplierRmaCapaAttestationRegistryWindow().Show(this));
            Add("Archivi attestazione mancanti", x.MissingAttestationArchives, "Critica", "Ripristina o rigenera l'archivio attestato.", () => new SupplierRmaCapaAttestationRegistryWindow().Show(this));
            Add("Esportazioni modificate", x.InvalidExports, "Critica", "Verifica l'integrita dell'esportazione.", () => new SupplierRmaCapaAttestationExportHistoryWindow().Show(this));
            Add("File esportazione mancanti", x.MissingExports, "Critica", "Ripristina o genera nuovamente il fascicolo esportato.", () => new SupplierRmaCapaAttestationExportHistoryWindow().Show(this));
            Add("Conservazioni fascicolo scadute", x.RetentionOverdue, "Critica", "Rinnova la conservazione del fascicolo.", () => new SupplierRmaCapaAttestationExportHistoryWindow().Show(this));
            Add("Riesami Governance scaduti", x.PeriodicReviewsOverdue, "Critica", "Registra il nuovo riesame periodico Governance.", () => new SupplierRmaCapaGovernanceReviewsWindow().Show(this));
            Add("Attestazioni riesame non valide", x.InvalidPeriodicReviewAttestations, "Critica", "Verifica e rigenera l'attestazione del riesame.", () => new SupplierRmaCapaGovernanceReviewsWindow().Show(this));
            Add("Conservazioni riesame non valide", x.InvalidPeriodicReviewRetentions, "Critica", "Verifica la copia conservata e la relativa impronta.", () => new SupplierRmaCapaGovernanceReviewRetentionRegistryWindow().Show(this));
            Add("Riesami fascicolo in scadenza", x.ReviewsDue, "Avviso", "Pianifica il riesame prima della scadenza.", () => new SupplierRmaCapaDossierRegistryWindow().Show(this));
            Add("Conservazioni fascicolo in scadenza", x.RetentionDue, "Avviso", "Pianifica il rinnovo della conservazione.", () => new SupplierRmaCapaAttestationExportHistoryWindow().Show(this));
            Add("Conservazioni riesame da gestire", x.PeriodicReviewRetentionsDue, "Avviso", "Controlla scadenza e rinnovo della conservazione.", () => new SupplierRmaCapaGovernanceReviewRetentionRegistryWindow().Show(this));
            _summary.Text = x.CriticalCount == 0 ? "Nessuna criticita bloccante rilevata." : $"{x.CriticalCount} criticita richiedono attenzione.";
            _summary.Foreground = UiTokens.Brush(x.CriticalCount == 0 ? UiTokens.Success : UiTokens.Danger);
            if (_rows.Children.Count == 0) _rows.Children.Add(new Border { Padding = new Thickness(18), Background = UiTokens.Brush(UiTokens.SurfaceAlt), Child = new TextBlock { Text = "Nessuna criticita o scadenza da gestire.", Foreground = UiTokens.Brush(UiTokens.Success) } });
        }
        catch (Exception ex) { _summary.Text = $"Registro non disponibile: {ex.Message}"; _summary.Foreground = UiTokens.Brush(UiTokens.Danger); }
    }

    private void Add(string title, int count, string severity, string guidance, Action open)
    {
        if (count <= 0) return;
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("90,*,150"), Margin = new Thickness(0) };
        var number = new TextBlock { Text = count.ToString(), FontSize = 24, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(severity == "Critica" ? UiTokens.Danger : UiTokens.Warning), VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(number, 0); grid.Children.Add(number);
        var detail = new StackPanel { Spacing = 3, Children = { new TextBlock { Text = title, FontSize = 16, FontWeight = FontWeight.SemiBold }, new TextBlock { Text = $"{severity} - {guidance}", TextWrapping = TextWrapping.Wrap, Foreground = UiTokens.Brush(UiTokens.TextSecondary) } } };
        Grid.SetColumn(detail, 1); grid.Children.Add(detail);
        var button = SupplierRmaCorrectiveActionsWindow.Button("Apri registro", open, true);
        Grid.SetColumn(button, 2); grid.Children.Add(button);
        _rows.Children.Add(new Border { Padding = new Thickness(14, 11), BorderBrush = UiTokens.Brush(UiTokens.Border), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8), Background = UiTokens.Brush(UiTokens.Surface), Child = grid });
    }
}
