# CORE-001 - Warning Cleanup

## Obiettivo

Eliminare i warning emersi dopo gli sprint Search e Document Center.

## Correzioni

- Inizializzazione esplicita di `SettingsService` e `DocumentService` in `DeliveryReportPdfService`.
- Correzione chiamata asincrona non attesa in `AssetManagementView`.

## Cosa NON cambia

- Nessuna nuova funzionalità.
- Nessuna modifica database.
- Nessuna modifica UI visibile.

## Test

Eseguire:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-smoke.ps1
```
