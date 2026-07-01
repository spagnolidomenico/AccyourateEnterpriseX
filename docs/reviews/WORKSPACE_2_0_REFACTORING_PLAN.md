# Workspace 2.0 - Refactoring Plan

## Obiettivo

Rendere la Workspace modulare, scalabile e pronta ad accogliere nuovi moduli senza aumentare la complessità.

## Problema attuale

La Workspace contiene ancora logica specifica per alcuni moduli. Questo approccio va bene per pochi moduli, ma non scala quando Accyourate avrà:

- Reports
- Medical Dashboard
- ECG Viewer
- Telemetry
- Documents
- Quality
- Admin
- Cloud Sync

## Target Architecture

```text
EnterpriseWorkspaceWindow
    │
    ▼
WorkspaceShell
    │
    ├── WorkspaceModuleRegistry
    │       ├── DashboardModule
    │       ├── DigitalTwinModule
    │       ├── AiAssistantModule
    │       └── Future modules
    │
    ├── WorkspaceTabManager
    ├── WorkspaceHost
    └── OpenModule API
```

## Sprint 11.1.0 RC1 - Module Registry

### Introdurre

- `IWorkspaceModule`
- `WorkspaceModuleDescriptor`
- `WorkspaceModuleRegistry`
- `DashboardWorkspaceModule`
- `DigitalTwinWorkspaceModuleAdapter`
- `AiAssistantWorkspaceModule`

### Non modificare ancora

- Universal Command Bar
- Action Engine
- Control Room
- AI Intent Catalog

## Sprint 11.1.0 RC2 - OpenModule API

### Introdurre

- `OpenModule(moduleId)`
- gestione errori centralizzata
- tab generation centralizzata

### Rimuovere gradualmente

- rami speciali per Dashboard, Digital Twin, AI Assistant

## Sprint 11.1.0 RC3 - Workspace Explorer

Aggiungere una vista laterale con:

- moduli disponibili;
- recenti;
- preferiti foundation.

## Sprint 11.1.0 RC4 - Status Bar 2.0

Mostrare:

- utente;
- versione;
- moduli aperti;
- stato AI;
- stato Digital Twin;
- ultima attività.

## Criteri di successo

- aggiungere un modulo nuovo richiede una classe e una registrazione;
- EnterpriseWorkspaceWindow si riduce;
- niente duplicazioni di tab;
- build e smoke test passano;
- test funzionale invariato.
