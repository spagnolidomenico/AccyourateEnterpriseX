using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using Accyourate.App.Platform.Notifications;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SupplierRmaCapaGovernanceReviewAttestation
{
    public int Id { get; init; }
    public int ReviewId { get; init; }
    public int Revision { get; init; }
    public string AttestedBy { get; init; } = "";
    public string Role { get; init; } = "";
    public string DocumentHash { get; init; } = "";
    public string DocumentPath { get; init; } = "";
    public string AttestedAt { get; init; } = "";
    public string CertificatePath { get; init; } = "";
    public int CurrentRevision { get; init; }
    public string CurrentApprovalStatus { get; init; } = "";
    public bool DocumentAvailable => File.Exists(DocumentPath);
    public bool CertificateAvailable => File.Exists(CertificatePath);
    public bool HashMatches => DocumentAvailable && string.Equals(Hash(DocumentPath), DocumentHash, StringComparison.OrdinalIgnoreCase);
    public bool IsCurrent => Revision == CurrentRevision && CurrentApprovalStatus == "Approvato";
    public bool IsValid => HashMatches && IsCurrent;
    public string ValidationStatus => !DocumentAvailable ? "Documento mancante" : !HashMatches ? "Documento modificato" : !IsCurrent ? "Attestazione superata" : "Valida";

    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
}

public sealed class SupplierRmaCapaGovernanceReviewAttestationService
{
    private readonly string _connectionString;

