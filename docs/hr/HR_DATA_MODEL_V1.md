# HR Data Model v1

## Employee

```text
Employee
├── Id
├── FirstName
├── LastName
├── FullName
├── Email
├── Phone
├── RoleId
├── DepartmentId
├── SiteId
├── ManagerId
├── EmploymentStatus
├── HireDate
├── TerminationDate
├── Notes
├── CreatedAt
└── UpdatedAt
```

## Department

```text
Department
├── Id
├── Name
├── Code
├── SiteId
├── ManagerId
├── IsActive
└── Notes
```

## Site

```text
Site
├── Id
├── Name
├── Address
├── City
├── Province
├── Country
├── IsMain
└── Notes
```

## EmploymentContract

```text
EmploymentContract
├── Id
├── EmployeeId
├── ContractType
├── StartDate
├── EndDate
├── JobTitle
├── Level
├── Status
└── Notes
```

## EmployeeDocument

```text
EmployeeDocument
├── Id
├── EmployeeId
├── DocumentType
├── Title
├── FilePath
├── ExpirationDate
├── UploadedAt
├── UploadedBy
└── Notes
```

## Relazioni

```text
Employee 1 -> N EmploymentContract
Employee 1 -> N EmployeeDocument
Employee 1 -> N AssetAssignment
Department 1 -> N Employee
Site 1 -> N Department
Site 1 -> N Employee
```
