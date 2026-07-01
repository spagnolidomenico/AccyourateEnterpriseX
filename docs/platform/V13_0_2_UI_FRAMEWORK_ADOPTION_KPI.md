# Sprint 13.0.2 - UI Framework Adoption KPI

## Obiettivo

Iniziare l'adozione reale dell'Enterprise UI Framework migrando un solo componente alla volta.

## Modifica effettuata

Asset Management ora usa `EnterpriseKpiCard` per le KPI superiori.

## Cosa cambia

- Le KPI Asset usano il componente condiviso.
- Rimossa duplicazione del metodo locale `Kpi()` in `AssetManagementView`.

## Cosa NON cambia

- Nessuna modifica al database.
- Nessuna modifica alla logica CRUD.
- Toolbar, ricerca e dettagli restano invariati.
- Master Data non viene ancora modificato.

## Criteri di accettazione

- Build locale superata.
- GitHub Actions verde.
- Asset Management si apre.
- KPI Asset visibili.
- Nuovo/Modifica/Elimina Asset funzionanti.
- Nessuna regressione sui moduli principali.
