# ADR-0016 - Asset Management come Workspace Module

## Stato

Accettato

## Contesto

La Workspace 2.0 è stata stabilizzata e può ospitare moduli business reali.

## Decisione

Asset Management viene introdotto come primo modulo business registrato nel `WorkspaceModuleRegistry`.

## Motivazione

- Valida la piattaforma Workspace con un modulo enterprise reale.
- Usa il nuovo livello dati introdotto in 12.0.1.
- Fornisce valore concreto all'utente.
- Prepara future integrazioni con AI, Digital Twin e import/export Excel.

## Conseguenze

- Asset Management diventa il primo modulo della Versione 12.
- I futuri moduli business dovranno seguire lo stesso pattern.
