# Sprint 12.0.6 - Enterprise Master Data Workspace

## Obiettivo

Rendere visibile il modulo Anagrafica Aziendale nella Workspace.

## Cosa cambia

- Aggiunto `MasterDataView`.
- Aggiunto `MasterDataWorkspaceModule`.
- Registrazione nel `WorkspaceModuleRegistry`.
- Nuova voce sidebar: `Anagrafica Aziendale`.

## Funzionalità UI

- KPI:
  - aziende;
  - sedi;
  - reparti;
  - dipendenti;
  - fornitori.
- Navigazione tra sezioni:
  - Aziende;
  - Sedi;
  - Reparti;
  - Dipendenti;
  - Fornitori.
- Lista centrale.
- Ricerca.
- Pannello dettagli laterale.
- Pulsanti placeholder:
  - Nuovo;
  - Modifica;
  - Elimina.

## Cosa NON cambia

- Creazione/modifica/eliminazione record non ancora implementata.
- Asset Management non è ancora collegato a Master Data.
- Assegnazioni asset saranno introdotte dopo la stabilizzazione del modulo Master Data.

## Criteri di accettazione

- Build locale superata.
- GitHub Actions verde.
- Anagrafica Aziendale visibile nella sidebar.
- Apertura come tab Workspace.
- KPI visibili.
- Navigazione tra sezioni funzionante.
- Ricerca funzionante.
- Pannello dettagli funzionante.
