# M7.9 — Asset Maintenance Lifecycle

## Funzioni

- apertura intervento dal dettaglio asset;
- titolo, descrizione, priorità, tecnico e data prevista;
- stato automatico `In manutenzione`;
- completamento con risoluzione e costo;
- ripristino automatico a `Disponibile` o `Assegnato`;
- storico persistente SQLite con migrazione non distruttiva;
- timeline dell'asset aggiornata;
- notifiche di apertura e completamento;
- verbale PDF con branding, QR e firme;
- apertura del PDF dal pannello manutenzioni e dalla timeline.

## Verifica

1. Apri Asset Management e il dettaglio di un asset.
2. In `Manutenzioni` premi `Nuovo intervento`.
3. Compila i dati e salva: lo stato diventa `In manutenzione`.
4. Riapri il dettaglio e premi `Completa`.
5. Inserisci risoluzione e costo, lasciando attivo il PDF.
6. Controlla PDF, stato dell'asset, timeline e Centro Notifiche.
