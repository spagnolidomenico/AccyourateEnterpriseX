# M7.7 — Registro Consegne

## Funzioni

- elenco delle consegne registrate;
- ricerca per asset, dipendente e note;
- filtri per stato e intervallo di date;
- riconsegna direttamente dal registro;
- apertura o generazione del verbale PDF;
- apertura del dettaglio dell'asset nel workspace;
- accesso dalle aree Asset e Persone;
- righe alternate e colonne azione allineate.

## Verifica

```powershell
dotnet clean
dotnet build
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

Apri `Asset → Registro consegne`, prova ricerca e filtri, apri un PDF,
apri il dettaglio asset e verifica una riconsegna.
