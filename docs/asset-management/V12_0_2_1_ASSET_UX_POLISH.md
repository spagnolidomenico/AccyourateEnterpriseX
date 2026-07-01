# Sprint 12.0.2.1 - Asset UX Polish

## Obiettivo

Rifinire la prima schermata Asset Management dopo la validazione visiva.

## Problema rilevato

Il pannello dettagli laterale poteva essere tagliato in basso su alcune risoluzioni.

## Correzioni

- Pannello dettagli inserito in `ScrollViewer`.
- Lista asset e pannello dettagli hanno scroll indipendente.
- Toolbar riorganizzata.
- Aggiunto pulsante `Aggiorna` nella toolbar del modulo.
- Migliorata la gestione del contenuto dettagli per layout più adattivo.

## Criteri di accettazione

- Build locale superata.
- GitHub Actions verde.
- Asset Management si apre come tab.
- Pannello dettagli scorre correttamente.
- Lista asset scorre indipendentemente.
- Ricerca e filtri funzionano.
- Nessuna regressione sui moduli principali.
