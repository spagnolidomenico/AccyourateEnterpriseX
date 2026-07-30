using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System.Text;
using Accyourate.App.AssetManagement.Models;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class SparePartsInventoryView : UserControl
{
    private readonly SparePartsInventoryRepository _repository = new();
    private readonly TextBox _search = new();
    private readonly CheckBox _lowOnly = new() { Content = "Solo sotto scorta" };
    private readonly Grid _kpis = new() { ColumnDefinitions = new ColumnDefinitions("*,*,*,*") };
    private StackPanel _rows = new();
    private readonly ContentControl _host = new();
    private readonly TextBlock _message = new();
    private readonly TextBlock _summary = new();

    public SparePartsInventoryView()
    {
        Background=UiTokens.Brush(UiTokens.Background);Content=Build();Load();
    }
    private Control Build()
    {
        var root=new DockPanel();
        var header=new Grid{ColumnDefinitions=new ColumnDefinitions("*,Auto"),Margin=new Thickness(24,20,24,12)};
        header.Children.Add(new StackPanel{Spacing=4,Children={
            new TextBlock{Text="Magazzino Ricambi",FontSize=30,FontWeight=FontWeight.Bold},
            new TextBlock{Text="Giacenze, valori, soglie minime e movimenti di magazzino.",Foreground=UiTokens.Brush(UiTokens.TextSecondary)}}});
        var headerActions=new StackPanel{Orientation=Orientation.Horizontal,Spacing=6};
        headerActions.Children.Add(Button("Registro movimenti",ShowAllMovements));
        headerActions.Children.Add(Button("Esporta CSV",ExportCsv));
        headerActions.Children.Add(Button("Nuovo ricambio",NewItem,true));
        Grid.SetColumn(headerActions,1);header.Children.Add(headerActions);
        DockPanel.SetDock(header,Dock.Top);root.Children.Add(header);
        _kpis.Margin=new Thickness(24,0,24,10);DockPanel.SetDock(_kpis,Dock.Top);root.Children.Add(_kpis);
        var filters=new Grid{ColumnDefinitions=new ColumnDefinitions("*,180"),Margin=new Thickness(24,0,24,8)};
        _search.Watermark="Cerca codice, descrizione, fornitore...";_search.TextChanged+=(_,_)=>Load();filters.Children.Add(_search);
        _lowOnly.HorizontalAlignment=HorizontalAlignment.Center;_lowOnly.VerticalAlignment=VerticalAlignment.Center;_lowOnly.IsCheckedChanged+=(_,_)=>Load();
        Grid.SetColumn(_lowOnly,1);filters.Children.Add(_lowOnly);DockPanel.SetDock(filters,Dock.Top);root.Children.Add(filters);
        _message.Margin=new Thickness(24,0,24,4);DockPanel.SetDock(_message,Dock.Top);root.Children.Add(_message);
        _summary.Margin=new Thickness(24,0,24,8);_summary.Foreground=UiTokens.Brush(UiTokens.TextSecondary);DockPanel.SetDock(_summary,Dock.Top);root.Children.Add(_summary);
        root.Children.Add(_host);return root;
    }
    private void Load()
    {
        try
        {
            var all=_repository.GetItems();var q=(_search.Text??"").Trim();
            var visible=all.Where(i=>_lowOnly.IsChecked!=true||i.IsLowStock)
                .Where(i=>q.Length==0||$"{i.PartCode} {i.Description} {i.Supplier} {i.Location}".Contains(q,StringComparison.OrdinalIgnoreCase)).ToList();
            BuildKpis(all);_summary.Text=_lowOnly.IsChecked==true
                ? $"Filtro attivo: solo sotto scorta · {visible.Count} di {all.Count} ricambi visualizzati"
                : $"{visible.Count} di {all.Count} ricambi visualizzati";
            _summary.Foreground=UiTokens.Brush(_lowOnly.IsChecked==true?UiTokens.Danger:UiTokens.TextSecondary);
            _rows=new StackPanel{MinWidth=1180};_rows.Children.Add(Header());
            for(var i=0;i<visible.Count;i++)_rows.Children.Add(Row(visible[i],i));
            _host.Content=new ScrollViewer{Content=_rows,Margin=new Thickness(24,0,24,24),HorizontalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,VerticalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto};
        }catch(Exception ex){Show($"Errore magazzino: {ex.Message}",true);}
    }
    private void BuildKpis(IReadOnlyList<SparePartInventoryItem> items)
    {
        _kpis.Children.Clear();
        AddKpi(0,"Articoli",items.Count,UiTokens.BrandBlue);AddKpi(1,"Unità disponibili",items.Sum(i=>i.Quantity).ToString("N2"),UiTokens.Success);
        AddKpi(2,"Sotto scorta",items.Count(i=>i.IsLowStock),UiTokens.Danger,true);AddKpi(3,"Valore magazzino",$"EUR {items.Sum(i=>i.StockValue):N2}",UiTokens.BrandBlue);
    }
    private void AddKpi(int col,string label,object value,string color,bool lowStockFilter=false){var active=lowStockFilter&&_lowOnly.IsChecked==true;var c=new Border{Background=UiTokens.Brush(active?UiTokens.SurfaceAlt:UiTokens.Surface),BorderBrush=UiTokens.Brush(active?UiTokens.Danger:UiTokens.Border),BorderThickness=new Thickness(active?2:1),CornerRadius=new CornerRadius(10),Padding=new Thickness(14,9),Margin=new Thickness(0,0,10,0),Child=new StackPanel{Children={new TextBlock{Text=value.ToString(),FontSize=22,FontWeight=FontWeight.Bold,Foreground=UiTokens.Brush(color)},new TextBlock{Text=active?"Sotto scorta · filtro attivo":label,Foreground=UiTokens.Brush(active?UiTokens.Danger:UiTokens.TextSecondary)}}}};if(lowStockFilter)c.PointerPressed+=(_,_)=>{_lowOnly.IsChecked=_lowOnly.IsChecked!=true;};Grid.SetColumn(c,col);_kpis.Children.Add(c);}
    private Control Header(){var g=GridRow();foreach(var x in new[]{("Codice",0),("Descrizione",1),("Fornitore",2),("Ubicazione",3),("Giacenza",4),("Minimo",5),("Valore",6),("Azione",7)})AddText(g,x.Item1,x.Item2,true);return new Border{Background=UiTokens.Brush(UiTokens.SurfaceAlt),Padding=new Thickness(9),Child=g};}
    private Control Row(SparePartInventoryItem item,int index)
    {
        var g=GridRow();AddText(g,item.PartCode,0,true);AddText(g,item.Description,1);AddText(g,item.Supplier,2);AddText(g,item.Location,3);
        AddText(g,item.IsLowStock?$"{item.Quantity:N2} · SOTTO SCORTA":item.Quantity.ToString("N2"),4,true,item.IsLowStock);AddText(g,item.MinimumQuantity.ToString("N2"),5);AddText(g,$"EUR {item.StockValue:N2}",6);
        var actions=new StackPanel{Orientation=Orientation.Horizontal,Spacing=4};
        actions.Children.Add(Button("Movimenti",()=>ShowMovements(item)));
        actions.Children.Add(Button("Carico / Scarico",()=>ManualMovement(item)));
        actions.Children.Add(Button("Modifica",()=>EditItem(item)));
        actions.Children.Add(Button("Rettifica",()=>Adjust(item)));
        Add(g,actions,7);
        return new Border{Background=UiTokens.Brush(index%2==0?UiTokens.Surface:UiTokens.SurfaceAlt),BorderBrush=UiTokens.Brush(item.IsLowStock?UiTokens.Danger:UiTokens.Border),BorderThickness=item.IsLowStock?new Thickness(3,0,0,1):new Thickness(0,0,0,1),Padding=new Thickness(9,6),Child=g};
    }
    private async void NewItem(){var owner=TopLevel.GetTopLevel(this) as Window;if(owner is null)return;var item=await new SparePartInventoryDialog().ShowDialog<SparePartInventoryItem?>(owner);if(item is null)return;_repository.SaveItem(item);Show("Ricambio salvato.");Load();}
    private async void EditItem(SparePartInventoryItem item){var owner=TopLevel.GetTopLevel(this) as Window;if(owner is null)return;var updated=await new SparePartInventoryDialog(item).ShowDialog<SparePartInventoryItem?>(owner);if(updated is null)return;_repository.SaveItem(updated);Show("Ricambio aggiornato.");Load();}
    private async void Adjust(SparePartInventoryItem item){var owner=TopLevel.GetTopLevel(this) as Window;if(owner is null)return;var value=await new InventoryAdjustmentDialog(item).ShowDialog<decimal?>(owner);if(value is null)return;_repository.Adjust(item.Id,value.Value,"Rettifica da Magazzino Ricambi");Show("Giacenza aggiornata.");Load();}
    private async void ShowMovements(SparePartInventoryItem item){var owner=TopLevel.GetTopLevel(this) as Window;if(owner is null)return;await new SparePartMovementsDialog(item,_repository.GetMovements(item.Id)).ShowDialog(owner);}
    private async void ShowAllMovements(){var owner=TopLevel.GetTopLevel(this) as Window;if(owner is null)return;await new InventoryMovementsWindow(_repository).ShowDialog(owner);}
    private async void ManualMovement(SparePartInventoryItem item)
    {
        var owner=TopLevel.GetTopLevel(this) as Window;if(owner is null)return;
        var request=await new ManualInventoryMovementDialog(item).ShowDialog<ManualInventoryMovementRequest?>(owner);
        if(request is null)return;
        try
        {
            _repository.RegisterManualMovement(item.Id,request.Inbound,request.Quantity,request.UnitCost,request.Reason,request.Reference,request.Notes);
            Show($"{(request.Inbound?"Carico":"Scarico")} registrato.");Load();
        }
        catch(Exception ex){Show($"Movimento non registrato: {ex.Message}",true);}
    }
    private void ExportCsv()
    {
        try
        {
            var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),"Accyourate Enterprise X","exports");
            Directory.CreateDirectory(folder);
            var path=Path.Combine(folder,$"magazzino_ricambi_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            var lines=new List<string>{"Codice;Descrizione;Fornitore;Ubicazione;Giacenza;Scorta minima;Costo medio;Valore;Sotto scorta"};
            foreach(var item in _repository.GetItems())
                lines.Add(string.Join(";",Csv(item.PartCode),Csv(item.Description),Csv(item.Supplier),Csv(item.Location),
                    item.Quantity.ToString("0.00"),item.MinimumQuantity.ToString("0.00"),item.AverageUnitCost.ToString("0.00"),
                    item.StockValue.ToString("0.00"),item.IsLowStock?"Sì":"No"));
            File.WriteAllLines(path,lines,new UTF8Encoding(true));
            Show($"Esportazione creata: {path}");
        }
        catch(Exception ex){Show($"Errore esportazione: {ex.Message}",true);}
    }
    private static string Csv(string value)=>$"\"{(value??string.Empty).Replace("\"","\"\"")}\"";
    private static Grid GridRow()=>new(){ColumnDefinitions=new ColumnDefinitions("110,220,180,130,170,90,120,430")};
    private static Button Button(string text,Action action,bool primary=false){var b=new Button{Content=text,MinHeight=34,Margin=new Thickness(3),Background=UiTokens.Brush(primary?UiTokens.BrandBlue:UiTokens.SurfaceAlt),Foreground=primary?Brushes.White:UiTokens.Brush(UiTokens.TextPrimary)};b.Click+=(_,_)=>action();return b;}
    private static void AddText(Grid g,string text,int col,bool strong=false,bool danger=false)=>Add(g,new TextBlock{Text=string.IsNullOrWhiteSpace(text)?"—":text,FontWeight=strong?FontWeight.SemiBold:FontWeight.Normal,Foreground=UiTokens.Brush(danger?UiTokens.Danger:strong?UiTokens.TextPrimary:UiTokens.TextSecondary),TextTrimming=TextTrimming.CharacterEllipsis,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(3)},col);
    private static void Add(Grid g,Control c,int col){Grid.SetColumn(c,col);g.Children.Add(c);}
    private void Show(string text,bool error=false){_message.Text=text;_message.Foreground=UiTokens.Brush(error?UiTokens.Danger:UiTokens.Success);}
}

