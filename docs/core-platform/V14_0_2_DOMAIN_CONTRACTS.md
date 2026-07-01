# 14.0.2 - Domain Contracts

## Obiettivo

Introdurre contratti di dominio e repository condivisi senza modificare il comportamento dell'applicazione.

## Aggiunte

- `IRepository<TEntity>`
- `IReadOnlyRepository<TEntity>`
- repository contract per Employee, Asset, AssetAssignment, Company, Site, Department e Supplier
- `IUnitOfWork`
- `IDatabaseInitializer`
- `IClock`
- `SystemClock`
- `AssetStatus`
- `AssignmentStatus`

## Cosa NON cambia

- Nessuna UI modificata.
- Nessun database modificato.
- Nessun servizio esistente sostituito.
- Nessuna migrazione dati.
