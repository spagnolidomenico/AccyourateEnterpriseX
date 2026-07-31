using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Input;
using System.Diagnostics;
using Accyourate.App.AssetManagement.Models;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class SparePartLocationsView : UserControl
{
    private readonly SparePartLocationsRepository _repository=new();
    private readonly SparePartsInventoryRepository _inventory=new();
    private readonly SparePartLabelPdfService _labels=new();
    private readonly StackPanel _rows=new();private readonly TextBlock _message=new();private readonly TextBox _search=new();
    public SparePartLocationsView(){Background=UiTokens.Brush(UiTokens.Background);_repository.EnsureInitialAllocations(_inventory.GetItems());Content=Build();Load();}
    private Control Build()
    {
        var root=new DockPanel();var header=new Grid{ColumnDefinitions=new ColumnDefinitions("*,Auto"),Margin=new Thickness(24,20,24,12)};
        header.Children.Add(new StackPanel{Spacing=4,Children={new TextBlock{Text="Ubicazioni Magazzino",FontSize=30,FontWeight=FontWeight.Bold},new TextBlock{Text="Magazzini, scaffali, giacenze distribuite e trasferimenti.",Foreground=UiTokens.Brush(UiTokens.TextSecondary)}}});
        var actions=new StackPanel{Orientation=Orientation.Horizontal,Spacing=6};actions.Children.Add(Button("Verifica coerenza",CheckConsistency));actions.Children.Add(Button("Registro prelievi",PickHistory));actions.Children.Add(Button("Storico trasferimenti",History));actions.Children.Add(Button("Etichette QR",Labels));actions.Children.Add(Button("Trasferisci",Transfer));actions.Children.Add(Button("Nuova ubicazione",NewLocation,true));Grid.SetColumn(actions,1);header.Children.Add(actions);
        DockPanel.SetDock(header,Dock.Top);root.Children.Add(header);
        _search.Watermark="Cerca ubicazione, magazzino o ricambio...";_search.Margin=new Thickness(24,0,24,8);_search.TextChanged+=(_,_)=>Load();DockPanel.SetDock(_search,Dock.Top);root.Children.Add(_search);
        _message.Margin=new Thickness(24,0,24,8);DockPanel.SetDock(_message,Dock.Top);root.Children.Add(_message);
        root.Children.Add(new ScrollViewer{Content=_rows,Margin=new Thickness(24,0,24,24),HorizontalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,VerticalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto});return root;
    }
    private void Load()
    {
        try
        {
            _repository.EnsureInitialAllocations(_inventory.GetItems());
            var locations=_repository.GetLocations();var balances=_repository.GetBalances();var items=_inventory.GetItems().ToDictionary(x=>x.Id);var q=(_search.Text??"").Trim();
            _rows.Children.Clear();_rows.MinWidth=1000;_rows.Children.Add(Header());
            var visible=locations.Where(location=>q.Length==0||$"{location.Code} {location.Name} {location.Warehouse} {location.Aisle} {location.Shelf} {string.Join(" ",balances.Where(x=>x.LocationId==location.Id).Select(x=>items.TryGetValue(x.InventoryItemId,out var item)?$"{item.PartCode} {item.Description}":""))}".Contains(q,StringComparison.OrdinalIgnoreCase)).ToList();
            for(var i=0;i<visible.Count;i++)_rows.Children.Add(Row(visible[i],balances,items,i));
        }catch(Exception ex){Show($"Errore ubicazioni: {ex.Message}",true);}
    }
    private Control Header(){var g=GridRow();foreach(var x in new[]{("Codice",0),("Nome",1),("Magazzino",2),("Corridoio",3),("Scaffale",4),("Articoli",5),("Quantità",6),("Stato",7)})AddText(g,x.Item1,x.Item2,true);return new Border{Background=UiTokens.Brush(UiTokens.SurfaceAlt),Padding=new Thickness(9),Child=g};}
    private Control Row(SparePartWarehouseLocation location,IReadOnlyList<SparePartLocationBalance> balances,IReadOnlyDictionary<int,SparePartInventoryItem> items,int index)
    {
        var local=balances.Where(x=>x.LocationId==location.Id&&x.Quantity!=0).ToList();var g=GridRow();AddText(g,location.Code,0,true);AddText(g,location.Name,1);AddText(g,location.Warehouse,2);AddText(g,location.Aisle,3);AddText(g,location.Shelf,4);AddText(g,local.Count.ToString(),5);AddText(g,local.Sum(x=>x.Quantity).ToString("N2"),6,true);AddText(g,location.IsActive?"Attiva":"Disattiva",7,false,!location.IsActive);
        return new Border{Background=UiTokens.Brush(index%2==0?UiTokens.Surface:UiTokens.SurfaceAlt),BorderBrush=UiTokens.Brush(UiTokens.Border),BorderThickness=new Thickness(0,0,0,1),Padding=new Thickness(9,7),Child=g};
    }
    private async void NewLocation(){var owner=TopLevel.GetTopLevel(this) as Window;if(owner is null)return;var location=await new WarehouseLocationDialog().ShowDialog<SparePartWarehouseLocation?>(owner);if(location is null)return;try{_repository.SaveLocation(location);Show("Ubicazione salvata.");Load();}catch(Exception ex){Show($"Ubicazione non salvata: {ex.Message}",true);}}
    private async void Transfer(){var owner=TopLevel.GetTopLevel(this) as Window;if(owner is null)return;var items=_inventory.GetItems();var locations=_repository.GetLocations().Where(x=>x.IsActive).ToList();var balances=_repository.GetBalances();if(locations.Count<2){Show("Crea almeno due ubicazioni.",true);return;}var request=await new LocationTransferDialog(items,locations,balances).ShowDialog<LocationTransferRequest?>(owner);if(request is null)return;try{_repository.Transfer(request.ItemId,request.FromId,request.ToId,request.Quantity,request.Reference,request.Notes,Environment.UserName);Show("Trasferimento registrato.");Load();}catch(Exception ex){Show($"Trasferimento non eseguito: {ex.Message}",true);}}
    private async void History(){var owner=TopLevel.GetTopLevel(this) as Window;if(owner is null)return;await new LocationTransfersWindow(_repository,_inventory.GetItems()).ShowDialog(owner);}
    private async void PickHistory(){var owner=TopLevel.GetTopLevel(this) as Window;if(owner is null)return;await new LocationPicksWindow(_repository,_inventory.GetItems()).ShowDialog(owner);}
    private void Labels(){try{var path=_labels.GenerateLocations(_repository.GetLocations());Process.Start(new ProcessStartInfo{FileName=path,UseShellExecute=true});Show($"Etichette create: {path}");}catch(Exception ex){Show($"Etichette non create: {ex.Message}",true);}}
    private async void CheckConsistency()
    {
        var differences=_repository.GetDiscrepancies(_inventory.GetItems());
        if(differences.Count==0){Show("Giacenze totali e locali perfettamente allineate.");return;}
        var locations=_repository.GetLocations().Where(x=>x.IsActive).ToList();var owner=TopLevel.GetTopLevel(this) as Window;
        if(owner is null||locations.Count==0){Show("Nessuna ubicazione disponibile per il riallineamento.",true);return;}
        var destination=await new LocationReconciliationDialog(differences,locations).ShowDialog<SparePartWarehouseLocation?>(owner);
        if(destination is null)return;_repository.ReconcileToLocation(differences,destination.Id);Show($"Riallineate {differences.Count} giacenze su {destination.Code}.");Load();
    }
    private static Grid GridRow()=>new(){ColumnDefinitions=new ColumnDefinitions("120,210,180,110,110,90,100,100")};
    private static Button Button(string text,Action action,bool primary=false){var b=new Button{Content=text,MinHeight=34,Margin=new Thickness(3),Background=UiTokens.Brush(primary?UiTokens.BrandBlue:UiTokens.SurfaceAlt),Foreground=primary?Brushes.White:UiTokens.Brush(UiTokens.TextPrimary)};b.Click+=(_,_)=>action();return b;}
    private static void AddText(Grid g,string text,int col,bool strong=false,bool danger=false){var t=new TextBlock{Text=string.IsNullOrWhiteSpace(text)?"—":text,FontWeight=strong?FontWeight.SemiBold:FontWeight.Normal,Foreground=UiTokens.Brush(danger?UiTokens.Danger:strong?UiTokens.TextPrimary:UiTokens.TextSecondary),VerticalAlignment=VerticalAlignment.Center,TextTrimming=TextTrimming.CharacterEllipsis,Margin=new Thickness(3)};Grid.SetColumn(t,col);g.Children.Add(t);}
    private void Show(string text,bool error=false){_message.Text=text;_message.Foreground=UiTokens.Brush(error?UiTokens.Danger:UiTokens.Success);}
}

