using Avalonia;
using Avalonia.Controls;
using Accyourate.App.Data;
using Accyourate.App.Models;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App;

public sealed class ActionEngineWindow : Window
{
    public ActionEngineWindow(DatabaseService database, CurrentUser user)
    {
        Title = "Accyourate Enterprise X 11.1.0 RC3 - Action Engine";
        Width = 1180;
        Height = 820;
        MinWidth = 1040;
        MinHeight = 720;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = UiTokens.Brush(UiTokens.Background);
        Content = new ActionEngineView(database, user);
    }
}
