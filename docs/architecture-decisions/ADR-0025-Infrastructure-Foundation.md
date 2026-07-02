# ADR-0025 - Infrastructure Foundation

## Stato

Accettato

## Decisione

Introdurre `AccyourateDatabaseContext`, `SqliteRepositoryBase`, `EmployeeRepository` e `AssetRepository`.

## Motivazione

- Centralizzare la gestione SQLite.
- Preparare test automatici.
- Ridurre accessi diretti al database dai servizi.
- Consentire migrazione incrementale.
