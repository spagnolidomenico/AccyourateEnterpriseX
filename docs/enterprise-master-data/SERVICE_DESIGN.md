# Enterprise Master Data - Service Design

## Servizi previsti

### MasterDataService

Responsabilità:

- gestione azienda;
- sedi;
- reparti;
- dipendenti;
- fornitori.

Metodi ipotetici:

```csharp
GetCompanies()
GetSites()
GetDepartments()
GetEmployees()
GetSuppliers()

CreateEmployee()
UpdateEmployee()
DeleteEmployee()

CreateSupplier()
UpdateSupplier()
DeleteSupplier()
```

## Integrazione futura

Asset Management userà MasterDataService per:

- selezionare dipendente assegnatario;
- selezionare sede;
- selezionare reparto;
- selezionare fornitore;
- generare report per sede/reparto.
