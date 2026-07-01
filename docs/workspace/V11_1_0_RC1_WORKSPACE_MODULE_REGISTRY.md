# Versione 11.1.0 RC1 - Workspace Module Registry

## Obiettivo

Introdurre un registro centralizzato dei moduli Workspace, riducendo la logica speciale dentro `EnterpriseWorkspaceWindow`.

## Componenti introdotti

- `IWorkspaceModule`
- `DelegateWorkspaceModule`
- `WorkspaceModuleRegistry`

## Moduli registrati in RC1

- Dashboard
- Digital Twin
- AI Assistant

## Cosa cambia

- Dashboard, Digital Twin e AI Assistant vengono aperti passando dal registro moduli.
- Il pulsante AI nella top bar apre l'AI Assistant nella Workspace invece che come finestra separata.
- La voce AI Assistant nella sidebar apre una scheda interna.
- La status bar mostra anche il numero di moduli registrati.

## Cosa NON cambia

- Action Engine resta finestra separata.
- Universal Command Bar resta finestra separata.
- AI Intent Catalog resta finestra separata.
- Control Room resta invariata.

## Criteri di accettazione

- Build riuscita.
- Workspace avviata.
- Dashboard funziona come tab.
- Digital Twin funziona come tab.
- AI Assistant funziona come tab.
- Click ripetuti non duplicano le tab.
- Nessuna regressione su Command Bar e Action Engine.

## Prossimo step

11.1.0 RC2 - API unica `OpenModule()`.
