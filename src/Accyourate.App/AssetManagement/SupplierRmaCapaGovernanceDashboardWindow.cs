using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.DesignSystem;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class SupplierRmaCapaGovernanceDashboardWindow : Window
{
    private readonly SupplierRmaCapaGovernanceDashboardService _service = new();
    private readonly StackPanel _content = new();
    private readonly TextBlock _message = new();
    private SupplierRmaCapaGovernanceSnapshot _snapshot = new();

    public SupplierRmaCapaGovernanceDashboardWindow()
    {
        Title = "Dashboard Governance CAPA"; Width = 1320; Height = 820;
        WindowStartupLocation = WindowStartupLocation.CenterOwner; Content = Build(); Load();
    }

    private Control Build()
    {
        var root = new DockPanel { Margin = new Thickness(24) };
        var head = AxResponsivePageHeader.Create("Dashboard Governance CAPA", "Fascicoli, riesami periodici, attestazioni e conservazione.", SupplierRmaCorrectiveActionsWindow.Button("Criticita", () => new SupplierRmaCapaGovernanceCriticalitiesWindow().Show(this)), SupplierRmaCorrectiveActionsWindow.Button("Riesami governance", () => new SupplierRmaCapaGovernanceReviewsWindow().Show(this)), SupplierRmaCorrectiveActionsWindow.Button("Piano azioni", () => new SupplierRmaCapaGovernanceActionsWindow().Show(this), true), SupplierRmaCorrectiveActionsWindow.Button("Report PDF", Report), SupplierRmaCorrectiveActionsWindow.Button("Aggiorna", Load));
        head.Margin = new Thickness(0, 0, 0, 14); DockPanel.SetDock(head, Dock.Top); root.Children.Add(head);
        DockPanel.SetDock(_message, Dock.Top); root.Children.Add(_message);
        root.Children.Add(new ScrollViewer { Content = _content }); return root;
    }

    private void Load()
    {
        try
        {
            _snapshot = _service.Load(); _content.Children.Clear(); _content.Spacing = 14;
            _content.Children.Add(Section("Riesami periodici Governance", new[]
            {
                Card("Riesami",_snapshot.PeriodicReviews,UiTokens.BrandBlue,OpenReviews),
                Card("In approvazione",_snapshot.PeriodicReviewsPendingApproval,UiTokens.Warning,OpenReviews),
                Card("Approvati",_snapshot.PeriodicReviewsApproved,UiTokens.Success,OpenReviews),
                Card("Scaduti",_snapshot.PeriodicReviewsOverdue,UiTokens.Danger,OpenReviews),
                Card("Attestazioni valide",_snapshot.ValidPeriodicReviewAttestations,UiTokens.Success,OpenReviews),
                Card("Attestazioni non valide",_snapshot.InvalidPeriodicReviewAttestations,UiTokens.Danger,OpenReviews),
                Card("Conservazioni valide",_snapshot.ValidPeriodicReviewRetentions,UiTokens.Success,OpenRetention),
                Card("Conservazioni da gestire",_snapshot.PeriodicReviewRetentionsDue+_snapshot.InvalidPeriodicReviewRetentions,UiTokens.Warning,OpenRetention)
            }));
            _content.Children.Add(Section("Fascicoli e riesami documentali", new[]
            {
                Card("Fascicoli",_snapshot.Dossiers,UiTokens.BrandBlue,OpenDossiers),Card("Attivi",_snapshot.ActiveDossiers,UiTokens.Success,OpenDossiers),
                Card("Archiviati",_snapshot.ArchivedDossiers,UiTokens.TextSecondary,OpenDossiers),Card("Approvati",_snapshot.ApprovedDossiers,UiTokens.Success,OpenDossiers),
                Card("Documenti mancanti",_snapshot.MissingDocuments,UiTokens.Danger,OpenDossiers),Card("Riesami in scadenza",_snapshot.ReviewsDue,UiTokens.Warning,OpenDossiers),
                Card("Riesami scaduti",_snapshot.ReviewsOverdue,UiTokens.Danger,OpenDossiers)
            }));
            _content.Children.Add(Section("Attestazioni fascicoli", new[]
            {
                Card("Totali",_snapshot.Attestations,UiTokens.BrandBlue,OpenAttestations),Card("Valide",_snapshot.ValidAttestations,UiTokens.Success,OpenAttestations),
                Card("Non valide",_snapshot.InvalidAttestations,UiTokens.Danger,OpenAttestations),Card("Archivi mancanti",_snapshot.MissingAttestationArchives,UiTokens.Danger,OpenAttestations)
            }));
            _content.Children.Add(Section("Esportazioni e conservazione fascicoli", new[]
            {
                Card("Esportazioni",_snapshot.Exports,UiTokens.BrandBlue,OpenExports),Card("Integre",_snapshot.ValidExports,UiTokens.Success,OpenExports),
                Card("Modificate",_snapshot.InvalidExports,UiTokens.Danger,OpenExports),Card("File mancanti",_snapshot.MissingExports,UiTokens.Danger,OpenExports),
                Card("In scadenza",_snapshot.RetentionDue,UiTokens.Warning,OpenExports),Card("Scadute",_snapshot.RetentionOverdue,UiTokens.Danger,OpenExports)
            }));
            _message.Text = _snapshot.CriticalCount == 0 ? "Nessuna criticita di governance rilevata." : $"{_snapshot.CriticalCount} criticita richiedono attenzione.";
            _message.Foreground = UiTokens.Brush(_snapshot.CriticalCount == 0 ? UiTokens.Success : UiTokens.Danger);
            _message.FontWeight = FontWeight.SemiBold; _message.Margin = new Thickness(0, 0, 0, 12);
        }
        catch (Exception exception) { _message.Text = $"Dashboard non disponibile: {exception.Message}"; _message.Foreground = UiTokens.Brush(UiTokens.Danger); }
    }

    private static Control Section(string title, IEnumerable<Control> cards) { var root = new StackPanel { Spacing = 8 }; root.Children.Add(new TextBlock { Text = title, FontSize = 20, FontWeight = FontWeight.Bold }); var wrap = new WrapPanel { Orientation = Orientation.Horizontal }; foreach (var card in cards) wrap.Children.Add(card); root.Children.Add(wrap); return root; }
    private static Control Card(string label, int value, string color, Action action) { var button = new Button { Width = 205, Height = 96, Margin = new Thickness(0, 0, 10, 10), HorizontalContentAlignment = HorizontalAlignment.Stretch, Content = new StackPanel { Spacing = 3, Children = { new TextBlock { Text = value.ToString(), FontSize = 28, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(color) }, new TextBlock { Text = label, FontSize = 15, TextWrapping = TextWrapping.Wrap } } } }; button.Click += (_, _) => action(); return button; }
    private void Report() { try { var path = _service.ExportPdf(_snapshot); _message.Text = $"Report creato: {Path.GetFileName(path)}"; _message.Foreground = UiTokens.Brush(UiTokens.Success); Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true }); } catch (Exception exception) { _message.Text = $"Errore report: {exception.Message}"; _message.Foreground = UiTokens.Brush(UiTokens.Danger); } }
    private void OpenReviews() => new SupplierRmaCapaGovernanceReviewsWindow().Show(this);
    private void OpenRetention() => new SupplierRmaCapaGovernanceReviewRetentionRegistryWindow().Show(this);
    private void OpenDossiers() => new SupplierRmaCapaDossierRegistryWindow().Show(this);
    private void OpenAttestations() => new SupplierRmaCapaAttestationRegistryWindow().Show(this);
    private void OpenExports() => new SupplierRmaCapaAttestationExportHistoryWindow().Show(this);
    private static void Add(Grid grid, Control control, int column) { Grid.SetColumn(control, column); grid.Children.Add(control); }
}
