# ADR-0026 - Employee Unification Preflight

## Stato

Accettato

## Contesto

Il progetto ha due database separati e due tabelle Employees. Il tentativo di collegare direttamente Asset e Master Data ha prodotto errori di foreign key.

## Decisione

Prima di implementare la migrazione, introdurre una fase di preflight e ispezione dei database reali.

## Motivazione

- Evitare perdita dati.
- Evitare errori FK.
- Capire lo schema reale.
- Preparare una migrazione sicura e incrementale.

## Conseguenze

La 14.0.4 viene divisa in:

- 14.0.4A Preflight
- 14.0.4B Employee Sync Bridge
- 14.0.4C Relationship Engine Rebuild
