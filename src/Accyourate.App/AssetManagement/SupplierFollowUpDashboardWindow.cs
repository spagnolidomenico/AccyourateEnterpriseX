using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Accyourate.App.AssetManagement.Models;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class SupplierFollowUpDashboardWindow : Window
{
    private readonly SupplierRmaPortalRepository _repository = new();
    private readonly StackPanel _root = new();
    private readonly TextBox _search = new() { Watermark = "Cerca fornitore, oggetto o responsabile..." };
    private readonly ComboBox _status = new() { ItemsSource = new[] { "Tutti gli stati", "Aperti", "Scaduti", "In scadenza", "Completati", "Annullati" }, SelectedIndex = 0 };
    private readonly ComboBox _priority = new() { ItemsSource = new[] { "Tutte le priorità", "Bassa", "Normale", "Alta", "Urgente" }, SelectedIndex = 0 };
    private readonly TextBlock _summary = new();
    private Grid? _filterHost;

    public SupplierFollowUpDashboardWindow()
    {
        Title = "Cruscotto solleciti fornitori";
        Width = 1420; Height = 820; MinWidth = 1050; MinHeight = 620;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        _search.TextChanged += (_, _) => Dispatcher.UIThread.Post(Load);
        _status.SelectionChanged += (_, _) => Dispatcher.UIThread.Post(Load);
        _priority.SelectionChanged += (_, _) => Dispatcher.UIThread.Post(Load);
        Content = new ScrollViewer { Content = _root, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto };
        Load();
    }

    private void Load()
    {
        var all = _repository.GetAllFollowUps();
        var q = (_search.Text ?? "").Trim();
        var values = all.Where(x => q.Length == 0 || $"{x.SupplierName} {x.Subject} {x.Message} {x.Owner}".Contains(q, StringComparison.OrdinalIgnoreCase))
            .Where(MatchesStatus).Where(x => (_priority.SelectedItem?.ToString() ?? "Tutte le priorità") == "Tutte le priorità" || x.Priority == _priority.SelectedItem?.ToString()).ToList();

        // I controlli di filtro sono persistenti: prima di ricostruire la pagina
        // vanno sganciati esplicitamente dal vecchio Grid per evitare il doppio parent Avalonia.
        _filterHost?.Children.Clear();
        _root.Children.Clear(); _root.Margin = new Thickness(24); _root.Spacing = 14;
        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
        header.Children.Add(new TextBlock { Text = "Cruscotto solleciti fornitori", FontSize = 30, FontWeight = FontWeight.Bold });
        var automation = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        automation.Children.Add(Button("Esegui automazione", RunAutomation, true));
        automation.Children.Add(Button("Regole", () => new SupplierFollowUpAutomationRulesWindow().Show(this)));
        automation.Children.Add(Button("Registro", () => new SupplierFollowUpAutomationLogWindow().Show(this)));
        Grid.SetColumn(automation, 1); header.Children.Add(automation); _root.Children.Add(header);
        _root.Children.Add(new TextBlock { Text = "Attività aperte, scadenze, responsabilità e tempi di risposta dei fornitori.", Foreground = UiTokens.Brush(UiTokens.TextSecondary) });
        var kpis = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        kpis.Children.Add(Kpi("Totali", all.Count.ToString(), UiTokens.BrandBlue));
        kpis.Children.Add(Kpi("Aperti", all.Count(x => x.Status == "Aperta").ToString(), UiTokens.BrandBlue));
        kpis.Children.Add(Kpi("Scaduti", all.Count(IsOverdue).ToString(), UiTokens.Danger));
        kpis.Children.Add(Kpi("In scadenza 7 gg", all.Count(IsDueSoon).ToString(), UiTokens.Warning));
        kpis.Children.Add(Kpi("Completati", all.Count(x => x.Status == "Completata").ToString(), UiTokens.Success));
        kpis.Children.Add(Kpi("Tempo medio", AverageDays(all), UiTokens.BrandBlue));
        _root.Children.Add(kpis);

        var filters = new Grid { ColumnDefinitions = new ColumnDefinitions("*,220,190,Auto") };
        _filterHost = filters;
        Add(filters, _search, 0); Add(filters, _status, 1); Add(filters, _priority, 2);
        var refresh = Button("Aggiorna", Load, true); Add(filters, refresh, 3); _root.Children.Add(filters);
        _summary.Text = $"{values.Count} solleciti visualizzati"; _summary.Foreground = UiTokens.Brush(UiTokens.TextSecondary); _root.Children.Add(_summary);
        var rows = new StackPanel(); rows.Children.Add(Header());
        for (var i = 0; i < values.Count; i++) rows.Children.Add(Row(values[i], i));
        if (values.Count == 0) rows.Children.Add(new TextBlock { Text = "Nessun sollecito corrisponde ai filtri selezionati.", Margin = new Thickness(12), Foreground = UiTokens.Brush(UiTokens.TextSecondary) });
        _root.Children.Add(new ScrollViewer { Content = rows, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto });
    }

    private Control Header()
    {
        var g = GridRow();
        HeaderText(g, "Fornitore", 0); HeaderText(g, "Oggetto", 1); HeaderText(g, "Stato", 2); HeaderText(g, "Priorità", 3);
        HeaderText(g, "Responsabile", 4); HeaderText(g, "Scadenza", 5); HeaderText(g, "Ritardo", 6); HeaderText(g, "Azioni", 7);
        return new Border { Padding = new Thickness(9), Background = UiTokens.Brush(UiTokens.SurfaceAlt), Child = g };
    }

    private Control Row(SupplierPortalInteraction x, int index)
    {
        var g = GridRow(); Text(g, x.SupplierName, 0, true); Text(g, x.Subject, 1, true); Text(g, x.Status, 2, IsOverdue(x));
        Text(g, x.Priority, 3, x.Priority is "Alta" or "Urgente"); Text(g, string.IsNullOrWhiteSpace(x.Owner) ? "—" : x.Owner, 4);
        Text(g, Date(x.FollowUpDate), 5); Text(g, Delay(x), 6, IsOverdue(x));
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5 };
        actions.Children.Add(Button("Portale", () => new SupplierRmaPortalWindow(x.SupplierId, x.RmaId).Show(this), true));
        actions.Children.Add(Button("Assegna", () => Assign(x)));
        Add(g, actions, 7);
        return new Border { Padding = new Thickness(9, 7), Background = UiTokens.Brush(index % 2 == 0 ? UiTokens.Surface : UiTokens.SurfaceAlt), BorderBrush = UiTokens.Brush(UiTokens.Border), BorderThickness = new Thickness(0, 0, 0, 1), Child = g };
    }

    private async void Assign(SupplierPortalInteraction item)
    {
        var result = await new SupplierFollowUpAssignmentDialog(item).ShowDialog<(string Priority, string Owner)?>(this);
        if (result is null) return;
        _repository.UpdateFollowUpAssignment(item.Id, result.Value.Priority, result.Value.Owner); Load();
    }
    private void RunAutomation(){_repository.RunFollowUpAutomation();Load();}

    private bool MatchesStatus(SupplierPortalInteraction x) => (_status.SelectedItem?.ToString() ?? "Tutti gli stati") switch
    {
        "Aperti" => x.Status == "Aperta", "Scaduti" => IsOverdue(x), "In scadenza" => IsDueSoon(x),
        "Completati" => x.Status == "Completata", "Annullati" => x.Status == "Annullata", _ => true
    };
    private static bool IsOverdue(SupplierPortalInteraction x) => x.Status == "Aperta" && DateTime.TryParse(x.FollowUpDate, out var d) && d.Date < DateTime.Today;
    private static bool IsDueSoon(SupplierPortalInteraction x) => x.Status == "Aperta" && DateTime.TryParse(x.FollowUpDate, out var d) && d.Date >= DateTime.Today && d.Date <= DateTime.Today.AddDays(7);
    private static string Delay(SupplierPortalInteraction x) => IsOverdue(x) && DateTime.TryParse(x.FollowUpDate, out var d) ? $"{(DateTime.Today - d.Date).Days} gg" : "—";
    private static string AverageDays(IEnumerable<SupplierPortalInteraction> values){var days=values.Where(x=>x.Status=="Completata").Select(x=>DateTime.TryParse(x.CreatedAt,out var a)&&DateTime.TryParse(x.CompletedAt,out var b)?(double?)(b-a).TotalDays:null).Where(x=>x.HasValue).Select(x=>x!.Value).ToList();return days.Count==0?"—":$"{days.Average():N1} gg";}
    private static Grid GridRow()=>new(){MinWidth=1320,ColumnDefinitions=new ColumnDefinitions("175,260,115,105,155,115,85,300")};
    private static void HeaderText(Grid g,string value,int col)=>Text(g,value,col,true);
    private static void Text(Grid g,string value,int col,bool strong=false)=>Add(g,new TextBlock{Text=string.IsNullOrWhiteSpace(value)?"—":value,FontWeight=strong?FontWeight.SemiBold:FontWeight.Normal,TextTrimming=TextTrimming.CharacterEllipsis,VerticalAlignment=VerticalAlignment.Center,Foreground=UiTokens.Brush(UiTokens.TextPrimary)},col);
    private static void Add(Grid g,Control c,int col){c.Margin=new Thickness(0,0,8,0);Grid.SetColumn(c,col);g.Children.Add(c);}
    private static Button Button(string text,Action action,bool primary=false){var b=new Button{Content=text,Padding=new Thickness(8,5),Background=UiTokens.Brush(primary?UiTokens.BrandBlue:UiTokens.SurfaceAlt),Foreground=primary?Brushes.White:UiTokens.Brush(UiTokens.TextPrimary)};b.Click+=(_,_)=>action();return b;}
    private static Control Kpi(string label,string value,string color)=>new Border{Width=205,Padding=new Thickness(12),Background=UiTokens.Brush(UiTokens.Surface),BorderBrush=UiTokens.Brush(UiTokens.Border),BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(12),Child=new StackPanel{Children={new TextBlock{Text=value,FontSize=22,FontWeight=FontWeight.Bold,Foreground=UiTokens.Brush(color)},new TextBlock{Text=label,Foreground=UiTokens.Brush(UiTokens.TextSecondary)}}}};
    private static string Date(string value)=>DateTime.TryParse(value,out var d)?d.ToString("dd/MM/yyyy"):"—";
}

