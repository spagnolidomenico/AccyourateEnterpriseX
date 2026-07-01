# ADR-0010 - Workspace Module Registry

## Stato

Accettato

## Contesto

La Workspace stava accumulando logiche speciali per aprire moduli specifici, ad esempio Dashboard, Digital Twin e AI Assistant.

## Decisione

Introdurre `IWorkspaceModule` e `WorkspaceModuleRegistry`.

Ogni modulo registrato dichiara:

- Id
- Titolo
- Icona
- chiudibilità
- stato pinned
- factory della view

## Motivazione

Questa decisione riduce gli `if` speciali e prepara l'API unica `OpenModule()` prevista nella RC2.

## Conseguenze

- I moduli iniziano a essere descritti in modo uniforme.
- La Workspace diventa più estendibile.
- I moduli futuri potranno essere aggiunti con meno modifiche al codice della finestra principale.
