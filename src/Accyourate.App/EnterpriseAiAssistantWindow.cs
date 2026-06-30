using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.Models;
using Accyourate.App.UIFramework.AI;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App;

public sealed class EnterpriseAiAssistantWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;
    private readonly AiAssistantEngine _engine;

    private readonly TextBox _input = new();
    private readonly StackPanel _conversation = new();

    public EnterpriseAiAssistantWindow(DatabaseService database, CurrentUser user)
    {
        _database = database;
        _user = user;
        _engine = new AiAssistantEngine(_database);

        Title = "Accyourate Enterprise X 10.0 RC1 - AI Routing Assistant";
        Width = 1040;
        Height = 760;
        MinWidth = 860;
        MinHeight = 640;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();

        AddAssistantMessage("Ciao, sono l'assistente enterprise di Accyourate. Posso aiutarti a orientarti tra moduli, documenti, dispositivi, KPI e configurazioni.");
        AddAssistantMessage("Esempi: “Quanti dispositivi medici ci sono?”, “Apri analytics”, “Cerca documenti”, “Mostrami asset IT”.");
    }

    private Control BuildLayout()
    {
        var root = new DockPanel();

        var header = new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(24, 18),
            Child = new StackPanel
            {
                Spacing = 4,
                Children =
                {
                    new TextBlock { Text = "Enterprise AI Assistant", FontSize = 28, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary) },
                    new TextBlock { Text = "Routing Engine con sinonimi, confidenza e intenti Digital Twin", Foreground = UiTokens.Brush(UiTokens.TextSecondary) }
                }
            }
        };
        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        var inputBar = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,120"),
            Background = UiTokens.Brush(UiTokens.Surface),
            Margin = new Thickness(0),
        };

        _input.Watermark = "Chiedi qualcosa al gestionale...";
        _input.Margin = new Thickness(18);
        Add(inputBar, _input, 0, 0);

        var send = new Button
        {
            Content = "Invia",
            Background = UiTokens.Brush(UiTokens.BrandBlue),
            Foreground = Brushes.White,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(14, 10),
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(0, 18, 18, 18)
        };
        send.Click += (_, _) => Send();
        Add(inputBar, send, 1, 0);

        DockPanel.SetDock(inputBar, Dock.Bottom);
        root.Children.Add(inputBar);

        var quick = new WrapPanel { Margin = new Thickness(18), ItemWidth = 220, ItemHeight = 48 };
        quick.Children.Add(Quick("Quanti dispositivi medici ci sono?"));
        quick.Children.Add(Quick("Mostrami i documenti"));
        quick.Children.Add(Quick("Apri Analytics"));
        quick.Children.Add(Quick("Asset IT disponibili"));
        quick.Children.Add(Quick("Quanti test qualità ci sono?"));
        quick.Children.Add(Quick("Mostrami i Digital Twin"));
        quick.Children.Add(Quick("Telemetria dispositivi ECG"));
        quick.Children.Add(Quick("Battito cardiaco e batteria"));
        var catalog = Quick("Catalogo intenti");
        catalog.Click += (_, _) => new AiIntentCatalogManagerWindow().Show();
        quick.Children.Add(catalog);
        var action = Quick("Action Engine");
        action.Click += (_, _) => new ActionEngineWindow(_database, _user).Show();
        quick.Children.Add(action);
        quick.Children.Add(Quick("Manutenzioni aperte"));
        DockPanel.SetDock(quick, Dock.Top);
        root.Children.Add(quick);

        root.Children.Add(new ScrollViewer
        {
            Content = _conversation,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        });

        return root;
    }

    private Button Quick(string text)
    {
        var b = new Button
        {
            Content = text,
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            Foreground = UiTokens.Brush(UiTokens.TextPrimary),
            Padding = new Thickness(10, 8),
            Margin = new Thickness(6),
            CornerRadius = new CornerRadius(12)
        };
        b.Click += (_, _) =>
        {
            _input.Text = text;
            Send();
        };
        return b;
    }

    private void Send()
    {
        var q = _input.Text ?? "";
        if (string.IsNullOrWhiteSpace(q))
            return;

        AddUserMessage(q);
        var result = _engine.Analyze(q);
        AddAssistantMessage($"{result.Category}\n\n{result.Explanation}\n\nAzione suggerita: {result.SuggestedAction}");
        _input.Text = "";
    }

    private void AddUserMessage(string text)
    {
        _conversation.Children.Add(Message($"👤 {_user.Username}", text, "#E8F1FF"));
    }

    private void AddAssistantMessage(string text)
    {
        _conversation.Children.Add(Message("🤖 Accyourate AI", text, "#FFFFFF"));
    }

    private static Border Message(string who, string text, string background)
    {
        var stack = new StackPanel { Spacing = 6 };
        stack.Children.Add(new TextBlock { Text = who, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary) });
        stack.Children.Add(new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap, Foreground = UiTokens.Brush(UiTokens.TextSecondary) });

        return new Border
        {
            Background = UiTokens.Brush(background),
            CornerRadius = new CornerRadius(18),
            Padding = new Thickness(16),
            Margin = new Thickness(18, 8),
            Child = stack
        };
    }

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
