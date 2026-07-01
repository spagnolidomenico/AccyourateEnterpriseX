# 11.1.0 RC2.2.1 - Ambiguous References Hotfix

## Problema

La build falliva perché nel progetto erano presenti riferimenti ambigui:

- `IWorkspaceModule`
- `WorkspaceModuleRegistry`

Il compilatore trovava due namespace compatibili:

- `Accyourate.App.UIFramework.WorkspaceModules`
- `Accyourate.App.UIFramework.WorkspaceTabs`

## Correzione

In `EnterpriseWorkspaceWindow.cs` sono stati introdotti alias espliciti:

```csharp
using WorkspaceModuleRegistryCore = Accyourate.App.UIFramework.WorkspaceModules.WorkspaceModuleRegistry;
using WorkspaceModuleCore = Accyourate.App.UIFramework.WorkspaceModules.IWorkspaceModule;
using DashboardWorkspaceModuleCore = Accyourate.App.UIFramework.WorkspaceModules.DashboardWorkspaceModule;
```

## Criteri di accettazione

- Build locale riuscita.
- Dashboard funziona come prima.
- Nessuna regressione su Digital Twin, AI Assistant, Action Engine e Universal Command Bar.
