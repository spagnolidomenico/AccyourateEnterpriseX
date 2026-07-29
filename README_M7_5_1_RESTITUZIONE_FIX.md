# M7.5.1 — Correzione riconsegna

Corregge l'errore SQLite:

`SQLite Error 1: near "=": syntax error`

La query che recupera la consegna attiva ora separa correttamente la clausola
`WHERE` dalla query base.

## Verifica

```powershell
dotnet clean
dotnet build
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

Seleziona un asset assegnato e premi `Restituisci`. L'asset deve tornare
disponibile e il registro consegne deve essere marcato come riconsegnato.
