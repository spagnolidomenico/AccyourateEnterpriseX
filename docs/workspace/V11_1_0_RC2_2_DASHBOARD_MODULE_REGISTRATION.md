# 11.1.0 RC2.2 - Dashboard Module Registration

## Obiettivo

Registrare la Dashboard come primo `IWorkspaceModule`.

## Cosa cambia

- Aggiunto `DashboardWorkspaceModule`.
- La Dashboard viene registrata nel `WorkspaceModuleRegistry`.
- L'apertura della Dashboard nella Workspace passa dal registry.
- Il comportamento visibile resta invariato.

## Cosa NON cambia

- Digital Twin non viene ancora migrato al registry.
- AI Assistant non viene ancora migrato al registry.
- Action Engine resta invariato.
- Universal Command Bar resta invariata.

## Criteri di accettazione

- Build locale riuscita.
- GitHub Actions verde.
- Dashboard si apre come prima.
- Dashboard non si duplica.
- Digital Twin funziona.
- AI Assistant funziona.
- Action Engine funziona.
- Universal Command Bar funziona.

## Prossimo step

`11.1.0 RC2.3 - Digital Twin Module Registration`.
