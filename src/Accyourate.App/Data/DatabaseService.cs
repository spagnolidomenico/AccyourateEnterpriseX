using Microsoft.Data.Sqlite;
using Accyourate.App.Security;

namespace Accyourate.App.Data;

public sealed class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService()
    {
        _connectionString = $"Data Source={AppPaths.DatabasePath}";
    }

    public void Initialize()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
CREATE TABLE IF NOT EXISTS users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    username TEXT NOT NULL UNIQUE,
    display_name TEXT NOT NULL,
    password_hash TEXT NOT NULL,
    role TEXT NOT NULL,
    is_active INTEGER NOT NULL DEFAULT 1,
    created_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS audit_logs (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    username TEXT NOT NULL,
    action TEXT NOT NULL,
    details TEXT,
    created_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS roles (
    code TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    is_system INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS permissions (
    code TEXT PRIMARY KEY,
    description TEXT NOT NULL,
    module TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS role_permissions (
    role_code TEXT NOT NULL,
    permission_code TEXT NOT NULL,
    PRIMARY KEY (role_code, permission_code)
);

CREATE TABLE IF NOT EXISTS employees (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    employee_code TEXT NOT NULL UNIQUE,
    first_name TEXT NOT NULL,
    last_name TEXT NOT NULL,
    department TEXT,
    job_title TEXT,
    email TEXT,
    phone TEXT,
    hire_date TEXT,
    is_archived INTEGER NOT NULL DEFAULT 0,
    created_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS assets (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    asset_code TEXT NOT NULL UNIQUE,
    category TEXT NOT NULL,
    brand TEXT,
    model TEXT,
    serial_number TEXT,
    operating_system TEXT,
    status TEXT NOT NULL,
    assigned_employee_id INTEGER,
    purchase_date TEXT,
    warranty_end TEXT,
    notes TEXT,
    is_archived INTEGER NOT NULL DEFAULT 0,
    created_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS workflow_events (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    entity_type TEXT NOT NULL,
    entity_id INTEGER NOT NULL,
    entity_code TEXT NOT NULL,
    from_status TEXT,
    to_status TEXT NOT NULL,
    event_type TEXT NOT NULL,
    notes TEXT,
    created_by TEXT NOT NULL,
    created_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS database_versions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    version TEXT NOT NULL UNIQUE,
    description TEXT NOT NULL,
    applied_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS app_settings (
    key TEXT PRIMARY KEY,
    value TEXT NOT NULL,
    group_name TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS medical_devices (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    device_code TEXT NOT NULL UNIQUE,
    device_type TEXT NOT NULL,
    model TEXT,
    serial_number TEXT,
    lot_number TEXT,
    rfid_code TEXT,
    qr_code TEXT,
    status TEXT NOT NULL,
    production_date TEXT,
    test_date TEXT,
    notes TEXT,
    is_archived INTEGER NOT NULL DEFAULT 0,
    created_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS control_units (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    medical_device_id INTEGER NOT NULL,
    firmware_version TEXT,
    hardware_revision TEXT,
    mac_address TEXT,
    battery_status TEXT,
    last_functional_test_date TEXT,
    last_functional_test_result TEXT,
    notes TEXT
);

CREATE TABLE IF NOT EXISTS textile_items (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    medical_device_id INTEGER NOT NULL,
    textile_type TEXT NOT NULL,
    size TEXT,
    color TEXT,
    lot_number TEXT,
    rfid_code TEXT,
    wash_count INTEGER NOT NULL DEFAULT 0,
    last_functional_test_date TEXT,
    last_functional_test_result TEXT,
    conformity_status TEXT,
    notes TEXT
);

CREATE TABLE IF NOT EXISTS production_orders (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    order_code TEXT NOT NULL UNIQUE,
    medical_device_id INTEGER NOT NULL,
    lot_number TEXT,
    status TEXT NOT NULL,
    planned_date TEXT,
    operator_name TEXT,
    notes TEXT,
    created_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS quality_tests (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    medical_device_id INTEGER NOT NULL,
    test_code TEXT NOT NULL UNIQUE,
    checklist_name TEXT,
    functional_result TEXT,
    electrical_result TEXT,
    conformity_result TEXT,
    final_result TEXT,
    operator_name TEXT,
    test_date TEXT,
    notes TEXT,
    created_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS warehouse_locations (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    location_code TEXT NOT NULL UNIQUE,
    warehouse TEXT NOT NULL,
    aisle TEXT,
    shelf TEXT,
    level TEXT,
    description TEXT,
    is_active INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS stock_movements (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    medical_device_id INTEGER NOT NULL,
    movement_type TEXT NOT NULL,
    from_location_id INTEGER,
    to_location_id INTEGER,
    quantity TEXT NOT NULL,
    reason TEXT,
    operator_name TEXT,
    created_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS shipments (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    shipment_code TEXT NOT NULL UNIQUE,
    medical_device_id INTEGER NOT NULL,
    destination TEXT,
    status TEXT NOT NULL,
    tracking_code TEXT,
    operator_name TEXT,
    ship_date TEXT,
    return_date TEXT,
    notes TEXT,
    created_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS laundry_cycles (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    medical_device_id INTEGER NOT NULL,
    cycle_code TEXT NOT NULL UNIQUE,
    program_name TEXT,
    temperature TEXT,
    wash_date TEXT,
    operator_name TEXT,
    result TEXT,
    notes TEXT,
    wash_count_after INTEGER NOT NULL DEFAULT 0,
    created_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS maintenance_records (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    medical_device_id INTEGER NOT NULL,
    maintenance_code TEXT NOT NULL UNIQUE,
    maintenance_type TEXT,
    fault_description TEXT,
    action_taken TEXT,
    parts_replaced TEXT,
    result TEXT,
    operator_name TEXT,
    maintenance_date TEXT,
    notes TEXT,
    created_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS documents (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    document_code TEXT NOT NULL UNIQUE,
    title TEXT NOT NULL,
    category TEXT,
    entity_type TEXT,
    entity_id INTEGER,
    entity_code TEXT,
    file_name TEXT,
    file_path TEXT,
    version TEXT NOT NULL,
    status TEXT NOT NULL,
    created_by TEXT,
    created_at TEXT NOT NULL,
    notes TEXT
);
";
            command.ExecuteNonQuery();
        }

        EnsureRolesAndPermissions(connection);
        EnsureAdmin(connection);
        EnsureDatabaseVersion(connection);
        EnsureDefaultSettings(connection);
    }

    private static void EnsureRolesAndPermissions(SqliteConnection connection)
    {
        var roles = new[]
        {
            ("Admin", "Amministratore"),
            ("Operatore", "Operatore"),
            ("Lettura", "Solo lettura")
        };

        foreach (var role in roles)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT OR IGNORE INTO roles (code, name, is_system) VALUES ($code, $name, 1)";
            cmd.Parameters.AddWithValue("$code", role.Item1);
            cmd.Parameters.AddWithValue("$name", role.Item2);
            cmd.ExecuteNonQuery();
        }

        var permissions = new[]
        {
            (PermissionCodes.DashboardView, "Dashboard", "dashboard"),
            (PermissionCodes.PeopleView, "Persone", "people"),
            (PermissionCodes.AssetsView, "Asset IT", "assets"),
            (PermissionCodes.MedicalView, "Dispositivi Medici", "medical"),
            (PermissionCodes.NetworkView, "Rete", "network"),
            (PermissionCodes.DocumentsView, "Documenti", "documents"),
            (PermissionCodes.UsersManage, "Gestione utenti", "admin"),
            (PermissionCodes.DiagnosticsView, "Diagnostica", "admin"),
            (PermissionCodes.PasswordChange, "Cambio password", "account")
        };

        foreach (var permission in permissions)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT OR IGNORE INTO permissions (code, description, module) VALUES ($code, $description, $module)";
            cmd.Parameters.AddWithValue("$code", permission.Item1);
            cmd.Parameters.AddWithValue("$description", permission.Item2);
            cmd.Parameters.AddWithValue("$module", permission.Item3);
            cmd.ExecuteNonQuery();
        }

        AssignAll(connection, "Admin");

        Assign(connection, "Operatore", new[]
        {
            PermissionCodes.DashboardView,
            PermissionCodes.PeopleView,
            PermissionCodes.AssetsView,
            PermissionCodes.MedicalView,
            PermissionCodes.DocumentsView,
            PermissionCodes.PasswordChange
        });

        Assign(connection, "Lettura", new[]
        {
            PermissionCodes.DashboardView,
            PermissionCodes.PeopleView,
            PermissionCodes.AssetsView,
            PermissionCodes.MedicalView,
            PermissionCodes.DocumentsView,
            PermissionCodes.PasswordChange
        });
    }

    private static void AssignAll(SqliteConnection connection, string role)
    {
        using var select = connection.CreateCommand();
        select.CommandText = "SELECT code FROM permissions";

        using var reader = select.ExecuteReader();
        while (reader.Read())
        {
            InsertRolePermission(connection, role, reader.GetString(0));
        }
    }

    private static void Assign(SqliteConnection connection, string role, IEnumerable<string> permissions)
    {
        foreach (var permission in permissions)
            InsertRolePermission(connection, role, permission);
    }

    private static void InsertRolePermission(SqliteConnection connection, string role, string permission)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT OR IGNORE INTO role_permissions (role_code, permission_code) VALUES ($role, $permission)";
        cmd.Parameters.AddWithValue("$role", role);
        cmd.Parameters.AddWithValue("$permission", permission);
        cmd.ExecuteNonQuery();
    }

    private static void EnsureAdmin(SqliteConnection connection)
    {
        using var check = connection.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM users WHERE username = 'admin'";
        var count = Convert.ToInt64(check.ExecuteScalar());

        if (count > 0)
            return;

        using var insert = connection.CreateCommand();
        insert.CommandText = @"
INSERT INTO users (username, display_name, password_hash, role, is_active, created_at)
VALUES ($username, $displayName, $passwordHash, $role, 1, $createdAt)
";
        insert.Parameters.AddWithValue("$username", "admin");
        insert.Parameters.AddWithValue("$displayName", "Amministratore");
        insert.Parameters.AddWithValue("$passwordHash", PasswordHasher.Hash("admin123"));
        insert.Parameters.AddWithValue("$role", "Admin");
        insert.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("O"));
        insert.ExecuteNonQuery();
    }

    public UserRecord? FindActiveUser(string username)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT id, username, display_name, password_hash, role, is_active, created_at
FROM users
WHERE username = $username AND is_active = 1
";
        command.Parameters.AddWithValue("$username", username.Trim());

        using var reader = command.ExecuteReader();
        if (!reader.Read())
            return null;

        return new UserRecord
        {
            Id = reader.GetInt64(0),
            Username = reader.GetString(1),
            DisplayName = reader.GetString(2),
            PasswordHash = reader.GetString(3),
            Role = reader.GetString(4),
            IsActive = reader.GetInt32(5) == 1,
            CreatedAt = reader.GetString(6)
        };
    }

    public HashSet<string> GetPermissionsForRole(string role)
    {
        var permissions = new HashSet<string>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT permission_code FROM role_permissions WHERE role_code = $role";
        command.Parameters.AddWithValue("$role", role);

        using var reader = command.ExecuteReader();
        while (reader.Read())
            permissions.Add(reader.GetString(0));

        if (role == "Admin")
            permissions.Add("*");

        return permissions;
    }

    public List<UserRecord> GetUsers()
    {
        var users = new List<UserRecord>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT id, username, display_name, password_hash, role, is_active, created_at
FROM users
ORDER BY username
";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            users.Add(new UserRecord
            {
                Id = reader.GetInt64(0),
                Username = reader.GetString(1),
                DisplayName = reader.GetString(2),
                PasswordHash = reader.GetString(3),
                Role = reader.GetString(4),
                IsActive = reader.GetInt32(5) == 1,
                CreatedAt = reader.GetString(6)
            });
        }

        return users;
    }

    public bool CreateUser(string username, string displayName, string password, string role, string createdBy, out string error)
    {
        error = "";

        if (string.IsNullOrWhiteSpace(username))
        {
            error = "Username obbligatorio.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            error = "Password obbligatoria.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(displayName))
            displayName = username;

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
INSERT INTO users (username, display_name, password_hash, role, is_active, created_at)
VALUES ($username, $displayName, $passwordHash, $role, 1, $createdAt)
";
            command.Parameters.AddWithValue("$username", username.Trim());
            command.Parameters.AddWithValue("$displayName", displayName.Trim());
            command.Parameters.AddWithValue("$passwordHash", PasswordHasher.Hash(password));
            command.Parameters.AddWithValue("$role", role);
            command.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("O"));
            command.ExecuteNonQuery();

            WriteAudit(createdBy, "USER_CREATED", $"Creato utente {username}");
            return true;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            error = "Username già esistente.";
            return false;
        }
    }

    public bool ChangePassword(string username, string oldPassword, string newPassword, out string error)
    {
        error = "";

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            error = "La nuova password deve contenere almeno 6 caratteri.";
            return false;
        }

        var user = FindActiveUser(username);
        if (user is null)
        {
            error = "Utente non trovato.";
            return false;
        }

        if (!PasswordHasher.Verify(oldPassword, user.PasswordHash))
        {
            error = "Password attuale non corretta.";
            return false;
        }

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE users SET password_hash = $hash WHERE username = $username";
        command.Parameters.AddWithValue("$hash", PasswordHasher.Hash(newPassword));
        command.Parameters.AddWithValue("$username", username);
        command.ExecuteNonQuery();

        WriteAudit(username, "PASSWORD_CHANGED", "Cambio password effettuato");
        return true;
    }

    public void SetUserActive(long userId, bool isActive, string changedBy)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE users SET is_active = $active WHERE id = $id";
        command.Parameters.AddWithValue("$active", isActive ? 1 : 0);
        command.Parameters.AddWithValue("$id", userId);
        command.ExecuteNonQuery();

        WriteAudit(changedBy, isActive ? "USER_ENABLED" : "USER_DISABLED", $"UserId={userId}");
    }

    public void SetUserRole(long userId, string role, string changedBy)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE users SET role = $role WHERE id = $id";
        command.Parameters.AddWithValue("$role", role);
        command.Parameters.AddWithValue("$id", userId);
        command.ExecuteNonQuery();

        WriteAudit(changedBy, "USER_ROLE_CHANGED", $"UserId={userId}; Role={role}");
    }

    public List<AuditRecord> GetRecentAudit(int limit = 50)
    {
        var rows = new List<AuditRecord>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT id, username, action, details, created_at
FROM audit_logs
ORDER BY id DESC
LIMIT $limit
";
        command.Parameters.AddWithValue("$limit", limit);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rows.Add(new AuditRecord
            {
                Id = reader.GetInt64(0),
                Username = reader.GetString(1),
                Action = reader.GetString(2),
                Details = reader.IsDBNull(3) ? "" : reader.GetString(3),
                CreatedAt = reader.GetString(4)
            });
        }

        return rows;
    }

    public DatabaseDiagnostics GetDiagnostics()
    {
        var file = new FileInfo(AppPaths.DatabasePath);
        var users = GetUsers();
        var auditCount = 0;

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM audit_logs";
        auditCount = Convert.ToInt32(command.ExecuteScalar());

        return new DatabaseDiagnostics
        {
            DatabasePath = AppPaths.DatabasePath,
            Exists = file.Exists,
            SizeBytes = file.Exists ? file.Length : 0,
            UsersCount = users.Count,
            ActiveUsersCount = users.Count(x => x.IsActive),
            AuditCount = auditCount
        };
    }


    public List<EmployeeRecord> GetEmployees(string? search = null, bool includeArchived = false)
    {
        var rows = new List<EmployeeRecord>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT id, employee_code, first_name, last_name, department, job_title, email, phone, hire_date, is_archived, created_at
FROM employees
WHERE ($includeArchived = 1 OR is_archived = 0)
  AND (
      $search = ''
      OR employee_code LIKE $like
      OR first_name LIKE $like
      OR last_name LIKE $like
      OR department LIKE $like
      OR job_title LIKE $like
      OR email LIKE $like
  )
ORDER BY last_name, first_name
";
        var q = search?.Trim() ?? "";
        command.Parameters.AddWithValue("$includeArchived", includeArchived ? 1 : 0);
        command.Parameters.AddWithValue("$search", q);
        command.Parameters.AddWithValue("$like", $"%{q}%");

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rows.Add(new EmployeeRecord
            {
                Id = reader.GetInt64(0),
                EmployeeCode = reader.GetString(1),
                FirstName = reader.GetString(2),
                LastName = reader.GetString(3),
                Department = reader.IsDBNull(4) ? "" : reader.GetString(4),
                JobTitle = reader.IsDBNull(5) ? "" : reader.GetString(5),
                Email = reader.IsDBNull(6) ? "" : reader.GetString(6),
                Phone = reader.IsDBNull(7) ? "" : reader.GetString(7),
                HireDate = reader.IsDBNull(8) ? "" : reader.GetString(8),
                IsArchived = reader.GetInt32(9) == 1,
                CreatedAt = reader.GetString(10)
            });
        }

        return rows;
    }

    public bool CreateEmployee(EmployeeRecord employee, string createdBy, out string error)
    {
        error = "";

        if (string.IsNullOrWhiteSpace(employee.EmployeeCode))
        {
            error = "Matricola obbligatoria.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(employee.FirstName) || string.IsNullOrWhiteSpace(employee.LastName))
        {
            error = "Nome e cognome sono obbligatori.";
            return false;
        }

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
INSERT INTO employees (employee_code, first_name, last_name, department, job_title, email, phone, hire_date, is_archived, created_at)
VALUES ($employeeCode, $firstName, $lastName, $department, $jobTitle, $email, $phone, $hireDate, 0, $createdAt)
";
            command.Parameters.AddWithValue("$employeeCode", employee.EmployeeCode.Trim());
            command.Parameters.AddWithValue("$firstName", employee.FirstName.Trim());
            command.Parameters.AddWithValue("$lastName", employee.LastName.Trim());
            command.Parameters.AddWithValue("$department", employee.Department.Trim());
            command.Parameters.AddWithValue("$jobTitle", employee.JobTitle.Trim());
            command.Parameters.AddWithValue("$email", employee.Email.Trim());
            command.Parameters.AddWithValue("$phone", employee.Phone.Trim());
            command.Parameters.AddWithValue("$hireDate", employee.HireDate.Trim());
            command.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("O"));
            command.ExecuteNonQuery();

            WriteAudit(createdBy, "EMPLOYEE_CREATED", $"Dipendente {employee.EmployeeCode} - {employee.FirstName} {employee.LastName}");
            return true;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            error = "Matricola già esistente.";
            return false;
        }
    }


    public EmployeeRecord? GetEmployeeById(long employeeId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT id, employee_code, first_name, last_name, department, job_title, email, phone, hire_date, is_archived, created_at
FROM employees
WHERE id = $id
";
        command.Parameters.AddWithValue("$id", employeeId);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
            return null;

        return new EmployeeRecord
        {
            Id = reader.GetInt64(0),
            EmployeeCode = reader.GetString(1),
            FirstName = reader.GetString(2),
            LastName = reader.GetString(3),
            Department = reader.IsDBNull(4) ? "" : reader.GetString(4),
            JobTitle = reader.IsDBNull(5) ? "" : reader.GetString(5),
            Email = reader.IsDBNull(6) ? "" : reader.GetString(6),
            Phone = reader.IsDBNull(7) ? "" : reader.GetString(7),
            HireDate = reader.IsDBNull(8) ? "" : reader.GetString(8),
            IsArchived = reader.GetInt32(9) == 1,
            CreatedAt = reader.GetString(10)
        };
    }

    public bool UpdateEmployee(EmployeeRecord employee, string changedBy, out string error)
    {
        error = "";

        if (employee.Id <= 0)
        {
            error = "Dipendente non valido.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(employee.EmployeeCode))
        {
            error = "Matricola obbligatoria.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(employee.FirstName) || string.IsNullOrWhiteSpace(employee.LastName))
        {
            error = "Nome e cognome sono obbligatori.";
            return false;
        }

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
UPDATE employees
SET employee_code = $employeeCode,
    first_name = $firstName,
    last_name = $lastName,
    department = $department,
    job_title = $jobTitle,
    email = $email,
    phone = $phone,
    hire_date = $hireDate
WHERE id = $id
";
            command.Parameters.AddWithValue("$employeeCode", employee.EmployeeCode.Trim());
            command.Parameters.AddWithValue("$firstName", employee.FirstName.Trim());
            command.Parameters.AddWithValue("$lastName", employee.LastName.Trim());
            command.Parameters.AddWithValue("$department", employee.Department.Trim());
            command.Parameters.AddWithValue("$jobTitle", employee.JobTitle.Trim());
            command.Parameters.AddWithValue("$email", employee.Email.Trim());
            command.Parameters.AddWithValue("$phone", employee.Phone.Trim());
            command.Parameters.AddWithValue("$hireDate", employee.HireDate.Trim());
            command.Parameters.AddWithValue("$id", employee.Id);
            command.ExecuteNonQuery();

            WriteAudit(changedBy, "EMPLOYEE_UPDATED", $"Dipendente {employee.EmployeeCode} - {employee.FirstName} {employee.LastName}");
            return true;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            error = "Matricola già esistente.";
            return false;
        }
    }




    private static void EnsureDatabaseVersion(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
INSERT OR IGNORE INTO database_versions (version, description, applied_at)
VALUES ('3.1.0', 'Project Infrastructure baseline', $appliedAt)
";
        cmd.Parameters.AddWithValue("$appliedAt", DateTime.UtcNow.ToString("O"));
        cmd.ExecuteNonQuery();
    }

    private static void EnsureDefaultSettings(SqliteConnection connection)
    {
        var settings = new[]
        {
            ("company.name", "Accyourate Group", "company"),
            ("theme.primaryColor", "#B5162B", "theme"),
            ("backup.enabled", "true", "backup"),
            ("backup.retentionDays", "30", "backup"),
            ("release.channel", "developer", "release")
        };

        foreach (var setting in settings)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT OR IGNORE INTO app_settings (key, value, group_name) VALUES ($key, $value, $group)";
            cmd.Parameters.AddWithValue("$key", setting.Item1);
            cmd.Parameters.AddWithValue("$value", setting.Item2);
            cmd.Parameters.AddWithValue("$group", setting.Item3);
            cmd.ExecuteNonQuery();
        }
    }



    public List<ProductionOrderRecord> GetProductionOrders()
    {
        var rows = new List<ProductionOrderRecord>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT p.id, p.order_code, p.medical_device_id, m.device_code, m.device_type, p.lot_number,
       p.status, p.planned_date, p.operator_name, p.notes, p.created_at
FROM production_orders p
JOIN medical_devices m ON m.id = p.medical_device_id
ORDER BY p.id DESC
";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rows.Add(new ProductionOrderRecord
            {
                Id = reader.GetInt64(0),
                OrderCode = reader.GetString(1),
                MedicalDeviceId = reader.GetInt64(2),
                DeviceCode = reader.GetString(3),
                DeviceType = reader.GetString(4),
                LotNumber = reader.IsDBNull(5) ? "" : reader.GetString(5),
                Status = reader.GetString(6),
                PlannedDate = reader.IsDBNull(7) ? "" : reader.GetString(7),
                OperatorName = reader.IsDBNull(8) ? "" : reader.GetString(8),
                Notes = reader.IsDBNull(9) ? "" : reader.GetString(9),
                CreatedAt = reader.GetString(10)
            });
        }
        return rows;
    }

    public bool CreateProductionOrder(ProductionOrderRecord order, string createdBy, out string error)
    {
        error = "";
        if (string.IsNullOrWhiteSpace(order.OrderCode)) { error = "Codice ordine obbligatorio."; return false; }
        if (order.MedicalDeviceId <= 0) { error = "Dispositivo medico obbligatorio."; return false; }

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
INSERT INTO production_orders (order_code, medical_device_id, lot_number, status, planned_date, operator_name, notes, created_at)
VALUES ($orderCode, $deviceId, $lot, $status, $planned, $operator, $notes, $createdAt)
";
            command.Parameters.AddWithValue("$orderCode", order.OrderCode.Trim());
            command.Parameters.AddWithValue("$deviceId", order.MedicalDeviceId);
            command.Parameters.AddWithValue("$lot", order.LotNumber.Trim());
            command.Parameters.AddWithValue("$status", order.Status.Trim());
            command.Parameters.AddWithValue("$planned", order.PlannedDate.Trim());
            command.Parameters.AddWithValue("$operator", order.OperatorName.Trim());
            command.Parameters.AddWithValue("$notes", order.Notes.Trim());
            command.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("O"));
            command.ExecuteNonQuery();

            var device = GetMedicalDeviceById(order.MedicalDeviceId);
            if (device is not null)
            {
                AddWorkflowEvent("DispositivoMedico", device.Id, device.DeviceCode, device.Status, "Produzione", "PRODUCTION_ORDER_CREATED", $"Ordine {order.OrderCode}", createdBy);
                ChangeMedicalDeviceStatus(device.Id, "Produzione", $"Ordine produzione {order.OrderCode}", createdBy);
            }

            WriteAudit(createdBy, "PRODUCTION_ORDER_CREATED", $"Ordine {order.OrderCode}");
            return true;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            error = "Codice ordine già esistente.";
            return false;
        }
    }

    public void AdvanceProductionOrder(long orderId, string newStatus, string changedBy)
    {
        var order = GetProductionOrders().FirstOrDefault(x => x.Id == orderId);
        if (order is null) return;

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE production_orders SET status = $status WHERE id = $id";
        command.Parameters.AddWithValue("$status", newStatus);
        command.Parameters.AddWithValue("$id", orderId);
        command.ExecuteNonQuery();

        var device = GetMedicalDeviceById(order.MedicalDeviceId);
        if (device is not null)
            AddWorkflowEvent("DispositivoMedico", device.Id, device.DeviceCode, order.Status, newStatus, "PRODUCTION_STATUS_CHANGED", $"Ordine {order.OrderCode}", changedBy);

        WriteAudit(changedBy, "PRODUCTION_ORDER_STATUS_CHANGED", $"OrderId={orderId}; Status={newStatus}");
    }

    public List<QualityTestRecord> GetQualityTests()
    {
        var rows = new List<QualityTestRecord>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT q.id, q.medical_device_id, m.device_code, q.test_code, q.checklist_name,
       q.functional_result, q.electrical_result, q.conformity_result, q.final_result,
       q.operator_name, q.test_date, q.notes, q.created_at
FROM quality_tests q
JOIN medical_devices m ON m.id = q.medical_device_id
ORDER BY q.id DESC
";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rows.Add(new QualityTestRecord
            {
                Id = reader.GetInt64(0),
                MedicalDeviceId = reader.GetInt64(1),
                DeviceCode = reader.GetString(2),
                TestCode = reader.GetString(3),
                ChecklistName = reader.IsDBNull(4) ? "" : reader.GetString(4),
                FunctionalResult = reader.IsDBNull(5) ? "" : reader.GetString(5),
                ElectricalResult = reader.IsDBNull(6) ? "" : reader.GetString(6),
                ConformityResult = reader.IsDBNull(7) ? "" : reader.GetString(7),
                FinalResult = reader.IsDBNull(8) ? "" : reader.GetString(8),
                OperatorName = reader.IsDBNull(9) ? "" : reader.GetString(9),
                TestDate = reader.IsDBNull(10) ? "" : reader.GetString(10),
                Notes = reader.IsDBNull(11) ? "" : reader.GetString(11),
                CreatedAt = reader.GetString(12)
            });
        }
        return rows;
    }

    public bool CreateQualityTest(QualityTestRecord test, string createdBy, out string error)
    {
        error = "";
        if (string.IsNullOrWhiteSpace(test.TestCode)) { error = "Codice test obbligatorio."; return false; }
        if (test.MedicalDeviceId <= 0) { error = "Dispositivo medico obbligatorio."; return false; }

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
INSERT INTO quality_tests (medical_device_id, test_code, checklist_name, functional_result, electrical_result, conformity_result, final_result, operator_name, test_date, notes, created_at)
VALUES ($deviceId, $testCode, $checklist, $functional, $electrical, $conformity, $final, $operator, $testDate, $notes, $createdAt)
";
            command.Parameters.AddWithValue("$deviceId", test.MedicalDeviceId);
            command.Parameters.AddWithValue("$testCode", test.TestCode.Trim());
            command.Parameters.AddWithValue("$checklist", test.ChecklistName.Trim());
            command.Parameters.AddWithValue("$functional", test.FunctionalResult.Trim());
            command.Parameters.AddWithValue("$electrical", test.ElectricalResult.Trim());
            command.Parameters.AddWithValue("$conformity", test.ConformityResult.Trim());
            command.Parameters.AddWithValue("$final", test.FinalResult.Trim());
            command.Parameters.AddWithValue("$operator", test.OperatorName.Trim());
            command.Parameters.AddWithValue("$testDate", test.TestDate.Trim());
            command.Parameters.AddWithValue("$notes", test.Notes.Trim());
            command.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("O"));
            command.ExecuteNonQuery();

            var device = GetMedicalDeviceById(test.MedicalDeviceId);
            if (device is not null)
            {
                var status = test.FinalResult == "Conforme" ? "Qualità Conforme" : "Non Conforme";
                AddWorkflowEvent("DispositivoMedico", device.Id, device.DeviceCode, device.Status, status, "QUALITY_TEST_COMPLETED", $"Test {test.TestCode}: {test.FinalResult}", createdBy);
                ChangeMedicalDeviceStatus(device.Id, status, $"Test qualità {test.TestCode}", createdBy);
            }

            WriteAudit(createdBy, "QUALITY_TEST_CREATED", $"Test {test.TestCode}; Esito={test.FinalResult}");
            return true;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            error = "Codice test già esistente.";
            return false;
        }
    }


    public List<WarehouseLocationRecord> GetWarehouseLocations()
    {
        var rows = new List<WarehouseLocationRecord>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT id, location_code, warehouse, aisle, shelf, level, description, is_active
FROM warehouse_locations
ORDER BY warehouse, aisle, shelf, level
";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rows.Add(new WarehouseLocationRecord
            {
                Id = reader.GetInt64(0),
                LocationCode = reader.GetString(1),
                Warehouse = reader.GetString(2),
                Aisle = reader.IsDBNull(3) ? "" : reader.GetString(3),
                Shelf = reader.IsDBNull(4) ? "" : reader.GetString(4),
                Level = reader.IsDBNull(5) ? "" : reader.GetString(5),
                Description = reader.IsDBNull(6) ? "" : reader.GetString(6),
                IsActive = reader.GetInt32(7) == 1
            });
        }

        return rows;
    }

    public bool CreateWarehouseLocation(WarehouseLocationRecord location, string createdBy, out string error)
    {
        error = "";

        if (string.IsNullOrWhiteSpace(location.LocationCode))
        {
            error = "Codice ubicazione obbligatorio.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(location.Warehouse))
        {
            error = "Magazzino obbligatorio.";
            return false;
        }

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
INSERT INTO warehouse_locations (location_code, warehouse, aisle, shelf, level, description, is_active)
VALUES ($code, $warehouse, $aisle, $shelf, $level, $description, 1)
";
            command.Parameters.AddWithValue("$code", location.LocationCode.Trim());
            command.Parameters.AddWithValue("$warehouse", location.Warehouse.Trim());
            command.Parameters.AddWithValue("$aisle", location.Aisle.Trim());
            command.Parameters.AddWithValue("$shelf", location.Shelf.Trim());
            command.Parameters.AddWithValue("$level", location.Level.Trim());
            command.Parameters.AddWithValue("$description", location.Description.Trim());
            command.ExecuteNonQuery();

            WriteAudit(createdBy, "WAREHOUSE_LOCATION_CREATED", $"Ubicazione {location.LocationCode}");
            return true;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            error = "Codice ubicazione già esistente.";
            return false;
        }
    }

    public bool CreateStockMovement(StockMovementRecord movement, long? fromLocationId, long? toLocationId, string createdBy, out string error)
    {
        error = "";

        if (movement.MedicalDeviceId <= 0)
        {
            error = "Dispositivo medico obbligatorio.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(movement.MovementType))
        {
            error = "Tipo movimento obbligatorio.";
            return false;
        }

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO stock_movements (medical_device_id, movement_type, from_location_id, to_location_id, quantity, reason, operator_name, created_at)
VALUES ($deviceId, $type, $from, $to, $quantity, $reason, $operator, $createdAt)
";
        command.Parameters.AddWithValue("$deviceId", movement.MedicalDeviceId);
        command.Parameters.AddWithValue("$type", movement.MovementType.Trim());
        command.Parameters.AddWithValue("$from", fromLocationId.HasValue ? fromLocationId.Value : DBNull.Value);
        command.Parameters.AddWithValue("$to", toLocationId.HasValue ? toLocationId.Value : DBNull.Value);
        command.Parameters.AddWithValue("$quantity", string.IsNullOrWhiteSpace(movement.Quantity) ? "1" : movement.Quantity.Trim());
        command.Parameters.AddWithValue("$reason", movement.Reason.Trim());
        command.Parameters.AddWithValue("$operator", movement.OperatorName.Trim());
        command.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("O"));
        command.ExecuteNonQuery();

        var device = GetMedicalDeviceById(movement.MedicalDeviceId);
        if (device is not null)
        {
            AddWorkflowEvent("DispositivoMedico", device.Id, device.DeviceCode, device.Status, "Movimentato", "STOCK_MOVEMENT_CREATED", $"{movement.MovementType}: {movement.Reason}", createdBy);
            ChangeMedicalDeviceStatus(device.Id, "Magazzino", $"Movimento {movement.MovementType}", createdBy);
        }

        WriteAudit(createdBy, "STOCK_MOVEMENT_CREATED", $"DeviceId={movement.MedicalDeviceId}; Type={movement.MovementType}");
        return true;
    }

    public List<StockMovementRecord> GetStockMovements()
    {
        var rows = new List<StockMovementRecord>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT s.id, s.medical_device_id, m.device_code, s.movement_type,
       COALESCE(fl.location_code, '') AS from_location,
       COALESCE(tl.location_code, '') AS to_location,
       s.quantity, s.reason, s.operator_name, s.created_at
FROM stock_movements s
JOIN medical_devices m ON m.id = s.medical_device_id
LEFT JOIN warehouse_locations fl ON fl.id = s.from_location_id
LEFT JOIN warehouse_locations tl ON tl.id = s.to_location_id
ORDER BY s.id DESC
";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rows.Add(new StockMovementRecord
            {
                Id = reader.GetInt64(0),
                MedicalDeviceId = reader.GetInt64(1),
                DeviceCode = reader.GetString(2),
                MovementType = reader.GetString(3),
                FromLocationCode = reader.IsDBNull(4) ? "" : reader.GetString(4),
                ToLocationCode = reader.IsDBNull(5) ? "" : reader.GetString(5),
                Quantity = reader.GetString(6),
                Reason = reader.IsDBNull(7) ? "" : reader.GetString(7),
                OperatorName = reader.IsDBNull(8) ? "" : reader.GetString(8),
                CreatedAt = reader.GetString(9)
            });
        }

        return rows;
    }

    public bool CreateShipment(ShipmentRecord shipment, string createdBy, out string error)
    {
        error = "";

        if (string.IsNullOrWhiteSpace(shipment.ShipmentCode))
        {
            error = "Codice spedizione obbligatorio.";
            return false;
        }

        if (shipment.MedicalDeviceId <= 0)
        {
            error = "Dispositivo medico obbligatorio.";
            return false;
        }

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
INSERT INTO shipments (shipment_code, medical_device_id, destination, status, tracking_code, operator_name, ship_date, return_date, notes, created_at)
VALUES ($code, $deviceId, $destination, $status, $tracking, $operator, $shipDate, '', $notes, $createdAt)
";
            command.Parameters.AddWithValue("$code", shipment.ShipmentCode.Trim());
            command.Parameters.AddWithValue("$deviceId", shipment.MedicalDeviceId);
            command.Parameters.AddWithValue("$destination", shipment.Destination.Trim());
            command.Parameters.AddWithValue("$status", shipment.Status.Trim());
            command.Parameters.AddWithValue("$tracking", shipment.TrackingCode.Trim());
            command.Parameters.AddWithValue("$operator", shipment.OperatorName.Trim());
            command.Parameters.AddWithValue("$shipDate", shipment.ShipDate.Trim());
            command.Parameters.AddWithValue("$notes", shipment.Notes.Trim());
            command.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("O"));
            command.ExecuteNonQuery();

            var device = GetMedicalDeviceById(shipment.MedicalDeviceId);
            if (device is not null)
            {
                AddWorkflowEvent("DispositivoMedico", device.Id, device.DeviceCode, device.Status, "Spedito", "SHIPMENT_CREATED", $"Spedizione {shipment.ShipmentCode} verso {shipment.Destination}", createdBy);
                ChangeMedicalDeviceStatus(device.Id, "Spedito", $"Spedizione {shipment.ShipmentCode}", createdBy);
            }

            WriteAudit(createdBy, "SHIPMENT_CREATED", $"Shipment={shipment.ShipmentCode}");
            return true;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            error = "Codice spedizione già esistente.";
            return false;
        }
    }

    public void MarkShipmentReturned(long shipmentId, string changedBy)
    {
        var shipment = GetShipments().FirstOrDefault(x => x.Id == shipmentId);
        if (shipment is null)
            return;

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE shipments SET status = 'Rientrato', return_date = $returnDate WHERE id = $id";
        command.Parameters.AddWithValue("$returnDate", DateTime.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("$id", shipmentId);
        command.ExecuteNonQuery();

        var device = GetMedicalDeviceById(shipment.MedicalDeviceId);
        if (device is not null)
        {
            AddWorkflowEvent("DispositivoMedico", device.Id, device.DeviceCode, device.Status, "Rientrato", "RETURN_RECEIVED", $"Rientro spedizione {shipment.ShipmentCode}", changedBy);
            ChangeMedicalDeviceStatus(device.Id, "Rientrato", $"Rientro spedizione {shipment.ShipmentCode}", changedBy);
        }

        WriteAudit(changedBy, "SHIPMENT_RETURNED", $"ShipmentId={shipmentId}");
    }

    public List<ShipmentRecord> GetShipments()
    {
        var rows = new List<ShipmentRecord>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT sh.id, sh.shipment_code, sh.medical_device_id, m.device_code, sh.destination, sh.status,
       sh.tracking_code, sh.operator_name, sh.ship_date, sh.return_date, sh.notes, sh.created_at
FROM shipments sh
JOIN medical_devices m ON m.id = sh.medical_device_id
ORDER BY sh.id DESC
";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rows.Add(new ShipmentRecord
            {
                Id = reader.GetInt64(0),
                ShipmentCode = reader.GetString(1),
                MedicalDeviceId = reader.GetInt64(2),
                DeviceCode = reader.GetString(3),
                Destination = reader.IsDBNull(4) ? "" : reader.GetString(4),
                Status = reader.GetString(5),
                TrackingCode = reader.IsDBNull(6) ? "" : reader.GetString(6),
                OperatorName = reader.IsDBNull(7) ? "" : reader.GetString(7),
                ShipDate = reader.IsDBNull(8) ? "" : reader.GetString(8),
                ReturnDate = reader.IsDBNull(9) ? "" : reader.GetString(9),
                Notes = reader.IsDBNull(10) ? "" : reader.GetString(10),
                CreatedAt = reader.GetString(11)
            });
        }

        return rows;
    }


    public int GetTextileWashCountForDevice(long medicalDeviceId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COALESCE(MAX(wash_count), 0) FROM textile_items WHERE medical_device_id = $deviceId";
        command.Parameters.AddWithValue("$deviceId", medicalDeviceId);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void IncrementTextileWashCount(long medicalDeviceId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE textile_items SET wash_count = wash_count + 1 WHERE medical_device_id = $deviceId";
        command.Parameters.AddWithValue("$deviceId", medicalDeviceId);
        command.ExecuteNonQuery();
    }

    public bool CreateLaundryCycle(LaundryCycleRecord cycle, string createdBy, out string error)
    {
        error = "";

        if (cycle.MedicalDeviceId <= 0)
        {
            error = "Dispositivo medico obbligatorio.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(cycle.CycleCode))
        {
            error = "Codice ciclo lavaggio obbligatorio.";
            return false;
        }

        try
        {
            var currentWashCount = GetTextileWashCountForDevice(cycle.MedicalDeviceId);
            var nextWashCount = currentWashCount + 1;

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
INSERT INTO laundry_cycles (medical_device_id, cycle_code, program_name, temperature, wash_date, operator_name, result, notes, wash_count_after, created_at)
VALUES ($deviceId, $cycleCode, $program, $temperature, $washDate, $operator, $result, $notes, $washCountAfter, $createdAt)
";
            command.Parameters.AddWithValue("$deviceId", cycle.MedicalDeviceId);
            command.Parameters.AddWithValue("$cycleCode", cycle.CycleCode.Trim());
            command.Parameters.AddWithValue("$program", cycle.ProgramName.Trim());
            command.Parameters.AddWithValue("$temperature", cycle.Temperature.Trim());
            command.Parameters.AddWithValue("$washDate", cycle.WashDate.Trim());
            command.Parameters.AddWithValue("$operator", cycle.OperatorName.Trim());
            command.Parameters.AddWithValue("$result", cycle.Result.Trim());
            command.Parameters.AddWithValue("$notes", cycle.Notes.Trim());
            command.Parameters.AddWithValue("$washCountAfter", nextWashCount);
            command.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("O"));
            command.ExecuteNonQuery();

            IncrementTextileWashCount(cycle.MedicalDeviceId);

            var device = GetMedicalDeviceById(cycle.MedicalDeviceId);
            if (device is not null)
            {
                var status = nextWashCount >= 50 ? "Lavaggi limite" : "Lavato";
                AddWorkflowEvent("DispositivoMedico", device.Id, device.DeviceCode, device.Status, status, "LAUNDRY_CYCLE_CREATED", $"Ciclo {cycle.CycleCode}; lavaggi={nextWashCount}", createdBy);
                ChangeMedicalDeviceStatus(device.Id, status, $"Lavaggio {cycle.CycleCode}", createdBy);
            }

            WriteAudit(createdBy, "LAUNDRY_CYCLE_CREATED", $"Cycle={cycle.CycleCode}; DeviceId={cycle.MedicalDeviceId}; WashCount={nextWashCount}");
            return true;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            error = "Codice ciclo già esistente.";
            return false;
        }
    }

    public List<LaundryCycleRecord> GetLaundryCycles()
    {
        var rows = new List<LaundryCycleRecord>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT l.id, l.medical_device_id, m.device_code, l.cycle_code, l.program_name, l.temperature,
       l.wash_date, l.operator_name, l.result, l.notes, l.wash_count_after, l.created_at
FROM laundry_cycles l
JOIN medical_devices m ON m.id = l.medical_device_id
ORDER BY l.id DESC
";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rows.Add(new LaundryCycleRecord
            {
                Id = reader.GetInt64(0),
                MedicalDeviceId = reader.GetInt64(1),
                DeviceCode = reader.GetString(2),
                CycleCode = reader.GetString(3),
                ProgramName = reader.IsDBNull(4) ? "" : reader.GetString(4),
                Temperature = reader.IsDBNull(5) ? "" : reader.GetString(5),
                WashDate = reader.IsDBNull(6) ? "" : reader.GetString(6),
                OperatorName = reader.IsDBNull(7) ? "" : reader.GetString(7),
                Result = reader.IsDBNull(8) ? "" : reader.GetString(8),
                Notes = reader.IsDBNull(9) ? "" : reader.GetString(9),
                WashCountAfter = reader.GetInt32(10),
                CreatedAt = reader.GetString(11)
            });
        }

        return rows;
    }

    public bool CreateMaintenanceRecord(MaintenanceRecord record, string createdBy, out string error)
    {
        error = "";

        if (record.MedicalDeviceId <= 0)
        {
            error = "Dispositivo medico obbligatorio.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(record.MaintenanceCode))
        {
            error = "Codice manutenzione obbligatorio.";
            return false;
        }

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
INSERT INTO maintenance_records (medical_device_id, maintenance_code, maintenance_type, fault_description, action_taken, parts_replaced, result, operator_name, maintenance_date, notes, created_at)
VALUES ($deviceId, $code, $type, $fault, $action, $parts, $result, $operator, $date, $notes, $createdAt)
";
            command.Parameters.AddWithValue("$deviceId", record.MedicalDeviceId);
            command.Parameters.AddWithValue("$code", record.MaintenanceCode.Trim());
            command.Parameters.AddWithValue("$type", record.MaintenanceType.Trim());
            command.Parameters.AddWithValue("$fault", record.FaultDescription.Trim());
            command.Parameters.AddWithValue("$action", record.ActionTaken.Trim());
            command.Parameters.AddWithValue("$parts", record.PartsReplaced.Trim());
            command.Parameters.AddWithValue("$result", record.Result.Trim());
            command.Parameters.AddWithValue("$operator", record.OperatorName.Trim());
            command.Parameters.AddWithValue("$date", record.MaintenanceDate.Trim());
            command.Parameters.AddWithValue("$notes", record.Notes.Trim());
            command.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("O"));
            command.ExecuteNonQuery();

            var device = GetMedicalDeviceById(record.MedicalDeviceId);
            if (device is not null)
            {
                var status = record.Result == "Rientro in servizio" ? "In servizio" :
                             record.Result == "Fuori servizio" ? "Fuori servizio" :
                             "Manutenzione";
                AddWorkflowEvent("DispositivoMedico", device.Id, device.DeviceCode, device.Status, status, "MAINTENANCE_CREATED", $"Manutenzione {record.MaintenanceCode}: {record.Result}", createdBy);
                ChangeMedicalDeviceStatus(device.Id, status, $"Manutenzione {record.MaintenanceCode}", createdBy);
            }

            WriteAudit(createdBy, "MAINTENANCE_CREATED", $"Maintenance={record.MaintenanceCode}; DeviceId={record.MedicalDeviceId}");
            return true;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            error = "Codice manutenzione già esistente.";
            return false;
        }
    }

    public List<MaintenanceRecord> GetMaintenanceRecords()
    {
        var rows = new List<MaintenanceRecord>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT r.id, r.medical_device_id, m.device_code, r.maintenance_code, r.maintenance_type,
       r.fault_description, r.action_taken, r.parts_replaced, r.result, r.operator_name,
       r.maintenance_date, r.notes, r.created_at
FROM maintenance_records r
JOIN medical_devices m ON m.id = r.medical_device_id
ORDER BY r.id DESC
";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rows.Add(new MaintenanceRecord
            {
                Id = reader.GetInt64(0),
                MedicalDeviceId = reader.GetInt64(1),
                DeviceCode = reader.GetString(2),
                MaintenanceCode = reader.GetString(3),
                MaintenanceType = reader.IsDBNull(4) ? "" : reader.GetString(4),
                FaultDescription = reader.IsDBNull(5) ? "" : reader.GetString(5),
                ActionTaken = reader.IsDBNull(6) ? "" : reader.GetString(6),
                PartsReplaced = reader.IsDBNull(7) ? "" : reader.GetString(7),
                Result = reader.IsDBNull(8) ? "" : reader.GetString(8),
                OperatorName = reader.IsDBNull(9) ? "" : reader.GetString(9),
                MaintenanceDate = reader.IsDBNull(10) ? "" : reader.GetString(10),
                Notes = reader.IsDBNull(11) ? "" : reader.GetString(11),
                CreatedAt = reader.GetString(12)
            });
        }

        return rows;
    }


    public List<DocumentRecord> GetDocuments(string? search = null)
    {
        var rows = new List<DocumentRecord>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT id, document_code, title, category, entity_type, entity_id, entity_code,
       file_name, file_path, version, status, created_by, created_at, notes
FROM documents
WHERE (
    $search = ''
    OR document_code LIKE $like
    OR title LIKE $like
    OR category LIKE $like
    OR entity_type LIKE $like
    OR entity_code LIKE $like
    OR file_name LIKE $like
    OR status LIKE $like
)
ORDER BY id DESC
";
        var q = search?.Trim() ?? "";
        command.Parameters.AddWithValue("$search", q);
        command.Parameters.AddWithValue("$like", $"%{q}%");

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rows.Add(new DocumentRecord
            {
                Id = reader.GetInt64(0),
                DocumentCode = reader.GetString(1),
                Title = reader.GetString(2),
                Category = reader.IsDBNull(3) ? "" : reader.GetString(3),
                EntityType = reader.IsDBNull(4) ? "" : reader.GetString(4),
                EntityId = reader.IsDBNull(5) ? null : reader.GetInt64(5),
                EntityCode = reader.IsDBNull(6) ? "" : reader.GetString(6),
                FileName = reader.IsDBNull(7) ? "" : reader.GetString(7),
                FilePath = reader.IsDBNull(8) ? "" : reader.GetString(8),
                Version = reader.GetString(9),
                Status = reader.GetString(10),
                CreatedBy = reader.IsDBNull(11) ? "" : reader.GetString(11),
                CreatedAt = reader.GetString(12),
                Notes = reader.IsDBNull(13) ? "" : reader.GetString(13)
            });
        }

        return rows;
    }

    public bool CreateDocument(DocumentRecord doc, string createdBy, out string error)
    {
        error = "";

        if (string.IsNullOrWhiteSpace(doc.DocumentCode))
        {
            error = "Codice documento obbligatorio.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(doc.Title))
        {
            error = "Titolo documento obbligatorio.";
            return false;
        }

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
INSERT INTO documents (document_code, title, category, entity_type, entity_id, entity_code, file_name, file_path, version, status, created_by, created_at, notes)
VALUES ($code, $title, $category, $entityType, $entityId, $entityCode, $fileName, $filePath, $version, $status, $createdBy, $createdAt, $notes)
";
            command.Parameters.AddWithValue("$code", doc.DocumentCode.Trim());
            command.Parameters.AddWithValue("$title", doc.Title.Trim());
            command.Parameters.AddWithValue("$category", doc.Category.Trim());
            command.Parameters.AddWithValue("$entityType", doc.EntityType.Trim());
            command.Parameters.AddWithValue("$entityId", doc.EntityId.HasValue ? doc.EntityId.Value : DBNull.Value);
            command.Parameters.AddWithValue("$entityCode", doc.EntityCode.Trim());
            command.Parameters.AddWithValue("$fileName", doc.FileName.Trim());
            command.Parameters.AddWithValue("$filePath", doc.FilePath.Trim());
            command.Parameters.AddWithValue("$version", string.IsNullOrWhiteSpace(doc.Version) ? "1.0" : doc.Version.Trim());
            command.Parameters.AddWithValue("$status", string.IsNullOrWhiteSpace(doc.Status) ? "Attivo" : doc.Status.Trim());
            command.Parameters.AddWithValue("$createdBy", createdBy);
            command.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("O"));
            command.Parameters.AddWithValue("$notes", doc.Notes.Trim());
            command.ExecuteNonQuery();

            if (doc.EntityType == "DispositivoMedico" && doc.EntityId.HasValue)
            {
                AddWorkflowEvent("DispositivoMedico", doc.EntityId.Value, doc.EntityCode, "", "Documento allegato", "DOCUMENT_ATTACHED", $"{doc.DocumentCode} - {doc.Title}", createdBy);
            }

            WriteAudit(createdBy, "DOCUMENT_CREATED", $"Documento {doc.DocumentCode}; Entity={doc.EntityType}:{doc.EntityCode}");
            return true;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            error = "Codice documento già esistente.";
            return false;
        }
    }

    public void ArchiveDocument(long documentId, string changedBy)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE documents SET status = 'Archiviato' WHERE id = $id";
        command.Parameters.AddWithValue("$id", documentId);
        command.ExecuteNonQuery();

        WriteAudit(changedBy, "DOCUMENT_ARCHIVED", $"DocumentId={documentId}");
    }

    public string GenerateDocumentTxt(DocumentRecord doc, string generatedBy)
    {
        var documentsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Accyourate Enterprise X",
            "documents");

        Directory.CreateDirectory(documentsDir);

        var safeCode = string.Join("_", doc.DocumentCode.Split(Path.GetInvalidFileNameChars()));
        var path = Path.Combine(documentsDir, $"{safeCode}_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

        File.WriteAllText(path,
$@"ACCYOURATE ENTERPRISE X
DOCUMENTO GENERATO

Codice: {doc.DocumentCode}
Titolo: {doc.Title}
Categoria: {doc.Category}
Collegato a: {doc.EntityType} {doc.EntityCode}
Versione: {doc.Version}
Stato: {doc.Status}
Creato da: {generatedBy}
Data: {DateTime.Now:yyyy-MM-dd HH:mm:ss}

Note:
{doc.Notes}
");

        WriteAudit(generatedBy, "DOCUMENT_FILE_GENERATED", path);
        return path;
    }


    public int CountTable(string tableName)
    {
        var allowed = new HashSet<string>
        {
            "employees", "assets", "medical_devices", "production_orders", "quality_tests",
            "warehouse_locations", "stock_movements", "shipments", "laundry_cycles",
            "maintenance_records", "documents", "workflow_events", "audit_logs"
        };

        if (!allowed.Contains(tableName))
            return 0;

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {tableName}";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public List<DashboardMetricRecord> GetDashboardMetrics()
    {
        return new List<DashboardMetricRecord>
        {
            new DashboardMetricRecord { Title = "Persone", Value = CountTable("employees").ToString(), Description = "Dipendenti e collaboratori" },
            new DashboardMetricRecord { Title = "Asset IT", Value = CountTable("assets").ToString(), Description = "Dispositivi informatici" },
            new DashboardMetricRecord { Title = "Dispositivi Medici", Value = CountTable("medical_devices").ToString(), Description = "Digital Twin gestiti" },
            new DashboardMetricRecord { Title = "Produzione", Value = CountTable("production_orders").ToString(), Description = "Ordini produzione" },
            new DashboardMetricRecord { Title = "Qualità", Value = CountTable("quality_tests").ToString(), Description = "Test registrati" },
            new DashboardMetricRecord { Title = "Magazzino", Value = CountTable("stock_movements").ToString(), Description = "Movimentazioni" },
            new DashboardMetricRecord { Title = "Lavaggi", Value = CountTable("laundry_cycles").ToString(), Description = "Cicli lavaggio" },
            new DashboardMetricRecord { Title = "Documenti", Value = CountTable("documents").ToString(), Description = "Archivio documentale" }
        };
    }

    public List<GlobalSearchResultRecord> GlobalSearch(string? search)
    {
        var q = search?.Trim() ?? "";
        var rows = new List<GlobalSearchResultRecord>();

        if (string.IsNullOrWhiteSpace(q))
            return rows;

        foreach (var e in GetEmployees(q, true).Take(10))
        {
            rows.Add(new GlobalSearchResultRecord
            {
                Area = "Persone",
                Code = e.EmployeeCode,
                Title = e.FullName,
                Description = $"{e.Department} - {e.JobTitle}"
            });
        }

        foreach (var a in GetAssets(q, true).Take(10))
        {
            rows.Add(new GlobalSearchResultRecord
            {
                Area = "Asset IT",
                Code = a.AssetCode,
                Title = $"{a.Category} {a.Brand} {a.Model}",
                Description = $"{a.SerialNumber} - {a.Status}"
            });
        }

        foreach (var d in GetMedicalDevices(q, true).Take(10))
        {
            rows.Add(new GlobalSearchResultRecord
            {
                Area = "Medical",
                Code = d.DeviceCode,
                Title = $"{d.DeviceType} {d.Model}",
                Description = $"{d.SerialNumber} - {d.Status}"
            });
        }

        foreach (var doc in GetDocuments(q).Take(10))
        {
            rows.Add(new GlobalSearchResultRecord
            {
                Area = "Documenti",
                Code = doc.DocumentCode,
                Title = doc.Title,
                Description = $"{doc.Category} - {doc.EntityType} {doc.EntityCode}"
            });
        }

        return rows.Take(30).ToList();
    }



    public List<AnalyticsChartPointRecord> GetMedicalDeviceStatusChart()
    {
        var rows = new List<AnalyticsChartPointRecord>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT status, COUNT(*)
FROM medical_devices
GROUP BY status
ORDER BY COUNT(*) DESC
";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rows.Add(new AnalyticsChartPointRecord
            {
                Label = reader.IsDBNull(0) ? "N/D" : reader.GetString(0),
                Value = reader.GetInt32(1)
            });
        }

        return rows;
    }

    public List<AnalyticsChartPointRecord> GetOperationalVolumeChart()
    {
        return new List<AnalyticsChartPointRecord>
        {
            new AnalyticsChartPointRecord { Label = "Produzione", Value = CountTable("production_orders") },
            new AnalyticsChartPointRecord { Label = "Qualità", Value = CountTable("quality_tests") },
            new AnalyticsChartPointRecord { Label = "Magazzino", Value = CountTable("stock_movements") },
            new AnalyticsChartPointRecord { Label = "Lavaggi", Value = CountTable("laundry_cycles") },
            new AnalyticsChartPointRecord { Label = "Manutenzioni", Value = CountTable("maintenance_records") },
            new AnalyticsChartPointRecord { Label = "Documenti", Value = CountTable("documents") }
        };
    }

    public List<AnalyticsKpiRecord> GetAnalyticsKpis()
    {
        var kpis = new List<AnalyticsKpiRecord>
        {
            new AnalyticsKpiRecord { Code = "people", Title = "Persone", Value = CountTable("employees").ToString(), Subtitle = "Dipendenti e collaboratori", Area = "HR" },
            new AnalyticsKpiRecord { Code = "assets", Title = "Asset IT", Value = CountTable("assets").ToString(), Subtitle = "Dispositivi informatici", Area = "IT" },
            new AnalyticsKpiRecord { Code = "medical", Title = "Dispositivi Medici", Value = CountTable("medical_devices").ToString(), Subtitle = "Digital Twin attivi", Area = "Medical" },
            new AnalyticsKpiRecord { Code = "production", Title = "Produzione", Value = CountTable("production_orders").ToString(), Subtitle = "Ordini produzione", Area = "Medical" },
            new AnalyticsKpiRecord { Code = "quality", Title = "Qualità", Value = CountTable("quality_tests").ToString(), Subtitle = "Test qualità registrati", Area = "Medical" },
            new AnalyticsKpiRecord { Code = "warehouse", Title = "Magazzino", Value = CountTable("stock_movements").ToString(), Subtitle = "Movimentazioni", Area = "Logistica" },
            new AnalyticsKpiRecord { Code = "laundry", Title = "Lavaggi", Value = CountTable("laundry_cycles").ToString(), Subtitle = "Cicli lavaggio", Area = "Assistenza" },
            new AnalyticsKpiRecord { Code = "maintenance", Title = "Manutenzioni", Value = CountTable("maintenance_records").ToString(), Subtitle = "Interventi registrati", Area = "Assistenza" },
            new AnalyticsKpiRecord { Code = "documents", Title = "Documenti", Value = CountTable("documents").ToString(), Subtitle = "Archivio documentale", Area = "Documentale" },
            new AnalyticsKpiRecord { Code = "workflow", Title = "Digital Twin Events", Value = CountTable("workflow_events").ToString(), Subtitle = "Eventi tracciati", Area = "Core" }
        };

        return kpis;
    }

    public List<AnalyticsNotificationRecord> GetAnalyticsNotifications()
    {
        var notifications = new List<AnalyticsNotificationRecord>();

        var nonConform = CountMedicalDevicesByStatus("Non Conforme");
        if (nonConform > 0)
        {
            notifications.Add(new AnalyticsNotificationRecord
            {
                Severity = "Attenzione",
                Title = "Dispositivi non conformi",
                Message = $"{nonConform} dispositivo/i risultano non conformi.",
                Source = "Qualità"
            });
        }

        var outOfService = CountMedicalDevicesByStatus("Fuori servizio");
        if (outOfService > 0)
        {
            notifications.Add(new AnalyticsNotificationRecord
            {
                Severity = "Critico",
                Title = "Dispositivi fuori servizio",
                Message = $"{outOfService} dispositivo/i sono fuori servizio.",
                Source = "Assistenza"
            });
        }

        var archivedDocs = CountDocumentsByStatus("Archiviato");
        if (archivedDocs > 0)
        {
            notifications.Add(new AnalyticsNotificationRecord
            {
                Severity = "Info",
                Title = "Documenti archiviati",
                Message = $"{archivedDocs} documento/i risultano archiviati.",
                Source = "Documentale"
            });
        }

        if (notifications.Count == 0)
        {
            notifications.Add(new AnalyticsNotificationRecord
            {
                Severity = "OK",
                Title = "Nessuna criticità",
                Message = "Non sono presenti notifiche operative critiche.",
                Source = "Sistema"
            });
        }

        return notifications;
    }

    public int CountMedicalDevicesByStatus(string status)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM medical_devices WHERE status = $status";
        command.Parameters.AddWithValue("$status", status);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public int CountDocumentsByStatus(string status)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM documents WHERE status = $status";
        command.Parameters.AddWithValue("$status", status);
        return Convert.ToInt32(command.ExecuteScalar());
    }


    public string GetAppSettingValue(string key, string defaultValue = "")
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT value FROM app_settings WHERE key = $key LIMIT 1";
        command.Parameters.AddWithValue("$key", key);

        var result = command.ExecuteScalar();
        return result?.ToString() ?? defaultValue;
    }

    public void SetAppSettingValue(string key, string value, string groupName = "UI")
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO app_settings (key, value, group_name)
VALUES ($key, $value, $group)
ON CONFLICT(key) DO UPDATE SET value = excluded.value, group_name = excluded.group_name
";
        command.Parameters.AddWithValue("$key", key);
        command.Parameters.AddWithValue("$value", value);
        command.Parameters.AddWithValue("$group", groupName);
        command.ExecuteNonQuery();
    }

    public ThemePreferenceRecord GetThemePreferences()
    {
        return new ThemePreferenceRecord
        {
            CompanyName = GetAppSettingValue("ui.company_name", "Accyourate Group"),
            ThemeMode = GetAppSettingValue("ui.theme_mode", "Chiaro"),
            PrimaryColor = GetAppSettingValue("ui.primary_color", "#B5162B"),
            SidebarColor = GetAppSettingValue("ui.sidebar_color", "#111827"),
            WorkspaceColor = GetAppSettingValue("ui.workspace_color", "#F7F7F6"),
            MenuStyle = GetAppSettingValue("ui.menu_style", "Collassabile"),
            LogoPath = GetAppSettingValue("ui.logo_path", ""),
            MenuItemColor = GetAppSettingValue("ui.menu_item_color", "#111827"),
            MenuItemTextColor = GetAppSettingValue("ui.menu_item_text_color", "#FFFFFF"),
            MenuHoverColor = GetAppSettingValue("ui.menu_hover_color", "#374151"),
            MenuHoverTextColor = GetAppSettingValue("ui.menu_hover_text_color", "#FFFFFF"),
            MenuSelectedColor = GetAppSettingValue("ui.menu_selected_color", "#B5162B"),
            MenuSelectedTextColor = GetAppSettingValue("ui.menu_selected_text_color", "#FFFFFF")
        };
    }

    public void SaveThemePreferences(ThemePreferenceRecord preferences, string changedBy)
    {
        SetAppSettingValue("ui.company_name", preferences.CompanyName, "UI");
        SetAppSettingValue("ui.theme_mode", preferences.ThemeMode, "UI");
        SetAppSettingValue("ui.primary_color", preferences.PrimaryColor, "UI");
        SetAppSettingValue("ui.sidebar_color", preferences.SidebarColor, "UI");
        SetAppSettingValue("ui.workspace_color", preferences.WorkspaceColor, "UI");
        SetAppSettingValue("ui.menu_style", preferences.MenuStyle, "UI");
        SetAppSettingValue("ui.logo_path", preferences.LogoPath, "UI");
        SetAppSettingValue("ui.menu_item_color", preferences.MenuItemColor, "UI");
        SetAppSettingValue("ui.menu_item_text_color", preferences.MenuItemTextColor, "UI");
        SetAppSettingValue("ui.menu_hover_color", preferences.MenuHoverColor, "UI");
        SetAppSettingValue("ui.menu_hover_text_color", preferences.MenuHoverTextColor, "UI");
        SetAppSettingValue("ui.menu_selected_color", preferences.MenuSelectedColor, "UI");
        SetAppSettingValue("ui.menu_selected_text_color", preferences.MenuSelectedTextColor, "UI");

        WriteAudit(changedBy, "THEME_PREFERENCES_UPDATED", $"Tema={preferences.ThemeMode}; Primary={preferences.PrimaryColor}; Hover={preferences.MenuHoverColor}");
    }


    public BrandingPreferenceRecord GetBrandingPreferences()
    {
        return new BrandingPreferenceRecord
        {
            CompanyName = GetAppSettingValue("branding.company_name", "Accyourate Group"),
            ProductTitle = GetAppSettingValue("branding.product_title", "Accyourate Enterprise X"),
            HeroTitle = GetAppSettingValue("branding.hero_title", "Accyourate Enterprise X"),
            HeroSubtitle = GetAppSettingValue("branding.hero_subtitle", "La piattaforma integrata per aziende che guardano avanti."),
            HeroImagePath = GetAppSettingValue("branding.hero_image_path", ""),
            LogoPath = GetAppSettingValue("branding.logo_path", ""),
            IndustryLabel = GetAppSettingValue("branding.industry_label", "Medical Textile Suite"),
            Feature1Title = GetAppSettingValue("branding.feature1_title", "Gestione completa"),
            Feature1Text = GetAppSettingValue("branding.feature1_text", "Moduli integrati per ogni area aziendale"),
            Feature2Title = GetAppSettingValue("branding.feature2_title", "Sicurezza e conformità"),
            Feature2Text = GetAppSettingValue("branding.feature2_text", "Protezione dei dati e conformità normativa"),
            Feature3Title = GetAppSettingValue("branding.feature3_title", "Analytics avanzata"),
            Feature3Text = GetAppSettingValue("branding.feature3_text", "Dati, KPI e report per decisioni migliori"),
            Feature4Title = GetAppSettingValue("branding.feature4_title", "Innovazione continua"),
            Feature4Text = GetAppSettingValue("branding.feature4_text", "Tecnologia all'avanguardia per il tuo business")
        };
    }

    public void SaveBrandingPreferences(BrandingPreferenceRecord branding, string changedBy)
    {
        SetAppSettingValue("branding.company_name", branding.CompanyName, "Branding");
        SetAppSettingValue("branding.product_title", branding.ProductTitle, "Branding");
        SetAppSettingValue("branding.hero_title", branding.HeroTitle, "Branding");
        SetAppSettingValue("branding.hero_subtitle", branding.HeroSubtitle, "Branding");
        SetAppSettingValue("branding.hero_image_path", branding.HeroImagePath, "Branding");
        SetAppSettingValue("branding.logo_path", branding.LogoPath, "Branding");
        SetAppSettingValue("branding.industry_label", branding.IndustryLabel, "Branding");
        SetAppSettingValue("branding.feature1_title", branding.Feature1Title, "Branding");
        SetAppSettingValue("branding.feature1_text", branding.Feature1Text, "Branding");
        SetAppSettingValue("branding.feature2_title", branding.Feature2Title, "Branding");
        SetAppSettingValue("branding.feature2_text", branding.Feature2Text, "Branding");
        SetAppSettingValue("branding.feature3_title", branding.Feature3Title, "Branding");
        SetAppSettingValue("branding.feature3_text", branding.Feature3Text, "Branding");
        SetAppSettingValue("branding.feature4_title", branding.Feature4Title, "Branding");
        SetAppSettingValue("branding.feature4_text", branding.Feature4Text, "Branding");

        WriteAudit(changedBy, "BRANDING_UPDATED", $"Company={branding.CompanyName}; Industry={branding.IndustryLabel}");
    }

    public List<MedicalDeviceRecord> GetMedicalDevices(string? search = null, bool includeArchived = false)
    {
        var rows = new List<MedicalDeviceRecord>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT id, device_code, device_type, model, serial_number, lot_number, rfid_code, qr_code, status,
       production_date, test_date, notes, is_archived, created_at
FROM medical_devices
WHERE ($includeArchived = 1 OR is_archived = 0)
  AND (
      $search = ''
      OR device_code LIKE $like
      OR device_type LIKE $like
      OR model LIKE $like
      OR serial_number LIKE $like
      OR lot_number LIKE $like
      OR rfid_code LIKE $like
      OR status LIKE $like
  )
ORDER BY device_code
";
        var q = search?.Trim() ?? "";
        command.Parameters.AddWithValue("$includeArchived", includeArchived ? 1 : 0);
        command.Parameters.AddWithValue("$search", q);
        command.Parameters.AddWithValue("$like", $"%{q}%");

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rows.Add(new MedicalDeviceRecord
            {
                Id = reader.GetInt64(0),
                DeviceCode = reader.GetString(1),
                DeviceType = reader.GetString(2),
                Model = reader.IsDBNull(3) ? "" : reader.GetString(3),
                SerialNumber = reader.IsDBNull(4) ? "" : reader.GetString(4),
                LotNumber = reader.IsDBNull(5) ? "" : reader.GetString(5),
                RfidCode = reader.IsDBNull(6) ? "" : reader.GetString(6),
                QrCode = reader.IsDBNull(7) ? "" : reader.GetString(7),
                Status = reader.GetString(8),
                ProductionDate = reader.IsDBNull(9) ? "" : reader.GetString(9),
                TestDate = reader.IsDBNull(10) ? "" : reader.GetString(10),
                Notes = reader.IsDBNull(11) ? "" : reader.GetString(11),
                IsArchived = reader.GetInt32(12) == 1,
                CreatedAt = reader.GetString(13)
            });
        }

        return rows;
    }

    public bool CreateMedicalDevice(MedicalDeviceRecord device, string createdBy, out string error)
    {
        error = "";

        if (string.IsNullOrWhiteSpace(device.DeviceCode))
        {
            error = "Codice dispositivo obbligatorio.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(device.DeviceType))
        {
            error = "Tipo dispositivo obbligatorio.";
            return false;
        }

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
INSERT INTO medical_devices (device_code, device_type, model, serial_number, lot_number, rfid_code, qr_code, status, production_date, test_date, notes, is_archived, created_at)
VALUES ($code, $type, $model, $serial, $lot, $rfid, $qr, $status, $productionDate, $testDate, $notes, 0, $createdAt)
";
            command.Parameters.AddWithValue("$code", device.DeviceCode.Trim());
            command.Parameters.AddWithValue("$type", device.DeviceType.Trim());
            command.Parameters.AddWithValue("$model", device.Model.Trim());
            command.Parameters.AddWithValue("$serial", device.SerialNumber.Trim());
            command.Parameters.AddWithValue("$lot", device.LotNumber.Trim());
            command.Parameters.AddWithValue("$rfid", device.RfidCode.Trim());
            command.Parameters.AddWithValue("$qr", string.IsNullOrWhiteSpace(device.QrCode) ? device.DeviceCode.Trim() : device.QrCode.Trim());
            command.Parameters.AddWithValue("$status", device.Status.Trim());
            command.Parameters.AddWithValue("$productionDate", device.ProductionDate.Trim());
            command.Parameters.AddWithValue("$testDate", device.TestDate.Trim());
            command.Parameters.AddWithValue("$notes", device.Notes.Trim());
            command.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("O"));
            command.ExecuteNonQuery();

            
            long id;
            using (var idCommand = connection.CreateCommand())
            {
                idCommand.CommandText = "SELECT last_insert_rowid()";
                id = Convert.ToInt64(idCommand.ExecuteScalar());
            }
            AddWorkflowEvent("DispositivoMedico", id, device.DeviceCode.Trim(), "", device.Status.Trim(), "DEVICE_CREATED", "Creazione dispositivo medico", createdBy);
            WriteAudit(createdBy, "MEDICAL_DEVICE_CREATED", $"Dispositivo {device.DeviceCode} - {device.DeviceType}");
            return true;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            error = "Codice dispositivo già esistente.";
            return false;
        }
    }

    public void ArchiveMedicalDevice(long deviceId, bool archived, string changedBy)
    {
        var device = GetMedicalDeviceById(deviceId);
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE medical_devices SET is_archived = $archived WHERE id = $id";
        command.Parameters.AddWithValue("$archived", archived ? 1 : 0);
        command.Parameters.AddWithValue("$id", deviceId);
        command.ExecuteNonQuery();

        if (device is not null)
            AddWorkflowEvent("DispositivoMedico", deviceId, device.DeviceCode, device.Status, archived ? "Archiviato" : "Ripristinato", archived ? "DEVICE_ARCHIVED" : "DEVICE_RESTORED", "", changedBy);

        WriteAudit(changedBy, archived ? "MEDICAL_DEVICE_ARCHIVED" : "MEDICAL_DEVICE_RESTORED", $"DeviceId={deviceId}");
    }

    public MedicalDeviceRecord? GetMedicalDeviceById(long deviceId)
    {
        return GetMedicalDevices(null, true).FirstOrDefault(x => x.Id == deviceId);
    }

    public void ChangeMedicalDeviceStatus(long deviceId, string toStatus, string notes, string changedBy)
    {
        var device = GetMedicalDeviceById(deviceId);
        if (device is null)
            return;

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE medical_devices SET status = $status WHERE id = $id";
        command.Parameters.AddWithValue("$status", toStatus);
        command.Parameters.AddWithValue("$id", deviceId);
        command.ExecuteNonQuery();

        AddWorkflowEvent("DispositivoMedico", deviceId, device.DeviceCode, device.Status, toStatus, "STATUS_CHANGED", notes, changedBy);
    }

    public bool CreateControlUnit(ControlUnitRecord cu, string createdBy, out string error)
    {
        error = "";

        if (cu.MedicalDeviceId <= 0)
        {
            error = "Dispositivo medico obbligatorio.";
            return false;
        }

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO control_units (medical_device_id, firmware_version, hardware_revision, mac_address, battery_status, last_functional_test_date, last_functional_test_result, notes)
VALUES ($deviceId, $firmware, $hardware, $mac, $battery, $testDate, $testResult, $notes)
";
        command.Parameters.AddWithValue("$deviceId", cu.MedicalDeviceId);
        command.Parameters.AddWithValue("$firmware", cu.FirmwareVersion.Trim());
        command.Parameters.AddWithValue("$hardware", cu.HardwareRevision.Trim());
        command.Parameters.AddWithValue("$mac", cu.MacAddress.Trim());
        command.Parameters.AddWithValue("$battery", cu.BatteryStatus.Trim());
        command.Parameters.AddWithValue("$testDate", cu.LastFunctionalTestDate.Trim());
        command.Parameters.AddWithValue("$testResult", cu.LastFunctionalTestResult.Trim());
        command.Parameters.AddWithValue("$notes", cu.Notes.Trim());
        command.ExecuteNonQuery();

        var device = GetMedicalDeviceById(cu.MedicalDeviceId);
        if (device is not null)
            AddWorkflowEvent("ControlUnit", cu.MedicalDeviceId, device.DeviceCode, "", "Creata", "CONTROL_UNIT_CREATED", $"Firmware {cu.FirmwareVersion}", createdBy);

        return true;
    }

    public List<ControlUnitRecord> GetControlUnits()
    {
        var rows = new List<ControlUnitRecord>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT c.id, c.medical_device_id, m.device_code, c.firmware_version, c.hardware_revision, c.mac_address,
       c.battery_status, c.last_functional_test_date, c.last_functional_test_result, c.notes
FROM control_units c
JOIN medical_devices m ON m.id = c.medical_device_id
ORDER BY m.device_code
";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rows.Add(new ControlUnitRecord
            {
                Id = reader.GetInt64(0),
                MedicalDeviceId = reader.GetInt64(1),
                DeviceCode = reader.GetString(2),
                FirmwareVersion = reader.IsDBNull(3) ? "" : reader.GetString(3),
                HardwareRevision = reader.IsDBNull(4) ? "" : reader.GetString(4),
                MacAddress = reader.IsDBNull(5) ? "" : reader.GetString(5),
                BatteryStatus = reader.IsDBNull(6) ? "" : reader.GetString(6),
                LastFunctionalTestDate = reader.IsDBNull(7) ? "" : reader.GetString(7),
                LastFunctionalTestResult = reader.IsDBNull(8) ? "" : reader.GetString(8),
                Notes = reader.IsDBNull(9) ? "" : reader.GetString(9)
            });
        }

        return rows;
    }

    public bool CreateTextileItem(TextileItemRecord item, string createdBy, out string error)
    {
        error = "";

        if (item.MedicalDeviceId <= 0)
        {
            error = "Dispositivo medico obbligatorio.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(item.TextileType))
        {
            error = "Tipo capo obbligatorio.";
            return false;
        }

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO textile_items (medical_device_id, textile_type, size, color, lot_number, rfid_code, wash_count, last_functional_test_date, last_functional_test_result, conformity_status, notes)
VALUES ($deviceId, $type, $size, $color, $lot, $rfid, $washCount, $testDate, $testResult, $conformity, $notes)
";
        command.Parameters.AddWithValue("$deviceId", item.MedicalDeviceId);
        command.Parameters.AddWithValue("$type", item.TextileType.Trim());
        command.Parameters.AddWithValue("$size", item.Size.Trim());
        command.Parameters.AddWithValue("$color", item.Color.Trim());
        command.Parameters.AddWithValue("$lot", item.LotNumber.Trim());
        command.Parameters.AddWithValue("$rfid", item.RfidCode.Trim());
        command.Parameters.AddWithValue("$washCount", item.WashCount);
        command.Parameters.AddWithValue("$testDate", item.LastFunctionalTestDate.Trim());
        command.Parameters.AddWithValue("$testResult", item.LastFunctionalTestResult.Trim());
        command.Parameters.AddWithValue("$conformity", item.ConformityStatus.Trim());
        command.Parameters.AddWithValue("$notes", item.Notes.Trim());
        command.ExecuteNonQuery();

        var device = GetMedicalDeviceById(item.MedicalDeviceId);
        if (device is not null)
            AddWorkflowEvent("CapoTessile", item.MedicalDeviceId, device.DeviceCode, "", "Creato", "TEXTILE_CREATED", $"{item.TextileType} {item.Size}", createdBy);

        return true;
    }

    public List<TextileItemRecord> GetTextileItems()
    {
        var rows = new List<TextileItemRecord>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT t.id, t.medical_device_id, m.device_code, t.textile_type, t.size, t.color, t.lot_number, t.rfid_code,
       t.wash_count, t.last_functional_test_date, t.last_functional_test_result, t.conformity_status, t.notes
FROM textile_items t
JOIN medical_devices m ON m.id = t.medical_device_id
ORDER BY m.device_code
";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rows.Add(new TextileItemRecord
            {
                Id = reader.GetInt64(0),
                MedicalDeviceId = reader.GetInt64(1),
                DeviceCode = reader.GetString(2),
                TextileType = reader.GetString(3),
                Size = reader.IsDBNull(4) ? "" : reader.GetString(4),
                Color = reader.IsDBNull(5) ? "" : reader.GetString(5),
                LotNumber = reader.IsDBNull(6) ? "" : reader.GetString(6),
                RfidCode = reader.IsDBNull(7) ? "" : reader.GetString(7),
                WashCount = reader.GetInt32(8),
                LastFunctionalTestDate = reader.IsDBNull(9) ? "" : reader.GetString(9),
                LastFunctionalTestResult = reader.IsDBNull(10) ? "" : reader.GetString(10),
                ConformityStatus = reader.IsDBNull(11) ? "" : reader.GetString(11),
                Notes = reader.IsDBNull(12) ? "" : reader.GetString(12)
            });
        }

        return rows;
    }

    public void ExportMedicalDevicesCsv(string filePath, string? search, bool includeArchived, string exportedBy)
    {
        var rows = GetMedicalDevices(search, includeArchived);
        using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
        writer.WriteLine("Codice;Tipo;Modello;Seriale;Lotto;RFID;QR;Stato;Produzione;Collaudo;Note;Archivio");

        foreach (var d in rows)
        {
            writer.WriteLine($"{EscapeCsv(d.DeviceCode)};{EscapeCsv(d.DeviceType)};{EscapeCsv(d.Model)};{EscapeCsv(d.SerialNumber)};{EscapeCsv(d.LotNumber)};{EscapeCsv(d.RfidCode)};{EscapeCsv(d.QrCode)};{EscapeCsv(d.Status)};{EscapeCsv(d.ProductionDate)};{EscapeCsv(d.TestDate)};{EscapeCsv(d.Notes)};{(d.IsArchived ? "Archiviato" : "Attivo")}");
        }

        WriteAudit(exportedBy, "MEDICAL_DEVICES_EXPORTED", $"File={filePath}; Count={rows.Count}");
    }

    public List<DatabaseVersionRecord> GetDatabaseVersions()
    {
        var rows = new List<DatabaseVersionRecord>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, version, description, applied_at FROM database_versions ORDER BY id DESC";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rows.Add(new DatabaseVersionRecord
            {
                Id = reader.GetInt64(0),
                Version = reader.GetString(1),
                Description = reader.GetString(2),
                AppliedAt = reader.GetString(3)
            });
        }

        return rows;
    }

    public List<AppSettingRecord> GetSettings()
    {
        var rows = new List<AppSettingRecord>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT key, value, group_name FROM app_settings ORDER BY group_name, key";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rows.Add(new AppSettingRecord
            {
                Key = reader.GetString(0),
                Value = reader.GetString(1),
                GroupName = reader.GetString(2)
            });
        }

        return rows;
    }

    public void AddWorkflowEvent(string entityType, long entityId, string entityCode, string fromStatus, string toStatus, string eventType, string notes, string createdBy)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO workflow_events (entity_type, entity_id, entity_code, from_status, to_status, event_type, notes, created_by, created_at)