public sealed class WarehouseLocationDialog : Window
{
    private readonly TextBox _code=new(),_name=new(),_warehouse=new(),_aisle=new(),_shelf=new();private readonly TextBlock _message=new();
    public WarehouseLocationDialog(){Title="Nuova ubicazione";Width=500;Height=480;WindowStartupLocation=WindowStartupLocation.CenterOwner;var save=new Button{Content="Salva",Height=40};save.Click+=(_,_)=>{if(string.IsNullOrWhiteSpace(_code.Text)||string.IsNullOrWhiteSpace(_name.Text)){_message.Text="Inserisci codice e nome.";return;}Close(new SparePartWarehouseLocation{Code=_code.Text.Trim().ToUpperInvariant(),Name=_name.Text.Trim(),Warehouse=_warehouse.Text?.Trim()??"",Aisle=_aisle.Text?.Trim()??"",Shelf=_shelf.Text?.Trim()??""});};Content=new StackPanel{Margin=new Thickness(24),Spacing=9,Children={new TextBlock{Text="Nuova ubicazione",FontSize=24,FontWeight=FontWeight.Bold},Field("Codice",_code),Field("Nome",_name),Field("Magazzino",_warehouse),Field("Corridoio",_aisle),Field("Scaffale",_shelf),_message,save}};}
    private static Control Field(string label,Control control)=>new StackPanel{Spacing=3,Children={new TextBlock{Text=label,FontWeight=FontWeight.SemiBold},control}};
}

