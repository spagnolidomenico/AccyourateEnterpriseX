# ADR-0023 - Core Platform Refactoring

## Stato

Accettato

## Contesto

Accyourate Enterprise X ha raggiunto una fase in cui Asset Management e Master Data devono collaborare.

Durante lo sviluppo della relazione Dipendente ↔ Asset è emersa una duplicazione del modello Employee:

- una tabella Employees nel dominio Asset;
- una tabella Employees nel dominio Master Data.

Questa duplicazione ha generato errori di vincolo SQLite.

## Decisione

Aprire la Versione 14 come fase di Core Platform Refactoring.

## Motivazione

- Unificare il modello dati.
- Evitare duplicazioni.
- Preparare repository e dominio condiviso.
- Rendere più sicuro lo sviluppo futuro.
- Abilitare relazioni enterprise reali tra moduli.

## Conseguenze

Prima di aggiungere nuove relazioni tra moduli, il progetto dovrà introdurre una base architetturale comune.
