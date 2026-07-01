# Sprint 13.0.1 - Enterprise UI Framework Foundation

## Obiettivo

Introdurre una prima libreria interna di componenti UI riutilizzabili per ridurre duplicazione e rendere coerente l'interfaccia dei moduli.

## Componenti aggiunti

- `EnterpriseKpiCard`
- `EnterpriseStatusBadge`
- `EnterpriseSearchBar`
- `EnterpriseToolbar`
- `EnterpriseSectionHeader`
- `EnterpriseDetailsPanel`
- `EnterpriseCard`
- `EnterpriseDialogBase`

## Cosa cambia

In questa prima fase vengono aggiunti i componenti base, ma i moduli esistenti non vengono ancora rifattorizzati in modo massivo.

## Cosa NON cambia

- Asset Management resta funzionante come prima.
- Anagrafica Aziendale resta funzionante come prima.
- Nessuna modifica al database.
- Nessuna nuova funzionalità utente finale.

## Criteri di accettazione

- Build locale superata.
- GitHub Actions verde.
- Workspace funzionante.
- Asset Management funzionante.
- Anagrafica Aziendale funzionante.
- Nessuna regressione sui moduli principali.

## Prossimo step

`13.0.2 - UI Framework Adoption`

Migrare gradualmente Asset Management e Master Data ai nuovi componenti condivisi.
