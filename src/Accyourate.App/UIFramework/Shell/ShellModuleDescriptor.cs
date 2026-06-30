using Accyourate.App.UIFramework.Contracts;

namespace Accyourate.App.UIFramework.Shell;

public sealed class ShellModuleDescriptor : IShellModule
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string Section { get; init; } = "";
    public string Icon { get; init; } = "";
}
