using System.Diagnostics;
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
    private readonly SupplierRmaCapaGovernanceActionService _actions = new();
    private readonly SupplierRmaCapaGovernanceCriticalityReportService _report = new();
    private readonly StackPanel _rows = new();
    private readonly TextBlock _summary = new();
    private readonly HashSet<string> _current = new(StringComparer.OrdinalIgnoreCase);

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
        var commands = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        commands.Children.Add(SupplierRmaCorrectiveActionsWindow.Button("Report audit PDF", ExportReport));
        commands.Children.Add(SupplierRmaCorrectiveActionsWindow.Button("Aggiorna", Load, true));
        Grid.SetColumn(commands, 1); header.Children.Add(commands);
        DockPanel.SetDock(header, Dock.Top); root.Children.Add(header);
        _summary.Margin = new Thickness(0, 0, 0, 12); _summary.FontWeight = FontWeight.SemiBold;
        DockPanel.SetDock(_summary, Dock.Top); root.Children.Add(_summary);
        root.Children.Add(new ScrollViewer { Content = _rows, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto });
        return root;
    }

    private void ExportReport()
    {
        try { var path = _report.Export(); _summary.Text = $"Report creato: {Path.GetFileName(path)}"; _summary.Foreground = UiTokens.Brush(UiTokens.Success); Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true }); }
        catch (Exception ex) { _summary.Text = $"Report non creato: {ex.Message}"; _summary.Foreground = UiTokens.Brush(UiTokens.Danger); }
    }

    private void Load()
    {
        try
        {
            var x = _service.Load();
            _rows.Children.Clear(); _rows.Spacing = 8; _current.Clear();
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
            AddVerifiedClosures();
            _summary.Text = x.CriticalCount == 0 ? "Nessuna criticita bloccante rilevata." : $"{x.CriticalCount} criticita richiedono attenzione.";
            _summary.Foreground = UiTokens.Brush(x.CriticalCount == 0 ? UiTokens.Success : UiTokens.Danger);
            if (_rows.Children.Count == 0) _rows.Children.Add(new Border { Padding = new Thickness(18), Background = UiTokens.Brush(UiTokens.SurfaceAlt), Child = new TextBlock { Text = "Nessuna criticita o scadenza da gestire.", Foreground = UiTokens.Brush(UiTokens.Success) } });
        }
        catch (Exception ex) { _summary.Text = $"Registro non disponibile: {ex.Message}"; _summary.Foreground = UiTokens.Brush(UiTokens.Danger); }
    }

    private void Add(string title, int count, string severity, string guidance, Action open)
    {
        if (count <= 0) return;
        _current.Add(title);
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("90,*,150,160"), Margin = new Thickness(0) };
        var number = new TextBlock { Text = count.ToString(), FontSize = 24, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(severity == "Critica" ? UiTokens.Danger : UiTokens.Warning), VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(number, 0); grid.Children.Add(number);
        var detail = new StackPanel { Spacing = 3, Children = { new TextBlock { Text = title, FontSize = 16, FontWeight = FontWeight.SemiBold }, new TextBlock { Text = $"{severity} - {guidance}", TextWrapping = TextWrapping.Wrap, Foreground = UiTokens.Brush(UiTokens.TextSecondary) } } };
        Grid.SetColumn(detail, 1); grid.Children.Add(detail);
        var button = SupplierRmaCorrectiveActionsWindow.Button("Apri registro", open, true);
        Grid.SetColumn(button, 2); grid.Children.Add(button);
        var related = _actions.GetAll().Where(x => x.SourceType == "Criticita Governance CAPA" && x.SourceReference == title).OrderByDescending(x => x.Id).ToList();
        var active = related.FirstOrDefault(x => x.Status != "Completata");
        var completed = related.FirstOrDefault(x => x.Status == "Completata");
        var label = active is not null ? "Gia in carico" : completed is not null ? "Verifica chiusura" : "Prendi in carico";
        Action command = completed is not null && active is null
            ? () => new SupplierRmaCapaCriticalityVerificationDialog(completed, _actions, Load).Show(this)
            : () => new SupplierRmaCapaCriticalityAssignmentDialog(title, severity, guidance, _actions, Load).Show(this);
        var take = SupplierRmaCorrectiveActionsWindow.Button(label, command);
        take.IsEnabled = active is null;
        Grid.SetColumn(take, 3); grid.Children.Add(take);
        _rows.Children.Add(new Border { Padding = new Thickness(14, 11), BorderBrush = UiTokens.Brush(UiTokens.Border), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8), Background = UiTokens.Brush(UiTokens.Surface), Child = grid });
    }

    private void AddVerifiedClosures()
    {
        var completed = _actions.GetAll().Where(x => x.SourceType == "Criticita Governance CAPA" && x.Status == "Completata" && !_current.Contains(x.SourceReference)).GroupBy(x => x.SourceReference).Select(x => x.OrderByDescending(y => y.Id).First()).ToList();
        foreach (var item in completed)
        {
            var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("90,*,160"), Margin = new Thickness(0) };
            var mark = new TextBlock { Text = "OK", FontSize = 20, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.Success), VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(mark, 0); grid.Children.Add(mark);
            var detail = new StackPanel { Spacing = 3, Children = { new TextBlock { Text = item.SourceReference, FontSize = 16, FontWeight = FontWeight.SemiBold }, new TextBlock { Text = "Chiusura verificata: la criticita non e piu rilevata dal controllo.", TextWrapping = TextWrapping.Wrap, Foreground = UiTokens.Brush(UiTokens.Success) } } };
            Grid.SetColumn(detail, 1); grid.Children.Add(detail);
            var history = SupplierRmaCorrectiveActionsWindow.Button("Storico", () => new SupplierRmaCapaGovernanceActionHistoryWindow(item, _actions).Show(this));
            Grid.SetColumn(history, 2); grid.Children.Add(history);
            _rows.Children.Add(new Border { Padding = new Thickness(14, 11), BorderBrush = UiTokens.Brush(UiTokens.Success), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8), Background = UiTokens.Brush(UiTokens.Surface), Child = grid });
        }
    }
}

