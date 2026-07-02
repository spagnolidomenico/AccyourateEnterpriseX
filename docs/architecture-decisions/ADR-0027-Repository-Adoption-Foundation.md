# ADR-0027 - Repository Adoption Foundation

## Stato

Accettato

## Contesto

Il sistema usa due database reali:

- `accyourate-assets.db`
- `accyourate-master-data.db`

La prima versione del repository foundation non distingueva esplicitamente i due database.

## Decisione

Introdurre una factory di contesti e repository esplicitamente allineati ai database reali.

## Motivazione

- Evitare ambiguità sui database.
- Preparare l'adozione graduale dei repository.
- Evitare sincronizzazioni temporanee premature.
- Conservare stabilità dell'app.

## Conseguenze

La UI e i servizi legacy restano invariati.
I prossimi sprint potranno adottare i repository un servizio alla volta.
