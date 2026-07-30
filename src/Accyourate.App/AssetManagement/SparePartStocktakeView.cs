using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Input;
using System.Text;
using System.Diagnostics;
using Accyourate.App.AssetManagement.Models;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class SparePartStocktakeView : UserControl
{
    private readonly SparePartStocktakeRepository _repository=new();
    private readonly SparePartsInventoryRepository _inventory=new();
    private readonly StackPanel _rows=new();
    private readonly TextBlock _message=new();
    private readonly Grid _kpis=new(){ColumnDefinitions=new ColumnDefinitions("*,*,*,*")};
    private readonly SparePartLabelPdfService _labels=new();
    public SparePartStocktakeView(){Background=UiTokens.Brush(UiTokens.Background);Content=Build();Load();}
    private Control Build()
    {
        var root=new DockPanel();
        var header=new Grid{ColumnDefinitions=new ColumnDefinitions("*,Auto"),Margin=new Thickness(24,20,24,12)};
        header.Children.Add(new StackPanel{Spacing=4,Children={new TextBlock{Text="Inventario Fisico Ricambi",FontSize=30,FontWeight=FontWeight.Bold},new TextBlock{Text="Conteggi, differenze, riconciliazione e valore economico.",Foreground=UiTokens.Brush(UiTokens.TextSecondary)}}});
        var actions=new StackPanel{Orientation=Orientation.Horizontal,Spacing=6};
        actions.Children.Add(Button("Etichette QR",PrintLabels));
        actions.Children.Add(Button("Nuova sessione",NewSession,true));
        Grid.SetColumn(actions,1);header.Children.Add(actions);
        DockPanel.SetDock(header,Dock.Top);root.Children.Add(header);
        _kpis.Margin=new Thickness(24,0,24,10);DockPanel.SetDock(_kpis,Dock.Top);root.Children.Add(_kpis);
        _message.Margin=new Thickness(24,0,24,8);DockPanel.SetDock(_message,Dock.Top);root.Children.Add(_message);
        root.Children.Add(new ScrollViewer{Content=_rows,Margin=new Thickness(24,0,24,24),HorizontalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,VerticalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto});
        return root;
    }
    private void Load()
    {
        try
        {
            var sessions=_repository.GetAll();BuildKpis(sessions);_rows.Children.Clear();_rows.MinWidth=1080;_rows.Children.Add(Header());
            for(var i=0;i<sessions.Count;i++)_rows.Children.Add(Row(sessions[i],i));
        }catch(Exception ex){Show($"Errore inventario: {ex.Message}",true);}
    }
    private async void NewSession()
    {
        var owner=TopLevel.GetTopLevel(this) as Window;if(owner is null)return;
        var description=await new NewStocktakeDialog().ShowDialog<string?>(owner);if(string.IsNullOrWhiteSpace(description))return;
        try{_repository.Create(description,Environment.UserName,_inventory.GetItems());Show("Sessione inventariale creata.");Load();}
        catch(Exception ex){Show($"Sessione non creata: {ex.Message}",true);}
    }
    private void PrintLabels()
    {
        try
        {
            var path=_labels.Generate(_inventory.GetItems());
            Process.Start(new ProcessStartInfo{FileName=path,UseShellExecute=true});
            Show($"Etichette create: {path}");
        }
        catch(Exception ex){Show($"Etichette non create: {ex.Message}",true);}
    }
    private async void Open(SparePartStocktake session)
    {
        var owner=TopLevel.GetTopLevel(this) as Window;if(owner is null)return;
        await new StocktakeSessionWindow(_repository,_inventory,session.Id).ShowDialog(owner);Load();
    }
    private void Export(SparePartStocktake session)
    {
        try
        {
            session=_repository.Get(session.Id);
            var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),"Accyourate Enterprise X","exports");Directory.CreateDirectory(folder);
            var path=Path.Combine(folder,$"inventario_{session.SessionNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            var lines=new List<string>{"Codice;Descrizione;Quantità registrata;Quantità contata;Differenza;Costo unitario;Valore differenza;Note"};
            foreach(var line in session.Lines)lines.Add(string.Join(";",Csv(line.PartCode),Csv(line.Description),line.ExpectedQuantity.ToString("0.00"),line.CountedQuantity?.ToString("0.00")??"",line.Difference.ToString("0.00"),line.UnitCost.ToString("0.00"),line.DifferenceValue.ToString("0.00"),Csv(line.Notes)));
            File.WriteAllLines(path,lines,new UTF8Encoding(true));Show($"Report creato: {path}");
        }catch(Exception ex){Show($"Errore esportazione: {ex.Message}",true);}
    }
    private void BuildKpis(IReadOnlyList<SparePartStocktake> sessions)
    {
        _kpis.Children.Clear();AddKpi(0,"Aperte",sessions.Count(x=>x.Status==StocktakeStatus.Open),UiTokens.BrandBlue);
        AddKpi(1,"In verifica",sessions.Count(x=>x.Status==StocktakeStatus.Review),UiTokens.Warning);
        AddKpi(2,"Chiuse",sessions.Count(x=>x.Status==StocktakeStatus.Closed),UiTokens.Success);
        AddKpi(3,"Valore differenze",$"EUR {sessions.Where(x=>x.Status==StocktakeStatus.Closed).Sum(x=>x.DifferenceValue):N2}",UiTokens.Danger);
    }
    private Control Header(){var grid=GridRow();foreach(var x in new[]{("Sessione",0),("Descrizione",1),("Stato",2),("Operatore",3),("Data",4),("Conteggi",5),("Differenze",6),("Valore",7),("Azioni",8)})AddText(grid,x.Item1,x.Item2,true);return new Border{Background=UiTokens.Brush(UiTokens.SurfaceAlt),Padding=new Thickness(9),Child=grid};}
    private Control Row(SparePartStocktake session,int index)
    {
        var grid=GridRow();AddText(grid,session.SessionNumber,0,true);AddText(grid,session.Description,1);Add(grid,Badge(session.Status,session.Status==StocktakeStatus.Closed?UiTokens.Success:session.Status==StocktakeStatus.Review?UiTokens.Warning:UiTokens.BrandBlue),2);AddText(grid,session.OperatorName,3);AddText(grid,Date(session.CreatedAt),4);AddText(grid,$"{session.CountedLines}/{session.Lines.Count}",5);AddText(grid,session.DifferenceLines.ToString(),6,false,session.DifferenceLines>0);AddText(grid,$"EUR {session.DifferenceValue:N2}",7,false,session.DifferenceValue!=0);
        var actions=new StackPanel{Orientation=Orientation.Horizontal,Spacing=4};actions.Children.Add(Button(session.Status==StocktakeStatus.Closed?"Consulta":"Conta",()=>Open(session)));actions.Children.Add(Button("CSV",()=>Export(session)));Add(grid,actions,8);
        return new Border{Background=UiTokens.Brush(index%2==0?UiTokens.Surface:UiTokens.SurfaceAlt),BorderBrush=UiTokens.Brush(UiTokens.Border),BorderThickness=new Thickness(0,0,0,1),Padding=new Thickness(9,6),Child=grid};
    }
    private static string Date(string value)=>DateTime.TryParse(value,out var d)?d.ToString("dd/MM/yyyy HH:mm"):"—";
    private static string Csv(string value)=>$"\"{(value??"").Replace("\"","\"\"")}\"";
    private static Grid GridRow()=>new(){ColumnDefinitions=new ColumnDefinitions("155,220,110,120,135,90,90,120,150")};
    private void AddKpi(int col,string label,object value,string color){var c=new Border{Background=UiTokens.Brush(UiTokens.Surface),BorderBrush=UiTokens.Brush(UiTokens.Border),BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(10),Padding=new Thickness(14,9),Margin=new Thickness(0,0,10,0),Child=new StackPanel{Children={new TextBlock{Text=value.ToString(),FontSize=22,FontWeight=FontWeight.Bold,Foreground=UiTokens.Brush(color)},new TextBlock{Text=label,Foreground=UiTokens.Brush(UiTokens.TextSecondary)}}}};Grid.SetColumn(c,col);_kpis.Children.Add(c);}
    private static Control Badge(string text,string color)=>new Border{BorderBrush=UiTokens.Brush(color),BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(9),Padding=new Thickness(7,4),Margin=new Thickness(3),Child=new TextBlock{Text=text,Foreground=UiTokens.Brush(color),HorizontalAlignment=HorizontalAlignment.Center,FontWeight=FontWeight.Bold,FontSize=11}};
    private static Button Button(string text,Action action,bool primary=false){var b=new Button{Content=text,MinHeight=34,Margin=new Thickness(3),Background=UiTokens.Brush(primary?UiTokens.BrandBlue:UiTokens.SurfaceAlt),Foreground=primary?Brushes.White:UiTokens.Brush(UiTokens.TextPrimary)};b.Click+=(_,_)=>action();return b;}
    private static void AddText(Grid g,string text,int col,bool strong=false,bool danger=false)=>Add(g,new TextBlock{Text=string.IsNullOrWhiteSpace(text)?"—":text,FontWeight=strong?FontWeight.SemiBold:FontWeight.Normal,Foreground=UiTokens.Brush(danger?UiTokens.Danger:strong?UiTokens.TextPrimary:UiTokens.TextSecondary),TextTrimming=TextTrimming.CharacterEllipsis,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(3)},col);
    private static void Add(Grid g,Control c,int col){Grid.SetColumn(c,col);g.Children.Add(c);}
    private void Show(string text,bool error=false){_message.Text=text;_message.Foreground=UiTokens.Brush(error?UiTokens.Danger:UiTokens.Success);}
}

