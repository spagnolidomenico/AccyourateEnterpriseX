using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Services;
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
        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Margin = new Thickness(0, 0, 0, 12) };
        var title = new StackPanel { Children = { new TextBlock { Text = "Storico esportazioni attestazioni", FontSize = 28, FontWeight = FontWeight.Bold }, new TextBlock { Text = "Integrita, conservazione, proroghe e archiviazione controllata.", Foreground = UiTokens.Brush(UiTokens.TextSecondary) } } };
        Grid.SetColumn(title, 0); header.Children.Add(title);
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, VerticalAlignment = VerticalAlignment.Center };
        actions.Children.Add(new TextBlock { Text = "Conservazione (giorni)", VerticalAlignment = VerticalAlignment.Center }); actions.Children.Add(_retentionDays);
        actions.Children.Add(SupplierRmaCorrectiveActionsWindow.Button("Audit conservazione",()=>new SupplierRmaCapaAttestationRetentionAuditWindow().Show(this))); actions.Children.Add(SupplierRmaCorrectiveActionsWindow.Button("Salva periodo", SaveRetention)); actions.Children.Add(SupplierRmaCorrectiveActionsWindow.Button("Verifica e notifica", Verify, true));
        Grid.SetColumn(actions, 1); header.Children.Add(actions); DockPanel.SetDock(header, Dock.Top); root.Children.Add(header);
        DockPanel.SetDock(_summary, Dock.Top); root.Children.Add(_summary);
        root.Children.Add(new ScrollViewer { Content = _rows, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto }); return root;
    }

    private void Verify(){var count=_service.PublishRetentionNotifications();Load();if(count>0)Message($"Pubblicate {count} nuove notifiche di conservazione.",false);}
    private void SaveRetention(){try{if(!int.TryParse(_retentionDays.Text,out var days))throw new InvalidOperationException("Inserisci un numero di giorni valido.");_service.SetRetentionDays(days);Message("Periodo di conservazione salvato. Sarà applicato alle nuove esportazioni.",false);}catch(Exception ex){Message(ex.Message,true);}}
    private void Extend(int id){try{_service.Extend(id);Load();Message("Conservazione prorogata di 365 giorni.",false);}catch(Exception ex){Message(ex.Message,true);}}
    private void Archive(int id){try{var path=_service.Archive(id);Load();Message($"Copia di conservazione creata: {Path.GetFileName(path)}",false);Open(path);}catch(Exception ex){Message(ex.Message,true);}}

    private void Load()
    {
        var values = _service.GetExports(); _rows.Children.Clear(); _rows.MinWidth = 1430;
        _rows.Children.Add(Row(new SupplierRmaCapaAttestationExportRecord { Format="Formato", ExportedAt="Data", ExportedBy="Operatore", FilterDescription="Filtri" }, true));
        foreach (var value in values) _rows.Children.Add(Row(value, false));
        _summary.Text = $"{values.Count} esportazioni · {values.Count(x => x.IsValid)} integre · {values.Count(x => x.RetentionStatus == "In scadenza")} in scadenza · {values.Count(x => x.RetentionStatus == "Scaduta")} scadute";
        _summary.Foreground = UiTokens.Brush(values.Any(x => x.RetentionStatus is "In scadenza" or "Scaduta") ? UiTokens.Danger : UiTokens.TextSecondary); _summary.Margin = new Thickness(0, 0, 0, 10);
    }

    private Control Row(SupplierRmaCapaAttestationExportRecord item, bool header)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("75,135,120,210,70,105,125,120,130,330") };
        Cell(grid,item.Format,0,header);Cell(grid,header?item.ExportedAt:Date(item.ExportedAt),1,header);Cell(grid,item.ExportedBy,2,header);Cell(grid,item.FilterDescription,3,header);
        Cell(grid,header?"Record":item.RecordCount.ToString(),4,header);Cell(grid,header?"Integrita":item.IntegrityStatus,5,true);Cell(grid,header?"Scadenza":DateOnly(item.RetainUntil),6,header);Cell(grid,header?"Conservazione":item.RetentionStatus,7,true);Cell(grid,header?"Residui":$"{item.DaysRemaining} gg",8,header);
        if(!header){var actions=new StackPanel{Orientation=Orientation.Horizontal,Spacing=5};var open=SupplierRmaCorrectiveActionsWindow.Button("Apri",()=>Open(item.FilePath));open.IsEnabled=item.FileAvailable;actions.Children.Add(open);actions.Children.Add(SupplierRmaCorrectiveActionsWindow.Button("Proroga",()=>Extend(item.Id)));var archive=SupplierRmaCorrectiveActionsWindow.Button("Archivia",()=>Archive(item.Id));archive.IsEnabled=item.FileAvailable&&string.IsNullOrWhiteSpace(item.ArchivedAt);actions.Children.Add(archive);var copy=SupplierRmaCorrectiveActionsWindow.Button("Copia",()=>Open(item.ArchiveCopyPath));copy.IsEnabled=File.Exists(item.ArchiveCopyPath);actions.Children.Add(copy);Add(grid,actions,9);}else Cell(grid,"Azioni",9,true);
        return new Border{Padding=new Thickness(9),Background=UiTokens.Brush(header?UiTokens.SurfaceAlt:UiTokens.Surface),BorderBrush=UiTokens.Brush(UiTokens.Border),BorderThickness=new Thickness(0,0,0,1),Child=grid};
    }

    private void Message(string text,bool error){_summary.Text=text;_summary.Foreground=UiTokens.Brush(error?UiTokens.Danger:UiTokens.Success);}
    private static void Open(string path){if(File.Exists(path))Process.Start(new ProcessStartInfo{FileName=path,UseShellExecute=true});}
    private static string Date(string value)=>DateTime.TryParse(value,out var date)?date.ToString("dd/MM/yyyy HH:mm"):value;
    private static string DateOnly(string value)=>DateTime.TryParse(value,out var date)?date.ToString("dd/MM/yyyy"):"—";
    private static void Cell(Grid grid,string text,int column,bool bold=false)=>Add(grid,new TextBlock{Text=text,FontWeight=bold?FontWeight.Bold:FontWeight.Normal,TextTrimming=TextTrimming.CharacterEllipsis},column);
    private static void Add(Grid grid,Control control,int column){control.Margin=new Thickness(0,0,8,0);Grid.SetColumn(control,column);grid.Children.Add(control);}
}
