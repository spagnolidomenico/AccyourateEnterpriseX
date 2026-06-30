# Contratto modulo

Ogni modulo di Accyourate Enterprise X deve definire:

- codice modulo;
- nome visualizzato;
- permessi richiesti;
- tabelle database;
- workflow supportati;
- eventi audit;
- schermate;
- export;
- report;
- test manuali.

## Esempio

```text
Modulo: Medical.Quality
Permesso: medical.view
Tabelle: quality_tests
Workflow: QUALITY_TEST_COMPLETED
Audit: QUALITY_TEST_CREATED
```
