# HR-002B - Database & Repository

## Obiettivo

Aggiungere persistenza SQLite e repository al modulo Human Resources.

## Componenti introdotti

- `HrDatabase`
- `EmployeeRepository`
- `SiteRepository`
- `DepartmentRepository`
- `RoleRepository`
- `EmploymentContractRepository`
- `EmployeeDocumentRepository`
- `EmployeeService`
- `HrLookupService`

## Database

Il database HR viene creato in:

```text
%APPDATA%/AccyourateEnterpriseX/accyourate-hr.db
```

## Tabelle

- `Sites`
- `Departments`
- `Roles`
- `Employees`
- `EmploymentContracts`
- `EmployeeDocuments`

## Integrazioni

`EmployeeService` integra:

- `AuditService`
- `NotificationService`

## Cosa NON cambia

- Nessuna UI HR ancora.
- Nessuna modifica Asset Management.
- Nessuna migrazione dei dati Master Data esistenti.

## Prossimo sprint

`HR-002C - HR Services & Validation`
