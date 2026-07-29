# M7.7.5 — Configurable QR Destination

## Configurazione

Apri:

`Amministrazione → Branding Center → Template documenti`

Nel riquadro `Destinazione QR Code` inserisci l'indirizzo base HTTPS del
portale o dell'intranet, ad esempio:

`https://portale.azienda.it/accyourate/`

Il gestionale genera automaticamente:

- `https://portale.azienda.it/accyourate/assets/CODICE-ASSET`
- `https://portale.azienda.it/accyourate/delivery-reports/NUMERO-VERBALE`

## Importante

L'indirizzo deve essere raggiungibile dallo smartphone. Questa patch genera
correttamente il collegamento, ma non crea né pubblica il portale web.

Se il campo resta vuoto, il QR continua a contenere il riepilogo testuale.
