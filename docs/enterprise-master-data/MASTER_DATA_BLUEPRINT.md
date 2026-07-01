# Sprint 12.0.4 - Enterprise Master Data Blueprint

## Obiettivo

Progettare il modulo Anagrafica Aziendale, che diventerà la base comune per tutti i moduli enterprise.

## Entità principali

### Company
Azienda principale.

Campi:
- Id
- Name
- VatNumber
- FiscalCode
- Address
- City
- Province
- Country
- Email
- Phone
- Website
- Notes

### Site
Sede aziendale.

Campi:
- Id
- CompanyId
- Name
- Address
- City
- Province
- Country
- IsMainSite
- Notes

### Department
Reparto o area aziendale.

Campi:
- Id
- Name
- Description
- SiteId
- ManagerEmployeeId

### Employee
Dipendente o collaboratore.

Campi:
- Id
- FullName
- Email
- Phone
- Role
- DepartmentId
- SiteId
- IsActive
- Notes

### Supplier
Fornitore.

Campi:
- Id
- Name
- VatNumber
- ContactName
- Email
- Phone
- Category
- Notes

## Collegamento con Asset Management

Ogni asset potrà essere collegato a:

- dipendente;
- sede;
- reparto;
- fornitore;
- manutentore.

## Sprint successivi

- 12.0.5 Database Master Data
- 12.0.6 UI Anagrafica Aziendale
- 12.0.7 Collegamento Asset → Employee/Site/Supplier
