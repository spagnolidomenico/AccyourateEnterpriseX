# Workspace 2.0 - Migration Plan

## Stato attuale

Completato:

- 11.0.1 Workspace Foundation
- 11.0.2 Dashboard Tab
- 11.0.3 Digital Twin Tab
- 11.0.4 Workspace Stabilization
- 11.0.5 AI Assistant Tab

## Nuova fase

### 11.1.0 RC1 - Workspace Module Registry

- introdurre `IWorkspaceModule`;
- introdurre `WorkspaceModuleRegistry`;
- registrare Dashboard;
- registrare Digital Twin;
- registrare AI Assistant;
- nessun cambiamento visivo rilevante.

### 11.1.0 RC2 - OpenModule API

- creare API unica `OpenModule(moduleId)`;
- sostituire aperture speciali con API centralizzata;
- mantenere comportamento corrente.

### 11.1.0 RC3 - Workspace Explorer

- aggiungere Explorer laterale;
- mostrare moduli;
- mostrare recenti;
- mostrare preferiti foundation.

### 11.1.0 RC4 - Status Bar 2.0

- utente;
- versione;
- moduli aperti;
- stato AI;
- stato Digital Twin;
- orario ultima attività.

### 11.1.0 Stable

- stabilizzazione;
- pulizia codice;
- aggiornamento Project Book;
- tag Git.
