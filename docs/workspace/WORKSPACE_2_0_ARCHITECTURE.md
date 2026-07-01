# Workspace 2.0 - Architecture

## Architettura logica

```text
EnterpriseWorkspaceWindow
        │
        ▼
WorkspaceShell
        │
        ├── WorkspaceExplorer
        ├── WorkspaceTabHost
        ├── WorkspaceStatusBar
        └── UniversalCommandBar
                │
                ▼
        WorkspaceModuleRegistry
                │
                ▼
        IWorkspaceModule
```

## Contratto modulo

```csharp
public interface IWorkspaceModule
{
    string Id { get; }
    string Title { get; }
    string Icon { get; }
    bool CanClose { get; }
    bool IsPinned { get; }

    Control CreateView();
}
```

## API principale

```csharp
OpenModule("dashboard");
OpenModule("digital-twin");
OpenModule("ai-assistant");
OpenModule("action-engine");
```

## Regola

Nessuna parte dell'applicazione deve aprire direttamente un modulo con logica propria.

Tutti devono passare da:

```csharp
WorkspaceModuleRegistry
WorkspaceTabManager
OpenModule()
```

## Benefici

- meno duplicazione;
- meno finestre separate;
- navigazione uniforme;
- facile integrazione con AI;
- base per plugin futuri;
- base per persistenza Workspace.
