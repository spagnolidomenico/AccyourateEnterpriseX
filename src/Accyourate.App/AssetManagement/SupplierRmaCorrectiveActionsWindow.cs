using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class SupplierRmaCorrectiveActionsWindow:Window
{
    private readonly SupplierRmaCorrectiveActionService _service=new();
    private readonly StackPanel _rows=new(),_kpis=new(){Orientation=Orientation.Horizontal,Spacing=10};
    private readonly TextBox _search=new(){Watermark="Cerca pratica, azione o responsabile..."};
    private readonly ComboBox _status=new(){ItemsSource=new[]{"Tutti gli stati","Aperta","In corso","Completata","Annullata","Scaduta","Efficacia da verificare","Efficace","Non efficace"},SelectedIndex=0,MinWidth=190};
    private readonly TextBlock _message=new();
    public SupplierRmaCorrectiveActionsWindow(){Title="Piano azioni correttive RMA";Width=1380;Height=780;MinWidth=980;MinHeight=600;WindowStartupLocation=WindowStartupLocation.CenterOwner;Content=Build();_search.TextChanged+=(_,_)=>LoadData();_status.SelectionChanged+=(_,_)=>LoadData();_service.PublishDueNotifications();_service.PublishEffectivenessNotifications();LoadData();}
    private Control Build(){var root=new DockPanel{Margin=new Thickness(24)};var head=new Grid{ColumnDefinitions=new ColumnDefinitions("*,Auto"),Margin=new Thickness(0,0,0,12)};Add(head,new StackPanel{Spacing=3,Children={new TextBlock{Text="Piano azioni correttive RMA",FontSize=28,FontWeight=FontWeight.Bold},new TextBlock{Text="Responsabili, scadenze, avanzamento e verifica delle non conformita.",Foreground=UiTokens.Brush(UiTokens.TextSecondary)}}},0);var actions=new StackPanel{Orientation=Orientation.Horizontal,Spacing=8};actions.Children.Add(Button("Report CAPA",ExportPdf));actions.Children.Add(Button("Aggiorna",LoadData,true));Add(head,actions,1);DockPanel.SetDock(head,Dock.Top);root.Children.Add(head);_kpis.Margin=new Thickness(0,0,0,10);DockPanel.SetDock(_kpis,Dock.Top);root.Children.Add(_kpis);var filters=new Grid{ColumnDefinitions=new ColumnDefinitions("*,190"),Margin=new Thickness(0,0,0,8)};Add(filters,_search,0);Add(filters,_status,1);DockPanel.SetDock(filters,Dock.Top);root.Children.Add(filters);DockPanel.SetDock(_message,Dock.Top);root.Children.Add(_message);root.Children.Add(new ScrollViewer{Content=_rows,HorizontalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,VerticalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto});return root;}
    private void LoadData(){try{var all=_service.GetAll();var q=(_search.Text??"").Trim();var status=_status.SelectedItem?.ToString()??"Tutti gli stati";var values=all.Where(x=>MatchStatus(x,status)).Where(x=>q.Length==0||$"{x.CaseNumber} {x.Title} {x.Description} {x.Responsible} {x.EffectivenessNotes}".Contains(q,StringComparison.OrdinalIgnoreCase)).ToList();_kpis.Children.Clear();_kpis.Children.Add(Kpi("Totali",all.Count,UiTokens.BrandBlue));_kpis.Children.Add(Kpi("Aperte",all.Count(x=>x.Status=="Aperta"),UiTokens.Warning));_kpis.Children.Add(Kpi("In corso",all.Count(x=>x.Status=="In corso"),UiTokens.BrandBlue));_kpis.Children.Add(Kpi("Scadute",all.Count(x=>x.IsOverdue),all.Any(x=>x.IsOverdue)?UiTokens.Danger:UiTokens.Success));_kpis.Children.Add(Kpi("Efficacia da verificare",all.Count(x=>x.Status=="Completata"&&x.EffectivenessStatus=="Da verificare"),UiTokens.Warning));_kpis.Children.Add(Kpi("Efficaci",all.Count(x=>x.EffectivenessStatus=="Efficace"),UiTokens.Success));_rows.Children.Clear();_rows.MinWidth=1380;_rows.Children.Add(Header());for(var i=0;i<values.Count;i++)_rows.Children.Add(Row(values[i],i));_message.Text=$"{values.Count} azioni visualizzate";_message.Foreground=UiTokens.Brush(UiTokens.TextSecondary);}catch(Exception ex){Status($"Azioni non caricate: {ex.Message}",true);}}
    private Control Header(){var g=GridRow();var labels=new[]{"Pratica","Azione","Responsabile","Scadenza","Priorita","Stato","Efficacia","Comandi"};for(var i=0;i<labels.Length;i++)Text(g,labels[i],i,true);return new Border{Background=UiTokens.Brush(UiTokens.SurfaceAlt),Padding=new Thickness(8),Child=g};}
    private Control Row(SupplierRmaCorrectiveAction x,int index){var g=GridRow();Text(g,x.CaseNumber,0,true);Text(g,x.Title,1);Text(g,x.Responsible,2);Text(g,Date(x.DueDate),3);Add(g,Badge(x.Priority,x.Priority=="Urgente"?UiTokens.Danger:UiTokens.Warning),4);Add(g,Badge(x.IsOverdue?"Scaduta":x.Status,StatusColor(x)),5);Add(g,Badge(x.EffectivenessStatus,EffectivenessColor(x.EffectivenessStatus)),6);var actions=new StackPanel{Orientation=Orientation.Horizontal,Spacing=5};actions.Children.Add(Button("Storico",()=>new SupplierRmaCorrectiveActionHistoryWindow(x,_service).Show(this)));actions.Children.Add(Button("Evidenze",()=>new SupplierRmaCorrectiveActionAttachmentsWindow(x,_service).Show(this)));if(x.Status=="Aperta")actions.Children.Add(Button("Avvia",()=>Change(x,"In corso")));if(x.Status is "Aperta" or "In corso")actions.Children.Add(Button("Completa",()=>Complete(x),true));if(x.Status=="Completata")actions.Children.Add(Button(x.EffectivenessStatus=="Da verificare"?"Verifica efficacia":"Rivedi efficacia",()=>VerifyEffectiveness(x),true));if(x.Status=="Annullata")actions.Children.Add(Button("Riapri",()=>Change(x,"Aperta")));Add(g,actions,7);return new Border{Background=UiTokens.Brush(index%2==0?UiTokens.Surface:UiTokens.SurfaceAlt),BorderBrush=UiTokens.Brush(UiTokens.Border),BorderThickness=new Thickness(0,0,0,1),Padding=new Thickness(8,6),Child=g};}
    private void Change(SupplierRmaCorrectiveAction value,string status){try{_service.ChangeStatus(value.Id,status,value.VerificationNotes,Environment.UserName);Status($"Azione aggiornata: {status}.",false);LoadData();}catch(Exception ex){Status(ex.Message,true);}}
    private async void Complete(SupplierRmaCorrectiveAction value){var notes=await new SupplierRmaCorrectiveActionCompletionDialog(value).ShowDialog<string?>(this);if(notes is null)return;try{_service.ChangeStatus(value.Id,"Completata",notes,Environment.UserName);Status("Azione completata e verificata.",false);LoadData();}catch(Exception ex){Status(ex.Message,true);}}
    private async void VerifyEffectiveness(SupplierRmaCorrectiveAction value){var result=await new SupplierRmaEffectivenessDialog(value).ShowDialog<(bool Effective,string Notes)?>(this);if(result is null)return;try{_service.VerifyEffectiveness(value.Id,result.Value.Effective,result.Value.Notes,Environment.UserName);Status(result.Value.Effective?"Efficacia confermata.":"Efficacia non confermata: azione riaperta.",!result.Value.Effective);LoadData();}catch(Exception ex){Status(ex.Message,true);}}
    private void ExportPdf(){try{var path=new SupplierRmaCapaPdfService().Generate(_service.GetAll());Process.Start(new ProcessStartInfo{FileName=path,UseShellExecute=true});Status($"Report CAPA creato: {path}",false);}catch(Exception ex){Status($"Report CAPA non creato: {ex.Message}",true);}}
    private static bool MatchStatus(SupplierRmaCorrectiveAction x,string status)=>status switch{"Tutti gli stati"=>true,"Scaduta"=>x.IsOverdue,"Efficacia da verificare"=>x.Status=="Completata"&&x.EffectivenessStatus=="Da verificare","Efficace"=>x.EffectivenessStatus=="Efficace","Non efficace"=>x.EffectivenessStatus=="Non efficace",_=>x.Status==status};
    private static Grid GridRow()=>new(){ColumnDefinitions=new ColumnDefinitions("140,*,160,115,110,120,145,240")};private static string StatusColor(SupplierRmaCorrectiveAction x)=>x.IsOverdue?UiTokens.Danger:x.Status switch{"Completata"=>UiTokens.Success,"In corso"=>UiTokens.BrandBlue,"Annullata"=>UiTokens.TextSecondary,_=>UiTokens.Warning};private static string EffectivenessColor(string value)=>value switch{"Efficace"=>UiTokens.Success,"Non efficace"=>UiTokens.Danger,_=>UiTokens.Warning};private static Border Badge(string text,string color)=>new(){BorderBrush=UiTokens.Brush(color),BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(8),Padding=new Thickness(6,3),Child=new TextBlock{Text=string.IsNullOrWhiteSpace(text)?"Da verificare":text,Foreground=UiTokens.Brush(color),HorizontalAlignment=HorizontalAlignment.Center,FontWeight=FontWeight.SemiBold}};private static Control Kpi(string label,int value,string color)=>new Border{Width=190,Padding=new Thickness(13,9),Background=UiTokens.Brush(UiTokens.Surface),BorderBrush=UiTokens.Brush(UiTokens.Border),BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(12),Child=new StackPanel{Spacing=2,Children={new TextBlock{Text=value.ToString(),FontSize=22,FontWeight=FontWeight.Bold,Foreground=UiTokens.Brush(color)},new TextBlock{Text=label,Foreground=UiTokens.Brush(UiTokens.TextSecondary)}}}};
    internal static Button Button(string text,Action action,bool primary=false){var b=new Button{Content=text,MinHeight=34,Background=UiTokens.Brush(primary?UiTokens.BrandBlue:UiTokens.SurfaceAlt),Foreground=primary?Brushes.White:UiTokens.Brush(UiTokens.TextPrimary)};b.Click+=(_,_)=>action();return b;}private static void Text(Grid g,string value,int col,bool strong=false)=>Add(g,new TextBlock{Text=string.IsNullOrWhiteSpace(value)?"—":value,FontWeight=strong?FontWeight.SemiBold:FontWeight.Normal,VerticalAlignment=VerticalAlignment.Center,TextTrimming=TextTrimming.CharacterEllipsis},col);private static void Add(Grid g,Control c,int col){c.Margin=new Thickness(0,0,8,0);Grid.SetColumn(c,col);g.Children.Add(c);}private static string Date(string value)=>DateTime.TryParse(value,out var d)?d.ToString("dd/MM/yyyy"):"—";private void Status(string text,bool error){_message.Text=text;_message.Foreground=UiTokens.Brush(error?UiTokens.Danger:UiTokens.Success);}
}

