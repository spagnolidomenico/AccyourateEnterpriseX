# 14.0.3 - Infrastructure Foundation

## Obiettivo

Introdurre il primo livello infrastrutturale concreto senza modificare il comportamento dell'applicazione.

## Componenti introdotti

- `AccyourateDatabaseOptions`
- `AccyourateDatabaseContext`
- `SqliteRepositoryBase`
- `EmployeeRepository`
- `AssetRepository`

## Cosa NON cambia

- La UI non viene modificata.
- `AssetService` e `MasterDataService` non vengono ancora sostituiti.
- Nessuna migrazione database.
- Nessun comportamento utente cambia.

## Nota

I repository sono introdotti come infrastruttura parallela. Nei prossimi sprint verranno collegati gradualmente ai servizi esistenti.
