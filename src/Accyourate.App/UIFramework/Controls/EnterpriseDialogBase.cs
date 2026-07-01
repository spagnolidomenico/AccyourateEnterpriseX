using Avalonia;
using Avalonia.Controls;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.Controls;

public abstract class EnterpriseDialogBase : Window
{
    protected EnterpriseDialogBase(string title, double width = 760, double height = 720)
    {
        Title = title;
        Width = width;
        Height = height;
        MinWidth = Math.Min(620, width);
        MinHeight = Math.Min(560, height);
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = UiTokens.Brush(UiTokens.Background);
    }
}
