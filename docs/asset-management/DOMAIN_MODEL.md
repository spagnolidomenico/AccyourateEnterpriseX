# Asset Management - Domain Model

## Entità principali

### Asset

Rappresenta un bene aziendale.

Campi principali:

- Id
- AssetCode
- Category
- Manufacturer
- Model
- SerialNumber
- AssetTag
- Status
- PurchaseDate
- WarrantyEndDate
- OperatingSystem
- BitLockerEnabled
- Notes
- CreatedAt
- UpdatedAt

Categorie iniziali:

- Desktop PC
- Notebook
- Mac
- Stampante
- Smartphone
- Tablet
- Monitor
- Accessorio
- Licenza software
- Dispositivo medicale
- Altro

Stati iniziali:

- Attivo
- Assegnato
- Disponibile
- In manutenzione
- Dismesso
- Smarrito
- Da verificare

---

### Employee

Rappresenta un dipendente o utilizzatore.

Campi principali:

- Id
- FullName
- Email
- Department
- Role
- Site
- IsActive

---

### AssetAssignment

Rappresenta l'assegnazione di un asset a una persona.

Campi principali:

- Id
- AssetId
- EmployeeId
- AssignedAt
- ReturnedAt
- AssignedBy
- Notes
- Status

---

### MaintenanceTicket

Rappresenta un intervento tecnico.

Campi principali:

- Id
- AssetId
- Title
- Description
- Priority
- Status
- OpenedAt
- ClosedAt
- Technician
- ResolutionNotes

---

### AssetDocument

Documento collegato a un asset.

Campi principali:

- Id
- AssetId
- DocumentType
- FileName
- FilePath
- UploadedAt
- Notes

---

### AssetCredential

Credenziale o informazione sensibile collegata a un asset.

Campi principali:

- Id
- AssetId
- CredentialType
- Username
- SecretReference
- Notes
- UpdatedAt

Nota: le password non dovranno essere salvate in chiaro nel lungo periodo.
