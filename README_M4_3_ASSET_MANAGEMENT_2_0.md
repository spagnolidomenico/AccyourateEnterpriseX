# M4.3 — Asset Management 2.0

## Funzioni introdotte

- Vista Lista e vista Card nello stesso workspace.
- KPI cliccabili per filtrare Totale, Disponibili, Assegnati, Manutenzione e Garanzie in scadenza.
- Indicazione del filtro KPI attivo nel riepilogo risultati.
- Card asset con identità, stato, seriale, assegnatario, garanzia e accesso al dettaglio.
- Pulsanti Lista/Card con stato visivo attivo.
- Reimposta filtri rimuove anche il filtro KPI.

## Verifica

```powershell
dotnet build
dotnet run --project src/Accyourate.App/Accyourate.App.csproj
powershell -ExecutionPolicy Bypass -File .\scripts\test-m4-3-asset-management-2-0.ps1
```
