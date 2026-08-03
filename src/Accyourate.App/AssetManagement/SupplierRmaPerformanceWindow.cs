using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Models;
using Accyourate.App.AssetManagement.Services;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class SupplierRmaPerformanceWindow : Window
{
    private readonly SparePartRmaAnalyticsService _analytics=new();
    private readonly StackPanel _rows=new();
    private readonly StackPanel _kpis=new(){Orientation=Orientation.Horizontal,Spacing=10};
    private readonly TextBox _search=new(){Watermark="Cerca fornitore..."};
    private readonly ComboBox _rating=new(){ItemsSource=new[]{"Tutte le valutazioni","Affidabili (>= 80)","Da monitorare (50-79)","Critici (< 50)"},SelectedIndex=0};
    private readonly TextBlock _summary=new();

    public SupplierRmaPerformanceWindow()
    {
        Title="Prestazioni fornitori RMA";Width=1380;Height=760;MinWidth=1050;MinHeight=580;WindowStartupLocation=WindowStartupLocation.CenterOwner;
        _search.TextChanged+=(_,_)=>Load();_rating.SelectionChanged+=(_,_)=>Load();Content=Build();Load();
    }

    private Control Build()
    {
        var root=new DockPanel{Margin=new Thickness(24)};
        var title=new StackPanel{Spacing=3,Margin=new Thickness(0,0,0,12),Children={new TextBlock{Text="Dashboard fornitori e SLA RMA",FontSize=28,FontWeight=FontWeight.Bold},new TextBlock{Text="Affidabilità, tempi di risoluzione, costi ed esiti per fornitore.",Foreground=UiTokens.Brush(UiTokens.TextSecondary)}}};
        DockPanel.SetDock(title,Dock.Top);root.Children.Add(title);
        _kpis.Margin=new Thickness(0,0,0,12);DockPanel.SetDock(_kpis,Dock.Top);root.Children.Add(_kpis);
        var filters=new Grid{ColumnDefinitions=new ColumnDefinitions("*,240"),Margin=new Thickness(0,0,0,8)};Add(filters,_search,0);Add(filters,_rating,1);DockPanel.SetDock(filters,Dock.Top);root.Children.Add(filters);
        _summary.Foreground=UiTokens.Brush(UiTokens.TextSecondary);_summary.Margin=new Thickness(0,0,0,8);DockPanel.SetDock(_summary,Dock.Top);root.Children.Add(_summary);
        root.Children.Add(new ScrollViewer{Content=_rows,HorizontalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,VerticalScrollBarVisibility=Avalonia.Controls.Primitives.ScrollBarVisibility.Auto});return root;
    }

    private void Load()
    {
        var all=_analytics.GetPerformance();UpdateKpis(all);var q=(_search.Text??"").Trim();var filter=_rating.SelectedItem?.ToString()??"Tutte le valutazioni";
        var values=all.Where(x=>q.Length==0||x.SupplierName.Contains(q,StringComparison.OrdinalIgnoreCase)).Where(x=>Matches(x,filter)).ToList();
        _summary.Text=$"{values.Count} fornitori visualizzati · {values.Sum(x=>x.TotalCases)} pratiche · costi EUR {values.Sum(x=>x.TotalCost):N2}";
        _rows.Children.Clear();_rows.MinWidth=1240;_rows.Children.Add(Header());for(var i=0;i<values.Count;i++)_rows.Children.Add(Row(values[i],i));
    }

    private void UpdateKpis(IReadOnlyList<SupplierRmaPerformance> values)
    {
        var cases=values.Sum(x=>x.TotalCases);var evaluated=values.Sum(x=>x.EvaluatedSlaCases);var onTime=values.Sum(x=>x.OnTimeCases);var sla=evaluated==0?0:onTime*100d/evaluated;
        _kpis.Children.Clear();_kpis.Children.Add(Kpi("Fornitori monitorati",values.Count.ToString(),UiTokens.BrandBlue));_kpis.Children.Add(Kpi("Pratiche RMA",cases.ToString(),UiTokens.BrandBlue));_kpis.Children.Add(Kpi("SLA complessivo",evaluated==0?"—":$"{sla:N0}%",evaluated>0&&sla>=80?UiTokens.Success:UiTokens.Warning));_kpis.Children.Add(Kpi("Costi complessivi",$"EUR {values.Sum(x=>x.TotalCost):N2}",UiTokens.Warning));
    }

    private static Control Header(){var g=GridRow();var names=new[]{"Fornitore","Pratiche","Aperte","Scadute","SLA","Tempo medio","Riparaz.","Sostituz.","Rimborsi","Costi","Affidabilità","Azioni"};for(var i=0;i<names.Length;i++)Text(g,names[i],i,true);return new Border{Background=UiTokens.Brush(UiTokens.SurfaceAlt),Padding=new Thickness(8),Child=g};}
    private Control Row(SupplierRmaPerformance x,int index){var g=GridRow();Text(g,x.SupplierName,0,true);Text(g,x.TotalCases.ToString(),1);Text(g,x.ActiveCases.ToString(),2);Text(g,x.OverdueCases.ToString(),3,x.OverdueCases>0);Text(g,x.EvaluatedSlaCases==0?"—":$"{x.SlaCompliancePercent:N0}%",4,true);Text(g,x.ClosedCases==0?"—":$"{x.AverageResolutionDays:N1} gg",5);Text(g,x.RepairCases.ToString(),6);Text(g,x.ReplacementCases.ToString(),7);Text(g,x.RefundCases.ToString(),8);Text(g,$"EUR {x.TotalCost:N2}",9);Add(g,Score(x.ReliabilityScore),10);var open=new Button{Content="Dettaglio",Padding=new Thickness(8,5)};open.Click+=(_,_)=>new SupplierRmaDetailWindow(x.SupplierId).Show(this);Add(g,open,11);return new Border{Background=UiTokens.Brush(index%2==0?UiTokens.Surface:UiTokens.SurfaceAlt),BorderBrush=UiTokens.Brush(UiTokens.Border),BorderThickness=new Thickness(0,0,0,1),Padding=new Thickness(8,7),Child=g};}
    private static Control Score(double value){var color=value>=80?UiTokens.Success:value>=50?UiTokens.Warning:UiTokens.Danger;return new Border{BorderBrush=UiTokens.Brush(color),BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(8),Padding=new Thickness(6,3),Child=new TextBlock{Text=$"{value:N0}/100",Foreground=UiTokens.Brush(color),FontWeight=FontWeight.Bold,HorizontalAlignment=HorizontalAlignment.Center}};}
    private static bool Matches(SupplierRmaPerformance x,string filter)=>filter switch{"Affidabili (>= 80)"=>x.ReliabilityScore>=80,"Da monitorare (50-79)"=>x.ReliabilityScore>=50&&x.ReliabilityScore<80,"Critici (< 50)"=>x.ReliabilityScore<50,_=>true};
    private static Control Kpi(string label,string value,string color)=>new Border{Width=240,Padding=new Thickness(14,10),Background=UiTokens.Brush(UiTokens.Surface),BorderBrush=UiTokens.Brush(UiTokens.Border),BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(12),Child=new StackPanel{Spacing=2,Children={new TextBlock{Text=value,FontSize=22,FontWeight=FontWeight.Bold,Foreground=UiTokens.Brush(color)},new TextBlock{Text=label,Foreground=UiTokens.Brush(UiTokens.TextSecondary)}}}};
    private static Grid GridRow()=>new(){ColumnDefinitions=new ColumnDefinitions("210,75,70,75,80,105,80,85,75,115,110,100")};
    private static void Text(Grid g,string value,int col,bool strong=false)=>Add(g,new TextBlock{Text=value,FontWeight=strong?FontWeight.SemiBold:FontWeight.Normal,VerticalAlignment=VerticalAlignment.Center,TextTrimming=TextTrimming.CharacterEllipsis,Margin=new Thickness(3)},col);
    private static void Add(Grid g,Control c,int col){c.Margin=new Thickness(0,0,6,0);Grid.SetColumn(c,col);g.Children.Add(c);}
}
