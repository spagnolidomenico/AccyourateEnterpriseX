# M6.1 — Component Library Foundation

Questa patch consolida la libreria proprietaria di componenti di Accyourate Enterprise X.

## Incluso

- nuovo `AxCommandButton` per toolbar e command bar;
- nuovo `AxComponentCatalog` come registro ufficiale dei componenti;
- migrazione della toolbar Asset al componente canonico;
- migrazione dei badge di stato Asset ad `AxStatusBadge`;
- guida ufficiale `docs/design-system/AX_COMPONENT_LIBRARY.md`;
- nessuno script PowerShell richiesto per il collaudo.

## Verifica

1. chiudere l'applicazione;
2. eseguire `dotnet clean`;
3. eseguire `dotnet build`;
4. avviare l'app e aprire Asset Management;
5. verificare toolbar, selettori Lista/Card e badge degli stati.