public sealed class SupplierRmaCapaCriticalityVerificationDialog : Window
{
    private readonly SupplierRmaCapaGovernanceAction _item;
    private readonly SupplierRmaCapaGovernanceActionService _service;
    private readonly Action _saved;
    private readonly TextBox _notes = new() { AcceptsReturn = true, MinHeight = 100, TextWrapping = TextWrapping.Wrap, Text = "La criticita risulta ancora presente dopo il nuovo controllo." };
    private readonly TextBlock _message = new() { TextWrapping = TextWrapping.Wrap };

    public SupplierRmaCapaCriticalityVerificationDialog(SupplierRmaCapaGovernanceAction item, SupplierRmaCapaGovernanceActionService service, Action saved)
    {
        _item = item; _service = service; _saved = saved;
        Title = "Verifica chiusura criticita"; Width = 620; Height = 440; WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var root = new StackPanel { Margin = new Thickness(24), Spacing = 10, Children = { new TextBlock { Text = "Verifica non superata", FontSize = 26, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.Danger) }, new TextBlock { Text = item.Title, FontSize = 18, FontWeight = FontWeight.SemiBold, TextWrapping = TextWrapping.Wrap }, new TextBlock { Text = "L'azione e stata completata, ma la criticita e ancora rilevata. Riapri l'azione per registrare ulteriori interventi ed evidenze.", TextWrapping = TextWrapping.Wrap }, new TextBlock { Text = "Esito della verifica", FontWeight = FontWeight.SemiBold }, _notes, _message } };
        root.Children.Add(SupplierRmaCorrectiveActionsWindow.Button("Riapri azione", Reopen, true)); Content = root;
    }

    private void Reopen()
    {
        try { _service.ReopenAfterFailedVerification(_item.Id, _notes.Text ?? "", Environment.UserName); _saved(); Close(); }
        catch (Exception ex) { _message.Text = ex.Message; _message.Foreground = UiTokens.Brush(UiTokens.Danger); }
    }
}

public sealed class SupplierRmaCapaCriticalityAssignmentDialog : Window
{
    private readonly string _title;
    private readonly string _severity;
    private readonly SupplierRmaCapaGovernanceActionService _service;
    private readonly Action _saved;
    private readonly TextBox _owner = new() { Text = Environment.UserName };
    private readonly ComboBox _priority = new() { ItemsSource = new[] { "Bassa", "Media", "Alta", "Critica" }, SelectedItem = "Alta" };
    private readonly TextBox _due = new() { Text = DateTime.Today.AddDays(14).ToString("yyyy-MM-dd") };
    private readonly TextBox _description = new() { AcceptsReturn = true, MinHeight = 100, TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _message = new() { TextWrapping = TextWrapping.Wrap };

    public SupplierRmaCapaCriticalityAssignmentDialog(string title, string severity, string guidance, SupplierRmaCapaGovernanceActionService service, Action saved)
    {
        _title = title; _severity = severity; _service = service; _saved = saved;
        _priority.SelectedItem = severity == "Critica" ? "Critica" : "Alta";
        _due.Text = DateTime.Today.AddDays(severity == "Critica" ? 7 : 14).ToString("yyyy-MM-dd");
        _description.Text = guidance;
        Title = "Presa in carico criticita"; Width = 620; Height = 570; WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var root = new StackPanel { Margin = new Thickness(24), Spacing = 9, Children = { new TextBlock { Text = "Prendi in carico", FontSize = 26, FontWeight = FontWeight.Bold }, new TextBlock { Text = title, FontSize = 18, FontWeight = FontWeight.SemiBold, TextWrapping = TextWrapping.Wrap }, new TextBlock { Text = $"Gravita rilevata: {severity}", Foreground = UiTokens.Brush(severity == "Critica" ? UiTokens.Danger : UiTokens.Warning) } } };
        Field(root, "Responsabile", _owner); Field(root, "Priorita", _priority); Field(root, "Scadenza (AAAA-MM-GG)", _due); Field(root, "Descrizione e azione richiesta", _description);
        root.Children.Add(_message); root.Children.Add(SupplierRmaCorrectiveActionsWindow.Button("Crea azione CAPA", Save, true));
        Content = new ScrollViewer { Content = root };
    }

    private void Save()
    {
        try
        {
            if (_service.GetAll().Any(x => x.Status != "Completata" && x.SourceType == "Criticita Governance CAPA" && x.SourceReference == _title)) throw new InvalidOperationException("La criticita e gia associata a un'azione aperta.");
            _service.Create("Criticita Governance CAPA", _title, _title, _description.Text ?? "", _owner.Text ?? "", _priority.SelectedItem?.ToString() ?? "Alta", _due.Text ?? "", Environment.UserName);
            _saved(); Close();
        }
        catch (Exception ex) { _message.Text = ex.Message; _message.Foreground = UiTokens.Brush(UiTokens.Danger); }
    }

    private static void Field(Panel root, string label, Control control)
    {
        root.Children.Add(new TextBlock { Text = label, FontWeight = FontWeight.SemiBold }); root.Children.Add(control);
    }
}
