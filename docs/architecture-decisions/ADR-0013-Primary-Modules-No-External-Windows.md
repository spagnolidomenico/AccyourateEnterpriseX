# ADR-0013 - Moduli principali senza finestre esterne

## Stato

Accettato

## Contesto

Durante la migrazione alla Workspace 2.0 alcuni pulsanti aprivano ancora finestre esterne.

## Decisione

I moduli principali devono aprirsi nella Workspace come tab.

Sono considerati moduli principali:

- Dashboard
- Digital Twin
- AI Assistant
- Action Engine
- Universal Command Bar

## Eccezioni

Restano finestre esterne solo:

- login;
- dialoghi modali;
- selezione file;
- stampa;
- strumenti temporanei non ancora migrati.

## Conseguenze

La Workspace diventa il centro dell'esperienza utente.
