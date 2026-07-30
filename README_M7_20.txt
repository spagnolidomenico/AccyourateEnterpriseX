ACCYOURATE ENTERPRISE X - M7.20 INVENTARIO FISICO E RICONCILIAZIONE

NOVITA
- Nuova sezione Asset > Inventario fisico.
- Apertura di sessioni inventariali numerate.
- Fotografia iniziale di quantità e costo dei ricambi.
- Inserimento dei conteggi fisici e delle note.
- Calcolo differenza quantitativa ed economica.
- Stati Aperta, In verifica e Chiusa.
- Chiusura consentita solo con tutti i conteggi compilati.
- Rettifica automatica delle giacenze alla chiusura.
- Registrazione delle rettifiche nello storico movimenti.
- Esportazione CSV della sessione.
- Operatore e date tracciati.

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
1. Aprire Asset > Inventario fisico.
2. Premere Nuova sessione e inserire una descrizione.
3. Premere Conta.
4. Inserire una quantità fisica per ogni ricambio.
5. Premere Salva conteggi.
6. Verificare le differenze.
7. Premere Chiudi e rettifica.
8. Controllare le nuove giacenze in Magazzino ricambi.
9. Controllare le Rettifiche nel Registro movimenti.
10. Esportare il CSV dalla sessione.

CONTROLLI ESEGUITI
- dotnet build: 0 errori, 0 avvisi.
- Test ciclo inventariale SQLite: superato.
