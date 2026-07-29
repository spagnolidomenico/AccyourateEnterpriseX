# M7.5.2 — Sidebar Readability Fix

## Correzioni

- colore del testo esplicito per ogni voce della sidebar;
- contrasto più elevato per stato normale, hover e selezionato;
- etichette non più dipendenti dal colore ereditato dal tema Avalonia;
- stato visivo ripristinato correttamente quando il puntatore esce;
- migliore leggibilità del titolo `AREE`;
- comportamento coerente anche con menu compresso e cambio tema;
- barra contestuale con testo e hover ad alto contrasto.

## Verifica

```powershell
dotnet clean
dotnet build
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

Passa più volte tra le aree, comprimi/espandi la sidebar e cambia tema.
Tutte le voci devono rimanere leggibili in stato normale, hover e selezionato.
