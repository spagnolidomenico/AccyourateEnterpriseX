namespace Accyourate.App.UIFramework.WorkspaceTabs;

public sealed class WorkspaceModuleRegistry
{
    private readonly Dictionary<string, IWorkspaceModule> _modules = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<IWorkspaceModule> Modules => _modules.Values.ToList();

    public void Register(IWorkspaceModule module)
    {
        if (string.IsNullOrWhiteSpace(module.Id))
            return;

        _modules[module.Id] = module;
    }

    public bool TryGet(string moduleId, out IWorkspaceModule module)
    {
        return _modules.TryGetValue(moduleId, out module!);
    }

    public IWorkspaceModule? Find(string moduleId)
    {
        return _modules.TryGetValue(moduleId, out var module) ? module : null;
    }
}