public sealed class LocationTransferRequest{public int ItemId{get;init;}public int FromId{get;init;}public int ToId{get;init;}public decimal Quantity{get;init;}public string Reference{get;init;}="";public string Notes{get;init;}="";}
public sealed class LocationTransferDialog : Window
{
    private readonly IReadOnlyList<SparePartInventoryItem> _items;private readonly IReadOnlyList<SparePartWarehouseLocation> _locations;private readonly IReadOnlyList<SparePartLocationBalance> _balances;
    private readonly ComboBox _item=new(),_from=new(),_to=new();private readonly TextBox _quantity=new(){Text="1"},_reference=new(),_notes=new();private readonly TextBlock _message=new();
    private readonly TextBox _scanItem=new(){Watermark="AXPART:codice"},_scanFrom=new(){Watermark="AXLOC:origine"},_scanTo=new(){Watermark="AXLOC:destinazione"};
    public LocationTransferDialog(IReadOnlyList<SparePartInventoryItem> items,IReadOnlyList<SparePartWarehouseLocation> locations,IReadOnlyList<SparePartLocationBalance> balances)
    {
        _items=items;_locations=locations;_balances=balances;Title="Trasferisci ricambio";Width=540;Height=520;WindowStartupLocation=WindowStartupLocation.CenterOwner;
        _item.ItemsSource=items.Select(x=>$"{x.PartCode} · {x.Description}").ToList();_item.SelectedIndex=items.Count>0?0:-1;_from.ItemsSource=locations.Select(x=>x.DisplayName).ToList();_from.SelectedIndex=0;_to.ItemsSource=locations.Select(x=>x.DisplayName).ToList();_to.SelectedIndex=locations.Count>1?1:0;
        _scanItem.KeyDown+=(_,e)=>ScanItem(e);_scanFrom.KeyDown+=(_,e)=>ScanLocation(e,_scanFrom,_from);_scanTo.KeyDown+=(_,e)=>ScanLocation(e,_scanTo,_to);
        var save=new Button{Content="Registra trasferimento",Height=40};save.Click+=(_,_)=>Confirm();
        Content=new ScrollViewer{Content=new StackPanel{Margin=new Thickness(24),Spacing=9,Children={new TextBlock{Text="Trasferimento",FontSize=24,FontWeight=FontWeight.Bold},Field("Scansiona ricambio",_scanItem),Field("Ricambio",_item),Field("Scansiona origine",_scanFrom),Field("Da ubicazione",_from),Field("Scansiona destinazione",_scanTo),Field("A ubicazione",_to),Field("Quantità",_quantity),Field("Riferimento",_reference),Field("Note",_notes),_message,save}}};
    }
    private void ScanItem(KeyEventArgs e){if(e.Key!=Key.Enter)return;e.Handled=true;var code=Normalize(_scanItem.Text,"AXPART:");var index=_items.ToList().FindIndex(x=>string.Equals(x.PartCode,code,StringComparison.OrdinalIgnoreCase));if(index<0){Error($"Ricambio non riconosciuto: {code}");return;}_item.SelectedIndex=index;_scanFrom.Focus();}
    private void ScanLocation(KeyEventArgs e,TextBox source,ComboBox target){if(e.Key!=Key.Enter)return;e.Handled=true;var code=Normalize(source.Text,"AXLOC:");var index=_locations.ToList().FindIndex(x=>string.Equals(x.Code,code,StringComparison.OrdinalIgnoreCase));if(index<0){Error($"Ubicazione non riconosciuta: {code}");return;}target.SelectedIndex=index;if(ReferenceEquals(source,_scanFrom))_scanTo.Focus();else _quantity.Focus();}
    private static string Normalize(string? value,string prefix){var text=(value??"").Trim();return text.StartsWith(prefix,StringComparison.OrdinalIgnoreCase)?text[prefix.Length..].Trim():text;}
    private void Confirm(){if(_item.SelectedIndex<0||_from.SelectedIndex<0||_to.SelectedIndex<0){Error("Completa le selezioni.");return;}if(!decimal.TryParse(_quantity.Text,out var q)||q<=0){Error("Inserisci una quantità valida.");return;}var item=_items[_item.SelectedIndex];var from=_locations[_from.SelectedIndex];var available=_balances.FirstOrDefault(x=>x.InventoryItemId==item.Id&&x.LocationId==from.Id)?.Quantity??0;if(q>available){Error($"Disponibili nell'origine: {available:N2}.");return;}Close(new LocationTransferRequest{ItemId=item.Id,FromId=from.Id,ToId=_locations[_to.SelectedIndex].Id,Quantity=q,Reference=_reference.Text?.Trim()??"",Notes=_notes.Text?.Trim()??""});}
    private void Error(string text){_message.Text=text;_message.Foreground=UiTokens.Brush(UiTokens.Danger);}
    private static Control Field(string label,Control control)=>new StackPanel{Spacing=3,Children={new TextBlock{Text=label,FontWeight=FontWeight.SemiBold},control}};
}

