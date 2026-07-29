# M7.7.4 — QR Mobile Compatibility Fix

## Correzione

Il QR non usa più lo schema applicativo interno `ACCYOURATE:`, che alcune
fotocamere interpretavano come collegamento a un'app non installata.

Il contenuto è ora testo standard leggibile:

- tipo di documento;
- numero verbale o codice asset;
- dipendente;
- asset e beni;
- data;
- seriale e modello nelle schede asset.

## Verifica

Rigenera il PDF dopo aver applicato la patch. Inquadra il QR con la fotocamera
o con un lettore QR: deve essere mostrato il riepilogo testuale del documento.
