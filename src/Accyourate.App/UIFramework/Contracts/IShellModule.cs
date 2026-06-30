namespace Accyourate.App.UIFramework.Contracts;

public interface IShellModule
{
    string Id { get; }
    string Title { get; }
    string Section { get; }
    string Icon { get; }
}
