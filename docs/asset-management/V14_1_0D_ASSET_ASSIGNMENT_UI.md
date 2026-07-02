# 14.1.0D - Asset Assignment UI

## Obiettivo

Collegare il motore `AssetAssignmentEngine` alla UI di Asset Management.

## Funzionalità

- Pulsante globale `Assegna`.
- Pulsante `Assegna` nella scheda asset.
- Pulsante `Restituisci` nella scheda asset.
- Dialog di assegnazione con:
  - dipendente Master Data;
  - asset disponibile;
  - note.
- Campo `Assegnato a` nella scheda asset.
- Aggiornamento stato asset a:
  - `Assegnato`;
  - `Disponibile`.

## Cosa NON cambia

- Nessuna modifica login.
- Nessuna modifica Branding.
- Nessuna migrazione distruttiva.
- Nessuna eliminazione dati.

## Test

- Creare/selezionare dipendente in Anagrafica Aziendale.
- Aprire Asset Management.
- Assegnare un asset disponibile.
- Verificare `Assegnato a`.
- Restituire l'asset.