internal sealed class SupplierFollowUpAssignmentDialog : Window
{
    private readonly ComboBox _priority=new(){ItemsSource=new[]{"Bassa","Normale","Alta","Urgente"}};private readonly TextBox _owner=new();
    public SupplierFollowUpAssignmentDialog(SupplierPortalInteraction item){Title="Responsabilità sollecito";Width=480;Height=330;WindowStartupLocation=WindowStartupLocation.CenterOwner;_priority.SelectedItem=item.Priority;_owner.Text=item.Owner;var p=SupplierContactDialog.Panel("Priorità e responsabile");p.Children.Add(new TextBlock{Text=item.Subject,FontWeight=FontWeight.SemiBold,TextWrapping=TextWrapping.Wrap});SupplierContactDialog.Field(p,"Priorità",_priority);SupplierContactDialog.Field(p,"Responsabile",_owner);p.Children.Add(SupplierContactDialog.SaveButton("Salva",()=>Close((_priority.SelectedItem?.ToString()??"Normale",_owner.Text??""))));Content=p;}
}

internal sealed class SupplierFollowUpAutomationRulesWindow : Window
{
    private readonly SupplierRmaPortalRepository _repository=new();private readonly StackPanel _rows=new();
    public SupplierFollowUpAutomationRulesWindow(){Title="Regole automazione solleciti";Width=900;Height=680;WindowStartupLocation=WindowStartupLocation.CenterOwner;var root=new StackPanel{Margin=new Thickness(24),Spacing=12};root.Children.Add(new TextBlock{Text="Regole automazione",FontSize=27,FontWeight=FontWeight.Bold});root.Children.Add(new TextBlock{Text="Configura promemoria ed escalation per tutti i fornitori o per un singolo fornitore.",Foreground=UiTokens.Brush(UiTokens.TextSecondary)});root.Children.Add(_rows);Content=new ScrollViewer{Content=root};Load();}
    private void Load(){_rows.Children.Clear();var suppliers=new MaintenancePurchasingRepository().GetSuppliers();_rows.Children.Add(RuleRow(0,"Regola predefinita"));foreach(var supplier in suppliers)_rows.Children.Add(RuleRow(supplier.Id,supplier.Name));}
    private Control RuleRow(int supplierId,string name){var rule=_repository.GetAutomationRule(supplierId);if(rule.SupplierId!=supplierId)rule=new(){SupplierId=supplierId,ReminderDaysBefore=rule.ReminderDaysBefore,EscalateAfterDays=rule.EscalateAfterDays,IsEnabled=rule.IsEnabled};var reminder=new NumericUpDown{Minimum=0,Maximum=30,Value=rule.ReminderDaysBefore,Width=100};var escalation=new NumericUpDown{Minimum=0,Maximum=30,Value=rule.EscalateAfterDays,Width=100};var enabled=new CheckBox{Content="Attiva",IsChecked=rule.IsEnabled};var g=new Grid{ColumnDefinitions=new ColumnDefinitions("*,180,180,110,100")};Add(g,new TextBlock{Text=name,FontWeight=FontWeight.SemiBold,VerticalAlignment=VerticalAlignment.Center},0);Add(g,new StackPanel{Spacing=2,Children={new TextBlock{Text="Promemoria (giorni prima)",FontSize=11},reminder}},1);Add(g,new StackPanel{Spacing=2,Children={new TextBlock{Text="Escalation (giorni dopo)",FontSize=11},escalation}},2);Add(g,enabled,3);var save=new Button{Content="Salva",Padding=new Thickness(9,6)};save.Click+=(_,_)=>_repository.SaveAutomationRule(new(){SupplierId=supplierId,ReminderDaysBefore=(int)(reminder.Value??0),EscalateAfterDays=(int)(escalation.Value??0),IsEnabled=enabled.IsChecked==true});Add(g,save,4);return new Border{Padding=new Thickness(10),BorderBrush=UiTokens.Brush(UiTokens.Border),BorderThickness=new Thickness(0,0,0,1),Child=g};}
    private static void Add(Grid g,Control c,int col){c.Margin=new Thickness(0,0,8,0);Grid.SetColumn(c,col);g.Children.Add(c);}
}

