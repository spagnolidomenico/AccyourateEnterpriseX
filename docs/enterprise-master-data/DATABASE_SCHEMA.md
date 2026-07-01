# Enterprise Master Data - Database Schema Draft

## Tabelle

```text
Companies
Sites
Departments
Employees
Suppliers
```

## Companies

| Campo | Tipo |
|---|---|
| Id | INTEGER |
| Name | TEXT |
| VatNumber | TEXT |
| FiscalCode | TEXT |
| Address | TEXT |
| City | TEXT |
| Province | TEXT |
| Country | TEXT |
| Email | TEXT |
| Phone | TEXT |
| Website | TEXT |
| Notes | TEXT |

## Sites

| Campo | Tipo |
|---|---|
| Id | INTEGER |
| CompanyId | INTEGER |
| Name | TEXT |
| Address | TEXT |
| City | TEXT |
| Province | TEXT |
| Country | TEXT |
| IsMainSite | INTEGER |
| Notes | TEXT |

## Departments

| Campo | Tipo |
|---|---|
| Id | INTEGER |
| Name | TEXT |
| Description | TEXT |
| SiteId | INTEGER |
| ManagerEmployeeId | INTEGER |

## Employees

| Campo | Tipo |
|---|---|
| Id | INTEGER |
| FullName | TEXT |
| Email | TEXT |
| Phone | TEXT |
| Role | TEXT |
| DepartmentId | INTEGER |
| SiteId | INTEGER |
| IsActive | INTEGER |
| Notes | TEXT |

## Suppliers

| Campo | Tipo |
|---|---|
| Id | INTEGER |
| Name | TEXT |
| VatNumber | TEXT |
| ContactName | TEXT |
| Email | TEXT |
| Phone | TEXT |
| Category | TEXT |
| Notes | TEXT |
