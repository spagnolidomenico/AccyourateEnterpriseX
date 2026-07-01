# ADR-0017 - Asset Edit Dialog Foundation

## Stato

Accettato

## Contesto

Asset Management deve evolvere da vista consultiva a modulo operativo.

## Decisione

Introdurre un dialog dedicato `AssetEditDialog` per creazione e modifica asset.

## Motivazione

- Mantiene la schermata principale pulita.
- Permette di validare i campi prima del salvataggio.
- È estendibile in futuro con sezioni avanzate.
- Riduce il rischio di rompere il layout principale.

## Conseguenze

- La UI Asset diventa operativa.
- Il prossimo passo naturale sarà la gestione assegnazioni.
