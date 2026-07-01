# Asset Management - Database Schema Draft

## Tabelle previste

```text
Assets
Employees
AssetAssignments
MaintenanceTickets
AssetDocuments
AssetCredentials
```

## Assets

| Campo | Tipo | Note |
|---|---|---|
| Id | INTEGER | Primary Key |
| AssetCode | TEXT | Codice interno es. PC-001 |
| Category | TEXT | Categoria |
| Manufacturer | TEXT | Produttore |
| Model | TEXT | Modello |
| SerialNumber | TEXT | Seriale |
| AssetTag | TEXT | Etichetta inventario |
| Status | TEXT | Stato |
| PurchaseDate | TEXT | ISO date |
| WarrantyEndDate | TEXT | ISO date |
| OperatingSystem | TEXT | Windows/macOS/Linux/Altro |
| BitLockerEnabled | INTEGER | 0/1 |
| Notes | TEXT | Note |
| CreatedAt | TEXT | ISO datetime |
| UpdatedAt | TEXT | ISO datetime |

## Employees

| Campo | Tipo |
|---|---|
| Id | INTEGER |
| FullName | TEXT |
| Email | TEXT |
| Department | TEXT |
| Role | TEXT |
| Site | TEXT |
| IsActive | INTEGER |

## AssetAssignments

| Campo | Tipo |
|---|---|
| Id | INTEGER |
| AssetId | INTEGER |
| EmployeeId | INTEGER |
| AssignedAt | TEXT |
| ReturnedAt | TEXT |
| AssignedBy | TEXT |
| Notes | TEXT |
| Status | TEXT |

## MaintenanceTickets

| Campo | Tipo |
|---|---|
| Id | INTEGER |
| AssetId | INTEGER |
| Title | TEXT |
| Description | TEXT |
| Priority | TEXT |
| Status | TEXT |
| OpenedAt | TEXT |
| ClosedAt | TEXT |
| Technician | TEXT |
| ResolutionNotes | TEXT |

## AssetDocuments

| Campo | Tipo |
|---|---|
| Id | INTEGER |
| AssetId | INTEGER |
| DocumentType | TEXT |
| FileName | TEXT |
| FilePath | TEXT |
| UploadedAt | TEXT |
| Notes | TEXT |

## AssetCredentials

| Campo | Tipo |
|---|---|
| Id | INTEGER |
| AssetId | INTEGER |
| CredentialType | TEXT |
| Username | TEXT |
| SecretReference | TEXT |
| Notes | TEXT |
| UpdatedAt | TEXT |
```

## Nota sicurezza

`AssetCredentials` dovrà evolvere verso:

- cifratura locale;
- protezione tramite master key;
- controllo permessi;
- audit accessi;
- eventuale integrazione con vault esterno.
