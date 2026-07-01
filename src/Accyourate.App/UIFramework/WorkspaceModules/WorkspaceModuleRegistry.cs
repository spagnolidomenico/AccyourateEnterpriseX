namespace Accyourate.App.UIFramework.WorkspaceModules;

public sealed class WorkspaceModuleRegistry
{
    private readonly Dictionary<string, IWorkspaceModule> _modules = new(StringComparer.OrdinalIgnoreCase);

    public void Register(IWorkspaceModule module)
    {
        if (string.IsNullOrWhiteSpace(module.Id))
            throw new ArgumentException("Workspace module Id cannot be empty.", nameof(module));

        _modules[module.Id] = module;
    }

    public bool Contains(string moduleId)
    {
        return _modules.ContainsKey(moduleId);
    }

    public IWorkspaceModule? Find(string moduleId)
    {
        return _modules.TryGetValue(moduleId, out var module) ? module : null;
    }

    public IReadOnlyList<IWorkspaceModule> GetAll()
    {
        return _modules.Values
            .OrderBy(m => m.Title)
            .ToList();
    }

    public IReadOnlyList<WorkspaceModuleDescriptor> GetDescriptors()
    {
        return GetAll()
            .Select(m => new WorkspaceModuleDescriptor
            {
                Id = m.Id,
                Title = m.Title,
                Icon = m.Icon,
                CanClose = m.CanClose,
                IsPinned = m.IsPinned
            })
            .ToList();
    }
}