internal sealed class SupplierRmaCorrectiveActionDialog:Window
{
    private readonly SupplierRmaComplianceAudit _audit;private readonly TextBox _title=new(){Text="Risoluzione non conformita"},_description=new(){AcceptsReturn=true,TextWrapping=TextWrapping.Wrap,MinHeight=90},_responsible=new(){Text=Environment.UserName},_due=new(){Text=DateTime.Today.AddDays(14).ToString("yyyy-MM-dd")};private readonly ComboBox _priority=new(){ItemsSource=new[]{"Bassa","Normale","Alta","Urgente"},SelectedIndex=1};
    public SupplierRmaCorrectiveActionDialog(SupplierRmaComplianceAudit audit){_audit=audit;Title="Nuova azione correttiva";Width=620;Height=600;WindowStartupLocation=WindowStartupLocation.CenterOwner;_description.Text=string.IsNullOrWhiteSpace(audit.CorrectiveActions)?audit.Findings:audit.CorrectiveActions;var root=new StackPanel{Margin=new Thickness(24),Spacing=9,Children={new TextBlock{Text=$"Azione correttiva · {audit.CaseNumber}",FontSize=24,FontWeight=FontWeight.Bold}}};Field(root,"Titolo",_title);Field(root,"Descrizione e risultato atteso",_description);Field(root,"Responsabile",_responsible);Field(root,"Scadenza (AAAA-MM-GG)",_due);Field(root,"Priorita",_priority);root.Children.Add(SupplierRmaCorrectiveActionsWindow.Button("Crea azione",Save,true));Content=root;}
    private void Save(){if(string.IsNullOrWhiteSpace(_title.Text)||string.IsNullOrWhiteSpace(_responsible.Text)||!DateTime.TryParse(_due.Text,out var date))return;Close(new SupplierRmaCorrectiveAction{ComplianceAuditId=_audit.Id,RmaId=_audit.RmaId,CaseNumber=_audit.CaseNumber,Title=_title.Text.Trim(),Description=_description.Text??"",Responsible=_responsible.Text.Trim(),DueDate=date.ToString("yyyy-MM-dd"),Priority=_priority.SelectedItem?.ToString()??"Normale",CreatedBy=Environment.UserName});}private static void Field(StackPanel root,string label,Control control)=>root.Children.Add(new StackPanel{Spacing=3,Children={new TextBlock{Text=label,FontWeight=FontWeight.SemiBold},control}});
}

