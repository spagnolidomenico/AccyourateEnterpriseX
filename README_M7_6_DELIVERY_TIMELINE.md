# M7.6 — Storico Consegne e Timeline Asset

## Funzioni

- cronologia delle consegne nel pannello dettaglio dell'asset;
- dipendente associato a ogni consegna;
- data di consegna e riconsegna;
- stato tradotto e indicato con colore semantico;
- note operative visibili nella timeline;
- movimenti ordinati dal più recente;
- messaggio chiaro quando non esiste ancora uno storico.

## Applicazione

Copia la cartella `AccyourateEnterpriseX` sopra il progetto esistente, quindi:

```powershell
dotnet clean
dotnet build
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test

1. Apri Asset Management.
2. Apri il dettaglio di un asset già assegnato o riconsegnato.
3. Scorri fino alla sezione `Timeline`.
4. Verifica dipendente, date, stato e note.
5. Esegui una nuova consegna o riconsegna e riapri il dettaglio.
