ACCYOURATE ENTERPRISE X - M7.19 RICHIESTE DI APPROVVIGIONAMENTO

NOVITA
- Nuova sezione Asset > Approvvigionamento.
- Generazione automatica delle richieste dai ricambi sotto scorta.
- Quantità suggerita calcolata dalla soglia minima.
- Stati: Bozza, Approvata, Ordinata, Completata e Annullata.
- Selezione del fornitore e modifica della quantità richiesta.
- Trasformazione della richiesta approvata in ordine d'acquisto.
- Collegamento tra richiesta e ordine.
- Completamento automatico alla ricezione dell'ordine.
- Blocco delle richieste duplicate ancora aperte.
- Ricerca, filtri e KPI operativi.

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
1. Assicurarsi che esista almeno un ricambio sotto scorta.
2. Aprire Asset > Approvvigionamento.
3. Premere Genera da sotto scorta.
4. Premere Modifica e scegliere fornitore e quantità.
5. Premere Approva.
6. Premere Crea ordine.
7. Aprire Asset > Acquisti e fornitori e verificare l'ordine in bozza.
8. Portare l'ordine a Confermato e poi Ricevi.
9. Tornare in Approvvigionamento: la richiesta deve risultare Completata.

CONTROLLI ESEGUITI
- dotnet build: 0 errori, 0 avvisi.
- Test ciclo completo SQLite: superato.
- Verificati duplicati, approvazione, ordine e completamento.
