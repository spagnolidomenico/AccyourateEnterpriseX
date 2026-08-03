using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Models;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class SparePartQuarantineWindow : Window
{
    private readonly SparePartQuarantineRepository _repository=new();
    private readonly SparePartsInventoryRepository _inventory=new();
    private readonly SparePartLocationsRepository _locations=new();
    private readonly SparePartReturnRepository _returns=new();
    private readonly StackPanel _rows=new();private readonly TextBlock _message=new(),_summary=new();private readonly TextBox _search=new(){Watermark="Cerca pratica, ricambio, stato..."};private readonly ComboBox _status=new();
    public SparePartQuarantineWindow()
    {
        Title="Quarantena ricambi";Width=1360;Height=760;MinWidth=1020;MinHeight=560;WindowStartupLocation=WindowStartupLocation.CenterOwner;
        _status.ItemsSource=new[]{"Tutti gli stati",SparePartQuarantineStatus.Pending,SparePartQuarantineStatus.Repairable,SparePartQuarantineStatus.SupplierReturn,SparePartQuarantineStatus.DisposalApproved,SparePartQuarantineStatus.Reintegrated,SparePartQuarantineStatus.ReturnedToSupplier,SparePartQuarantineStatus.Disposed};_status.SelectedIndex=0;
        _search.TextChanged+=(_,_)=>Load();_status.SelectionChanged+=(_,_)=>Load();Content=Build();Load();
    }
    private Control Build()
    {
        var root=new DockPanel{Margin=new Thickness(24)};
        var title=new Grid{ColumnDefinitions=new ColumnDefinitions("*,Auto"),Margin=new Thickness(0,0,0,12)};
        Add(title,new StackPanel{Spacing=3,Children={new TextBlock{Text="Quarantena ricambi",FontSize=28,FontWeight=FontWeight.Bold},new TextBlock{Text="Valutazione, riparazione, reso al fornitore e smaltimento controllato.",Foreground=UiTokens.Brush(UiTokens.TextSecondary)}}},0);
        Add(title,Button("Pratiche RMA",()=>new SparePartRmaWindow().Show(),true),1);
        DockPanel.SetDock(title,Dock.Top);root.Children.Add(title);
        var filters=new Grid{ColumnDefinitions=new ColumnDefinitions("*,220"),Margin=new Thickness(0,0,0,8)};
        Add(filters,_search,0);Add(filters,_status,1);DockPanel.SetDock(filters,Dock.Top);root.Children.Add(filters);
        _message.Margin=new Thickness(0,0,0,5);DockPanel.SetDock(_message,Dock.Top);root.Children.Add(_message);
        _summary.Foreground=UiTokens.Brush(UiTokens.TextSecondary);_summary.Margin=new Thickness(0,0,0,8);
        DockPanel.SetDock(_summary,Dock.Top);root.Children.Add(_summary);
        root.Children.Add(new ScrollViewer{Content=_rows,HorizontalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,VerticalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto});
        return root;
    }
    private void Load()
    {
        try
        {
            var items=_inventory.GetItems().ToDictionary(x=>x.Id);var locations=_locations.GetLocations().ToDictionary(x=>x.Id);var returns=_returns.GetAll().ToDictionary(x=>x.Id);
            var selected=_status.SelectedItem?.ToString()??"Tutti gli stati";var q=(_search.Text??"").Trim();
            var values=_repository.GetAll().Where(x=>selected=="Tutti gli stati"||x.Status==selected).Where(x=>{items.TryGetValue(x.InventoryItemId,out var item);return q.Length==0||$"{x.CaseNumber} {item?.PartCode} {item?.Description} {x.Status} {x.EvaluationNotes}".Contains(q,StringComparison.OrdinalIgnoreCase);}).ToList();
            _summary.Text=$"{values.Count} pratiche · {values.Where(x=>string.IsNullOrWhiteSpace(x.ClosedAt)).Sum(x=>x.Quantity):N2} unità in gestione · costi stimati EUR {values.Sum(x=>x.EstimatedCost):N2}";
            _rows.Children.Clear();_rows.MinWidth=1270;_rows.Children.Add(Header());for(var i=0;i<values.Count;i++)_rows.Children.Add(Row(values[i],items,locations,returns,i));
        }catch(Exception ex){Status($"Errore quarantena: {ex.Message}",true);}
    }
    private Control Header(){var g=GridRow();foreach(var x in new[]{("Pratica",0),("Ricambio",1),("Quantità",2),("Ubicazione",3),("Condizione",4),("Stato",5),("Costo",6),("Azioni",7)})Text(g,x.Item1,x.Item2,true);return new Border{Background=UiTokens.Brush(UiTokens.SurfaceAlt),Padding=new Thickness(8),Child=g};}
    private Control Row(SparePartQuarantineItem value,IReadOnlyDictionary<int,SparePartInventoryItem> items,IReadOnlyDictionary<int,SparePartWarehouseLocation> locations,IReadOnlyDictionary<int,SparePartReturn> returns,int index)
    {
        items.TryGetValue(value.InventoryItemId,out var item);locations.TryGetValue(value.LocationId,out var location);returns.TryGetValue(value.ReturnId,out var source);var g=GridRow();
        Text(g,value.CaseNumber,0,true);Text(g,item is null?$"#{value.InventoryItemId}":$"{item.PartCode} · {item.Description}",1);Text(g,value.Quantity.ToString("N2"),2,true);Text(g,location?.Code??"—",3);Text(g,value.InitialCondition,4);Add(g,Badge(value.Status),5);Text(g,$"EUR {value.EstimatedCost:N2}",6);
        var actions=new StackPanel{Orientation=Orientation.Horizontal,Spacing=3};
        if(value.Status==SparePartQuarantineStatus.Pending)actions.Children.Add(Button("Valuta",()=>Evaluate(value)));
        if(value.Status==SparePartQuarantineStatus.Repairable)actions.Children.Add(Button("Reintegra",()=>Reintegrate(value),true));
        if(value.Status==SparePartQuarantineStatus.SupplierReturn)actions.Children.Add(Button("Apri RMA",()=>new SparePartRmaWindow().Show(),true));
        if(value.Status==SparePartQuarantineStatus.DisposalApproved)actions.Children.Add(Button("Smaltisci",()=>Run(()=>_repository.Dispose(value.Id,"Smaltimento completato",Environment.UserName),"Smaltimento registrato.")));
        actions.Children.Add(Button("PDF",()=>Pdf(value,item,location,source)));Add(g,actions,7);
        return new Border{Background=UiTokens.Brush(index%2==0?UiTokens.Surface:UiTokens.SurfaceAlt),BorderBrush=UiTokens.Brush(UiTokens.Border),BorderThickness=new Thickness(0,0,0,1),Padding=new Thickness(8,6),Child=g};
    }
    private async void Evaluate(SparePartQuarantineItem value){var owner=TopLevel.GetTopLevel(this) as Window;if(owner is null)return;var result=await new QuarantineEvaluationDialog(value).ShowDialog<SparePartQuarantineItem?>(owner);if(result is null)return;Run(()=>_repository.Evaluate(value.Id,result.Status,result.EstimatedCost,result.EvaluationNotes,Environment.UserName),"Valutazione registrata.");}
    private async void Reintegrate(SparePartQuarantineItem value){var owner=TopLevel.GetTopLevel(this) as Window;if(owner is null)return;var locations=_locations.GetLocations().Where(x=>x.IsActive).ToList();var result=await new QuarantineReintegrationDialog(locations).ShowDialog<(int LocationId,string Notes)?>(owner);if(result is null)return;Run(()=>_repository.Reintegrate(value.Id,result.Value.LocationId,result.Value.Notes,Environment.UserName),"Ricambio riparato e reintegrato.");}
    private void Pdf(SparePartQuarantineItem value,SparePartInventoryItem? item,SparePartWarehouseLocation? location,SparePartReturn? source){try{if(item is null||source is null)throw new InvalidOperationException("Dati pratica incompleti.");var path=new SparePartQuarantinePdfService().Generate(value,source,item,location);Process.Start(new ProcessStartInfo{FileName=path,UseShellExecute=true});Status($"Verbale creato: {path}",false);}catch(Exception ex){Status($"PDF non creato: {ex.Message}",true);}}
    private void Run(Action action,string success){try{action();Status(success,false);Load();}catch(Exception ex){Status(ex.Message,true);}}
    private static Grid GridRow()=>new(){ColumnDefinitions=new ColumnDefinitions("145,250,85,105,125,180,110,300")};
    private static Control Badge(string status){var color=status switch{SparePartQuarantineStatus.Reintegrated=>UiTokens.Success,SparePartQuarantineStatus.Disposed=>UiTokens.Danger,SparePartQuarantineStatus.DisposalApproved=>UiTokens.Danger,SparePartQuarantineStatus.Repairable=>UiTokens.Warning,_=>UiTokens.BrandBlue};return new Border{BorderBrush=UiTokens.Brush(color),BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(8),Padding=new Thickness(6,3),Child=new TextBlock{Text=status,Foreground=UiTokens.Brush(color),FontWeight=FontWeight.SemiBold,HorizontalAlignment=HorizontalAlignment.Center}};}
    private static Button Button(string text,Action action,bool primary=false){var b=new Button{Content=text,MinHeight=34,Background=UiTokens.Brush(primary?UiTokens.BrandBlue:UiTokens.SurfaceAlt),Foreground=primary?Brushes.White:UiTokens.Brush(UiTokens.TextPrimary)};b.Click+=(_,_)=>action();return b;}
    private static void Text(Grid g,string value,int col,bool strong=false)=>Add(g,new TextBlock{Text=string.IsNullOrWhiteSpace(value)?"—":value,FontWeight=strong?FontWeight.SemiBold:FontWeight.Normal,VerticalAlignment=VerticalAlignment.Center,TextTrimming=TextTrimming.CharacterEllipsis,Margin=new Thickness(3)},col);
    private static void Add(Grid g,Control c,int col){c.Margin=new Thickness(0,0,6,0);Grid.SetColumn(c,col);g.Children.Add(c);}
    private void Status(string text,bool error){_message.Text=text;_message.Foreground=UiTokens.Brush(error?UiTokens.Danger:UiTokens.Success);}
}

