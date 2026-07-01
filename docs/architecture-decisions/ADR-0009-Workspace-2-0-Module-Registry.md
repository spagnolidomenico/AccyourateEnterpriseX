# ADR-0009 - Workspace 2.0 e Module Registry

## Stato

Proposto

## Contesto

La Workspace sta diventando il centro dell'applicazione. I moduli devono essere aperti in modo uniforme, senza logiche speciali sparse nel codice.

## Decisione

Introdurre `IWorkspaceModule` e `WorkspaceModuleRegistry`.

Ogni modulo dichiarerà:

- Id
- Titolo
- Icona
- View
- comportamento tab

## Conseguenze positive

- apertura moduli uniforme;
- riduzione duplicazione;
- migliore integrazione con AI;
- base per plugin futuri;
- migliore testabilità.

## Conseguenze negative

- richiede una migrazione graduale dei moduli esistenti;
- aggiunge un livello di astrazione iniziale.

## Decisione finale

Da approvare prima dello Sprint 11.1.0 RC1.
