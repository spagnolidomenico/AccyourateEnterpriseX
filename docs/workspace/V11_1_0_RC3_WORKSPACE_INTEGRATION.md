# 11.1.0 RC3 - Workspace Integration

## Obiettivo

Portare i moduli principali della Workspace sotto il nuovo sistema `IWorkspaceModule` + `WorkspaceModuleRegistry`.

## Moduli integrati

- Dashboard
- Digital Twin
- AI Assistant
- Action Engine

## Cosa cambia

- Aggiunti moduli:
  - `DigitalTwinWorkspaceModule`
  - `AiAssistantWorkspaceModule`
  - `ActionEngineWorkspaceModule`
- Aggiunto `ActionEngineView`, view riutilizzabile per finestra e tab.
- `ActionEngineWindow` diventa wrapper della view.
- La Workspace prova ad aprire i moduli principali tramite registry.
- Ridotte aperture dirette con `new XxxWindow().Show()` per i moduli principali.

## Universal Command Bar

La Universal Command Bar resta ancora finestra separata/dialog operativo.
La sua integrazione completa nella Workspace sarà trattata in uno sprint dedicato, perché richiede una scelta UX diversa: pannello overlay, command palette interna o tab dedicata.

## Criteri di accettazione

- Build locale riuscita.
- GitHub Actions verde.
- Dashboard come tab.
- Digital Twin come tab.
- AI Assistant come tab.
- Action Engine come tab.
- Nessuna duplicazione tab.
- Universal Command Bar ancora funzionante.
