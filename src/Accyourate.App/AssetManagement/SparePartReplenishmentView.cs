using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Models;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class SparePartReplenishmentView : UserControl
{
    private readonly SparePartReplenishmentRepository _repository=new();
    private readonly SparePartsInventoryRepository _inventory=new();
    private readonly MaintenancePurchasingRepository _purchasing=new();
    private readonly TextBox _search=new();
    private readonly ComboBox _status=new();
    private readonly Grid _kpis=new(){ColumnDefinitions=new ColumnDefinitions("*,*,*,*")};
    private readonly StackPanel _rows=new();
    private readonly TextBlock _message=new();
    private readonly TextBlock _summary=new();

    public SparePartReplenishmentView()
    {
        Background=UiTokens.Brush(UiTokens.Background);
        Content=Build();Load();
    }

    private Control Build()
    {
        var root=new DockPanel();
        var header=new Grid{ColumnDefinitions=new ColumnDefinitions("*,Auto"),Margin=new Thickness(24,20,24,12)};
        header.Children.Add(new StackPanel{Spacing=4,Children={
            new TextBlock{Text="Approvvigionamento Ricambi",FontSize=30,FontWeight=FontWeight.Bold},
            new TextBlock{Text="Scorte critiche, richieste, approvazioni e ordini d'acquisto.",Foreground=UiTokens.Brush(UiTokens.TextSecondary)}}});
        var generate=Button("Genera da sotto scorta",Generate,true);Grid.SetColumn(generate,1);header.Children.Add(generate);
        DockPanel.SetDock(header,Dock.Top);root.Children.Add(header);
        _kpis.Margin=new Thickness(24,0,24,10);DockPanel.SetDock(_kpis,Dock.Top);root.Children.Add(_kpis);
        var filters=new Grid{ColumnDefinitions=new ColumnDefinitions("*,190"),Margin=new Thickness(24,0,24,8)};
        _search.Watermark="Cerca richiesta, ricambio o fornitore...";_search.TextChanged+=(_,_)=>Load();filters.Children.Add(_search);
        _status.ItemsSource=new[]{"Tutti gli stati",ReplenishmentRequestStatus.Draft,ReplenishmentRequestStatus.Approved,ReplenishmentRequestStatus.Ordered,ReplenishmentRequestStatus.Completed,ReplenishmentRequestStatus.Cancelled};
        _status.SelectedIndex=0;_status.SelectionChanged+=(_,_)=>Load();Grid.SetColumn(_status,1);filters.Children.Add(_status);
        DockPanel.SetDock(filters,Dock.Top);root.Children.Add(filters);
        _message.Margin=new Thickness(24,0,24,4);DockPanel.SetDock(_message,Dock.Top);root.Children.Add(_message);
        _summary.Margin=new Thickness(24,0,24,8);_summary.Foreground=UiTokens.Brush(UiTokens.TextSecondary);DockPanel.SetDock(_summary,Dock.Top);root.Children.Add(_summary);
        root.Children.Add(new ScrollViewer{Content=_rows,Margin=new Thickness(24,0,24,24),HorizontalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,VerticalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto});
        return root;
    }

    private void Load()
    {
        try
        {
            var requests=_repository.GetAll();var items=_inventory.GetItems().ToDictionary(x=>x.Id);
            var suppliers=_purchasing.GetSuppliers().ToDictionary(x=>x.Id);
            BuildKpis(requests,items.Values);
            var query=(_search.Text??"").Trim();
            var visible=requests.Where(x=>_status.SelectedIndex<=0||x.Status==_status.SelectedItem?.ToString())
                .Where(x=>{items.TryGetValue(x.InventoryItemId,out var item);suppliers.TryGetValue(x.SupplierId,out var supplier);return query.Length==0||$"{x.RequestNumber} {item?.PartCode} {item?.Description} {supplier?.Name}".Contains(query,StringComparison.OrdinalIgnoreCase);}).ToList();
            _rows.Children.Clear();_rows.MinWidth=1230;_rows.Children.Add(Header());
            for(var i=0;i<visible.Count;i++){items.TryGetValue(visible[i].InventoryItemId,out var item);suppliers.TryGetValue(visible[i].SupplierId,out var supplier);_rows.Children.Add(Row(visible[i],item,supplier,i));}
            _summary.Text=$"{visible.Count} richieste visualizzate · {items.Values.Count(x=>x.IsLowStock)} ricambi sotto scorta";
        }
        catch(Exception ex){Show($"Errore approvvigionamento: {ex.Message}",true);}
    }

    private void BuildKpis(IReadOnlyList<SparePartReplenishmentRequest> requests,IEnumerable<SparePartInventoryItem> items)
    {
        _kpis.Children.Clear();
        AddKpi(0,"Sotto scorta",items.Count(x=>x.IsLowStock),UiTokens.Danger);
        AddKpi(1,"Da approvare",requests.Count(x=>x.Status==ReplenishmentRequestStatus.Draft),UiTokens.Warning);
        AddKpi(2,"Ordinate",requests.Count(x=>x.Status==ReplenishmentRequestStatus.Ordered),UiTokens.BrandBlue);
        AddKpi(3,"Completate",requests.Count(x=>x.Status==ReplenishmentRequestStatus.Completed),UiTokens.Success);
    }

    private void Generate()
    {
        try
        {
            var suppliers=_purchasing.GetSuppliers();
            var created=0;var skipped=0;
            foreach(var item in _inventory.GetItems().Where(x=>x.IsLowStock))
            {
                var target=Math.Max(item.MinimumQuantity*2,item.MinimumQuantity+1);
                var quantity=Math.Max(1,target-item.Quantity);
                var supplier=suppliers.FirstOrDefault(x=>string.Equals(x.Name,item.Supplier,StringComparison.OrdinalIgnoreCase));
                try
                {
                    _repository.Create(new SparePartReplenishmentRequest
                    {
                        InventoryItemId=item.Id,SupplierId=supplier?.Id??0,SuggestedQuantity=quantity,
                        RequestedQuantity=quantity,Notes="Proposta automatica da soglia minima"
                    });
                    created++;
                }
                catch(InvalidOperationException){skipped++;}
            }
            Show(created==0?$"Nessuna nuova richiesta. {skipped} già aperte.":$"Create {created} richieste; {skipped} già presenti.");
            Load();
        }
        catch(Exception ex){Show($"Errore generazione richieste: {ex.Message}",true);}
    }

    private Control Header()
    {
        var grid=GridRow();
        foreach(var x in new[]{("Richiesta",0),("Ricambio",1),("Giacenza",2),("Quantità",3),("Fornitore",4),("Stato",5),("Ordine",6),("Azioni",7)})AddText(grid,x.Item1,x.Item2,true);
        return new Border{Background=UiTokens.Brush(UiTokens.SurfaceAlt),Padding=new Thickness(9),Child=grid};
    }

    private Control Row(SparePartReplenishmentRequest request,SparePartInventoryItem? item,MaintenanceSupplier? supplier,int index)
    {
        var grid=GridRow();AddText(grid,request.RequestNumber,0,true);AddText(grid,item is null?$"#{request.InventoryItemId}":$"{item.PartCode} · {item.Description}",1);
        AddText(grid,item?.Quantity.ToString("N2")??"—",2,item?.IsLowStock==true,item?.IsLowStock==true);
        AddText(grid,request.RequestedQuantity.ToString("N2"),3,true);AddText(grid,supplier?.Name??"Da selezionare",4,false,supplier is null);
        Add(grid,Badge(request.Status,StatusColor(request.Status)),5);AddText(grid,request.PurchaseOrderId>0?$"#{request.PurchaseOrderId}":"—",6);
        var actions=new StackPanel{Orientation=Orientation.Horizontal,Spacing=4};
        var edit=Button("Modifica",()=>Edit(request));edit.IsEnabled=request.Status is ReplenishmentRequestStatus.Draft or ReplenishmentRequestStatus.Approved;actions.Children.Add(edit);
        var approve=Button("Approva",()=>Approve(request));approve.IsEnabled=request.Status==ReplenishmentRequestStatus.Draft;actions.Children.Add(approve);
        var order=Button("Crea ordine",()=>CreateOrder(request,item));order.IsEnabled=request.Status==ReplenishmentRequestStatus.Approved;actions.Children.Add(order);
        Add(grid,actions,7);
        return new Border{Background=UiTokens.Brush(index%2==0?UiTokens.Surface:UiTokens.SurfaceAlt),BorderBrush=UiTokens.Brush(UiTokens.Border),BorderThickness=new Thickness(0,0,0,1),Padding=new Thickness(9,6),Child=grid};
    }

    private async void Edit(SparePartReplenishmentRequest request)
    {
        var suppliers=_purchasing.GetSuppliers();if(suppliers.Count==0){Show("Crea prima un fornitore in Acquisti e fornitori.",true);return;}
        var owner=TopLevel.GetTopLevel(this) as Window;if(owner is null)return;
        var result=await new ReplenishmentRequestDialog(request,suppliers).ShowDialog<ReplenishmentRequestEdit?>(owner);if(result is null)return;
        _repository.UpdateDetails(request.Id,result.SupplierId,result.Quantity,result.Notes);Show("Richiesta aggiornata.");Load();
    }

    private void Approve(SparePartReplenishmentRequest request)
    {
        if(request.SupplierId<=0){Show("Seleziona un fornitore con Modifica prima di approvare.",true);return;}
        if(request.RequestedQuantity<=0){Show("La quantità richiesta deve essere maggiore di zero.",true);return;}
        _repository.SetStatus(request.Id,ReplenishmentRequestStatus.Approved);Show("Richiesta approvata.");Load();
    }

    private void CreateOrder(SparePartReplenishmentRequest request,SparePartInventoryItem? item)
    {
        try
        {
            if(item is null)throw new InvalidOperationException("Ricambio non trovato.");
            if(request.SupplierId<=0)throw new InvalidOperationException("Fornitore non selezionato.");
            var order=new MaintenancePurchaseOrder
            {
                SupplierId=request.SupplierId,Status=PurchaseOrderStatus.Draft,OrderDate=DateTime.Today.ToString("s"),
                ExpectedDate=DateTime.Today.AddDays(7).ToString("s"),Notes=$"Generato da {request.RequestNumber}",
                Lines={new MaintenancePurchaseOrderLine{PartCode=item.PartCode,Description=item.Description,Quantity=request.RequestedQuantity,UnitCost=item.AverageUnitCost}}
            };
            _purchasing.CreateOrder(order);_repository.LinkOrder(request.Id,order.Id);
            Show($"Ordine {order.OrderNumber} creato in bozza.");Load();
        }
        catch(Exception ex){Show($"Ordine non creato: {ex.Message}",true);}
    }

    private static Grid GridRow()=>new(){ColumnDefinitions=new ColumnDefinitions("150,250,90,90,180,115,85,270")};
    private void AddKpi(int col,string label,object value,string color){var card=new Border{Background=UiTokens.Brush(UiTokens.Surface),BorderBrush=UiTokens.Brush(UiTokens.Border),BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(10),Padding=new Thickness(14,9),Margin=new Thickness(0,0,10,0),Child=new StackPanel{Children={new TextBlock{Text=value.ToString(),FontSize=22,FontWeight=FontWeight.Bold,Foreground=UiTokens.Brush(color)},new TextBlock{Text=label,Foreground=UiTokens.Brush(UiTokens.TextSecondary)}}}};Grid.SetColumn(card,col);_kpis.Children.Add(card);}
    private static string StatusColor(string status)=>status switch{ReplenishmentRequestStatus.Completed=>UiTokens.Success,ReplenishmentRequestStatus.Cancelled=>UiTokens.Danger,ReplenishmentRequestStatus.Approved=>UiTokens.Warning,_=>UiTokens.BrandBlue};
    private static Control Badge(string text,string color)=>new Border{BorderBrush=UiTokens.Brush(color),BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(9),Padding=new Thickness(7,4),Margin=new Thickness(3),Child=new TextBlock{Text=text,Foreground=UiTokens.Brush(color),HorizontalAlignment=HorizontalAlignment.Center,FontWeight=FontWeight.Bold,FontSize=11}};
    private static Button Button(string text,Action action,bool primary=false){var button=new Button{Content=text,MinHeight=34,Margin=new Thickness(3),Background=UiTokens.Brush(primary?UiTokens.BrandBlue:UiTokens.SurfaceAlt),Foreground=primary?Brushes.White:UiTokens.Brush(UiTokens.TextPrimary)};button.Click+=(_,_)=>action();return button;}
    private static void AddText(Grid grid,string text,int column,bool strong=false,bool danger=false)=>Add(grid,new TextBlock{Text=string.IsNullOrWhiteSpace(text)?"—":text,FontWeight=strong?FontWeight.SemiBold:FontWeight.Normal,Foreground=UiTokens.Brush(danger?UiTokens.Danger:strong?UiTokens.TextPrimary:UiTokens.TextSecondary),TextTrimming=TextTrimming.CharacterEllipsis,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(3)},column);
    private static void Add(Grid grid,Control control,int column){Grid.SetColumn(control,column);grid.Children.Add(control);}
    private void Show(string text,bool error=false){_message.Text=text;_message.Foreground=UiTokens.Brush(error?UiTokens.Danger:UiTokens.Success);}
}

