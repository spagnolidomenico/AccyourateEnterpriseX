using Avalonia.Controls;

namespace Accyourate.App.UIFramework.WorkspaceTabs;

public interface ITabContent
{
    string TabId { get; }
    string Title { get; }
    string Icon { get; }
    Control BuildContent();
}
