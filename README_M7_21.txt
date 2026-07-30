ACCYOURATE ENTERPRISE X - M7.21 INVENTARIO ASSISTITO QR/BARCODE

NOVITA
- Campo di scansione nella sessione inventariale.
- Compatibilità con lettori USB che funzionano come tastiera.
- Supporto dei QR ricambio con payload AXPART:CODICE.
- Ricerca del ricambio con Invio.
- Cursore spostato automaticamente sulla quantità da contare.
- Segnalazione dei codici sconosciuti.
- Avviso quando un ricambio è già stato contato.
- Modalità conteggio continuo.
- Progresso contati/totali sempre visibile.
- Generazione PDF delle etichette QR dal pulsante Etichette QR.

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

VERIFICA SENZA LETTORE
1. Aprire Asset > Inventario fisico.
2. Creare o aprire una sessione.
3. Nel campo di scansione digitare un codice ricambio e premere Invio.
4. Inserire la quantità e premere Invio.
5. Verificare il salvataggio e il ritorno al campo scansione.
6. Ripetere lo stesso codice: deve comparire l'avviso già contato.
7. Inserire un codice inesistente: deve comparire codice non riconosciuto.

ETICHETTE
Premere Etichette QR nella pagina Inventario fisico. Il PDF viene creato in:
Documenti\Accyourate Enterprise X\Etichette Ricambi

CONTROLLI ESEGUITI
- dotnet build: 0 errori, 0 avvisi.
- PDF generato e verificato visivamente.
- QR con payload AXPART:codice.
