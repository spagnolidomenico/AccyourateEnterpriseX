# Sprint M3.4 — Enterprise Workspace

## Funzioni introdotte

- Sidebar comprimibile dal pulsante menu nell'header.
- Persistenza locale dello stato sidebar in `%LOCALAPPDATA%/AccyourateEnterpriseX/workspace.sidebar`.
- Command Palette richiamabile dall'header o con `Ctrl+K`.
- Navigazione centralizzata dalla Command Palette verso i principali moduli.
- Pulsanti header operativi per tema e notifiche.
- Azioni rapide della dashboard collegate a ricerca, Asset Management e Analytics.
- Aggiornamento della release card a M3.4.

## Verifica

```powershell
dotnet restore
dotnet build
dotnet run --project src/Accyourate.App/Accyourate.App.csproj
```

Test manuali:

1. Premere `Ctrl+K` e aprire un modulo.
2. Usare il pulsante menu e riavviare l'app per verificare la persistenza.
3. Usare il pulsante tema.
4. Aprire Notifiche dall'header.
5. Provare le tre azioni rapide nella dashboard.
