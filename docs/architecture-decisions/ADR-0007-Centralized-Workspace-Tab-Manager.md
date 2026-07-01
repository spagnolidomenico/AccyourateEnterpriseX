# ADR-0007 - WorkspaceTabManager centralizzato

## Stato

Accettato

## Contesto

Dashboard e Digital Twin sono stati migrati come schede usando infrastruttura simile. Continuare con manager separati avrebbe creato duplicazione e complessità.

## Decisione

Consolidare Dashboard e Digital Twin su un `WorkspaceTabManager` centrale.

## Motivazione

- Riduce duplicazioni.
- Prepara la migrazione dei moduli successivi.
- Facilita la futura persistenza dello stato della Workspace.
- Migliora la gestione degli errori.

## Conseguenze

- Dashboard e Digital Twin condividono lo stesso host di schede.
- I moduli futuri potranno essere aperti tramite la stessa API.
