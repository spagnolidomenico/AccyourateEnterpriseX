# ADR-0015 - Asset Database Foundation

## Stato

Accettato

## Contesto

La Versione 12 introduce il primo modulo business: Enterprise Asset Management.

## Decisione

Creare un database SQLite dedicato per la fondazione Asset Management:

```text
accyourate-assets.db
```

## Motivazione

- Evita di rischiare regressioni sul database principale esistente.
- Permette di sviluppare il dominio Asset in modo incrementale.
- Facilita eventuale migrazione futura verso database condiviso/cloud.

## Conseguenze

- Il modulo Asset ha una base dati isolata.
- In futuro sarà necessario decidere se unificare i database o mantenere moduli separati.
- Le credenziali non devono essere considerate sicure finché non verrà introdotta cifratura.
