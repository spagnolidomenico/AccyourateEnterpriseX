using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Models;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class ManualInventoryMovementRequest
{
    public bool Inbound { get; init; }
    public decimal Quantity { get; init; }
    public decimal UnitCost { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string Reference { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
    public int? LocationId { get; init; }
}

public sealed class ManualInventoryMovementDialog : Window
{
    private readonly ComboBox _direction = new() { ItemsSource = new[] { "Carico", "Scarico" }, SelectedIndex = 0 };
    private readonly ComboBox _reason = new() { ItemsSource = new[] { "Acquisto", "Consumo", "Reso", "Trasferimento", "Rettifica" }, SelectedIndex = 0 };
    private readonly TextBox _quantity = new() { Text = "1" };
    private readonly TextBox _cost = new() { Text = "0" };
    private readonly TextBox _reference = new();
    private readonly TextBox _notes = new() { AcceptsReturn = true, MinHeight = 75 };
    private readonly TextBlock _message = new();
    private readonly ComboBox _location = new();
    private readonly IReadOnlyList<SparePartWarehouseLocation> _locations;

    public ManualInventoryMovementDialog(SparePartInventoryItem item,IReadOnlyList<SparePartWarehouseLocation> locations)
    {
        _locations=locations;
        Title = "Carico / Scarico ricambio";
        Width = 520;
        Height = 650;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        _cost.Text = item.AverageUnitCost.ToString("N2");
        _direction.SelectionChanged+=(_,_)=>RefreshLocations();
        RefreshLocations();
        var save = Button("Registra movimento", () => Confirm(item));
        Content = new ScrollViewer
        {
            Content = new StackPanel
            {
                Margin = new Thickness(24),
                Spacing = 10,
                Children =
                {
                    new TextBlock{Text="Movimento manuale",FontSize=25,FontWeight=FontWeight.Bold},
                    new TextBlock{Text=$"{item.PartCode} · {item.Description}",Foreground=UiTokens.Brush(UiTokens.TextSecondary)},
                    new TextBlock{Text=$"Giacenza disponibile: {item.Quantity:N2}",FontWeight=FontWeight.SemiBold},
                    Field("Operazione",_direction),Field("Causale",_reason),Field("Quantità",_quantity),
                    Field("Ubicazione (per lo scarico: preferita, poi prelievo automatico)",_location),
                    Field("Costo unitario (utilizzato per i carichi)",_cost),Field("Riferimento",_reference),
                    Field("Note",_notes),_message,save
                }
            }
        };
    }

    private void Confirm(SparePartInventoryItem item)
    {
        if(!decimal.TryParse(_quantity.Text,out var quantity)||quantity<=0){Error("Inserisci una quantità maggiore di zero.");return;}
        decimal.TryParse(_cost.Text,out var cost);
        var inbound=_direction.SelectedIndex==0;
        if(!inbound&&quantity>item.Quantity){Error($"Giacenza insufficiente: disponibili {item.Quantity:N2}.");return;}
        var locationIndex=_location.SelectedIndex-(inbound?0:1);
        int? locationId=locationIndex>=0&&locationIndex<_locations.Count?_locations[locationIndex].Id:null;
        if(inbound&&locationId is null){Error("Seleziona l'ubicazione di destinazione.");return;}
        Close(new ManualInventoryMovementRequest
        {
            Inbound=inbound,Quantity=quantity,UnitCost=Math.Max(0,cost),
            Reason=_reason.SelectedItem?.ToString()??"Movimento",
            Reference=_reference.Text?.Trim()??string.Empty,Notes=_notes.Text?.Trim()??string.Empty,
            LocationId=locationId
        });
    }

    private void RefreshLocations()
    {
        var inbound=_direction.SelectedIndex==0;
        var labels=_locations.Select(x=>x.DisplayName).ToList();
        if(!inbound)labels.Insert(0,"Automatico (prelievo da più ubicazioni)");
        _location.ItemsSource=labels;_location.SelectedIndex=labels.Count>0?0:-1;
    }

    private void Error(string text){_message.Text=text;_message.Foreground=UiTokens.Brush(UiTokens.Danger);}
    private static Control Field(string label,Control control)=>new StackPanel{Spacing=3,Children={new TextBlock{Text=label,FontWeight=FontWeight.SemiBold},control}};
    private static Button Button(string text,Action action){var button=new Button{Content=text,Height=40,Background=UiTokens.Brush(UiTokens.BrandBlue),Foreground=Brushes.White};button.Click+=(_,_)=>action();return button;}
}

public sealed class InventoryMovementsWindow : Window
{
    private readonly SparePartsInventoryRepository _repository;
    private readonly TextBox _search = new() { Watermark = "Cerca ricambio, causale, riferimento..." };
    private readonly ComboBox _type = new() { ItemsSource = new[] { "Tutti", "Carico", "Scarico", "Rettifica" }, SelectedIndex = 0 };
    private readonly DatePicker _from = new();
    private readonly DatePicker _to = new();
    private readonly ContentControl _host = new();
    private readonly TextBlock _summary = new();
    private IReadOnlyDictionary<int,SparePartInventoryItem> _items = new Dictionary<int,SparePartInventoryItem>();

    public InventoryMovementsWindow(SparePartsInventoryRepository repository)
    {
        _repository=repository;
        Title="Registro movimenti magazzino";
        Width=1180;Height=720;MinWidth=900;MinHeight=520;
        WindowStartupLocation=WindowStartupLocation.CenterOwner;
        _search.TextChanged+=(_,_)=>Load();
        _type.SelectionChanged+=(_,_)=>Load();
        _from.SelectedDateChanged+=(_,_)=>Load();
        _to.SelectedDateChanged+=(_,_)=>Load();
        Content=Build();
        Load();
    }

    private Control Build()
    {
        var root=new DockPanel{Margin=new Thickness(24)};
        var title=new StackPanel{Spacing=3,Margin=new Thickness(0,0,0,14),Children={new TextBlock{Text="Registro movimenti",FontSize=27,FontWeight=FontWeight.Bold},new TextBlock{Text="Carichi, scarichi e rettifiche con saldo precedente e successivo.",Foreground=UiTokens.Brush(UiTokens.TextSecondary)}}};
        DockPanel.SetDock(title,Dock.Top);root.Children.Add(title);
        var filters=new Grid{ColumnDefinitions=new ColumnDefinitions("*,160,150,150"),Margin=new Thickness(0,0,0,10)};
        Add(filters,_search,0);Add(filters,_type,1);Add(filters,_from,2);Add(filters,_to,3);
        DockPanel.SetDock(filters,Dock.Top);root.Children.Add(filters);
        _summary.Margin=new Thickness(0,0,0,8);_summary.Foreground=UiTokens.Brush(UiTokens.TextSecondary);
        DockPanel.SetDock(_summary,Dock.Top);root.Children.Add(_summary);
        root.Children.Add(_host);return root;
    }

    private void Load()
    {
        _items=_repository.GetItems().ToDictionary(x=>x.Id);
        var query=(_search.Text??string.Empty).Trim();
        var selected=_type.SelectedItem?.ToString()??"Tutti";
        var movements=_repository.GetAllMovements()
            .Where(x=>selected=="Tutti"||x.MovementType.StartsWith(selected,StringComparison.OrdinalIgnoreCase))
            .Where(x=>!_from.SelectedDate.HasValue||Parse(x.CreatedAt)>=_from.SelectedDate.Value.Date)
            .Where(x=>!_to.SelectedDate.HasValue||Parse(x.CreatedAt)<_to.SelectedDate.Value.Date.AddDays(1))
            .Where(x=>
            {
                _items.TryGetValue(x.InventoryItemId,out var item);
                return query.Length==0||$"{item?.PartCode} {item?.Description} {x.MovementType} {x.Reference} {x.Notes}".Contains(query,StringComparison.OrdinalIgnoreCase);
            }).ToList();
        _summary.Text=$"{movements.Count} movimenti visualizzati · carichi {movements.Where(x=>x.Quantity>0).Sum(x=>x.Quantity):N2} · scarichi {Math.Abs(movements.Where(x=>x.Quantity<0).Sum(x=>x.Quantity)):N2}";
        var rows=new StackPanel{MinWidth=1080};
        rows.Children.Add(Row("Data","Ricambio","Tipo","Quantità","Prima","Dopo","Costo","Riferimento","Note",true));
        foreach(var movement in movements)
        {
            _items.TryGetValue(movement.InventoryItemId,out var item);
            rows.Children.Add(Row(
                Parse(movement.CreatedAt).ToString("dd/MM/yyyy HH:mm"),
                item is null?$"ID {movement.InventoryItemId}":$"{item.PartCode} · {item.Description}",
                movement.MovementType,
                movement.Quantity>0?$"+{movement.Quantity:N2}":movement.Quantity.ToString("N2"),
                movement.BalanceBefore.ToString("N2"),movement.BalanceAfter.ToString("N2"),
                $"EUR {movement.UnitCost:N2}",movement.Reference,movement.Notes,false));
        }
        _host.Content=new ScrollViewer{Content=rows,HorizontalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,VerticalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto};
    }

    private static DateTimeOffset Parse(string value)=>DateTimeOffset.TryParse(value,out var result)?result:DateTimeOffset.MinValue;
    private static Control Row(string date,string item,string type,string quantity,string before,string after,string cost,string reference,string notes,bool header)
    {
        var grid=new Grid{ColumnDefinitions=new ColumnDefinitions("130,220,150,85,75,75,100,130,190")};
        var values=new[]{date,item,type,quantity,before,after,cost,reference,notes};
        for(var i=0;i<values.Length;i++)
        {
            var text=new TextBlock{Text=string.IsNullOrWhiteSpace(values[i])?"—":values[i],FontWeight=header?FontWeight.SemiBold:FontWeight.Normal,TextTrimming=TextTrimming.CharacterEllipsis,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(3)};
            Grid.SetColumn(text,i);grid.Children.Add(text);
        }
        return new Border{Background=header?UiTokens.Brush(UiTokens.SurfaceAlt):UiTokens.Brush(UiTokens.Surface),BorderBrush=UiTokens.Brush(UiTokens.Border),BorderThickness=new Thickness(0,0,0,1),Padding=new Thickness(7,9),Child=grid};
    }
    private static void Add(Grid grid,Control control,int column){control.Margin=new Thickness(0,0,8,0);Grid.SetColumn(control,column);grid.Children.Add(control);}
}
