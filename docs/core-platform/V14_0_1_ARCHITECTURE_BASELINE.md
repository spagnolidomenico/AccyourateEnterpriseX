# 14.0.1 - Architecture Baseline

## Obiettivo

Introdurre la struttura iniziale della Core Platform senza modificare il comportamento dell'applicazione.

## Progetti aggiunti

- `Accyourate.Shared`
- `Accyourate.Domain`
- `Accyourate.Core`
- `Accyourate.Infrastructure`

## Entità iniziali

### Domain

- `Employee`
- `Asset`
- `AssetAssignment`
- `Company`
- `Site`
- `Department`
- `Supplier`

### Core contracts

- `IEmployeeRepository`
- `IAssetRepository`
- `IAssetAssignmentRepository`
- `IAccyourateModule`

### Shared

- `Result`

## Cosa NON cambia

- Nessuna UI modificata.
- Nessun database modificato.
- Nessuna logica esistente spostata.
- Asset Management e Master Data continuano a usare i servizi attuali.

## Criteri di accettazione

- La solution compila.
- L'app si avvia.
- Asset Management funziona.
- Anagrafica Aziendale funziona.
- CRUD Dipendenti continua a funzionare.