internal sealed class SupplierRmaCorrectiveActionCompletionDialog:Window
{
    private readonly TextBox _notes=new(){AcceptsReturn=true,TextWrapping=TextWrapping.Wrap,MinHeight=130};
    public SupplierRmaCorrectiveActionCompletionDialog(SupplierRmaCorrectiveAction value){Title="Verifica completamento";Width=560;Height=390;WindowStartupLocation=WindowStartupLocation.CenterOwner;var root=new StackPanel{Margin=new Thickness(24),Spacing=10,Children={new TextBlock{Text="Completa azione correttiva",FontSize=24,FontWeight=FontWeight.Bold},new TextBlock{Text=value.Title,TextWrapping=TextWrapping.Wrap},new TextBlock{Text="Esito della verifica",FontWeight=FontWeight.SemiBold},_notes}};root.Children.Add(SupplierRmaCorrectiveActionsWindow.Button("Conferma completamento",()=>{if(!string.IsNullOrWhiteSpace(_notes.Text))Close(_notes.Text.Trim());},true));Content=root;}
}

internal sealed class SupplierRmaEffectivenessDialog:Window
{
    private readonly RadioButton _effective=new(){Content="Efficace",GroupName="effectiveness",IsChecked=true},_notEffective=new(){Content="Non efficace",GroupName="effectiveness"};
    private readonly TextBox _notes=new(){AcceptsReturn=true,TextWrapping=TextWrapping.Wrap,MinHeight=150,Watermark="Descrivi le evidenze osservate e il risultato della verifica"};
    public SupplierRmaEffectivenessDialog(SupplierRmaCorrectiveAction value)
    {
        Title="Verifica efficacia azione correttiva";Width=620;Height=500;WindowStartupLocation=WindowStartupLocation.CenterOwner;
        var root=new StackPanel{Margin=new Thickness(24),Spacing=10,Children={new TextBlock{Text="Verifica efficacia",FontSize=24,FontWeight=FontWeight.Bold},new TextBlock{Text=$"{value.CaseNumber} · {value.Title}",TextWrapping=TextWrapping.Wrap},new TextBlock{Text="L'azione ha eliminato la causa della non conformita?",FontWeight=FontWeight.SemiBold},new StackPanel{Orientation=Orientation.Horizontal,Spacing=24,Children={_effective,_notEffective}},new TextBlock{Text="Evidenze ed esito della verifica",FontWeight=FontWeight.SemiBold},_notes}};
        root.Children.Add(SupplierRmaCorrectiveActionsWindow.Button("Registra verifica",Save,true));Content=root;
    }
    private void Save(){if(string.IsNullOrWhiteSpace(_notes.Text))return;Close(((bool Effective,string Notes)?)(_effective.IsChecked==true,_notes.Text.Trim()));}
}