public sealed class LocationTransfersWindow : Window
{
    public LocationTransfersWindow(SparePartLocationsRepository repository,IReadOnlyList<SparePartInventoryItem> items)
    {
        Title="Storico trasferimenti";Width=1050;Height=650;WindowStartupLocation=WindowStartupLocation.CenterOwner;var locations=repository.GetLocations().ToDictionary(x=>x.Id);var itemMap=items.ToDictionary(x=>x.Id);var rows=new StackPanel{Margin=new Thickness(24),MinWidth=950};rows.Children.Add(new TextBlock{Text="Storico trasferimenti",FontSize=25,FontWeight=FontWeight.Bold,Margin=new Thickness(0,0,0,14)});
        foreach(var t in repository.GetTransfers()){locations.TryGetValue(t.FromLocationId,out var from);locations.TryGetValue(t.ToLocationId,out var to);itemMap.TryGetValue(t.InventoryItemId,out var item);rows.Children.Add(new Border{BorderBrush=UiTokens.Brush(UiTokens.Border),BorderThickness=new Thickness(0,0,0,1),Padding=new Thickness(8),Child=new TextBlock{Text=$"{Date(t.CreatedAt)} · {item?.PartCode??"#"+t.InventoryItemId} · {t.Quantity:N2} · {from?.Code??"?"} → {to?.Code??"?"} · {t.Reference} · {t.OperatorName}"}});}
        Content=new ScrollViewer{Content=rows,HorizontalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,VerticalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto};
    }
    private static string Date(string value)=>DateTime.TryParse(value,out var d)?d.ToString("dd/MM/yyyy HH:mm"):value;
}

public sealed class LocationPicksWindow : Window
{
    private readonly SparePartLocationsRepository _repository;
    private readonly IReadOnlyDictionary<int,SparePartInventoryItem> _items;
    private readonly IReadOnlyDictionary<int,SparePartWarehouseLocation> _locations;
    private readonly TextBox _search=new(){Watermark="Cerca ricambio, ubicazione, riferimento o operatore..."};
    private readonly DatePicker _from=new(),_to=new();
    private readonly ContentControl _host=new();
    private readonly TextBlock _summary=new(),_message=new();
    private IReadOnlyList<SparePartLocationPick> _visible=Array.Empty<SparePartLocationPick>();

    public LocationPicksWindow(SparePartLocationsRepository repository,IReadOnlyList<SparePartInventoryItem> items)
    {
        _repository=repository;_items=items.ToDictionary(x=>x.Id);_locations=repository.GetLocations().ToDictionary(x=>x.Id);
        Title="Registro prelievi per ubicazione";Width=1180;Height=720;MinWidth=900;MinHeight=520;
        WindowStartupLocation=WindowStartupLocation.CenterOwner;
        _search.TextChanged+=(_,_)=>Load();_from.SelectedDateChanged+=(_,_)=>Load();_to.SelectedDateChanged+=(_,_)=>Load();
        Content=Build();Load();
    }

    private Control Build()
    {
        var root=new DockPanel{Margin=new Thickness(24)};
        var title=new Grid{ColumnDefinitions=new ColumnDefinitions("*,Auto"),Margin=new Thickness(0,0,0,14)};
        title.Children.Add(new StackPanel{Spacing=3,Children={new TextBlock{Text="Registro prelievi",FontSize=27,FontWeight=FontWeight.Bold},new TextBlock{Text="Consumi suddivisi per ricambio, ubicazione, riferimento e operatore.",Foreground=UiTokens.Brush(UiTokens.TextSecondary)}}});
        var pdf=Button("Genera picking list PDF",GeneratePdf,true);Grid.SetColumn(pdf,1);title.Children.Add(pdf);
        DockPanel.SetDock(title,Dock.Top);root.Children.Add(title);
        var filters=new Grid{ColumnDefinitions=new ColumnDefinitions("*,160,160"),Margin=new Thickness(0,0,0,8)};
        Add(filters,_search,0);Add(filters,_from,1);Add(filters,_to,2);DockPanel.SetDock(filters,Dock.Top);root.Children.Add(filters);
        _message.Margin=new Thickness(0,0,0,5);DockPanel.SetDock(_message,Dock.Top);root.Children.Add(_message);
        _summary.Foreground=UiTokens.Brush(UiTokens.TextSecondary);_summary.Margin=new Thickness(0,0,0,8);DockPanel.SetDock(_summary,Dock.Top);root.Children.Add(_summary);
        root.Children.Add(_host);return root;
    }

    private void Load()
    {
        var query=(_search.Text??string.Empty).Trim();
        _visible=_repository.GetPicks().Where(x=>!_from.SelectedDate.HasValue||Parse(x.CreatedAt)>=_from.SelectedDate.Value.Date)
            .Where(x=>!_to.SelectedDate.HasValue||Parse(x.CreatedAt)<_to.SelectedDate.Value.Date.AddDays(1))
            .Where(x=>
            {
                _items.TryGetValue(x.InventoryItemId,out var item);_locations.TryGetValue(x.LocationId,out var location);
                return query.Length==0||$"{item?.PartCode} {item?.Description} {location?.Code} {location?.Name} {x.Reference} {x.Notes} {x.OperatorName}".Contains(query,StringComparison.OrdinalIgnoreCase);
            }).ToList();
        _summary.Text=$"{_visible.Count} righe di prelievo · quantità totale {_visible.Sum(x=>x.Quantity):N2}";
        var rows=new StackPanel{MinWidth=1040};rows.Children.Add(PickRow("Data","Ricambio","Ubicazione","Quantità","Riferimento","Operatore","Note",true));
        foreach(var pick in _visible)
        {
            _items.TryGetValue(pick.InventoryItemId,out var item);_locations.TryGetValue(pick.LocationId,out var location);
            rows.Children.Add(PickRow(Parse(pick.CreatedAt).ToString("dd/MM/yyyy HH:mm"),
                item is null?$"ID {pick.InventoryItemId}":$"{item.PartCode} · {item.Description}",
                location?.DisplayName??$"ID {pick.LocationId}",pick.Quantity.ToString("N2"),
                pick.Reference,pick.OperatorName,pick.Notes,false));
        }
        _host.Content=new ScrollViewer{Content=rows,HorizontalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,VerticalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto};
    }

    private void GeneratePdf()
    {
        try
        {
            if(_visible.Count==0){Status("Non ci sono prelievi da esportare.",true);return;}
            var path=new SparePartPickingPdfService().Generate(_visible,_items,_locations,_from.SelectedDate?.Date,_to.SelectedDate?.Date);
            Process.Start(new ProcessStartInfo{FileName=path,UseShellExecute=true});Status($"Picking list creata: {path}",false);
        }
        catch(Exception ex){Status($"PDF non creato: {ex.Message}",true);}
    }

    private static Control PickRow(string date,string item,string location,string quantity,string reference,string user,string notes,bool header)
    {
        var grid=new Grid{ColumnDefinitions=new ColumnDefinitions("135,230,190,90,150,130,190")};
        var values=new[]{date,item,location,quantity,reference,user,notes};
        for(var i=0;i<values.Length;i++){var text=new TextBlock{Text=string.IsNullOrWhiteSpace(values[i])?"—":values[i],FontWeight=header?FontWeight.SemiBold:FontWeight.Normal,TextTrimming=TextTrimming.CharacterEllipsis,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(3)};Grid.SetColumn(text,i);grid.Children.Add(text);}
        return new Border{Background=header?UiTokens.Brush(UiTokens.SurfaceAlt):UiTokens.Brush(UiTokens.Surface),BorderBrush=UiTokens.Brush(UiTokens.Border),BorderThickness=new Thickness(0,0,0,1),Padding=new Thickness(7,9),Child=grid};
    }
    private static DateTimeOffset Parse(string value)=>DateTimeOffset.TryParse(value,out var result)?result:DateTimeOffset.MinValue;
    private static void Add(Grid grid,Control control,int column){control.Margin=new Thickness(0,0,8,0);Grid.SetColumn(control,column);grid.Children.Add(control);}
    private static Button Button(string text,Action action,bool primary=false){var button=new Button{Content=text,MinHeight=36,Background=UiTokens.Brush(primary?UiTokens.BrandBlue:UiTokens.SurfaceAlt),Foreground=primary?Brushes.White:UiTokens.Brush(UiTokens.TextPrimary)};button.Click+=(_,_)=>action();return button;}
    private void Status(string text,bool error){_message.Text=text;_message.Foreground=UiTokens.Brush(error?UiTokens.Danger:UiTokens.Success);}
}

public sealed class LocationReconciliationDialog : Window
{
    private readonly IReadOnlyList<SparePartWarehouseLocation> _locations;private readonly ComboBox _destination=new();
    public LocationReconciliationDialog(IReadOnlyList<SparePartLocationDiscrepancy> differences,IReadOnlyList<SparePartWarehouseLocation> locations)
    {
        _locations=locations;Title="Riallineamento giacenze";Width=620;Height=430;WindowStartupLocation=WindowStartupLocation.CenterOwner;
        _destination.ItemsSource=locations.Select(x=>x.DisplayName).ToList();_destination.SelectedIndex=0;
        var details=new StackPanel();foreach(var item in differences.Take(12))details.Children.Add(new TextBlock{Text=$"{item.PartCode} · totale {item.TotalQuantity:N2} · ubicato {item.AllocatedQuantity:N2} · differenza {item.Difference:N2}"});
        var apply=new Button{Content="Assegna differenze all'ubicazione",Height=40};apply.Click+=(_,_)=>{if(_destination.SelectedIndex>=0)Close(_locations[_destination.SelectedIndex]);};
        Content=new ScrollViewer{Content=new StackPanel{Margin=new Thickness(24),Spacing=10,Children={new TextBlock{Text="Giacenze non allineate",FontSize=24,FontWeight=FontWeight.Bold},new TextBlock{Text="Le differenze verranno assegnate alla posizione selezionata."},details,new TextBlock{Text="Ubicazione di riallineamento",FontWeight=FontWeight.SemiBold},_destination,apply}}};
    }
}
