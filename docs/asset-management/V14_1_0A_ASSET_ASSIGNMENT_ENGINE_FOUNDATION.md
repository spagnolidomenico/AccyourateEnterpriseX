# 14.1.0A - Asset Assignment Engine Foundation

## Obiettivo

Introdurre il motore backend di assegnazione Asset ↔ Dipendente senza modificare ancora la UI.

## Problema risolto

Il database Asset contiene una tabella `Employees` usata come foreign key da `AssetAssignments`.
Il database Master Data contiene l'anagrafica ufficiale dei dipendenti.

Per evitare errori di foreign key, il motore crea un mirror controllato dei dipendenti Master Data nel database Asset.

## Componenti introdotti

- `AssetAssignmentEngine`
- `AssignableEmployee`
- `AssignableAsset`
- `AssetAssignmentSummary`

## Funzionalità backend

- lettura dipendenti da Master Data;
- sincronizzazione dipendenti verso tabella Asset `Employees`;
- assegnazione asset;
- restituzione assegnazione;
- lettura assegnazioni attive per dipendente;
- lettura assegnazione attiva per asset;
- aggiornamento stato asset.

## Cosa NON cambia

- Nessuna UI modificata.
- Nessun pulsante nuovo.
- Nessun comportamento visibile ancora.
- Nessun servizio legacy sostituito.