internal sealed class SupplierRmaCorrectiveActionHistoryWindow:Window
{
    public SupplierRmaCorrectiveActionHistoryWindow(SupplierRmaCorrectiveAction action,SupplierRmaCorrectiveActionService service)
    {
        Title="Storico azione correttiva";Width=920;Height=660;MinWidth=720;MinHeight=480;WindowStartupLocation=WindowStartupLocation.CenterOwner;
        var rows=new StackPanel{Spacing=0};var events=service.GetHistory(action.Id);
        if(events.Count==0)rows.Children.Add(new TextBlock{Text="Nessun evento registrato.",Foreground=UiTokens.Brush(UiTokens.TextSecondary),Margin=new Thickness(8)});
        foreach(var item in events)
        {
            var detail=string.IsNullOrWhiteSpace(item.OldValue)?item.NewValue:$"{item.OldValue} → {item.NewValue}";
            var content=new StackPanel{Spacing=4,Children={new Grid{ColumnDefinitions=new ColumnDefinitions("*,Auto"),Children={new TextBlock{Text=item.EventType,FontWeight=FontWeight.Bold,FontSize=16},new TextBlock{Text=FormatDate(item.CreatedAt),Foreground=UiTokens.Brush(UiTokens.TextSecondary)}}},new TextBlock{Text=detail,Foreground=UiTokens.Brush(UiTokens.BrandBlue),FontWeight=FontWeight.SemiBold},new TextBlock{Text=item.Notes,TextWrapping=TextWrapping.Wrap,IsVisible=!string.IsNullOrWhiteSpace(item.Notes)},new TextBlock{Text=$"Operatore: {item.CreatedBy}",Foreground=UiTokens.Brush(UiTokens.TextSecondary)}}};
            Grid.SetColumn(content.Children[0],0);var header=(Grid)content.Children[0];Grid.SetColumn(header.Children[1],1);
            rows.Children.Add(new Border{Padding=new Thickness(12),BorderBrush=UiTokens.Brush(UiTokens.Border),BorderThickness=new Thickness(0,0,0,1),Child=content});
        }
        var root=new DockPanel{Margin=new Thickness(24)};var head=new StackPanel{Spacing=3,Margin=new Thickness(0,0,0,14),Children={new TextBlock{Text="Audit trail CAPA",FontSize=26,FontWeight=FontWeight.Bold},new TextBlock{Text=$"{action.CaseNumber} · {action.Title}",TextWrapping=TextWrapping.Wrap,Foreground=UiTokens.Brush(UiTokens.TextSecondary)}}};DockPanel.SetDock(head,Dock.Top);root.Children.Add(head);root.Children.Add(new ScrollViewer{Content=rows,VerticalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto});Content=root;
    }
    private static string FormatDate(string value)=>DateTime.TryParse(value,out var date)?date.ToString("dd/MM/yyyy HH:mm"):"—";
}

