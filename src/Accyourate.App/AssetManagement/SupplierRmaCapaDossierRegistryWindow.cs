using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class SupplierRmaCapaDossierRegistryWindow:Window
{
    private readonly SupplierRmaCapaDossierRegistryService _service=new();
    private readonly StackPanel _rows=new();
    private readonly TextBox _search=new(){Watermark="Cerca pratica, azione, operatore o archivio..."};
    private readonly ComboBox _outcome=new(){ItemsSource=new[]{"Tutti gli esiti","Creato","Integro","Non conforme"},SelectedIndex=0,MinWidth=170};
    private readonly TextBlock _summary=new();
    public SupplierRmaCapaDossierRegistryWindow()
    {
        Title="Registro fascicoli CAPA";Width=1400;Height=780;MinWidth=980;MinHeight=580;WindowStartupLocation=WindowStartupLocation.CenterOwner;Content=Build();
        _search.TextChanged+=(_,_)=>LoadData();_outcome.SelectionChanged+=(_,_)=>LoadData();LoadData();
    }
    private Control Build()
    {
        var root=new DockPanel{Margin=new Thickness(24)};
        var head=new Grid{ColumnDefinitions=new ColumnDefinitions("*,Auto"),Margin=new Thickness(0,0,0,14)};
        var title=new StackPanel{Spacing=3,Children={new TextBlock{Text="Registro fascicoli CAPA",FontSize=28,FontWeight=FontWeight.Bold},new TextBlock{Text="Esportazioni, verifiche di integrita, anomalie e verbali.",Foreground=UiTokens.Brush(UiTokens.TextSecondary)}}};Grid.SetColumn(title,0);head.Children.Add(title);
        var refresh=SupplierRmaCorrectiveActionsWindow.Button("Aggiorna",LoadData,true);Grid.SetColumn(refresh,1);head.Children.Add(refresh);DockPanel.SetDock(head,Dock.Top);root.Children.Add(head);
        var filters=new Grid{ColumnDefinitions=new ColumnDefinitions("*,170"),Margin=new Thickness(0,0,0,8)};Add(filters,_search,0);Add(filters,_outcome,1);DockPanel.SetDock(filters,Dock.Top);root.Children.Add(filters);DockPanel.SetDock(_summary,Dock.Top);root.Children.Add(_summary);
        root.Children.Add(new ScrollViewer{Content=_rows,HorizontalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,VerticalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto});return root;
    }
    private void LoadData()
    {
        try
        {
            var all=_service.GetAll();var query=(_search.Text??"").Trim();var outcome=_outcome.SelectedItem?.ToString()??"Tutti gli esiti";
            var values=all.Where(x=>outcome=="Tutti gli esiti"||x.Outcome==outcome).Where(x=>query.Length==0||$"{x.CaseNumber} {x.ActionTitle} {x.CreatedBy} {x.ArchivePath}".Contains(query,StringComparison.OrdinalIgnoreCase)).ToList();
            _rows.Children.Clear();_rows.MinWidth=1250;_rows.Children.Add(Header());for(var i=0;i<values.Count;i++)_rows.Children.Add(Row(values[i],i));
            _summary.Text=$"{values.Count} registrazioni · {all.Count(x=>x.Outcome=="Integro")} integre · {all.Count(x=>x.Outcome=="Non conforme")} non conformi";_summary.Foreground=UiTokens.Brush(UiTokens.TextSecondary);_summary.Margin=new Thickness(0,0,0,8);
        }
        catch(Exception ex){_summary.Text=$"Registro non caricato: {ex.Message}";_summary.Foreground=UiTokens.Brush(UiTokens.Danger);}
    }
    private Control Header(){var grid=GridRow();var labels=new[]{"Data","Pratica","Azione","Operazione","Esito","File","Anomalie","Operatore","Documenti"};for(var i=0;i<labels.Length;i++)Cell(grid,labels[i],i,true);return new Border{Padding=new Thickness(9),Background=UiTokens.Brush(UiTokens.SurfaceAlt),Child=grid};}
    private Control Row(SupplierRmaCapaDossierRegistryRecord item,int index)
    {
        var grid=GridRow();Cell(grid,Date(item.CreatedAt),0);Cell(grid,item.CaseNumber,1,true);Cell(grid,item.ActionTitle,2);Cell(grid,item.Operation,3);Add(grid,Badge(item.Outcome),4);Cell(grid,item.FileCount.ToString(),5);Cell(grid,item.AnomalyCount.ToString(),6,item.AnomalyCount>0);Cell(grid,item.CreatedBy,7);
        var actions=new StackPanel{Orientation=Orientation.Horizontal,Spacing=6};var archive=SupplierRmaCorrectiveActionsWindow.Button("Archivio",()=>Open(item.ArchivePath));archive.IsEnabled=item.ArchiveAvailable;actions.Children.Add(archive);var report=SupplierRmaCorrectiveActionsWindow.Button("Verbale",()=>Open(item.ReportPath));report.IsEnabled=item.ReportAvailable;actions.Children.Add(report);Add(grid,actions,8);
        return new Border{Padding=new Thickness(9,7),Background=UiTokens.Brush(index%2==0?UiTokens.Surface:UiTokens.SurfaceAlt),BorderBrush=UiTokens.Brush(UiTokens.Border),BorderThickness=new Thickness(0,0,0,1),Child=grid};
    }
    private static Grid GridRow()=>new(){ColumnDefinitions=new ColumnDefinitions("140,145,*,105,125,65,80,120,170")};
    private static Border Badge(string value){var color=value switch{"Integro"=>UiTokens.Success,"Non conforme"=>UiTokens.Danger,_=>UiTokens.BrandBlue};return new(){BorderBrush=UiTokens.Brush(color),BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(8),Padding=new Thickness(7,3),Child=new TextBlock{Text=value,Foreground=UiTokens.Brush(color),HorizontalAlignment=HorizontalAlignment.Center,FontWeight=FontWeight.SemiBold}};}
    private static void Open(string path){if(File.Exists(path))Process.Start(new ProcessStartInfo{FileName=path,UseShellExecute=true});}
    private static string Date(string value)=>DateTime.TryParse(value,out var date)?date.ToString("dd/MM/yyyy HH:mm"):value;
    private static void Cell(Grid grid,string value,int column,bool strong=false)=>Add(grid,new TextBlock{Text=string.IsNullOrWhiteSpace(value)?"—":value,FontWeight=strong?FontWeight.SemiBold:FontWeight.Normal,TextTrimming=TextTrimming.CharacterEllipsis,VerticalAlignment=VerticalAlignment.Center},column);
    private static void Add(Grid grid,Control control,int column){control.Margin=new Thickness(0,0,8,0);Grid.SetColumn(control,column);grid.Children.Add(control);}
}
