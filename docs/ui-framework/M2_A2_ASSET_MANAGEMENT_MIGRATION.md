# M2-A2 - Asset Management Migration

## Obiettivo

Migrare la tabella del modulo Asset Management dal layout manuale al componente riutilizzabile `AxEnterpriseTable<Asset>`.

## Modifiche

- eliminata la costruzione manuale di intestazione e righe;
- definite sei colonne tramite `AxEnterpriseColumn<Asset>`;
- mantenuti filtri, ricerca e pannello dettagli;
- selezione riga sincronizzata con il pannello dettagli;
- doppio clic sulla riga collegato alla modifica dell’asset;
- scrolling orizzontale delegato al componente enterprise;
- scrolling verticale mantenuto nel contenitore del modulo;
- selezione visibile preservata durante aggiornamenti e filtri.

## Definition of Done

- [x] Asset Management utilizza `AxEnterpriseTable<Asset>`.
- [x] Il vecchio metodo `Row(Asset)` è stato rimosso.
- [x] Header e celle condividono le stesse definizioni di colonna.
- [x] La selezione aggiorna i dettagli.
- [x] Il doppio clic apre la modifica.
- [ ] Build verificata su Windows con .NET 9.
- [ ] Controllo visivo a 100%, 125% e 150%.
