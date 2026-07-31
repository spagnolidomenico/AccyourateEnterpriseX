using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Models;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class SparePartPickRequestsWindow : Window
{
    private readonly SparePartPickRequestRepository _repository=new();
    private readonly SparePartsInventoryRepository _inventory=new();
    private readonly SparePartLocationsRepository _locations=new();
    private readonly MaintenanceRepository _maintenance=new();
    private readonly SparePartReturnRepository _returns=new();
    private readonly StackPanel _rows=new();private readonly TextBlock _message=new(),_summary=new();
    private readonly TextBox _search=new(){Watermark="Cerca numero, ricambio, tecnico..."};
    private readonly ComboBox _status=new(){ItemsSource=new[]{"Tutti gli stati",SparePartPickRequestStatus.Draft,SparePartPickRequestStatus.Approved,SparePartPickRequestStatus.Preparing,SparePartPickRequestStatus.Delivered,SparePartPickRequestStatus.Cancelled},SelectedIndex=0};

    public SparePartPickRequestsWindow()
    {
        Title="Richieste di prelievo";Width=1320;Height=760;MinWidth=980;MinHeight=560;WindowStartupLocation=WindowStartupLocation.CenterOwner;
        _search.TextChanged+=(_,_)=>Load();_status.SelectionChanged+=(_,_)=>Load();Content=Build();Load();
    }
    private Control Build()
    {
        var root=new DockPanel{Margin=new Thickness(24)};
        var header=new Grid{ColumnDefinitions=new ColumnDefinitions("*,Auto"),Margin=new Thickness(0,0,0,12)};
        header.Children.Add(new StackPanel{Spacing=3,Children={new TextBlock{Text="Richieste di prelievo",FontSize=28,FontWeight=FontWeight.Bold},new TextBlock{Text="Prenotazione, preparazione e consegna controllata dei ricambi.",Foreground=UiTokens.Brush(UiTokens.TextSecondary)}}});
        var headerActions=new StackPanel{Orientation=Orientation.Horizontal,Spacing=5};
        headerActions.Children.Add(Button("Storico resi",ReturnHistory));headerActions.Children.Add(Button("Nuova richiesta",NewRequest,true));
        Grid.SetColumn(headerActions,1);header.Children.Add(headerActions);DockPanel.SetDock(header,Dock.Top);root.Children.Add(header);
        var filters=new Grid{ColumnDefinitions=new ColumnDefinitions("*,190"),Margin=new Thickness(0,0,0,8)};Add(filters,_search,0);Add(filters,_status,1);DockPanel.SetDock(filters,Dock.Top);root.Children.Add(filters);
        _message.Margin=new Thickness(0,0,0,5);DockPanel.SetDock(_message,Dock.Top);root.Children.Add(_message);
        _summary.Foreground=UiTokens.Brush(UiTokens.TextSecondary);_summary.Margin=new Thickness(0,0,0,8);DockPanel.SetDock(_summary,Dock.Top);root.Children.Add(_summary);
        root.Children.Add(new ScrollViewer{Content=_rows,HorizontalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,VerticalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto});return root;
    }
    private void Load()
    {
        try
        {
            var items=_inventory.GetItems().ToDictionary(x=>x.Id);var locations=_locations.GetLocations().ToDictionary(x=>x.Id);
            var q=(_search.Text??"").Trim();if(q.StartsWith("AXPICK:",StringComparison.OrdinalIgnoreCase))q=q[7..].Trim();
            var selected=_status.SelectedItem?.ToString()??"Tutti gli stati";
            var requests=_repository.GetAll().Where(x=>selected=="Tutti gli stati"||x.Status==selected)
                .Where(x=>{items.TryGetValue(x.InventoryItemId,out var item);return q.Length==0||$"{x.RequestNumber} {item?.PartCode} {item?.Description} {x.Technician} {x.RequestedBy}".Contains(q,StringComparison.OrdinalIgnoreCase);}).ToList();
            _summary.Text=$"{requests.Count} richieste · prenotate {requests.Where(x=>x.Status is SparePartPickRequestStatus.Approved or SparePartPickRequestStatus.Preparing).Sum(x=>x.Quantity):N2}";
            _rows.Children.Clear();_rows.MinWidth=1220;_rows.Children.Add(Header());
            for(var i=0;i<requests.Count;i++)_rows.Children.Add(Row(requests[i],items,locations,i));
        }catch(Exception ex){Status($"Errore caricamento richieste: {ex.Message}",true);}
    }
    private Control Header(){var g=GridRow();foreach(var x in new[]{("Richiesta",0),("Ricambio",1),("Quantità",2),("Ubicazione",3),("Tecnico",4),("Stato",5),("Creata",6),("Azioni",7)})Text(g,x.Item1,x.Item2,true);return new Border{Background=UiTokens.Brush(UiTokens.SurfaceAlt),Padding=new Thickness(8),Child=g};}
    private Control Row(SparePartPickRequest request,IReadOnlyDictionary<int,SparePartInventoryItem> items,IReadOnlyDictionary<int,SparePartWarehouseLocation> locations,int index)
    {
        items.TryGetValue(request.InventoryItemId,out var item);locations.TryGetValue(request.PreferredLocationId,out var location);var g=GridRow();
        Text(g,request.RequestNumber,0,true);Text(g,item is null?$"ID {request.InventoryItemId}":$"{item.PartCode} · {item.Description}",1);
        Text(g,request.Quantity.ToString("N2"),2,true);Text(g,location?.Code??"Automatico",3);Text(g,request.Technician,4);Add(g,Badge(request.Status),5);Text(g,Date(request.CreatedAt),6);
        var actions=new StackPanel{Orientation=Orientation.Horizontal,Spacing=3};
        if(request.Status==SparePartPickRequestStatus.Draft)actions.Children.Add(Button("Approva",()=>Run(()=>_repository.Approve(request.Id),"Richiesta approvata.")));
        if(request.Status==SparePartPickRequestStatus.Approved)actions.Children.Add(Button("Prepara",()=>Run(()=>_repository.StartPreparation(request.Id),"Preparazione avviata.")));
        if(request.Status==SparePartPickRequestStatus.Preparing)actions.Children.Add(Button("Consegna",()=>Run(()=>_repository.Deliver(request.Id,Environment.UserName),"Ricambi consegnati e giacenze aggiornate."),true));
        var remaining=request.Quantity-_returns.ReturnedQuantity(request.Id);
        if(request.Status==SparePartPickRequestStatus.Delivered&&remaining>0)
            actions.Children.Add(Button($"Reso ({remaining:N2})",()=>Return(request,item)));
        if(request.Status is not (SparePartPickRequestStatus.Delivered or SparePartPickRequestStatus.Cancelled))actions.Children.Add(Button("Annulla",()=>Run(()=>_repository.Cancel(request.Id),"Richiesta annullata.")));
        actions.Children.Add(Button("PDF",()=>Pdf(request,item,location)));Add(g,actions,7);
        return new Border{Background=UiTokens.Brush(index%2==0?UiTokens.Surface:UiTokens.SurfaceAlt),BorderBrush=UiTokens.Brush(UiTokens.Border),BorderThickness=new Thickness(0,0,0,1),Padding=new Thickness(8,6),Child=g};
    }
    private async void NewRequest()
    {
        var items=_inventory.GetItems();_locations.EnsureInitialAllocations(items);var locations=_locations.GetLocations().Where(x=>x.IsActive).ToList();
        if(items.Count==0||locations.Count==0){Status("Servono almeno un ricambio e un'ubicazione attiva.",true);return;}
        var owner=TopLevel.GetTopLevel(this) as Window;if(owner is null)return;
        var request=await new SparePartPickRequestDialog(items,locations,_maintenance.GetAll()).ShowDialog<SparePartPickRequest?>(owner);
        if(request is null)return;try{_repository.Create(request);Status("Richiesta salvata in bozza.",false);Load();}catch(Exception ex){Status($"Richiesta non salvata: {ex.Message}",true);}
    }
    private async void Return(SparePartPickRequest request,SparePartInventoryItem? item)
    {
        if(item is null){Status("Ricambio non trovato.",true);return;}
        var locations=_locations.GetLocations().Where(x=>x.IsActive).ToList();var remaining=request.Quantity-_returns.ReturnedQuantity(request.Id);
        var owner=TopLevel.GetTopLevel(this) as Window;if(owner is null)return;
        var value=await new SparePartReturnDialog(request,item,locations,remaining).ShowDialog<SparePartReturn?>(owner);
        if(value is null)return;
        try{_returns.Register(request.Id,value.Quantity,value.LocationId,value.Condition,value.Reason,value.Notes,Environment.UserName);Status("Reso registrato.",false);Load();}
        catch(Exception ex){Status($"Reso non registrato: {ex.Message}",true);}
    }
    private async void ReturnHistory(){var owner=TopLevel.GetTopLevel(this) as Window;if(owner is null)return;await new SparePartReturnsWindow(_returns,_repository.GetAll(),_inventory.GetItems(),_locations.GetLocations()).ShowDialog(owner);Load();}
    private void Run(Action action,string success){try{action();Status(success,false);Load();}catch(Exception ex){Status(ex.Message,true);}}
    private void Pdf(SparePartPickRequest request,SparePartInventoryItem? item,SparePartWarehouseLocation? location){try{if(item is null)throw new InvalidOperationException("Ricambio non trovato.");var path=new SparePartPickRequestPdfService().Generate(request,item,location);Process.Start(new ProcessStartInfo{FileName=path,UseShellExecute=true});Status($"Documento creato: {path}",false);}catch(Exception ex){Status($"PDF non creato: {ex.Message}",true);}}
    private static Grid GridRow()=>new(){ColumnDefinitions=new ColumnDefinitions("150,260,85,120,140,125,120,320")};
    private static Control Badge(string status){var color=status switch{SparePartPickRequestStatus.Delivered=>UiTokens.Success,SparePartPickRequestStatus.Cancelled=>UiTokens.Danger,SparePartPickRequestStatus.Preparing=>UiTokens.Warning,_=>UiTokens.BrandBlue};return new Border{BorderBrush=UiTokens.Brush(color),BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(8),Padding=new Thickness(6,3),Child=new TextBlock{Text=status,Foreground=UiTokens.Brush(color),FontWeight=FontWeight.SemiBold,HorizontalAlignment=HorizontalAlignment.Center}};}
    private static string Date(string value)=>DateTime.TryParse(value,out var d)?d.ToString("dd/MM/yyyy HH:mm"):value;
    private static Button Button(string text,Action action,bool primary=false){var b=new Button{Content=text,MinHeight=34,Background=UiTokens.Brush(primary?UiTokens.BrandBlue:UiTokens.SurfaceAlt),Foreground=primary?Brushes.White:UiTokens.Brush(UiTokens.TextPrimary)};b.Click+=(_,_)=>action();return b;}
    private static void Text(Grid g,string value,int col,bool strong=false)=>Add(g,new TextBlock{Text=string.IsNullOrWhiteSpace(value)?"—":value,FontWeight=strong?FontWeight.SemiBold:FontWeight.Normal,TextTrimming=TextTrimming.CharacterEllipsis,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(3)},col);
    private static void Add(Grid g,Control c,int col){c.Margin=new Thickness(0,0,6,0);Grid.SetColumn(c,col);g.Children.Add(c);}
    private void Status(string text,bool error){_message.Text=text;_message.Foreground=UiTokens.Brush(error?UiTokens.Danger:UiTokens.Success);}
}

