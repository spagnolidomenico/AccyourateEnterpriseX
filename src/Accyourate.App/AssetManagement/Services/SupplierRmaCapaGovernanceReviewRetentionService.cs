using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using Accyourate.App.Platform.Notifications;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SupplierRmaCapaGovernanceReviewRetentionRecord
{
    public int Id { get; init; }
    public int ReviewId { get; init; }
    public int Revision { get; init; }
    public int AttestationId { get; init; }
    public string Custodian { get; init; } = "";
    public string ArchivePath { get; init; } = "";
    public string ArchiveHash { get; init; } = "";
    public string ArchivedAt { get; init; } = "";
    public string ArchivedBy { get; init; } = "";
    public string RetentionUntil { get; init; } = "";
    public int CurrentRevision { get; init; }
    public bool ArchiveAvailable => File.Exists(ArchivePath);
    public bool HashMatches => ArchiveAvailable && string.Equals(Hash(ArchivePath), ArchiveHash, StringComparison.OrdinalIgnoreCase);
    public bool IsCurrent => Revision == CurrentRevision;
    public bool IsValid => HashMatches && IsCurrent;
    public bool IsExpired => DateTime.TryParse(RetentionUntil, out var date) && date.Date < DateTime.Today;
    public bool IsDueSoon => DateTime.TryParse(RetentionUntil, out var date) && date.Date >= DateTime.Today && date.Date <= DateTime.Today.AddDays(90);
    public string ValidationStatus => !ArchiveAvailable ? "Archivio mancante" : !HashMatches ? "Archivio modificato" : !IsCurrent ? "Conservazione superata" : IsExpired ? "Conservazione scaduta" : IsDueSoon ? "In scadenza" : "Valida";

    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
}

public sealed class SupplierRmaCapaGovernanceReviewRetentionService
{
    private readonly string _connectionString;