internal sealed class SupplierRmaCorrectiveActionAttachmentsWindow:Window
{
    private readonly SupplierRmaCorrectiveAction _action;private readonly SupplierRmaCorrectiveActionService _service;private readonly StackPanel _rows=new();private readonly TextBlock _message=new();
    public SupplierRmaCorrectiveActionAttachmentsWindow(SupplierRmaCorrectiveAction action,SupplierRmaCorrectiveActionService service)
    {
        _action=action;_service=service;Title="Evidenze azione correttiva";Width=980;Height=680;MinWidth=760;MinHeight=500;WindowStartupLocation=WindowStartupLocation.CenterOwner;Content=Build();Load();
    }
    private Control Build()
    {
        var root=new DockPanel{Margin=new Thickness(24)};var head=new Grid{ColumnDefinitions=new ColumnDefinitions("*,Auto"),Margin=new Thickness(0,0,0,12)};var title=new StackPanel{Spacing=3,Children={new TextBlock{Text="Evidenze documentali CAPA",FontSize=26,FontWeight=FontWeight.Bold},new TextBlock{Text=$"{_action.CaseNumber} · {_action.Title}",TextWrapping=TextWrapping.Wrap,Foreground=UiTokens.Brush(UiTokens.TextSecondary)}}};Grid.SetColumn(title,0);head.Children.Add(title);var add=SupplierRmaCorrectiveActionsWindow.Button("Allega evidenza",AddFile,true);Grid.SetColumn(add,1);head.Children.Add(add);DockPanel.SetDock(head,Dock.Top);root.Children.Add(head);DockPanel.SetDock(_message,Dock.Top);root.Children.Add(_message);root.Children.Add(new ScrollViewer{Content=_rows,VerticalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto});return root;
    }
    private void Load()
    {
        _rows.Children.Clear();var values=_service.GetAttachments(_action.Id);if(values.Count==0){_rows.Children.Add(new TextBlock{Text="Nessuna evidenza allegata.",Foreground=UiTokens.Brush(UiTokens.TextSecondary),Margin=new Thickness(8)});return;}
        foreach(var item in values)
        {
            var grid=new Grid{ColumnDefinitions=new ColumnDefinitions("160,*,130,Auto"),Margin=new Thickness(0,0,0,4)};Cell(grid,item.Category,0,true);var detail=new StackPanel{Spacing=2,Children={new TextBlock{Text=item.FileName,FontWeight=FontWeight.SemiBold,TextWrapping=TextWrapping.Wrap},new TextBlock{Text=item.Notes,TextWrapping=TextWrapping.Wrap,IsVisible=!string.IsNullOrWhiteSpace(item.Notes)},new TextBlock{Text=$"SHA-256: {item.Sha256}",FontSize=10,TextTrimming=TextTrimming.CharacterEllipsis,Foreground=UiTokens.Brush(UiTokens.TextSecondary)}}};Grid.SetColumn(detail,1);grid.Children.Add(detail);Cell(grid,$"{Size(item.FileSize)}\n{FormatDate(item.CreatedAt)}",2);var open=SupplierRmaCorrectiveActionsWindow.Button(item.IsAvailable?"Apri":"Non disponibile",()=>Open(item.StoredPath));open.IsEnabled=item.IsAvailable;Grid.SetColumn(open,3);grid.Children.Add(open);_rows.Children.Add(new Border{Padding=new Thickness(10),BorderBrush=UiTokens.Brush(UiTokens.Border),BorderThickness=new Thickness(0,0,0,1),Child=grid});
        }
    }
    private async void AddFile()
    {
        try
        {
            var files=await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions{Title="Seleziona evidenza CAPA",AllowMultiple=false});if(files.Count==0)return;var path=files[0].TryGetLocalPath();if(string.IsNullOrWhiteSpace(path))return;var info=await new SupplierRmaAttachmentInfoDialog(Path.GetFileName(path)).ShowDialog<(string Category,string Notes)?>(this);if(info is null)return;_service.AttachFile(_action,path,info.Value.Category,info.Value.Notes,Environment.UserName);_message.Text="Evidenza archiviata e registrata nello storico.";_message.Foreground=UiTokens.Brush(UiTokens.Success);Load();
        }
        catch(Exception ex){_message.Text=$"Evidenza non archiviata: {ex.Message}";_message.Foreground=UiTokens.Brush(UiTokens.Danger);}
    }
    private static void Open(string path){if(File.Exists(path))Process.Start(new ProcessStartInfo{FileName=path,UseShellExecute=true});}
    private static void Cell(Grid grid,string text,int column,bool strong=false){var value=new TextBlock{Text=text,TextWrapping=TextWrapping.Wrap,VerticalAlignment=VerticalAlignment.Center,FontWeight=strong?FontWeight.SemiBold:FontWeight.Normal,Margin=new Thickness(0,0,8,0)};Grid.SetColumn(value,column);grid.Children.Add(value);}private static string FormatDate(string value)=>DateTime.TryParse(value,out var date)?date.ToString("dd/MM/yyyy HH:mm"):"—";private static string Size(long value)=>value>=1024*1024?$"{value/(1024d*1024d):N1} MB":$"{value/1024d:N1} KB";
}

