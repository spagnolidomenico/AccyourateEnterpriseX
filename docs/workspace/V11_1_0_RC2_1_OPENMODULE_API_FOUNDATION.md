# 11.1.0 RC2.1 - OpenModule API Foundation

## Obiettivo

Introdurre le fondamenta dell'API unificata di apertura moduli senza modificare il comportamento visibile della Workspace.

## Componenti introdotti

- `IWorkspaceModule`
- `WorkspaceModule`
- `WorkspaceModuleDescriptor`
- `WorkspaceModuleRegistry`

## Cosa cambia

A livello tecnico viene introdotto un contratto comune per descrivere i moduli della Workspace.

Ogni modulo potrà dichiarare:

- Id
- Titolo
- Icona
- Chiudibilità
- Stato pinned
- Factory della view

## Cosa NON cambia

- La Workspace continua ad aprire i moduli come prima.
- Non viene ancora rimossa la logica speciale esistente.
- Nessun modulo viene ancora migrato al registry.
- Nessun cambiamento UI visibile.

## Criteri di accettazione

- Build locale riuscita.
- GitHub Actions verde.
- App avviata.
- Dashboard funzionante.
- Digital Twin funzionante.
- AI Assistant funzionante.
- Action Engine funzionante.
- Universal Command Bar funzionante.

## Prossimo step

`11.1.0 RC2.2 - Dashboard Module Registration`

In quella fase registreremo Dashboard come primo `IWorkspaceModule`.
