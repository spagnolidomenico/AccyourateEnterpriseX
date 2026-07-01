# Sprint 11.1.0 RC1 - Workspace Module Registry

## Obiettivo

Introdurre il registro dei moduli Workspace senza modificare drasticamente il comportamento visibile.

## File probabilmente coinvolti

- `src/Accyourate.App/UIFramework/WorkspaceModules/IWorkspaceModule.cs`
- `src/Accyourate.App/UIFramework/WorkspaceModules/WorkspaceModuleRegistry.cs`
- `src/Accyourate.App/UIFramework/WorkspaceModules/DashboardWorkspaceModule.cs`
- `src/Accyourate.App/UIFramework/WorkspaceModules/DigitalTwinWorkspaceModule.cs`
- `src/Accyourate.App/UIFramework/WorkspaceModules/AiAssistantWorkspaceModule.cs`
- `src/Accyourate.App/EnterpriseWorkspaceWindow.cs`
- `CHANGELOG.md`
- `PROJECT_STATUS.md`

## Cosa deve fare

- registrare Dashboard, Digital Twin e AI Assistant;
- mantenere il comportamento validato;
- preparare RC2 OpenModule API;
- evitare nuove finestre per moduli già tab.

## Cosa NON deve fare

- non migrare Action Engine;
- non cambiare Universal Command Bar;
- non modificare database;
- non introdurre persistenza tab.

## Test

- Dashboard tab
- Digital Twin tab
- AI Assistant tab
- nessuna duplicazione tab
- Action Engine ancora funzionante
- Universal Command Bar ancora funzionante
