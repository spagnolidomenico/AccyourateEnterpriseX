ACCYOURATE ENTERPRISE X - M7.23 INVENTARIO PER UBICAZIONE

NOVITA
- Nuova sezione Asset > Inventario per ubicazione.
- Selezione o scansione QR AXLOC dell'ubicazione.
- Sessioni separate per ogni posizione.
- Conteggio dei soli ricambi presenti nella posizione.
- Scansione AXPART durante il conteggio.
- Segnalazione dei ricambi trovati in un'altra ubicazione.
- Suggerimento di trasferimento per i ricambi fuori posizione.
- Totale generale invariato finché la sessione non viene chiusa.
- Riconciliazione locale e generale alla chiusura.
- Rettifiche registrate nello storico movimenti.
- Esportazione CSV delle differenze locali.
- Blocco di sessioni aperte duplicate sulla stessa ubicazione.

INSTALLAZIONE
1. Chiudere il gestionale.
2. Estrarre lo ZIP.
3. Copiare AccyourateEnterpriseX dentro C:\Progetti.
4. Confermare la sostituzione.
5. Eseguire:
   cd C:\Progetti\AccyourateEnterpriseX
   dotnet clean
   dotnet build
   dotnet run --project src\Accyourate.App\Accyourate.App.csproj

VERIFICA
1. Aprire Asset > Inventario per ubicazione.
2. Creare una sessione selezionando o scansionando AXLOC:CODICE.
3. Aprire Conta e scansionare AXPART:CODICE.
4. Inserire tutte le quantità e salvare.
5. Verificare che il totale generale non cambi prima della chiusura.
6. Chiudere e riconciliare.
7. Controllare ubicazione, magazzino ricambi e storico movimenti.
8. Provare il CSV.

CONTROLLI ESEGUITI
- dotnet build: 0 errori, 0 avvisi.
- Test riconciliazione locale SQLite: superato.