    public SupplierRmaCapaGovernanceReviewRetentionService(string? databasePath = null)
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AccyourateEnterpriseX");
        Directory.CreateDirectory(folder);
        _connectionString = $"Data Source={databasePath ?? Path.Combine(folder, "accyourate-assets.db")}";
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS SupplierRmaCapaGovernanceReviewRetention(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ReviewId INTEGER NOT NULL,
                Revision INTEGER NOT NULL,
                AttestationId INTEGER NOT NULL,
                Custodian TEXT NOT NULL,
                ArchivePath TEXT NOT NULL,
                ArchiveHash TEXT NOT NULL,
                ArchivedAt TEXT NOT NULL,
                ArchivedBy TEXT NOT NULL,
                RetentionUntil TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS SupplierRmaCapaGovernanceReviewRetentionNotifications(
                NotificationKey TEXT PRIMARY KEY,
                RetentionId INTEGER NOT NULL,
                CreatedAt TEXT NOT NULL);
            """;
        command.ExecuteNonQuery();
    }

    public SupplierRmaCapaGovernanceReviewRetentionRecord Archive(SupplierRmaCapaGovernanceReview review, string custodian, int retentionDays, string user)
    {
        if (review.ApprovalStatus != "Approvato") throw new InvalidOperationException("Il riesame deve essere approvato.");
        if (string.IsNullOrWhiteSpace(custodian)) throw new InvalidOperationException("Indica il custode della conservazione.");
        if (retentionDays < 1) throw new InvalidOperationException("La durata della conservazione deve essere maggiore di zero.");
        if (GetAll(review.Id).Any(x => x.Revision == review.Revision && x.IsValid)) throw new InvalidOperationException("La revisione corrente e gia conservata con un archivio valido.");
        var attestation = new SupplierRmaCapaGovernanceReviewAttestationService().GetAll(review.Id).FirstOrDefault(x => x.Revision == review.Revision && x.IsValid)
            ?? throw new InvalidOperationException("Prima della conservazione e necessaria un'attestazione valida della revisione corrente.");

        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Accyourate Enterprise X", "Conservazione Riesami Governance CAPA");
        Directory.CreateDirectory(folder);
        var archivePath = Path.Combine(folder, $"Riesame-Governance-CAPA-{review.Id:D6}-R{review.Revision}-{DateTime.Now:yyyyMMdd-HHmmss}.zip");
        var staging = Path.Combine(Path.GetTempPath(), $"Accyourate-CAPA-{Guid.NewGuid():N}");
        Directory.CreateDirectory(staging);
        try
        {
            File.Copy(review.ReportPath, Path.Combine(staging, Path.GetFileName(review.ReportPath)), true);
            File.Copy(attestation.CertificatePath, Path.Combine(staging, Path.GetFileName(attestation.CertificatePath)), true);
            var until = DateTime.Today.AddDays(retentionDays);
            File.WriteAllText(Path.Combine(staging, "MANIFEST.txt"),
                $"CONSERVAZIONE RIESAME GOVERNANCE CAPA\n\nRiesame: {review.Id:D6}\nRevisione: {review.Revision}\n" +
                $"Esito: {review.Outcome}\nResponsabile: {review.Reviewer}\nApprovatore: {review.Approver}\n" +
                $"Attestazione: {attestation.Id:D6}\nCustode: {custodian.Trim()}\nArchiviato da: {user}\n" +
                $"Data archiviazione: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\nConservazione fino al: {until:dd/MM/yyyy}\n" +
                $"SHA-256 verbale: {attestation.DocumentHash}\n",
                new UTF8Encoding(true));
            ZipFile.CreateFromDirectory(staging, archivePath, CompressionLevel.Optimal, false);
            var archiveHash = Hash(archivePath);
            using var connection = Open();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO SupplierRmaCapaGovernanceReviewRetention(ReviewId,Revision,AttestationId,Custodian,ArchivePath,ArchiveHash,ArchivedAt,ArchivedBy,RetentionUntil) VALUES($review,$revision,$attestation,$custodian,$path,$hash,$at,$by,$until); SELECT last_insert_rowid();";
            command.Parameters.AddWithValue("$review", review.Id);
            command.Parameters.AddWithValue("$revision", review.Revision);
            command.Parameters.AddWithValue("$attestation", attestation.Id);
            command.Parameters.AddWithValue("$custodian", custodian.Trim());
            command.Parameters.AddWithValue("$path", archivePath);
            command.Parameters.AddWithValue("$hash", archiveHash);
            command.Parameters.AddWithValue("$at", DateTime.Now.ToString("s"));
            command.Parameters.AddWithValue("$by", user);
            command.Parameters.AddWithValue("$until", until.ToString("yyyy-MM-dd"));
            var id = Convert.ToInt32(command.ExecuteScalar());
            return GetAll(review.Id).First(x => x.Id == id);
        }
        finally
        {
            if (Directory.Exists(staging)) Directory.Delete(staging, true);
        }
    }

    public IReadOnlyList<SupplierRmaCapaGovernanceReviewRetentionRecord> GetAll(int reviewId)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT x.Id,x.ReviewId,x.Revision,x.AttestationId,x.Custodian,x.ArchivePath,x.ArchiveHash,x.ArchivedAt,x.ArchivedBy,x.RetentionUntil,COALESCE(r.Revision,0) FROM SupplierRmaCapaGovernanceReviewRetention x LEFT JOIN SupplierRmaCapaGovernanceReviews r ON r.Id=x.ReviewId WHERE x.ReviewId=$review ORDER BY x.Id DESC;";
        command.Parameters.AddWithValue("$review", reviewId);
        using var reader = command.ExecuteReader();
        var values = new List<SupplierRmaCapaGovernanceReviewRetentionRecord>();
        while (reader.Read()) values.Add(new()
        {
            Id = reader.GetInt32(0), ReviewId = reader.GetInt32(1), Revision = reader.GetInt32(2), AttestationId = reader.GetInt32(3),
            Custodian = reader.GetString(4), ArchivePath = reader.GetString(5), ArchiveHash = reader.GetString(6),
            ArchivedAt = reader.GetString(7), ArchivedBy = reader.GetString(8), RetentionUntil = reader.GetString(9), CurrentRevision = reader.GetInt32(10)
        });
        return values;
    }

    public int PublishAlerts(NotificationService? notifications = null)
    {
        notifications ??= new NotificationService();
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT DISTINCT ReviewId FROM SupplierRmaCapaGovernanceReviewRetention;";
        using var reader = command.ExecuteReader();
        var reviewIds = new List<int>();
        while (reader.Read()) reviewIds.Add(reader.GetInt32(0));
        reader.Close();
        var count = 0;
        foreach (var item in reviewIds.SelectMany(GetAll).Where(x => x.IsCurrent && (!x.HashMatches || x.IsExpired || x.IsDueSoon)))
        {
            var key = $"governance-review-retention:{item.Id}:{item.ValidationStatus}:{DateTime.Today:yyyyMMdd}";
            using var check = connection.CreateCommand();
            check.CommandText = "SELECT COUNT(*) FROM SupplierRmaCapaGovernanceReviewRetentionNotifications WHERE NotificationKey=$key;";
            check.Parameters.AddWithValue("$key", key);
            if (Convert.ToInt32(check.ExecuteScalar()) > 0) continue;
            var priority = !item.HashMatches || item.IsExpired ? NotificationPriority.Critical : NotificationPriority.High;
            notifications.Publish("Conservazione riesame Governance CAPA", $"Riesame #{item.ReviewId:D6}, revisione {item.Revision}: {item.ValidationStatus}.", NotificationCategory.Asset, priority, "Controllo conservazione riesami", "open-rma-corrective-actions", item.ReviewId.ToString());
            using var insert = connection.CreateCommand();
            insert.CommandText = "INSERT INTO SupplierRmaCapaGovernanceReviewRetentionNotifications(NotificationKey,RetentionId,CreatedAt) VALUES($key,$id,$date);";
            insert.Parameters.AddWithValue("$key", key); insert.Parameters.AddWithValue("$id", item.Id); insert.Parameters.AddWithValue("$date", DateTime.Now.ToString("s")); insert.ExecuteNonQuery();
            count++;
        }
        return count;
    }

    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
    private SqliteConnection Open() { var connection = new SqliteConnection(_connectionString); connection.Open(); return connection; }
}
