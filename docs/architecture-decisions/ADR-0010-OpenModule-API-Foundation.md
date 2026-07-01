# ADR-0010 - OpenModule API Foundation

## Stato

Accettato

## Contesto

La Workspace 2.0 richiede un sistema uniforme per aprire moduli senza logiche speciali sparse nel codice.

## Decisione

Introdurre `IWorkspaceModule` e `WorkspaceModuleRegistry` come fondamenta dell'API `OpenModule`.

## Motivazione

- Ridurre duplicazione nella Workspace.
- Rendere i moduli autodescrittivi.
- Preparare l'integrazione di nuovi moduli.
- Abilitare in futuro plugin, Explorer, preferiti e persistenza.

## Conseguenze

Questa RC non cambia il comportamento visibile, ma introduce l'astrazione necessaria per le RC successive.

## Alternative considerate

Continuare con `if (moduleId == "...")`, scartato perché non scalabile.
