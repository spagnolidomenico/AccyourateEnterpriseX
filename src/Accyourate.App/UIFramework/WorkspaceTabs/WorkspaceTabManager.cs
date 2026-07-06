namespace Accyourate.App.UIFramework.WorkspaceTabs;

public sealed class WorkspaceTabManager
{
    private readonly List<WorkspaceTab> _tabs = new();

    public event Action? Changed;

    public IReadOnlyList<WorkspaceTab> Tabs => _tabs;
    public WorkspaceTab? ActiveTab { get; private set; }
    public string ActiveTabId => ActiveTab?.Id ?? string.Empty;

    public void OpenOrActivate(WorkspaceTab tab)
    {
        var existing = _tabs.FirstOrDefault(t => t.Id == tab.Id);
        if (existing is not null)
        {
            ActiveTab = existing;
            Changed?.Invoke();
            return;
        }

        _tabs.Add(tab);
        ActiveTab = tab;
        Changed?.Invoke();
    }

    public void Activate(string id)
    {
        var tab = _tabs.FirstOrDefault(t => t.Id == id);
        if (tab is null)
            return;

        ActiveTab = tab;
        Changed?.Invoke();
    }

    public bool Close(string id)
    {
        var tab = _tabs.FirstOrDefault(t => t.Id == id);
        if (tab is null || !tab.CanClose || tab.IsPinned)
            return false;

        var index = _tabs.IndexOf(tab);
        _tabs.Remove(tab);

        if (ActiveTab?.Id == id)
        {
            if (_tabs.Count == 0)
                ActiveTab = null;
            else
                ActiveTab = _tabs[Math.Clamp(index - 1, 0, _tabs.Count - 1)];
        }

        Changed?.Invoke();
        return true;
    }

    public IReadOnlyList<string> OpenTabsSnapshot()
    {
        return _tabs.Select(t => t.Id).ToList();
    }

    public void CloseAllClosable()
    {
        _tabs.RemoveAll(t => t.CanClose && !t.IsPinned);
        ActiveTab = _tabs.FirstOrDefault(t => t.IsPinned) ?? _tabs.FirstOrDefault();
        Changed?.Invoke();
    }

    public void CloseOthers(string id)
    {
        _tabs.RemoveAll(t => t.Id != id && t.CanClose && !t.IsPinned);
        Activate(id);
        Changed?.Invoke();
    }
}
