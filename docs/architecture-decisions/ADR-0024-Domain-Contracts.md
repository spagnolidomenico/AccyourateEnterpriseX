# ADR-0024 - Domain Contracts

## Stato

Accettato

## Contesto

La Versione 14 introduce una nuova Core Platform. Prima di spostare logica e dati è necessario definire contratti stabili tra dominio, infrastruttura e UI.

## Decisione

Introdurre contratti repository e interfacce di base in `Accyourate.Core`.

## Motivazione

- Separare dominio e accesso diretto a SQLite.
- Preparare repository reali.
- Rendere possibili test automatici.
- Evitare nuove dipendenze dirette tra moduli.
