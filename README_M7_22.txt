ACCYOURATE ENTERPRISE X - M7.22 UBICAZIONI E TRASFERIMENTI

NOVITA
- Nuova sezione Asset > Ubicazioni magazzino.
- Anagrafica ubicazioni con codice, magazzino, corridoio e scaffale.
- Distribuzione della giacenza dello stesso ricambio su più posizioni.
- Migrazione iniziale dalle ubicazioni già presenti nei ricambi.
- Trasferimenti senza variazione della giacenza totale.
- Controllo disponibilità nell'ubicazione di origine.
- Storico trasferimenti con operatore, data e riferimento.
- Scansione AXPART:codice e AXLOC:codice nel trasferimento.
- PDF con etichette QR delle ubicazioni.
- Ricerca per posizione, magazzino e ricambio.

INSTALLAZIONE
1. Chiudere completamente il gestionale.
2. Estrarre lo ZIP.
3. Copiare AccyourateEnterpriseX dentro C:\Progetti.
4. Confermare la sostituzione.
5. Eseguire:

   cd C:\Progetti\AccyourateEnterpriseX
   dotnet clean
   dotnet build
   dotnet run --project src\Accyourate.App\Accyourate.App.csproj

VERIFICA
1. Aprire Asset > Ubicazioni magazzino.
2. Creare una seconda ubicazione.
3. Premere Trasferisci.
4. Selezionare ricambio, origine, destinazione e quantità.
5. Confermare e verificare che il totale non cambi.
6. Aprire Storico trasferimenti.
7. Premere Etichette QR e controllare il PDF.
8. Provare nei campi scansione:
   AXPART:CODICE-RICAMBIO
   AXLOC:CODICE-UBICAZIONE

CONTROLLI ESEGUITI
- dotnet build: 0 errori, 0 avvisi.
- Test trasferimento SQLite: superato.
- Totale giacenza invariato e limite disponibilità verificato.
- PDF ubicazioni verificato visivamente.