public sealed class QuarantineEvaluationDialog : Window
{
    private readonly ComboBox _decision=new(){ItemsSource=new[]{SparePartQuarantineStatus.Repairable,SparePartQuarantineStatus.SupplierReturn,SparePartQuarantineStatus.DisposalApproved},SelectedIndex=0};private readonly TextBox _cost=new(){Text="0"},_notes=new(){AcceptsReturn=true,MinHeight=90};private readonly TextBlock _message=new();
    public QuarantineEvaluationDialog(SparePartQuarantineItem value){Title="Valuta materiale in quarantena";Width=520;Height=430;WindowStartupLocation=WindowStartupLocation.CenterOwner;var save=Button("Registra valutazione",()=>{if(!decimal.TryParse(_cost.Text,out var cost)||cost<0){_message.Text="Inserisci un costo valido.";return;}Close(new SparePartQuarantineItem{Status=_decision.SelectedItem?.ToString()??SparePartQuarantineStatus.Repairable,EstimatedCost=cost,EvaluationNotes=_notes.Text?.Trim()??""});});Content=new StackPanel{Margin=new Thickness(24),Spacing=10,Children={new TextBlock{Text="Valutazione tecnica",FontSize=24,FontWeight=FontWeight.Bold},new TextBlock{Text=$"{value.CaseNumber} · {value.Quantity:N2} unità",Foreground=UiTokens.Brush(UiTokens.TextSecondary)},Field("Esito",_decision),Field("Costo stimato",_cost),Field("Note tecniche",_notes),_message,save}};}
    private static Control Field(string label,Control c)=>new StackPanel{Spacing=3,Children={new TextBlock{Text=label,FontWeight=FontWeight.SemiBold},c}};private static Button Button(string t,Action a){var b=new Button{Content=t,Height=40};b.Click+=(_,_)=>a();return b;}
}
public sealed class QuarantineReintegrationDialog : Window
{
    private readonly IReadOnlyList<SparePartWarehouseLocation> _locations;private readonly ComboBox _location=new();private readonly TextBox _notes=new(){AcceptsReturn=true,MinHeight=80};
    public QuarantineReintegrationDialog(IReadOnlyList<SparePartWarehouseLocation> locations){_locations=locations;Title="Reintegra ricambio riparato";Width=500;Height=340;WindowStartupLocation=WindowStartupLocation.CenterOwner;_location.ItemsSource=locations.Select(x=>x.DisplayName).ToList();_location.SelectedIndex=locations.Count>0?0:-1;var save=Button("Conferma reintegro",()=>{if(_location.SelectedIndex>=0)Close((_locations[_location.SelectedIndex].Id,_notes.Text?.Trim()??""));});Content=new StackPanel{Margin=new Thickness(24),Spacing=10,Children={new TextBlock{Text="Reintegro dopo riparazione",FontSize=24,FontWeight=FontWeight.Bold},Field("Ubicazione",_location),Field("Note intervento",_notes),save}};}
    private static Control Field(string label,Control c)=>new StackPanel{Spacing=3,Children={new TextBlock{Text=label,FontWeight=FontWeight.SemiBold},c}};private static Button Button(string t,Action a){var b=new Button{Content=t,Height=40};b.Click+=(_,_)=>a();return b;}
}
