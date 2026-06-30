# ADR-0003 - Enterprise Search Service

## Stato

Accettato

## Contesto

La Universal Command Bar deve cercare in più moduli senza dipendere direttamente da ognuno di essi.

## Decisione

Introduciamo un Enterprise Search Service con provider registrabili.

Ogni modulo può implementare `ISearchProvider` e restituire risultati standardizzati tramite `SearchResult`.

## Conseguenze

- La Command Bar parla con un solo servizio.
- Nuovi moduli possono aggiungere ricerca senza modificare la Command Bar.
- I risultati possono essere trasformati in ActionRequest ed eseguiti tramite Action Engine.
