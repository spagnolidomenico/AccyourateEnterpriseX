ACCYOURATE ENTERPRISE X - M7.24 MOVIMENTI CON UBICAZIONE

NOVITA
- Scelta dell'ubicazione durante la ricezione di un ordine.
- Carico automatico della quantità nella posizione selezionata.
- Sincronizzazione tra giacenza complessiva e quantità locali.
- Pulsante Verifica coerenza in Ubicazioni magazzino.
- Rilevazione delle differenze positive e negative.
- Procedura guidata di riallineamento.
- Ripartizione sicura delle riduzioni tra le ubicazioni disponibili.
- Gestione corretta dei nuovi ricambi ricevuti per la prima volta.

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
1. Aprire Acquisti e fornitori.
2. Ricevere un ordine confermato.
3. Selezionare l'ubicazione di destinazione.
4. Aprire Ubicazioni magazzino e verificare la quantità.
5. Premere Verifica coerenza.
6. Se tutto è allineato deve comparire il messaggio positivo.
7. Se vengono trovate differenze, scegliere la posizione di riallineamento.

CONTROLLI ESEGUITI
- dotnet build: 0 errori, 0 avvisi.
- Test SQLite coerenza positivo/negativo: superato.
