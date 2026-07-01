# ADR-0019 - Master Data Database Foundation

## Stato

Accettato

## Contesto

Asset Management necessita di anagrafiche condivise per dipendenti, sedi, reparti e fornitori.

## Decisione

Introdurre un database SQLite dedicato al modulo Enterprise Master Data:

```text
accyourate-master-data.db
```

## Motivazione

- Evita di appesantire il database Asset durante la fase iniziale.
- Permette di sviluppare Master Data in modo incrementale.
- Prepara una futura integrazione con assegnazioni, manutenzioni, fornitori e report.

## Conseguenze

- I database sono separati nella fase desktop iniziale.
- In futuro potremo decidere se unificarli o mantenerli modulari.
