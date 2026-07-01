# ADR-0021 - Enterprise UI Framework

## Stato

Accettato

## Contesto

Asset Management e Anagrafica Aziendale hanno pattern UI simili: KPI, toolbar, ricerca, pannelli dettagli, status badge.

## Decisione

Introdurre una libreria interna di controlli riutilizzabili sotto:

```text
Accyourate.App.UIFramework.Controls
```

## Motivazione

- Ridurre duplicazione.
- Uniformare UX.
- Velocizzare lo sviluppo dei nuovi moduli.
- Centralizzare lo stile visuale.
- Preparare il Generic CRUD Engine.

## Conseguenze

I moduli futuri dovranno usare i controlli enterprise condivisi.
La migrazione dei moduli esistenti avverrà gradualmente.