public sealed class ReplenishmentRequestEdit
{
    public int SupplierId{get;init;}public decimal Quantity{get;init;}public string Notes{get;init;}="";
}

public sealed class ReplenishmentRequestDialog : Window
{
    private readonly ComboBox _supplier=new();private readonly TextBox _quantity=new();private readonly TextBox _notes=new(){AcceptsReturn=true,MinHeight=90};private readonly TextBlock _message=new();
    private readonly IReadOnlyList<MaintenanceSupplier> _suppliers;
    public ReplenishmentRequestDialog(SparePartReplenishmentRequest request,IReadOnlyList<MaintenanceSupplier> suppliers)
    {
        _suppliers=suppliers;
        Title="Modifica richiesta";Width=500;Height=440;WindowStartupLocation=WindowStartupLocation.CenterOwner;
        _supplier.ItemsSource=suppliers.Select(x=>x.Name).ToList();
        var selected=suppliers.ToList().FindIndex(x=>x.Id==request.SupplierId);_supplier.SelectedIndex=selected>=0?selected:0;
        _quantity.Text=request.RequestedQuantity.ToString("N2");_notes.Text=request.Notes;
        var save=new Button{Content="Salva",Height=40,Background=UiTokens.Brush(UiTokens.BrandBlue),Foreground=Brushes.White};
        save.Click+=(_,_)=>{if(_supplier.SelectedIndex<0||_supplier.SelectedIndex>=_suppliers.Count){Error("Seleziona il fornitore.");return;}if(!decimal.TryParse(_quantity.Text,out var quantity)||quantity<=0){Error("Inserisci una quantità valida.");return;}Close(new ReplenishmentRequestEdit{SupplierId=_suppliers[_supplier.SelectedIndex].Id,Quantity=quantity,Notes=_notes.Text?.Trim()??""});};
        Content=new StackPanel{Margin=new Thickness(24),Spacing=10,Children={new TextBlock{Text=request.RequestNumber,FontSize=24,FontWeight=FontWeight.Bold},Field("Fornitore",_supplier),Field("Quantità richiesta",_quantity),Field("Note",_notes),_message,save}};
    }
    private void Error(string text){_message.Text=text;_message.Foreground=UiTokens.Brush(UiTokens.Danger);}
    private static Control Field(string label,Control control)=>new StackPanel{Spacing=3,Children={new TextBlock{Text=label,FontWeight=FontWeight.SemiBold},control}};
}