internal sealed class SupplierRmaAttachmentInfoDialog:Window
{
    private readonly ComboBox _category=new(){ItemsSource=new[]{"Evidenza efficacia","Verbale","Fotografia","Comunicazione fornitore","Documento tecnico","Altro"},SelectedIndex=0};private readonly TextBox _notes=new(){AcceptsReturn=true,TextWrapping=TextWrapping.Wrap,MinHeight=100};
    public SupplierRmaAttachmentInfoDialog(string fileName){Title="Classifica evidenza";Width=560;Height=420;WindowStartupLocation=WindowStartupLocation.CenterOwner;var root=new StackPanel{Margin=new Thickness(24),Spacing=9,Children={new TextBlock{Text="Classifica evidenza",FontSize=24,FontWeight=FontWeight.Bold},new TextBlock{Text=fileName,TextWrapping=TextWrapping.Wrap},new TextBlock{Text="Categoria",FontWeight=FontWeight.SemiBold},_category,new TextBlock{Text="Note",FontWeight=FontWeight.SemiBold},_notes}};root.Children.Add(SupplierRmaCorrectiveActionsWindow.Button("Archivia evidenza",()=>Close(((string Category,string Notes)?)(_category.SelectedItem?.ToString()??"Evidenza",_notes.Text??"")),true));Content=root;}
}
