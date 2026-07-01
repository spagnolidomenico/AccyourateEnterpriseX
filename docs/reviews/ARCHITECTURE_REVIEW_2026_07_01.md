# Accyourate Enterprise X - Architecture Review Report

Data: 2026-07-01  
Baseline analizzata: repository reale caricato dopo validazione 11.0.5.

## Executive Summary

Il repository ha superato la fase di prototipo iniziale. Sono già presenti componenti importanti:

- Enterprise Workspace
- Dashboard
- Digital Twin
- AI Assistant
- Action Engine
- Enterprise Search Service
- Universal Command Bar
- Workspace Tabs foundation
- documentazione, roadmap e ADR

La direzione architetturale è corretta: la Workspace sta diventando il centro dell'applicazione e il Digital Twin sta emergendo come nucleo del prodotto.

Il prossimo salto di qualità non deve essere una nuova funzionalità visibile, ma una rifattorizzazione controllata: `Workspace Module Registry` e `OpenModule API`.

## Metriche repository

- File C#: 159
- File Markdown: 127
- Cartelle: 377
- File più grandi:
  - `src/Accyourate.App/Data/DatabaseService.cs`: 2753 righe
  - `src/Accyourate.App/MainWindow.cs`: 556 righe
  - `src/Accyourate.App/AppleStyleDashboardWindow.cs`: 490 righe
  - `src/Accyourate.App/ThemePersonalizationWindow.cs`: 481 righe
  - `src/Accyourate.App/EnterpriseWorkspaceWindow.cs`: 465 righe
  - `src/Accyourate.App/UIFramework/Shell/WorkspaceModuleFactory.cs`: 461 righe
  - `src/Accyourate.App/WarehouseLogisticsWindow.cs`: 447 righe
  - `src/Accyourate.App/AssetsWindow.cs`: 409 righe
  - `src/Accyourate.App/BrandedHomeWindow.cs`: 372 righe
  - `src/Accyourate.App/EmployeesWindow.cs`: 364 righe

## Punti di forza

### 1. Visione prodotto chiara

Il progetto non è più un gestionale generico. La direzione è una piattaforma enterprise per Digital Twin medicali, AI e Workspace modulare.

### 2. Processo di sviluppo migliorato

Sono presenti:

- GitHub
- sprint validati
- patch incrementali
- smoke test PowerShell
- documentazione tecnica
- ADR
- changelog

### 3. Workspace come centro dell'esperienza

Dashboard, Digital Twin e AI Assistant sono già stati avviati verso un modello a schede.

### 4. AI e Action Engine già separati

AI Routing, Intent Catalog, Action Engine e Capability Registry sono concetti corretti e scalabili.

## Criticità osservate

### 1. EnterpriseWorkspaceWindow sta diventando troppo centrale

`EnterpriseWorkspaceWindow.cs` contiene ancora logica di:

- layout;
- navigazione;
- apertura moduli;
- eccezioni per moduli specifici;
- gestione tab;
- apertura finestre esterne.

È il punto più importante da rifattorizzare.

### 2. Coesistono più concetti di registry

Sono presenti:

- `Framework/ModuleRegistry`
- `Shell/ShellRegistry`
- `WorkspaceModuleFactory`
- nuove idee di `WorkspaceModuleRegistry`

Questi concetti devono essere chiariti e separati:

- ShellRegistry = voci di navigazione.
- WorkspaceModuleRegistry = moduli apribili nella Workspace.
- Framework/ModuleRegistry = vecchi placeholder CRUD, da valutare come legacy o area Enterprise Admin.

### 3. WorkspaceTabHost e WorkspaceHost duplicano concetti

Esistono sia `WorkspaceTabHost` sia `WorkspaceHost`. Questo può creare confusione.

Raccomandazione: mantenere un solo componente principale per l'hosting delle schede.

### 4. DatabaseService è troppo grande

`DatabaseService.cs` ha molte righe. Non è urgente, ma diventerà un problema quando aumenteranno dati medicali, telemetria e report.

### 5. Alcuni moduli sono ancora finestre

Action Engine, Universal Command Bar, AI Intent Catalog e altre funzioni devono gradualmente diventare Workspace Module o dialoghi temporanei, secondo una regola chiara.

## Raccomandazioni prioritarie

### Priorità 1 - Workspace Module Registry

Introdurre:

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

e un registro centrale.

### Priorità 2 - OpenModule API

Tutte le aperture modulo devono passare da:

```csharp
OpenModule("dashboard");
OpenModule("digital-twin");
OpenModule("ai-assistant");
```

### Priorità 3 - eliminare if/switch speciali

Ridurre progressivamente blocchi del tipo:

```csharp
if (moduleId == "dashboard") ...
if (moduleId == "digital-twin") ...
if (moduleId == "ai-assistant") ...
```

### Priorità 4 - unificare WorkspaceHost

Scegliere un unico componente host tab e deprecare l'altro.

### Priorità 5 - introdurre Technical Debt Log

Annotare debito tecnico senza interrompere gli sprint.

## Roadmap consigliata

| Release | Obiettivo |
|---|---|
| 11.1.0 RC1 | Workspace Module Registry |
| 11.1.0 RC2 | OpenModule API |
| 11.1.0 RC3 | Workspace Explorer |
| 11.1.0 RC4 | Status Bar 2.0 |
| 11.1.0 Stable | Workspace 2.0 consolidata |
| 12.0 | Medical/Digital Twin demo flow |