public sealed class SparePartReturnDialog : Window
{
    private readonly IReadOnlyList<SparePartWarehouseLocation> _locations;
    private readonly ComboBox _condition=new(){ItemsSource=new[]{SparePartReturnCondition.Reusable,SparePartReturnCondition.Damaged,SparePartReturnCondition.Discarded},SelectedIndex=0};
    private readonly ComboBox _location=new();private readonly TextBox _quantity=new(),_reason=new(),_notes=new(){AcceptsReturn=true,MinHeight=70};private readonly TextBlock _message=new();
    public SparePartReturnDialog(SparePartPickRequest request,SparePartInventoryItem item,IReadOnlyList<SparePartWarehouseLocation> locations,decimal remaining)
    {
        _locations=locations;Title="Registra reso ricambio";Width=540;Height=580;WindowStartupLocation=WindowStartupLocation.CenterOwner;
        _quantity.Text=remaining.ToString("N2");_location.ItemsSource=locations.Select(x=>x.DisplayName).ToList();_location.SelectedIndex=locations.Count>0?0:-1;
        var save=Button("Registra reso",()=>Confirm(remaining));
        Content=new ScrollViewer{Content=new StackPanel{Margin=new Thickness(24),Spacing=9,Children={new TextBlock{Text="Reso ricambio",FontSize=24,FontWeight=FontWeight.Bold},new TextBlock{Text=$"{request.RequestNumber} · {item.PartCode} · restituibile {remaining:N2}",Foreground=UiTokens.Brush(UiTokens.TextSecondary)},Field("Quantità",_quantity),Field("Condizione",_condition),Field("Ubicazione di rientro (obbligatoria se riutilizzabile)",_location),Field("Motivo",_reason),Field("Note",_notes),_message,save}}};
    }
    private void Confirm(decimal remaining){if(!decimal.TryParse(_quantity.Text,out var quantity)||quantity<=0||quantity>remaining){Error($"Inserisci una quantità tra 0 e {remaining:N2}.");return;}var condition=_condition.SelectedItem?.ToString()??SparePartReturnCondition.Reusable;var locationId=_location.SelectedIndex>=0?_locations[_location.SelectedIndex].Id:0;if(condition==SparePartReturnCondition.Reusable&&locationId==0){Error("Seleziona l'ubicazione di rientro.");return;}Close(new SparePartReturn{Quantity=quantity,Condition=condition,LocationId=locationId,Reason=_reason.Text?.Trim()??"",Notes=_notes.Text?.Trim()??""});}
    private void Error(string text){_message.Text=text;_message.Foreground=UiTokens.Brush(UiTokens.Danger);}
    private static Control Field(string label,Control control)=>new StackPanel{Spacing=3,Children={new TextBlock{Text=label,FontWeight=FontWeight.SemiBold},control}};
    private static Button Button(string text,Action action){var b=new Button{Content=text,Height=40,Background=UiTokens.Brush(UiTokens.BrandBlue),Foreground=Brushes.White};b.Click+=(_,_)=>action();return b;}
}