    public SupplierRmaCapaGovernanceReviewAttestationService(string? databasePath = null)
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AccyourateEnterpriseX");
        Directory.CreateDirectory(folder);
        _connectionString = $"Data Source={databasePath ?? Path.Combine(folder, "accyourate-assets.db")}";
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS SupplierRmaCapaGovernanceReviewAttestations(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ReviewId INTEGER NOT NULL,
                Revision INTEGER NOT NULL,
                AttestedBy TEXT NOT NULL,
                Role TEXT NOT NULL,
                DocumentHash TEXT NOT NULL,
                DocumentPath TEXT NOT NULL,
                AttestedAt TEXT NOT NULL,
                CertificatePath TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS SupplierRmaCapaGovernanceReviewAttestationNotifications(
                NotificationKey TEXT PRIMARY KEY,
                AttestationId INTEGER NOT NULL,
                CreatedAt TEXT NOT NULL);
            """;
        command.ExecuteNonQuery();
    }

    public SupplierRmaCapaGovernanceReviewAttestation Attest(SupplierRmaCapaGovernanceReview review, string role, string user)
    {
        if (review.ApprovalStatus != "Approvato") throw new InvalidOperationException("Il riesame deve essere approvato prima dell'attestazione.");
        if (!File.Exists(review.ReportPath)) throw new InvalidOperationException("Il verbale PDF non e disponibile.");
        if (string.IsNullOrWhiteSpace(role)) throw new InvalidOperationException("Indica il ruolo dell'attestatore.");
        if (GetAll(review.Id).Any(x => x.Revision == review.Revision && x.IsValid)) throw new InvalidOperationException("La revisione corrente possiede gia un'attestazione valida.");

        var hash = Hash(review.ReportPath);
        var attestedAt = DateTime.Now.ToString("s");
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Accyourate Enterprise X", "Attestazioni Riesami Governance CAPA");
        Directory.CreateDirectory(folder);
        var certificate = Path.Combine(folder, $"Attestazione-Riesame-{review.Id:D6}-R{review.Revision}-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
        File.WriteAllText(certificate,
            $"ATTESTAZIONE RIESAME GOVERNANCE CAPA\n\n" +
            $"Riesame: {review.Id:D6}\nRevisione: {review.Revision}\nEsito: {review.Outcome}\n" +
            $"Responsabile: {review.Reviewer}\nApprovatore: {review.Approver}\n" +
            $"Attestatore: {user}\nRuolo: {role.Trim()}\nData attestazione: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n" +
            $"Documento: {review.ReportPath}\nSHA-256: {hash}\n",
            new UTF8Encoding(true));

        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO SupplierRmaCapaGovernanceReviewAttestations(ReviewId,Revision,AttestedBy,Role,DocumentHash,DocumentPath,AttestedAt,CertificatePath) VALUES($review,$revision,$user,$role,$hash,$document,$date,$certificate); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$review", review.Id);
        command.Parameters.AddWithValue("$revision", review.Revision);
        command.Parameters.AddWithValue("$user", user);
        command.Parameters.AddWithValue("$role", role.Trim());
        command.Parameters.AddWithValue("$hash", hash);
        command.Parameters.AddWithValue("$document", review.ReportPath);
        command.Parameters.AddWithValue("$date", attestedAt);
        command.Parameters.AddWithValue("$certificate", certificate);
        var id = Convert.ToInt32(command.ExecuteScalar());
        return GetAll(review.Id).First(x => x.Id == id);
    }

    public IReadOnlyList<SupplierRmaCapaGovernanceReviewAttestation> GetAll(int reviewId)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT a.Id,a.ReviewId,a.Revision,a.AttestedBy,a.Role,a.DocumentHash,a.DocumentPath,a.AttestedAt,a.CertificatePath,COALESCE(r.Revision,0),COALESCE(r.ApprovalStatus,'') FROM SupplierRmaCapaGovernanceReviewAttestations a LEFT JOIN SupplierRmaCapaGovernanceReviews r ON r.Id=a.ReviewId WHERE a.ReviewId=$review ORDER BY a.Id DESC;";
        command.Parameters.AddWithValue("$review", reviewId);
        using var reader = command.ExecuteReader();
        var values = new List<SupplierRmaCapaGovernanceReviewAttestation>();
        while (reader.Read()) values.Add(new()
        {
            Id = reader.GetInt32(0), ReviewId = reader.GetInt32(1), Revision = reader.GetInt32(2),
            AttestedBy = reader.GetString(3), Role = reader.GetString(4), DocumentHash = reader.GetString(5),
            DocumentPath = reader.GetString(6), AttestedAt = reader.GetString(7), CertificatePath = reader.GetString(8),
            CurrentRevision = reader.GetInt32(9), CurrentApprovalStatus = reader.GetString(10)
        });
        return values;
    }

    public int PublishInvalidNotifications(NotificationService? notifications = null)
    {
        notifications ??= new NotificationService();
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT DISTINCT ReviewId FROM SupplierRmaCapaGovernanceReviewAttestations;";
        using var reader = command.ExecuteReader();
        var reviewIds = new List<int>();
        while (reader.Read()) reviewIds.Add(reader.GetInt32(0));
        reader.Close();
        var count = 0;
        foreach (var item in reviewIds.SelectMany(GetAll).Where(x => !x.IsValid && x.IsCurrent))
        {
            var key = $"governance-review-attestation:{item.Id}:{item.ValidationStatus}:{DateTime.Today:yyyyMMdd}";
            using var check = connection.CreateCommand();
            check.CommandText = "SELECT COUNT(*) FROM SupplierRmaCapaGovernanceReviewAttestationNotifications WHERE NotificationKey=$key;";
            check.Parameters.AddWithValue("$key", key);
            if (Convert.ToInt32(check.ExecuteScalar()) > 0) continue;
            notifications.Publish("Attestazione riesame Governance CAPA non valida", $"Riesame #{item.ReviewId:D6}, revisione {item.Revision}: {item.ValidationStatus}.", NotificationCategory.Asset, NotificationPriority.Critical, "Controllo integrita riesami", "open-rma-corrective-actions", item.ReviewId.ToString());
            using var insert = connection.CreateCommand();
            insert.CommandText = "INSERT INTO SupplierRmaCapaGovernanceReviewAttestationNotifications(NotificationKey,AttestationId,CreatedAt) VALUES($key,$id,$date);";
            insert.Parameters.AddWithValue("$key", key);
            insert.Parameters.AddWithValue("$id", item.Id);
            insert.Parameters.AddWithValue("$date", DateTime.Now.ToString("s"));
            insert.ExecuteNonQuery();
            count++;
        }
        return count;
    }

    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
    private SqliteConnection Open() { var connection = new SqliteConnection(_connectionString); connection.Open(); return connection; }
}
