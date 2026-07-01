# Sprint 13.1.0a - Master Data Employees CRUD

## Obiettivo

Rendere operativa la sezione Dipendenti dell'Anagrafica Aziendale.

## Funzionalità introdotte

- Nuovo Dipendente.
- Modifica Dipendente.
- Elimina Dipendente.
- Validazione del campo Nome completo.
- Salvataggio su SQLite tramite `MasterDataService`.
- Aggiornamento lista, KPI e pannello dettagli dopo le operazioni.

## Limite intenzionale

In questa micro-patch il CRUD è attivo solo nella sezione `Dipendenti`.

Le sezioni Aziende, Sedi, Reparti e Fornitori restano consultive.

## Criteri di accettazione

- Build locale superata.
- GitHub Actions verde.
- Anagrafica Aziendale si apre.
- Sezione Dipendenti funzionante.
- Nuovo Dipendente funzionante.
- Modifica Dipendente funzionante.
- Elimina Dipendente funzionante.
- Asset Management non regredisce.
