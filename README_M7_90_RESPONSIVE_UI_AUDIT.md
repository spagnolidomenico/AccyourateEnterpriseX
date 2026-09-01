# M7.90 - Responsive UI Audit

Questa versione introduce un controllo statico ripetibile dell'interfaccia Avalonia.

## Controlli

- larghezze minime pari o superiori a 900 px;
- griglie con molte colonne fisse e larghezza complessiva elevata;
- intestazioni legacy `*,Auto`;
- scorrimento orizzontale automatico;
- StackPanel orizzontali che non possono andare a capo.

## Esecuzione

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\audit-responsive-ui.ps1
```

I risultati vengono salvati in:

```text
artifacts\responsive-ui-audit\responsive-ui-audit.md
artifacts\responsive-ui-audit\responsive-ui-findings.csv
```

Per una verifica automatica che restituisca errore quando esistono segnalazioni High:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\audit-responsive-ui.ps1 -FailOnHigh
```

La presenza di una segnalazione non significa sempre che la schermata sia difettosa: le tabelle realmente tabellari possono mantenere uno scorrimento orizzontale locale. Il report serve a stabilire l'ordine del collaudo e delle migrazioni.
