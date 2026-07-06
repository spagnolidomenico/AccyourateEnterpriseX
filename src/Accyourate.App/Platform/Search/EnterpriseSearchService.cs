using Microsoft.Data.Sqlite;

namespace Accyourate.App.Platform.Search;

public sealed class EnterpriseSearchService
{
    private readonly string _appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AccyourateEnterpriseX");

    public IReadOnlyList<SearchResult> Search(SearchRequest request)
    {
        var query = (request.Query ?? string.Empty).Trim();
        if (query.Length < 2) return Array.Empty<SearchResult>();
        var limit = Math.Clamp(request.Limit, 10, 300);
        var r = new List<SearchResult>();
        r.AddRange(Employees(query, limit));
        r.AddRange(Assets(query, limit));
        r.AddRange(Reports(query, limit));
        r.AddRange(Documents(query, limit));
        r.AddRange(Notifications(query, limit));
        r.AddRange(Audit(query, limit));
        r.AddRange(Settings(query));
        return r.Take(limit).ToList();
    }

    private IReadOnlyList<SearchResult> Employees(string q, int l) => Query(Path.Combine(_appFolder, "accyourate-hr.db"), "Employees", """
        SELECT Id, EmployeeCode, FirstName, LastName, Email, EmploymentStatus FROM Employees
        WHERE EmployeeCode LIKE $Query OR FirstName LIKE $Query OR LastName LIKE $Query OR Email LIKE $Query OR Notes LIKE $Query
        ORDER BY LastName, FirstName LIMIT $Limit;
    """, q, l, r => new SearchResult { Category=SearchCategory.HumanResources, Icon="👥", Title=$"{S(r,2)} {S(r,3)}".Trim(), Subtitle=$"{S(r,1)} · {S(r,4)} · {S(r,5)}", EntityType="Employee", EntityId=r.GetInt32(0).ToString(), OpenAction="human-resources", Source="HR" });

    private IReadOnlyList<SearchResult> Assets(string q, int l) => Query(Path.Combine(_appFolder, "accyourate-assets.db"), "Assets", """
        SELECT Id, AssetCode, Category, Manufacturer, Model, SerialNumber, Status FROM Assets
        WHERE AssetCode LIKE $Query OR Category LIKE $Query OR Manufacturer LIKE $Query OR Model LIKE $Query OR SerialNumber LIKE $Query OR Status LIKE $Query
        ORDER BY AssetCode LIMIT $Limit;
    """, q, l, r => new SearchResult { Category=SearchCategory.Asset, Icon="💻", Title=$"{S(r,1)} · {S(r,3)} {S(r,4)}".Trim(), Subtitle=$"{S(r,2)} · S/N {S(r,5)} · {S(r,6)}", EntityType="Asset", EntityId=r.GetInt32(0).ToString(), OpenAction="asset-management", Source="Asset" });

    private IReadOnlyList<SearchResult> Reports(string q, int l) => Query(Path.Combine(_appFolder, "accyourate-assets.db"), "DeliveryReports", """
        SELECT Id, ReportNumber, EmployeeName, AssetCode, Status, ReportDate FROM DeliveryReports
        WHERE ReportNumber LIKE $Query OR EmployeeName LIKE $Query OR AssetCode LIKE $Query OR Status LIKE $Query OR Notes LIKE $Query
        ORDER BY ReportDate DESC LIMIT $Limit;
    """, q, l, r => new SearchResult { Category=SearchCategory.DeliveryReport, Icon="📄", Title=S(r,1), Subtitle=$"{S(r,2)} · {S(r,3)} · {S(r,4)}", EntityType="DeliveryReport", EntityId=r.GetInt32(0).ToString(), OpenAction="delivery-reports", Source="Asset" });

