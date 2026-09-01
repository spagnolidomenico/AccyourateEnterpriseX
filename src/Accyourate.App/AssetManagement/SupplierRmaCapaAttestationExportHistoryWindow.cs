using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.DesignSystem;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class SupplierRmaCapaAttestationExportHistoryWindow : Window
{
    private readonly SupplierRmaCapaAttestationExportService _service = new();
    private readonly StackPanel _rows = new();
    private readonly TextBlock _summary = new();
    private readonly TextBox _retentionDays = new() { Width = 90, Watermark = "Giorni" };

    public SupplierRmaCapaAttestationExportHistoryWindow()
    {
        Title = "Storico esportazioni attestazioni CAPA"; Width = 1520; Height = 780; WindowStartupLocation = WindowStartupLocation.CenterOwner;
        _retentionDays.Text = _service.GetRetentionDays().ToString(); Content = Build(); Load();
    }

    private Control Build()
    {
        var root = new DockPanel { Margin = new Thickness(24) };
        var retentionLabel = new TextBlock { Text = "Conservazione (giorni)", VerticalAlignment = VerticalAlignment.Center };
        var header = AxResponsivePageHeader.Create("Storico esportazioni attestazioni", "Integrita, conservazione, proroghe e archiviazione controllata.", retentionLabel, _retentionDays, SupplierRmaCorrectiveActionsWindow.Button("Audit conservazione",()=>new SupplierRmaCapaAttestationRetentionAuditWindow().Show(this)), SupplierRmaCorrectiveActionsWindow.Button("Salva periodo", SaveRetention), SupplierRmaCorrectiveActionsWindow.Button("Verifica e notifica", Verify, true));
        header.Margin = new Thickness(0, 0, 0, 12); DockPanel.SetDock(header, Dock.Top); root.Children.Add(header);
        DockPanel.SetDock(_summary, Dock.Top); root.Children.Add(_summary);
        root.Children.Add(new ScrollViewer { Content = _rows, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto }); return root;
    }

    private void Verify(){var count=_service.PublishRetentionNotifications();Load();if(count>0)Message($"Pubblicate {count} nuove notifiche di conservazione.",false);}
    private void SaveRetention(){try{if(!int.TryParse(_retentionDays.Text,out var days))throw new InvalidOperationException("Inserisci un numero di giorni valido.");_service.SetRetentionDays(days);Message("Periodo di conservazione salvato. Sarà applicato alle nuove esportazioni.",false);}catch(Exception ex){Message(ex.Message,true);}}
    private void Extend(int id){try{_service.Extend(id);Load();Message("Conservazione prorogata di 365 giorni.",false);}catch(Exception ex){Message(ex.Message,true);}}
    private void Archive(int id){try{var path=_service.Archive(id);Load();Message($"Copia di conservazione creata: {Path.GetFileName(path)}",false);Open(path);}catch(Exception ex){Message(ex.Message,true);}}

    private void Load()
    {
        var values = _service.GetExports(); _rows.Children.Clear(); _rows.MinWidth = 0; _rows.Spacing = 8;
        foreach (var value in values) _rows.Children.Add(Row(value));
        _summary.Text = $"{values.Count} esportazioni · {values.Count(x => x.IsValid)} integre · {values.Count(x => x.RetentionStatus == "In scadenza")} in scadenza · {values.Count(x => x.RetentionStatus == "Scaduta")} scadute";
        _summary.Foreground = UiTokens.Brush(values.Any(x => x.RetentionStatus is "In scadenza" or "Scaduta") ? UiTokens.Danger : UiTokens.TextSecondary); _summary.Margin = new Thickness(0, 0, 0, 10);
    }

    private Control Row(SupplierRmaCapaAttestationExportRecord item)
    {
        var open=SupplierRmaCorrectiveActionsWindow.Button("Apri",()=>Open(item.FilePath));open.IsEnabled=item.FileAvailable;
        var extend=SupplierRmaCorrectiveActionsWindow.Button("Proroga",()=>Extend(item.Id));
        var archive=SupplierRmaCorrectiveActionsWindow.Button("Archivia",()=>Archive(item.Id));archive.IsEnabled=item.FileAvailable&&string.IsNullOrWhiteSpace(item.ArchivedAt);
        var copy=SupplierRmaCorrectiveActionsWindow.Button("Copia",()=>Open(item.ArchiveCopyPath));copy.IsEnabled=File.Exists(item.ArchiveCopyPath);
        return AxResponsiveRecordCard.Create($"{item.Format} · {Date(item.ExportedAt)}",new[]{new AxResponsiveRecordField("Operatore",item.ExportedBy,160),new AxResponsiveRecordField("Filtri",item.FilterDescription,240),new AxResponsiveRecordField("Record",item.RecordCount.ToString(),100),new AxResponsiveRecordField("Integrita",item.IntegrityStatus,140,item.IsValid?UiTokens.Success:UiTokens.Danger),new AxResponsiveRecordField("Scadenza",DateOnly(item.RetainUntil),140),new AxResponsiveRecordField("Conservazione",item.RetentionStatus,160),new AxResponsiveRecordField("Residui",$"{item.DaysRemaining} gg",110)},open,extend,archive,copy);
    }

    private void Message(string text,bool error){_summary.Text=text;_summary.Foreground=UiTokens.Brush(error?UiTokens.Danger:UiTokens.Success);}
    private static void Open(string path){if(File.Exists(path))Process.Start(new ProcessStartInfo{FileName=path,UseShellExecute=true});}
    private static string Date(string value)=>DateTime.TryParse(value,out var date)?date.ToString("dd/MM/yyyy HH:mm"):value;
    private static string DateOnly(string value)=>DateTime.TryParse(value,out var date)?date.ToString("dd/MM/yyyy"):"—";
    private static void Cell(Grid grid,string text,int column,bool bold=false)=>Add(grid,new TextBlock{Text=text,FontWeight=bold?FontWeight.Bold:FontWeight.Normal,TextTrimming=TextTrimming.CharacterEllipsis},column);
    private static void Add(Grid grid,Control control,int column){control.Margin=new Thickness(0,0,8,0);Grid.SetColumn(control,column);grid.Children.Add(control);}
}
