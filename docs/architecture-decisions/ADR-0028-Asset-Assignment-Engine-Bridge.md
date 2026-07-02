# ADR-0028 - Asset Assignment Engine Bridge

## Stato

Accettato

## Contesto

Asset Management e Master Data usano database separati. `AssetAssignments.EmployeeId` richiede una FK verso la tabella `Employees` nel database Asset.

## Decisione

Introdurre un bridge controllato: Master Data resta la fonte ufficiale, ma i dipendenti vengono sincronizzati come mirror tecnico nel database Asset.

## Motivazione

- Mantiene valide le foreign key.
- Evita migrazione distruttiva.
- Non rompe i dati esistenti.
- Permette l'assegnazione asset in modo sicuro.
