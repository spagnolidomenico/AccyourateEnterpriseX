# M7.4 — Enterprise Tables Alignment

La patch uniforma le tabelle di Dipendenti, Asset IT e Dispositivi medici.

## Modifiche

- larghezze coerenti tra intestazioni e righe;
- colonne azione dimensionate anche per `Ripristina`;
- pulsanti con altezza, padding e allineamento uniformi;
- intestazioni delle azioni e stati centrati;
- testo delle celle troncato in modo sicuro quando necessario;
- regole condivise in `AxTableLayout`;
- nessun uso di `ColumnSpacing` o API Avalonia non supportate.

## Applicazione e test

Copia il contenuto della patch sopra `C:\Progetti\AccyourateEnterpriseX`,
conferma la sostituzione e poi esegui:

```powershell
dotnet clean
dotnet restore
dotnet build
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

Apri Dipendenti, Asset IT e Dispositivi medici. Controlla righe attive e
archiviate: tutti i comandi devono essere centrati, leggibili per intero e
allineati con le rispettive intestazioni.
