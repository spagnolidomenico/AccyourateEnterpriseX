ACCYOURATE ENTERPRISE X - M7.18 MOVIMENTI E VALORIZZAZIONE MAGAZZINO

NOVITA
- Carico e scarico manuale per ogni ricambio.
- Causali: Acquisto, Consumo, Reso, Trasferimento e Rettifica.
- Blocco automatico degli scarichi superiori alla giacenza disponibile.
- Costo medio ponderato aggiornato durante i carichi.
- Registrazione del saldo precedente e successivo per ogni movimento.
- Registro movimenti globale con ricerca e filtri per tipo e periodo.
- Riepilogo delle quantità caricate e scaricate.
- Esportazione CSV dell'inventario valorizzato.

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
1. Aprire Asset > Magazzino ricambi.
2. Premere Carico / Scarico su un ricambio.
3. Registrare un carico e controllare l'aumento della giacenza.
4. Registrare uno scarico e controllare la diminuzione.
5. Provare uno scarico superiore alla disponibilità: deve essere bloccato.
6. Aprire Registro movimenti e provare ricerca, tipo e date.
7. Premere Esporta CSV e controllare il percorso mostrato nell'app.

CONTROLLI ESEGUITI
- dotnet build: 0 errori, 0 avvisi.
- Test SQLite: superato.
- Verificati carico, scarico, costo medio, saldi e blocco giacenza.
