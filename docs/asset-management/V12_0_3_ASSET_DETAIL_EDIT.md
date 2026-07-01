# Sprint 12.0.3 - Asset Detail & Edit Foundation

## Obiettivo

Rendere il modulo Asset Management operativo per la gestione base degli asset.

## Funzionalità introdotte

- Dialog `AssetEditDialog`.
- Creazione nuovo asset.
- Modifica asset esistente.
- Eliminazione asset.
- Validazione campi obbligatori.
- Controllo duplicati su `AssetCode`.
- Aggiornamento lista dopo salvataggio.
- Messaggi di conferma/errore.
- Doppio click sulla riga per modificare l'asset.

## Campi gestiti

- Codice Asset
- Categoria
- Produttore
- Modello
- Numero di serie
- Asset Tag
- Stato
- Sistema operativo
- Data acquisto
- Fine garanzia
- BitLocker
- Note

## Cosa NON cambia

- Nessuna gestione assegnazioni.
- Nessun import/export Excel.
- Nessuna gestione documenti.
- Nessuna timeline asset.

## Criteri di accettazione

- Build locale superata.
- GitHub Actions verde.
- Creazione asset funzionante.
- Modifica asset funzionante.
- Eliminazione asset funzionante.
- Codice asset duplicato bloccato.
- Ricerca e filtri continuano a funzionare.
