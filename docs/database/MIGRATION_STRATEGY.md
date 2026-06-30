# Strategia migrazioni database

## Stato attuale

Le tabelle sono ancora create automaticamente all'avvio per semplicità.

## Stato futuro

Passaggio progressivo a migrazioni versionate:

```text
001_core.sql
002_people_assets.sql
003_workflow.sql
004_medical.sql
004_001_production_quality.sql
004_002_warehouse_logistics.sql
004_003_laundry_maintenance.sql
005_documents.sql
005_006_architecture.sql
```

## Regola

Ogni nuova release che modifica il database dovrà avere uno script dedicato.
