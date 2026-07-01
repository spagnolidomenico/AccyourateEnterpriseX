using Avalonia;
using Avalonia.Controls;
using Accyourate.App.Data;
using Accyourate.App.Models;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App;

public sealed class UniversalCommandBarWindow : Window
{
    public UniversalCommandBarWindow(DatabaseService database, CurrentUser user, Action<string, string>? navigate = null)
    {
        Title = "Universal Command Bar";
        Width = 920;
        Height = 720;
        MinWidth = 820;
        MinHeight = 620;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = UiTokens.Brush(UiTokens.Background);
        Content = new UniversalCommandBarView(database, user, navigate);
    }
}