public sealed class SparePartInventoryDialog : Window
{
    private readonly TextBox _code=new(),_description=new(),_supplier=new(),_location=new(),_quantity=new(){Text="0"},_minimum=new(){Text="0"},_cost=new(){Text="0"};private readonly TextBlock _message=new();
    private readonly SparePartInventoryItem? _existing;
    public SparePartInventoryDialog(SparePartInventoryItem? existing=null){_existing=existing;Title=existing is null?"Nuovo ricambio":"Modifica ricambio";Width=520;Height=560;WindowStartupLocation=WindowStartupLocation.CenterOwner;if(existing is not null){_code.Text=existing.PartCode;_description.Text=existing.Description;_supplier.Text=existing.Supplier;_location.Text=existing.Location;_quantity.Text=existing.Quantity.ToString("N2");_quantity.IsReadOnly=true;_minimum.Text=existing.MinimumQuantity.ToString("N2");_cost.Text=existing.AverageUnitCost.ToString("N2");}var r=new StackPanel{Margin=new Thickness(24),Spacing=9,Children={new TextBlock{Text=Title,FontSize=25,FontWeight=FontWeight.Bold},Field("Codice",_code),Field("Descrizione",_description),Field("Fornitore",_supplier),Field("Ubicazione",_location),Field(existing is null?"Giacenza iniziale":"Giacenza (usa Rettifica per modificarla)",_quantity),Field("Scorta minima",_minimum),Field("Costo medio",_cost),_message}};var b=Button("Salva",Confirm);r.Children.Add(b);Content=r;}
    private void Confirm(){if(string.IsNullOrWhiteSpace(_code.Text)||string.IsNullOrWhiteSpace(_description.Text)){_message.Text="Inserisci codice e descrizione.";return;}decimal.TryParse(_quantity.Text,out var q);decimal.TryParse(_minimum.Text,out var m);decimal.TryParse(_cost.Text,out var c);Close(new SparePartInventoryItem{Id=_existing?.Id??0,PartCode=_code.Text.Trim(),Description=_description.Text.Trim(),Supplier=_supplier.Text?.Trim()??"",Location=_location.Text?.Trim()??"",Quantity=Math.Max(0,q),MinimumQuantity=Math.Max(0,m),AverageUnitCost=Math.Max(0,c),UpdatedAt=_existing?.UpdatedAt??DateTime.Now.ToString("s")});}
    private static Control Field(string l,Control c)=>new StackPanel{Spacing=3,Children={new TextBlock{Text=l,FontWeight=FontWeight.SemiBold},c}};private static Button Button(string t,Action a){var b=new Button{Content=t,Height=38,Background=UiTokens.Brush(UiTokens.BrandBlue),Foreground=Brushes.White};b.Click+=(_,_)=>a();return b;}
}
public sealed class InventoryAdjustmentDialog : Window
{
    private readonly TextBox _quantity=new();public InventoryAdjustmentDialog(SparePartInventoryItem item){Title="Rettifica giacenza";Width=420;Height=240;WindowStartupLocation=WindowStartupLocation.CenterOwner;_quantity.Text=item.Quantity.ToString("N2");var b=new Button{Content="Conferma",Height=38};b.Click+=(_,_)=>{if(decimal.TryParse(_quantity.Text,out var q)&&q>=0)Close(q);};Content=new StackPanel{Margin=new Thickness(24),Spacing=12,Children={new TextBlock{Text=item.Description,FontSize=21,FontWeight=FontWeight.Bold},new TextBlock{Text="Nuova giacenza"},_quantity,b}};}
}