internal sealed class SupplierFollowUpAutomationLogWindow : Window
{
    public SupplierFollowUpAutomationLogWindow(){Title="Registro automazione solleciti";Width=1050;Height=680;WindowStartupLocation=WindowStartupLocation.CenterOwner;var root=new StackPanel{Margin=new Thickness(24),Spacing=8};root.Children.Add(new TextBlock{Text="Registro escalation e promemoria",FontSize=27,FontWeight=FontWeight.Bold});var values=new SupplierRmaPortalRepository().GetAutomationLog();if(values.Count==0)root.Children.Add(new TextBlock{Text="Nessuna automazione ancora eseguita.",Foreground=UiTokens.Brush(UiTokens.TextSecondary)});foreach(var x in values){var g=new Grid{ColumnDefinitions=new ColumnDefinitions("150,180,130,*,150")};Text(g,x.SupplierName,0,true);Text(g,x.EventType,1,true);Text(g,$"Sollecito #{x.InteractionId}",2);Text(g,x.Description,3);Text(g,DateTime.TryParse(x.CreatedAt,out var d)?d.ToString("dd/MM/yyyy HH:mm"):x.CreatedAt,4);root.Children.Add(new Border{Padding=new Thickness(9),BorderBrush=UiTokens.Brush(UiTokens.Border),BorderThickness=new Thickness(0,0,0,1),Child=g});}Content=new ScrollViewer{Content=root};}
    private static void Text(Grid g,string value,int col,bool strong=false){var t=new TextBlock{Text=value,FontWeight=strong?FontWeight.SemiBold:FontWeight.Normal,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,8,0)};Grid.SetColumn(t,col);g.Children.Add(t);}
}
