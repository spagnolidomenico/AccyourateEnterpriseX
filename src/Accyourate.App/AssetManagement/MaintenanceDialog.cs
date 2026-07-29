using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.AssetManagement.Models;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.AssetManagement;

public sealed class MaintenanceDialog : Window
{
    private readonly TextBox _title = new();
    private readonly TextBox _description = new() { AcceptsReturn=true, TextWrapping=TextWrapping.Wrap, MinHeight=90 };
    private readonly ComboBox _priority = new();
    private readonly TextBox _technician = new();
    private readonly TextBox _scheduled = new() { Watermark="gg/mm/aaaa" };
    private readonly TextBlock _message = new();

    public MaintenanceDialog(string assetCode)
    {
        Title="Nuovo intervento"; Width=560; Height=600; MinWidth=500; MinHeight=540;
        WindowStartupLocation=WindowStartupLocation.CenterOwner;
        Background=UiTokens.Brush(UiTokens.Background);
        _priority.ItemsSource=new[]{"Bassa","Media","Alta","Urgente"}; _priority.SelectedIndex=1;
        var root=new StackPanel{Spacing=12,Margin=new Thickness(24)};
        root.Children.Add(new TextBlock{Text="Apri manutenzione",FontSize=25,FontWeight=FontWeight.Bold});
        root.Children.Add(new TextBlock{Text=assetCode,Foreground=UiTokens.Brush(UiTokens.TextSecondary)});
        root.Children.Add(Field("Titolo",_title)); root.Children.Add(Field("Descrizione guasto",_description));
        root.Children.Add(Field("Priorità",_priority)); root.Children.Add(Field("Tecnico",_technician));
        root.Children.Add(Field("Data prevista",_scheduled)); root.Children.Add(_message);
        var buttons=new Grid{ColumnDefinitions=new ColumnDefinitions("*,120,150")};
        var cancel=Button("Annulla",false); cancel.Click+=(_,_)=>Close(null);
        var save=Button("Apri intervento",true); save.Click+=(_,_)=>Confirm();
        Add(buttons,cancel,1); Add(buttons,save,2); root.Children.Add(buttons); Content=root;
    }

    private void Confirm()
    {
        if(string.IsNullOrWhiteSpace(_title.Text)){_message.Text="Inserisci il titolo.";_message.Foreground=UiTokens.Brush(UiTokens.Danger);return;}
        Close(new MaintenanceTicket
        {
            Title=_title.Text.Trim(), Description=_description.Text?.Trim()??"",
            Priority=_priority.SelectedItem?.ToString()??"Media", Technician=_technician.Text?.Trim()??"",
            ScheduledAt=ParseDate(_scheduled.Text), Status="Aperto"
        });
    }
    private static string ParseDate(string? value)=>DateTime.TryParse(value,out var d)?d.ToString("s"):value?.Trim()??"";
    private static Control Field(string label,Control c)=>new StackPanel{Spacing=5,Children={new TextBlock{Text=label,FontWeight=FontWeight.SemiBold},c}};
    private static Button Button(string text,bool primary)=>new(){Content=text,Height=38,Margin=new Thickness(6,0,0,0),HorizontalContentAlignment=HorizontalAlignment.Center,Background=UiTokens.Brush(primary?UiTokens.BrandBlue:UiTokens.SurfaceAlt),Foreground=primary?Brushes.White:UiTokens.Brush(UiTokens.TextPrimary)};
    private static void Add(Grid g,Control c,int col){Grid.SetColumn(c,col);g.Children.Add(c);}
}

public sealed class MaintenanceCompletionDialog : Window
{
    private readonly TextBox _resolution=new(){AcceptsReturn=true,TextWrapping=TextWrapping.Wrap,MinHeight=110};
    private readonly TextBox _cost=new(){Watermark="0,00"};
    private readonly CheckBox _pdf=new(){Content="Genera e apri verbale PDF",IsChecked=true};
    private readonly TextBlock _message=new();
    public MaintenanceCompletionDialog(string title)
    {
        Title="Completa manutenzione";Width=540;Height=450;WindowStartupLocation=WindowStartupLocation.CenterOwner;
        Background=UiTokens.Brush(UiTokens.Background);
        var root=new StackPanel{Spacing=13,Margin=new Thickness(24),Children={
            new TextBlock{Text="Completa intervento",FontSize=25,FontWeight=FontWeight.Bold},
            new TextBlock{Text=title,Foreground=UiTokens.Brush(UiTokens.TextSecondary)},
            new TextBlock{Text="Risoluzione",FontWeight=FontWeight.SemiBold},_resolution,
            new TextBlock{Text="Costo (€)",FontWeight=FontWeight.SemiBold},_cost,_pdf,_message}};
        var buttons=new Grid{ColumnDefinitions=new ColumnDefinitions("*,120,150")};
        var cancel=new Button{Content="Annulla"};cancel.Click+=(_,_)=>Close(null);
        var save=new Button{Content="Completa",Background=UiTokens.Brush(UiTokens.BrandBlue),Foreground=Brushes.White};save.Click+=(_,_)=>Confirm();
        Grid.SetColumn(cancel,1);Grid.SetColumn(save,2);buttons.Children.Add(cancel);buttons.Children.Add(save);root.Children.Add(buttons);Content=root;
    }
    private void Confirm()
    {
        if(string.IsNullOrWhiteSpace(_resolution.Text)){_message.Text="Descrivi la risoluzione.";_message.Foreground=UiTokens.Brush(UiTokens.Danger);return;}
        decimal.TryParse(_cost.Text,out var cost);
        Close(new MaintenanceCompletionResult{Resolution=_resolution.Text.Trim(),Cost=cost,GeneratePdf=_pdf.IsChecked==true});
    }
}
public sealed class MaintenanceCompletionResult{public string Resolution{get;init;}="";public decimal Cost{get;init;}public bool GeneratePdf{get;init;}}