public sealed class SparePartMovementsDialog : Window
{
    public SparePartMovementsDialog(SparePartInventoryItem item,IReadOnlyList<SparePartInventoryMovement> movements)
    {
        Title=$"Movimenti - {item.PartCode}";
        Width=900;
        Height=560;
        MinWidth=720;
        MinHeight=420;
        WindowStartupLocation=WindowStartupLocation.CenterOwner;

        var rows=new StackPanel{MinWidth=820};
        rows.Children.Add(Row("Data","Tipo","Quantità","Costo unitario","Riferimento","Note",true));
        foreach(var movement in movements)
        {
            var quantity=movement.Quantity>0?$"+{movement.Quantity:N2}":movement.Quantity.ToString("N2");
            rows.Children.Add(Row(
                DateTime.TryParse(movement.CreatedAt,out var created)?created.ToString("dd/MM/yyyy HH:mm"):movement.CreatedAt,
                movement.MovementType,
                quantity,
                $"EUR {movement.UnitCost:N2}",
                movement.Reference,
                movement.Notes,
                false));
        }

        Control content=movements.Count==0
            ? new TextBlock{Text="Non sono ancora presenti movimenti per questo ricambio.",Margin=new Thickness(0,22),Foreground=UiTokens.Brush(UiTokens.TextSecondary)}
            : new ScrollViewer{Content=rows,HorizontalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,VerticalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto};

        Content=new Grid
        {
            RowDefinitions=new RowDefinitions("Auto,*"),
            Margin=new Thickness(24),
            Children=
            {
                new StackPanel
                {
                    Spacing=3,
                    Margin=new Thickness(0,0,0,16),
                    Children=
                    {
                        new TextBlock{Text="Storico movimenti",FontSize=25,FontWeight=FontWeight.Bold},
                        new TextBlock{Text=$"{item.PartCode} · {item.Description} · Giacenza attuale {item.Quantity:N2}",Foreground=UiTokens.Brush(UiTokens.TextSecondary)}
                    }
                },
                content
            }
        };
        Grid.SetRow(content,1);
    }

    private static Control Row(string date,string type,string quantity,string cost,string reference,string notes,bool header)
    {
        var grid=new Grid{ColumnDefinitions=new ColumnDefinitions("145,100,90,120,150,215")};
        Add(grid,date,0,header);Add(grid,type,1,header);Add(grid,quantity,2,header);Add(grid,cost,3,header);Add(grid,reference,4,header);Add(grid,notes,5,header);
        return new Border
        {
            Background=header?UiTokens.Brush(UiTokens.SurfaceAlt):UiTokens.Brush(UiTokens.Surface),
            BorderBrush=UiTokens.Brush(UiTokens.Border),
            BorderThickness=new Thickness(0,0,0,1),
            Padding=new Thickness(8,9),
            Child=grid
        };
    }

    private static void Add(Grid grid,string value,int column,bool header)
    {
        var text=new TextBlock
        {
            Text=string.IsNullOrWhiteSpace(value)?"—":value,
            FontWeight=header?FontWeight.SemiBold:FontWeight.Normal,
            TextTrimming=TextTrimming.CharacterEllipsis,
            VerticalAlignment=VerticalAlignment.Center,
            Margin=new Thickness(3)
        };
        Grid.SetColumn(text,column);
        grid.Children.Add(text);
    }
}
