# Sprint 12.0.3.1 - Asset Dialog Hotfix

## Problema

Cliccando `+ Nuovo`, l'app poteva chiudersi per un'eccezione non gestita nel dialog di creazione asset.

## Causa

Nel dialog `AssetEditDialog`, il campo `Note` veniva creato due volte e lo stesso controllo `_notes` veniva associato a due container diversi.

## Correzione

- Rimossa la creazione duplicata del campo `Note`.
- Aggiunta gestione difensiva degli errori nell'apertura di `Nuovo Asset` e `Modifica Asset`.

## Test

- `+ Nuovo` deve aprire il dialog.
- `Annulla` deve chiudere il dialog senza chiudere l'app.
- `Salva` deve creare l'asset.
- `Modifica` deve aprire il dialog.