public sealed class NewStocktakeDialog : Window
{
    private readonly TextBox _description=new(){Watermark="Esempio: Inventario mensile magazzino IT"};private readonly TextBlock _message=new();
    public NewStocktakeDialog(){Title="Nuova sessione inventariale";Width=480;Height=250;WindowStartupLocation=WindowStartupLocation.CenterOwner;var save=new Button{Content="Crea sessione",Height=40};save.Click+=(_,_)=>{if(string.IsNullOrWhiteSpace(_description.Text)){_message.Text="Inserisci una descrizione.";return;}Close(_description.Text.Trim());};Content=new StackPanel{Margin=new Thickness(24),Spacing=12,Children={new TextBlock{Text="Nuovo inventario",FontSize=24,FontWeight=FontWeight.Bold},new TextBlock{Text="Descrizione"},_description,_message,save}};}
}

public sealed class StocktakeSessionWindow : Window
{
    private readonly SparePartStocktakeRepository _repository;private readonly SparePartsInventoryRepository _inventory;private readonly int _sessionId;
    private readonly StackPanel _rows=new();private readonly TextBlock _summary=new();private readonly TextBlock _message=new();private readonly Dictionary<int,(TextBox Count,TextBox Notes)> _inputs=new();
    private readonly TextBox _scanner=new(){Watermark="Scansiona QR/barcode oppure digita il codice ricambio e premi Invio"};
    private readonly CheckBox _continuous=new(){Content="Conteggio continuo",IsChecked=true};
    private readonly TextBlock _scanMessage=new();
    private SparePartStocktake? _currentSession;
    public StocktakeSessionWindow(SparePartStocktakeRepository repository,SparePartsInventoryRepository inventory,int sessionId)
    {
        _repository=repository;_inventory=inventory;_sessionId=sessionId;Title="Conteggio inventariale";Width=1050;Height=720;MinWidth=850;MinHeight=520;WindowStartupLocation=WindowStartupLocation.CenterOwner;
        _scanner.KeyDown+=ScannerKeyDown;Content=Build();Load();Opened+=(_,_)=>_scanner.Focus();
    }
    private Control Build()
    {
        var root=new DockPanel{Margin=new Thickness(24)};
        var actions=new StackPanel{Orientation=Orientation.Horizontal,Spacing=6};actions.Children.Add(Button("Salva conteggi",Save,true));actions.Children.Add(Button("Chiudi e rettifica",CloseSession));
        var header=new Grid{ColumnDefinitions=new ColumnDefinitions("*,Auto"),Margin=new Thickness(0,0,0,12)};header.Children.Add(_summary);Grid.SetColumn(actions,1);header.Children.Add(actions);DockPanel.SetDock(header,Dock.Top);root.Children.Add(header);
        var scanGrid=new Grid{ColumnDefinitions=new ColumnDefinitions("*,170"),Margin=new Thickness(0,0,0,5)};
        scanGrid.Children.Add(_scanner);Grid.SetColumn(_continuous,1);_continuous.VerticalAlignment=VerticalAlignment.Center;_continuous.HorizontalAlignment=HorizontalAlignment.Center;scanGrid.Children.Add(_continuous);
        DockPanel.SetDock(scanGrid,Dock.Top);root.Children.Add(scanGrid);
        _scanMessage.Margin=new Thickness(0,0,0,8);DockPanel.SetDock(_scanMessage,Dock.Top);root.Children.Add(_scanMessage);
        _message.Margin=new Thickness(0,0,0,8);DockPanel.SetDock(_message,Dock.Top);root.Children.Add(_message);
        root.Children.Add(new ScrollViewer{Content=_rows,HorizontalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,VerticalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto});return root;
    }
    private void Load()
    {
        var session=_repository.Get(_sessionId);_currentSession=session;_summary.Text=$"{session.SessionNumber} · {session.Description} · {session.Status} · contati {session.CountedLines}/{session.Lines.Count}";_summary.FontSize=21;_summary.FontWeight=FontWeight.Bold;
        _rows.Children.Clear();_inputs.Clear();_rows.MinWidth=940;_rows.Children.Add(Row("Codice","Descrizione","Registrata","Contata","Differenza","Valore","Note",true,null));
        foreach(var line in session.Lines)_rows.Children.Add(LineRow(line,session.Status==StocktakeStatus.Closed));
    }
    private Control LineRow(SparePartStocktakeLine line,bool readOnly)
    {
        var count=new TextBox{Text=line.CountedQuantity?.ToString("N2")??"",Watermark="Da contare",IsReadOnly=readOnly,Margin=new Thickness(3),Tag=line.Id};
        count.KeyDown+=CountKeyDown;
        var notes=new TextBox{Text=line.Notes,IsReadOnly=readOnly,Margin=new Thickness(3)};
        _inputs[line.Id]=(count,notes);
        return Row(line.PartCode,line.Description,line.ExpectedQuantity.ToString("N2"),"",line.CountedQuantity.HasValue?line.Difference.ToString("N2"):"—",line.CountedQuantity.HasValue?$"EUR {line.DifferenceValue:N2}":"—","",false,(count,notes));
    }
    private void ScannerKeyDown(object? sender,KeyEventArgs e)
    {
        if(e.Key!=Key.Enter)return;e.Handled=true;
        var raw=(_scanner.Text??"").Trim();var code=raw.StartsWith("AXPART:",StringComparison.OrdinalIgnoreCase)?raw[7..].Trim():raw;
        if(string.IsNullOrWhiteSpace(code)||_currentSession is null)return;
        var line=_currentSession.Lines.FirstOrDefault(x=>string.Equals(x.PartCode,code,StringComparison.OrdinalIgnoreCase));
        if(line is null){ScanStatus($"Codice non riconosciuto: {code}",true);_scanner.SelectAll();return;}
        if(!_inputs.TryGetValue(line.Id,out var input))return;
        if(!string.IsNullOrWhiteSpace(input.Count.Text))ScanStatus($"{line.PartCode} è già stato contato. Verifica prima di modificare.",true);
        else ScanStatus($"Trovato: {line.PartCode} · {line.Description}. Inserisci la quantità.",false);
        input.Count.Focus();input.Count.SelectAll();
    }
    private void CountKeyDown(object? sender,KeyEventArgs e)
    {
        if(e.Key!=Key.Enter||sender is not TextBox box)return;e.Handled=true;
        if(!decimal.TryParse(box.Text,out var quantity)||quantity<0){ScanStatus("Inserisci una quantità valida.",true);box.SelectAll();return;}
        Save();
        if(_continuous.IsChecked==true){_scanner.Text="";_scanner.Focus();}
    }
    private void ScanStatus(string text,bool error){_scanMessage.Text=text;_scanMessage.Foreground=UiTokens.Brush(error?UiTokens.Danger:UiTokens.Success);}
    private static Control Row(string code,string description,string expected,string counted,string difference,string value,string notes,bool header,(TextBox Count,TextBox Notes)? editors)
    {
        var grid=new Grid{ColumnDefinitions=new ColumnDefinitions("120,230,100,110,100,115,210")};
        var values=new[]{code,description,expected,counted,difference,value,notes};
        for(var i=0;i<values.Length;i++){Control c=editors.HasValue&&i==3?editors.Value.Count:editors.HasValue&&i==6?editors.Value.Notes:new TextBlock{Text=values[i],FontWeight=header?FontWeight.SemiBold:FontWeight.Normal,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(3),TextTrimming=TextTrimming.CharacterEllipsis};Grid.SetColumn(c,i);grid.Children.Add(c);}
        return new Border{Background=header?UiTokens.Brush(UiTokens.SurfaceAlt):UiTokens.Brush(UiTokens.Surface),BorderBrush=UiTokens.Brush(UiTokens.Border),BorderThickness=new Thickness(0,0,0,1),Padding=new Thickness(7,8),Child=grid};
    }
    private void Save()
    {
        try
        {
            var session=_repository.Get(_sessionId);if(session.Status==StocktakeStatus.Closed){Show("La sessione è già chiusa.",true);return;}
            foreach(var line in session.Lines){var input=_inputs[line.Id];line.CountedQuantity=decimal.TryParse(input.Count.Text,out var quantity)&&quantity>=0?quantity:null;line.Notes=input.Notes.Text?.Trim()??"";}
            _repository.SaveCounts(_sessionId,session.Lines);Show("Conteggi salvati.");Load();
        }catch(Exception ex){Show($"Errore salvataggio: {ex.Message}",true);}
    }
    private void CloseSession(){try{Save();_repository.Close(_sessionId,_inventory);Show("Inventario chiuso e giacenze riconciliate.");Load();}catch(Exception ex){Show($"Chiusura non eseguita: {ex.Message}",true);}}
    private static Button Button(string text,Action action,bool primary=false){var b=new Button{Content=text,MinHeight=36,Background=UiTokens.Brush(primary?UiTokens.BrandBlue:UiTokens.SurfaceAlt),Foreground=primary?Brushes.White:UiTokens.Brush(UiTokens.TextPrimary)};b.Click+=(_,_)=>action();return b;}
    private void Show(string text,bool error=false){_message.Text=text;_message.Foreground=UiTokens.Brush(error?UiTokens.Danger:UiTokens.Success);}
}
