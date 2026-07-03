# HR-002C.1 - Validation Framework

## Obiettivo

Introdurre un framework di validazione riutilizzabile e i primi validatori HR.

## Componenti introdotti

- `ValidationResult`
- `ValidationMessage`
- `ValidationSeverity`
- `ValidationException`
- `EmployeeValidator`
- `DepartmentValidator`
- `EmploymentContractValidator`

## Integrazione

`EmployeeService` valida ora i dati prima di creare o aggiornare un dipendente.

## Cosa NON cambia

- Nessuna UI modificata.
- Nessun database modificato.
- Nessuna migrazione dati.

## Prossimo sprint

`HR-002C.2 - Business Rules`
