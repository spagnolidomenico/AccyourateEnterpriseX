# 14.0.4 - Repository Adoption Foundation

## Obiettivo

Preparare l'adozione reale dei repository, allineandoli ai database esistenti e ai nomi reali usati dall'applicazione.

## Database reali

Il preflight ha confermato:

```text
%APPDATA%/AccyourateEnterpriseX/accyourate-assets.db
%APPDATA%/AccyourateEnterpriseX/accyourate-master-data.db
```

## Componenti introdotti

- `AccyourateDatabaseNames`
- `AccyourateDatabaseContextFactory`
- `MasterDataEmployeeRepository`
- `AssetDatabaseAssetRepository`
- `EmployeeMapper`

## Cosa cambia

Viene resa esplicita la separazione tra:

- database Asset;
- database Master Data.

I repository ora possono essere costruiti puntando ai database corretti.

## Cosa NON cambia

- Nessuna UI modificata.
- Nessun servizio legacy sostituito.
- Nessuna migrazione database.
- Nessuna sincronizzazione ancora attiva.
- Nessuna assegnazione asset riattivata.

## Prossimo sprint

`14.0.5 - Master Data Repository Adoption`

Inizieremo a far usare il repository a Master Data in modo controllato.
