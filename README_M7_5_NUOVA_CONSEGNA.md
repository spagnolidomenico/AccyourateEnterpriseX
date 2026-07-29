# M7.5 — Nuova Consegna Attrezzature

## Funzioni

- selezione del dipendente e dell'asset disponibile;
- data di consegna obbligatoria e validata;
- note operative;
- salvataggio nel registro SQLite delle consegne;
- prevenzione di due consegne attive per lo stesso asset;
- generazione facoltativa e automatica del verbale PDF;
- riconsegna sincronizzata con il registro consegne;
- database del registro allineato al database reale di Asset Management.

## Applicazione

Copia la cartella `AccyourateEnterpriseX` sopra:

`C:\Progetti\AccyourateEnterpriseX`

Poi esegui:

```powershell
dotnet clean
dotnet restore
dotnet build
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test

1. Apri Asset Management.
2. Seleziona un asset disponibile e premi `Assegna`.
3. Scegli il dipendente, controlla la data e inserisci una nota.
4. Lascia selezionata la generazione del verbale e conferma.
5. Verifica che l'asset risulti assegnato e che il PDF venga creato.
6. Prova una riconsegna e verifica che l'asset torni disponibile.
