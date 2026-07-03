# HR-002F - Employee Asset Integration

## Obiettivo

Collegare il profilo dipendente HR alle assegnazioni asset esistenti.

## Funzionalità

- Nuovo `EmployeeAssetService`.
- Mapping HR -> Anagrafica Aziendale tramite:
  - email;
  - nome completo.
- Lettura asset attivi tramite `AssetAssignmentEngine`.
- Sezione `Asset assegnati` nel profilo dipendente con:
  - stato collegamento;
  - asset assegnati;
  - data assegnazione;
  - messaggio se non esistono asset.

## Cosa NON cambia

- Nessuna modifica database.
- Nessuna nuova tabella.
- Nessuna modifica all'assegnazione asset.
- Nessuna modifica al dialog Assegna Asset.

## Nota tecnica

L'Asset Assignment Engine usa i dipendenti di Anagrafica Aziendale come fonte.  
Per mostrare asset nella scheda HR, il dipendente HR deve avere email o nome completo corrispondente a un dipendente di Anagrafica Aziendale.

## Prossimo sprint

`ASSET-004 - Delivery Report / Verbale consegna`