    private IReadOnlyList<SearchResult> Documents(string q, int l) => Query(Path.Combine(_appFolder, "accyourate-platform.db"), "Documents", """
        SELECT Id, DocumentNumber, Title, Category, FileName, RelatedEntityLabel, CreatedAt FROM Documents
        WHERE DocumentNumber LIKE $Query OR Title LIKE $Query OR Category LIKE $Query OR FileName LIKE $Query OR RelatedEntityLabel LIKE $Query OR Notes LIKE $Query
        ORDER BY CreatedAt DESC LIMIT $Limit;
    """, q, l, r => new SearchResult { Category=SearchCategory.Document, Icon="📁", Title=$"{S(r,1)} · {S(r,2)}", Subtitle=$"{S(r,3)} · {S(r,4)} · {S(r,5)}", EntityType="Document", EntityId=r.GetInt32(0).ToString(), OpenAction="document-center", Source="Platform" });

    private IReadOnlyList<SearchResult> Notifications(string q, int l) => Query(Path.Combine(_appFolder, "accyourate-platform.db"), "Notifications", """
        SELECT Id, Title, Message, Category, CreatedAt FROM Notifications
        WHERE Title LIKE $Query OR Message LIKE $Query OR Category LIKE $Query
        ORDER BY CreatedAt DESC LIMIT $Limit;
    """, q, l, r => new SearchResult { Category=SearchCategory.Notification, Icon="🔔", Title=S(r,1), Subtitle=$"{S(r,3)} · {S(r,2)}", EntityType="Notification", EntityId=r.GetInt32(0).ToString(), OpenAction="notifications", Source="Platform" });

    private IReadOnlyList<SearchResult> Audit(string q, int l) => Query(Path.Combine(_appFolder, "accyourate-platform.db"), "AuditRecords", """
        SELECT Id, Action, Description, EntityType, EntityName, CreatedAt FROM AuditRecords
        WHERE Action LIKE $Query OR Description LIKE $Query OR EntityType LIKE $Query OR EntityName LIKE $Query
        ORDER BY CreatedAt DESC LIMIT $Limit;
    """, q, l, r => new SearchResult { Category=SearchCategory.Audit, Icon="📜", Title=$"{S(r,1)} · {S(r,4)}", Subtitle=$"{S(r,3)} · {S(r,2)}", EntityType="Audit", EntityId=r.GetInt32(0).ToString(), OpenAction="dashboard", Source="Platform" });

    private static IReadOnlyList<SearchResult> Settings(string q)
    {
        var terms = new[] { ("Azienda","Dati aziendali, logo, contatti"), ("Numerazioni","Prefissi per dipendenti, asset, verbali e documenti"), ("Documenti","Percorsi documenti e cartelle"), ("Tema","Aspetto e preferenze interfaccia") };
        return terms.Where(x => $"{x.Item1} {x.Item2}".Contains(q, StringComparison.OrdinalIgnoreCase)).Select(x => new SearchResult { Category=SearchCategory.Settings, Icon="⚙️", Title=x.Item1, Subtitle=x.Item2, EntityType="Settings", EntityId=x.Item1, OpenAction="settings-center", Source="Platform" }).ToList();
    }

    private static IReadOnlyList<SearchResult> Query(string db, string table, string sql, string q, int limit, Func<SqliteDataReader, SearchResult> map)
    {
        var res = new List<SearchResult>();
        try {
            if (!File.Exists(db)) return res;
            using var c = new SqliteConnection($"Data Source={db}"); c.Open();
            using var chk = c.CreateCommand(); chk.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=$t;"; chk.Parameters.AddWithValue("$t", table);
            if (Convert.ToInt32(chk.ExecuteScalar()) == 0) return res;
            using var cmd = c.CreateCommand(); cmd.CommandText = sql; cmd.Parameters.AddWithValue("$Query", $"%{q}%"); cmd.Parameters.AddWithValue("$Limit", limit);
            using var reader = cmd.ExecuteReader(); while (reader.Read()) res.Add(map(reader));
        } catch { return res; }
        return res;
    }

    private static string S(SqliteDataReader r, int i) => r.IsDBNull(i) ? string.Empty : r.GetString(i);
}