public sealed class SparePartReturnsWindow : Window
{
    public SparePartReturnsWindow(SparePartReturnRepository repository,IReadOnlyList<SparePartPickRequest> requests,IReadOnlyList<SparePartInventoryItem> items,IReadOnlyList<SparePartWarehouseLocation> locations)
    {
        Title="Storico resi ricambi";Width=1180;Height=700;WindowStartupLocation=WindowStartupLocation.CenterOwner;
        var requestMap=requests.ToDictionary(x=>x.Id);var itemMap=items.ToDictionary(x=>x.Id);var locationMap=locations.ToDictionary(x=>x.Id);
        var rows=new StackPanel{Margin=new Thickness(24),MinWidth=1080};rows.Children.Add(new TextBlock{Text="Storico resi ricambi",FontSize=27,FontWeight=FontWeight.Bold,Margin=new Thickness(0,0,0,14)});
        rows.Children.Add(Row("Data","Numero","Richiesta","Ricambio","Quantità","Condizione","Ubicazione","Operatore","PDF",true,null));
        foreach(var value in repository.GetAll())
        {
            requestMap.TryGetValue(value.PickRequestId,out var request);itemMap.TryGetValue(value.InventoryItemId,out var item);locationMap.TryGetValue(value.LocationId,out var location);
            rows.Children.Add(Row(Date(value.CreatedAt),value.ReturnNumber,request?.RequestNumber??$"#{value.PickRequestId}",item?.PartCode??$"#{value.InventoryItemId}",value.Quantity.ToString("N2"),value.Condition,location?.Code??"—",value.OperatorName,"PDF",false,()=>Pdf(value,request,item,location)));
        }
        Content=new ScrollViewer{Content=rows,HorizontalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,VerticalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto};
    }
    private static Control Row(string date,string number,string request,string item,string quantity,string condition,string location,string user,string pdf,bool header,Action? action)
    {
        var grid=new Grid{ColumnDefinitions=new ColumnDefinitions("130,145,145,150,90,125,110,130,80")};var values=new[]{date,number,request,item,quantity,condition,location,user};
        for(var i=0;i<values.Length;i++){var text=new TextBlock{Text=values[i],FontWeight=header?FontWeight.SemiBold:FontWeight.Normal,VerticalAlignment=VerticalAlignment.Center,TextTrimming=TextTrimming.CharacterEllipsis,Margin=new Thickness(3)};Grid.SetColumn(text,i);grid.Children.Add(text);}
        Control last;if(header)last=new TextBlock{Text=pdf,FontWeight=FontWeight.SemiBold,VerticalAlignment=VerticalAlignment.Center};else{var button=new Button{Content="PDF",MinHeight=32};button.Click+=(_,_)=>action?.Invoke();last=button;}Grid.SetColumn(last,8);grid.Children.Add(last);
        return new Border{Background=header?UiTokens.Brush(UiTokens.SurfaceAlt):UiTokens.Brush(UiTokens.Surface),BorderBrush=UiTokens.Brush(UiTokens.Border),BorderThickness=new Thickness(0,0,0,1),Padding=new Thickness(8),Child=grid};
    }
    private static void Pdf(SparePartReturn value,SparePartPickRequest? request,SparePartInventoryItem? item,SparePartWarehouseLocation? location){if(request is null||item is null)return;var path=new SparePartReturnPdfService().Generate(value,request,item,location);Process.Start(new ProcessStartInfo{FileName=path,UseShellExecute=true});}
    private static string Date(string value)=>DateTime.TryParse(value,out var d)?d.ToString("dd/MM/yyyy HH:mm"):value;
}