VALUES ($entityType, $entityId, $entityCode, $fromStatus, $toStatus, $eventType, $notes, $createdBy, $createdAt)
";
        command.Parameters.AddWithValue("$entityType", entityType);
        command.Parameters.AddWithValue("$entityId", entityId);
        command.Parameters.AddWithValue("$entityCode", entityCode);
        command.Parameters.AddWithValue("$fromStatus", fromStatus);
        command.Parameters.AddWithValue("$toStatus", toStatus);
        command.Parameters.AddWithValue("$eventType", eventType);
        command.Parameters.AddWithValue("$notes", notes);
        command.Parameters.AddWithValue("$createdBy", createdBy);
        command.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("O"));
        command.ExecuteNonQuery();

        WriteAudit(createdBy, "WORKFLOW_EVENT", $"{entityType}:{entityCode} {fromStatus}->{toStatus}");
    }

    public List<WorkflowEventRecord> GetWorkflowEvents(string? entityType = null, string? search = null, int limit = 200)
    {
        var rows = new List<WorkflowEventRecord>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT id, entity_type, entity_id, entity_code, from_status, to_status, event_type, notes, created_by, created_at
FROM workflow_events
WHERE ($entityType = '' OR entity_type = $entityType)
  AND (
      $search = ''
      OR entity_code LIKE $like
      OR from_status LIKE $like
      OR to_status LIKE $like
      OR event_type LIKE $like
      OR notes LIKE $like
      OR created_by LIKE $like
  )
ORDER BY id DESC
LIMIT $limit
";
        var q = search?.Trim() ?? "";
        command.Parameters.AddWithValue("$entityType", entityType ?? "");
        command.Parameters.AddWithValue("$search", q);
        command.Parameters.AddWithValue("$like", $"%{q}%");
        command.Parameters.AddWithValue("$limit", limit);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rows.Add(new WorkflowEventRecord
            {
                Id = reader.GetInt64(0),
                EntityType = reader.GetString(1),
                EntityId = reader.GetInt64(2),
                EntityCode = reader.GetString(3),
                FromStatus = reader.IsDBNull(4) ? "" : reader.GetString(4),
                ToStatus = reader.GetString(5),
                EventType = reader.GetString(6),
                Notes = reader.IsDBNull(7) ? "" : reader.GetString(7),
                CreatedBy = reader.GetString(8),
                CreatedAt = reader.GetString(9)
            });
        }

        return rows;
    }

    public void ChangeAssetStatus(long assetId, string toStatus, string notes, string changedBy)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        string assetCode = "";
        string fromStatus = "";

        using (var select = connection.CreateCommand())
        {
            select.CommandText = "SELECT asset_code, status FROM assets WHERE id = $id";
            select.Parameters.AddWithValue("$id", assetId);
            using var reader = select.ExecuteReader();
            if (reader.Read())
            {
                assetCode = reader.GetString(0);
                fromStatus = reader.GetString(1);
            }
        }

        using (var update = connection.CreateCommand())
        {
            update.CommandText = "UPDATE assets SET status = $status WHERE id = $id";
            update.Parameters.AddWithValue("$status", toStatus);
            update.Parameters.AddWithValue("$id", assetId);
            update.ExecuteNonQuery();
        }

        AddWorkflowEvent("AssetIT", assetId, assetCode, fromStatus, toStatus, "STATUS_CHANGED", notes, changedBy);
    }

    public List<AssetRecord> GetAssets(string? search = null, bool includeArchived = false)
    {
        var rows = new List<AssetRecord>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT a.id, a.asset_code, a.category, a.brand, a.model, a.serial_number, a.operating_system,
       a.status, a.assigned_employee_id,
       COALESCE(e.first_name || ' ' || e.last_name, '') AS assigned_employee_name,
       a.purchase_date, a.warranty_end, a.notes, a.is_archived, a.created_at
FROM assets a
LEFT JOIN employees e ON e.id = a.assigned_employee_id
WHERE ($includeArchived = 1 OR a.is_archived = 0)
  AND (
      $search = ''
      OR a.asset_code LIKE $like
      OR a.category LIKE $like
      OR a.brand LIKE $like
      OR a.model LIKE $like
      OR a.serial_number LIKE $like
      OR a.status LIKE $like
      OR e.first_name LIKE $like
      OR e.last_name LIKE $like
  )
ORDER BY a.asset_code
";
        var q = search?.Trim() ?? "";
        command.Parameters.AddWithValue("$includeArchived", includeArchived ? 1 : 0);
        command.Parameters.AddWithValue("$search", q);
        command.Parameters.AddWithValue("$like", $"%{q}%");

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rows.Add(new AssetRecord
            {
                Id = reader.GetInt64(0),
                AssetCode = reader.GetString(1),
                Category = reader.GetString(2),
                Brand = reader.IsDBNull(3) ? "" : reader.GetString(3),
                Model = reader.IsDBNull(4) ? "" : reader.GetString(4),
                SerialNumber = reader.IsDBNull(5) ? "" : reader.GetString(5),
                OperatingSystem = reader.IsDBNull(6) ? "" : reader.GetString(6),
                Status = reader.GetString(7),
                AssignedEmployeeId = reader.IsDBNull(8) ? null : reader.GetInt64(8),
                AssignedEmployeeName = reader.IsDBNull(9) ? "" : reader.GetString(9),
                PurchaseDate = reader.IsDBNull(10) ? "" : reader.GetString(10),
                WarrantyEnd = reader.IsDBNull(11) ? "" : reader.GetString(11),
                Notes = reader.IsDBNull(12) ? "" : reader.GetString(12),
                IsArchived = reader.GetInt32(13) == 1,
                CreatedAt = reader.GetString(14)
            });
        }

        return rows;
    }

    public bool CreateAsset(AssetRecord asset, string createdBy, out string error)
    {
        error = "";

        if (string.IsNullOrWhiteSpace(asset.AssetCode))
        {
            error = "Codice asset obbligatorio.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(asset.Category))
        {
            error = "Categoria obbligatoria.";
            return false;
        }

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
INSERT INTO assets (asset_code, category, brand, model, serial_number, operating_system, status, assigned_employee_id, purchase_date, warranty_end, notes, is_archived, created_at)
VALUES ($assetCode, $category, $brand, $model, $serial, $os, $status, $employeeId, $purchaseDate, $warrantyEnd, $notes, 0, $createdAt)
";
            command.Parameters.AddWithValue("$assetCode", asset.AssetCode.Trim());
            command.Parameters.AddWithValue("$category", asset.Category.Trim());
            command.Parameters.AddWithValue("$brand", asset.Brand.Trim());
            command.Parameters.AddWithValue("$model", asset.Model.Trim());
            command.Parameters.AddWithValue("$serial", asset.SerialNumber.Trim());
            command.Parameters.AddWithValue("$os", asset.OperatingSystem.Trim());
            command.Parameters.AddWithValue("$status", asset.Status.Trim());
            command.Parameters.AddWithValue("$employeeId", asset.AssignedEmployeeId.HasValue ? asset.AssignedEmployeeId.Value : DBNull.Value);
            command.Parameters.AddWithValue("$purchaseDate", asset.PurchaseDate.Trim());
            command.Parameters.AddWithValue("$warrantyEnd", asset.WarrantyEnd.Trim());
            command.Parameters.AddWithValue("$notes", asset.Notes.Trim());
            command.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("O"));
            command.ExecuteNonQuery();

            WriteAudit(createdBy, "ASSET_CREATED", $"Asset {asset.AssetCode} - {asset.Category} {asset.Brand} {asset.Model}");
            return true;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            error = "Codice asset già esistente.";
            return false;
        }
    }

    public void ArchiveAsset(long assetId, bool archived, string changedBy)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE assets SET is_archived = $archived WHERE id = $id";
        command.Parameters.AddWithValue("$archived", archived ? 1 : 0);
        command.Parameters.AddWithValue("$id", assetId);
        command.ExecuteNonQuery();

        WriteAudit(changedBy, archived ? "ASSET_ARCHIVED" : "ASSET_RESTORED", $"AssetId={assetId}");
    }

    public void AssignAsset(long assetId, long? employeeId, string changedBy)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE assets SET assigned_employee_id = $employeeId, status = $status WHERE id = $id";
        command.Parameters.AddWithValue("$employeeId", employeeId.HasValue ? employeeId.Value : DBNull.Value);
        command.Parameters.AddWithValue("$status", employeeId.HasValue ? "Assegnato" : "Disponibile");
        command.Parameters.AddWithValue("$id", assetId);
        command.ExecuteNonQuery();

        WriteAudit(changedBy, employeeId.HasValue ? "ASSET_ASSIGNED" : "ASSET_RETURNED", $"AssetId={assetId}; EmployeeId={employeeId}");
    }

    public void ExportAssetsCsv(string filePath, string? search, bool includeArchived, string exportedBy)
    {
        var rows = GetAssets(search, includeArchived);
        using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
        writer.WriteLine("Codice;Categoria;Marca;Modello;Seriale;SistemaOperativo;Stato;AssegnatoA;DataAcquisto;FineGaranzia;Note;Archivio");

        foreach (var a in rows)
        {
            writer.WriteLine($"{EscapeCsv(a.AssetCode)};{EscapeCsv(a.Category)};{EscapeCsv(a.Brand)};{EscapeCsv(a.Model)};{EscapeCsv(a.SerialNumber)};{EscapeCsv(a.OperatingSystem)};{EscapeCsv(a.Status)};{EscapeCsv(a.AssignedEmployeeName)};{EscapeCsv(a.PurchaseDate)};{EscapeCsv(a.WarrantyEnd)};{EscapeCsv(a.Notes)};{(a.IsArchived ? "Archiviato" : "Attivo")}");
        }

        WriteAudit(exportedBy, "ASSETS_EXPORTED", $"File={filePath}; Count={rows.Count}");
    }

    public int CountAssets(bool includeArchived = false)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = includeArchived
            ? "SELECT COUNT(*) FROM assets"
            : "SELECT COUNT(*) FROM assets WHERE is_archived = 0";

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void ExportEmployeesCsv(string filePath, string? search, bool includeArchived, string exportedBy)
    {
        var rows = GetEmployees(search, includeArchived);
        using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
        writer.WriteLine("Matricola;Nome;Cognome;Reparto;Mansione;Email;Telefono;DataAssunzione;Stato");

        foreach (var e in rows)
        {
            writer.WriteLine($"{EscapeCsv(e.EmployeeCode)};{EscapeCsv(e.FirstName)};{EscapeCsv(e.LastName)};{EscapeCsv(e.Department)};{EscapeCsv(e.JobTitle)};{EscapeCsv(e.Email)};{EscapeCsv(e.Phone)};{EscapeCsv(e.HireDate)};{(e.IsArchived ? "Archiviato" : "Attivo")}");
        }

        WriteAudit(exportedBy, "EMPLOYEES_EXPORTED", $"File={filePath}; Count={rows.Count}");
    }

    private static string EscapeCsv(string value)
    {
        return (value ?? "").Replace(";", ",").Replace("\r", " ").Replace("\n", " ");
    }

    public void ArchiveEmployee(long employeeId, bool archived, string changedBy)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE employees SET is_archived = $archived WHERE id = $id";
        command.Parameters.AddWithValue("$archived", archived ? 1 : 0);
        command.Parameters.AddWithValue("$id", employeeId);
        command.ExecuteNonQuery();

        WriteAudit(changedBy, archived ? "EMPLOYEE_ARCHIVED" : "EMPLOYEE_RESTORED", $"EmployeeId={employeeId}");
    }

    public int CountEmployees(bool includeArchived = false)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = includeArchived
            ? "SELECT COUNT(*) FROM employees"
            : "SELECT COUNT(*) FROM employees WHERE is_archived = 0";

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void WriteAudit(string username, string action, string? details = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO audit_logs (username, action, details, created_at)
VALUES ($username, $action, $details, $createdAt)
";
        command.Parameters.AddWithValue("$username", username);
        command.Parameters.AddWithValue("$action", action);
        command.Parameters.AddWithValue("$details", details ?? "");
        command.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("O"));
        command.ExecuteNonQuery();
    }
}
