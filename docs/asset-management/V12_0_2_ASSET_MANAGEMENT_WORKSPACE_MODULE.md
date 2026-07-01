# Sprint 12.0.2 - Asset Management Workspace Module

## Obiettivo

Introdurre la prima schermata del modulo Enterprise Asset Management nella Workspace.

## Cosa cambia

- Aggiunto `AssetManagementView`.
- Aggiunto `AssetManagementWorkspaceModule`.
- Asset Management registrato nel `WorkspaceModuleRegistry`.
- Nuova voce sidebar: `Asset Management`.
- Apertura come tab interna Workspace.

## Funzionalità UI

- KPI superiori:
  - asset totali;
  - assegnati;
  - in manutenzione;
  - garanzie in scadenza entro 90 giorni.
- Ricerca testuale.
- Filtro categoria.
- Filtro stato.
- Lista asset.
- Pannello laterale dettagli asset selezionato.
- Pulsanti placeholder:
  - Nuovo Asset;
  - Importa Excel;
  - Esporta Excel.

## Cosa NON cambia

- Non è ancora possibile creare/modificare asset da UI.
- Import/export Excel non è ancora implementato.
- Assegnazioni e manutenzioni saranno sprint successivi.

## Criteri di accettazione

- Build locale superata.
- GitHub Actions verde.
- Workspace aperta.
- Asset Management visibile nella sidebar.
- Asset Management aperto come tab.
- Asset demo visibili.
- Ricerca e filtri funzionanti.
- Nessuna regressione sui moduli principali.