public sealed class SparePartPickRequestDialog : Window
{
    private readonly IReadOnlyList<SparePartInventoryItem> _items;private readonly IReadOnlyList<SparePartWarehouseLocation> _locations;private readonly IReadOnlyList<MaintenanceTicket> _tickets;
    private readonly ComboBox _item=new(),_location=new(),_ticket=new();private readonly TextBox _quantity=new(){Text="1"},_requested=new(){Text=Environment.UserName},_technician=new(),_notes=new(){AcceptsReturn=true,MinHeight=70};private readonly TextBlock _message=new();
    public SparePartPickRequestDialog(IReadOnlyList<SparePartInventoryItem> items,IReadOnlyList<SparePartWarehouseLocation> locations,IReadOnlyList<MaintenanceTicket> tickets)
    {
        _items=items;_locations=locations;_tickets=tickets;Title="Nuova richiesta di prelievo";Width=560;Height=650;WindowStartupLocation=WindowStartupLocation.CenterOwner;
        _item.ItemsSource=items.Select(x=>$"{x.PartCode} · {x.Description} · disp. {x.Quantity:N2}").ToList();_item.SelectedIndex=0;
        _location.ItemsSource=new[]{"Automatico"}.Concat(locations.Select(x=>x.DisplayName)).ToList();_location.SelectedIndex=0;
        _ticket.ItemsSource=new[]{"Nessuna manutenzione"}.Concat(tickets.Select(x=>$"#{x.Id} · {x.Title}")).ToList();_ticket.SelectedIndex=0;
        var save=Button("Salva bozza",Confirm);Content=new ScrollViewer{Content=new StackPanel{Margin=new Thickness(24),Spacing=9,Children={new TextBlock{Text="Nuova richiesta di prelievo",FontSize=24,FontWeight=FontWeight.Bold},Field("Ricambio",_item),Field("Quantità",_quantity),Field("Ubicazione preferita",_location),Field("Manutenzione collegata",_ticket),Field("Richiedente",_requested),Field("Tecnico / destinatario",_technician),Field("Note",_notes),_message,save}}};
    }
    private void Confirm(){if(!decimal.TryParse(_quantity.Text,out var q)||q<=0){Error("Inserisci una quantità valida.");return;}var item=_items[_item.SelectedIndex];if(q>item.Quantity){Error($"Disponibilità totale insufficiente: {item.Quantity:N2}.");return;}Close(new SparePartPickRequest{InventoryItemId=item.Id,Quantity=q,PreferredLocationId=_location.SelectedIndex<=0?0:_locations[_location.SelectedIndex-1].Id,MaintenanceTicketId=_ticket.SelectedIndex<=0?0:_tickets[_ticket.SelectedIndex-1].Id,RequestedBy=_requested.Text?.Trim()??"",Technician=_technician.Text?.Trim()??"",Notes=_notes.Text?.Trim()??""});}
    private void Error(string text){_message.Text=text;_message.Foreground=UiTokens.Brush(UiTokens.Danger);}
    private static Control Field(string label,Control control)=>new StackPanel{Spacing=3,Children={new TextBlock{Text=label,FontWeight=FontWeight.SemiBold},control}};
    private static Button Button(string text,Action action){var b=new Button{Content=text,Height=40,Background=UiTokens.Brush(UiTokens.BrandBlue),Foreground=Brushes.White};b.Click+=(_,_)=>action();return b;}
}
