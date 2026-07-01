using Avalonia;
using Avalonia.Controls;
using Accyourate.App.Data;
using Accyourate.App.Models;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App;

public sealed class EnterpriseAiAssistantWindow : Window
{
    public EnterpriseAiAssistantWindow(DatabaseService database, CurrentUser user)
    {
        Title = "Accyourate Enterprise X 11.0.5 - AI Assistant";
        Width = 1040;
        Height = 760;
        MinWidth = 860;
        MinHeight = 640;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = UiTokens.Brush(UiTokens.Background);
        Content = new EnterpriseAiAssistantView(database, user);
    }
}
