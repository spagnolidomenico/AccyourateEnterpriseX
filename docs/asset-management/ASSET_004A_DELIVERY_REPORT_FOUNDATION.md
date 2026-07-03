# ASSET-004A - Delivery Report Foundation

## Obiettivo

Introdurre la base dati e il servizio applicativo per i verbali di consegna beni aziendali.

## Componenti introdotti

- `DeliveryReport`
- `DeliveryReportItem`
- `DeliveryReportStatus`
- `DeliveryReportRepository`
- `DeliveryReportService`

## Database

Le tabelle vengono create nel database Asset esistente:

```text
%APPDATA%/AccyourateEnterpriseX/accyourate-assets.db
```

Tabelle:

- `DeliveryReports`
- `DeliveryReportItems`

## Funzionalità

- Creazione verbale da assegnazione asset attiva.
- Numero verbale progressivo `VRB-YYYY-0001`.
- Storico verbali per asset.
- Storico verbali per dipendente.
- Integrazione con Audit.
- Integrazione con Notification Center.

## Cosa NON include ancora

- Generazione PDF.
- UI verbali.
- Firma digitale.
- Archiviazione documentale completa.

## Prossimo sprint

`ASSET-004B - Delivery Report PDF Generator`
