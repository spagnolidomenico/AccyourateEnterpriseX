using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SupplierRmaCapaDossierVerificationItem
{
    public string FileName { get; init; } = "";
    public string Status { get; init; } = "";
    public string Detail { get; init; } = "";
    public bool IsValid { get; init; }
}

public sealed class SupplierRmaCapaDossierVerificationResult
{
    public string ArchivePath { get; init; } = "";
    public string VerifiedAt { get; init; } = "";
    public IReadOnlyList<SupplierRmaCapaDossierVerificationItem> Items { get; init; } = Array.Empty<SupplierRmaCapaDossierVerificationItem>();
    public string ReportPath { get; init; } = "";
    public bool IsValid => Items.Count > 0 && Items.All(x => x.IsValid);
}

public sealed class SupplierRmaCapaDossierVerificationService
{
    public SupplierRmaCapaDossierVerificationResult Verify(string archivePath)
    {
        if (!File.Exists(archivePath)) throw new FileNotFoundException("Il fascicolo selezionato non e disponibile.", archivePath);
        if (!string.Equals(Path.GetExtension(archivePath), ".zip", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Seleziona un fascicolo ZIP.");

        var items = new List<SupplierRmaCapaDossierVerificationItem>();
        using var archive = ZipFile.OpenRead(archivePath);
        var duplicate = archive.Entries.GroupBy(x => Normalize(x.FullName), StringComparer.OrdinalIgnoreCase).FirstOrDefault(x => x.Count() > 1);
        if (duplicate is not null) items.Add(Invalid(duplicate.Key, "Voce duplicata", "Il fascicolo contiene piu file con lo stesso percorso."));

        var manifestEntry = archive.Entries.FirstOrDefault(x => string.Equals(Normalize(x.FullName), "manifest-sha256.csv", StringComparison.OrdinalIgnoreCase));
        if (manifestEntry is null)
        {
            items.Add(Invalid("manifest-sha256.csv", "Manifest mancante", "Impossibile verificare integrita e completezza del fascicolo."));
            return Complete(archivePath, items);
        }

        List<string[]> rows;
        using (var reader = new StreamReader(manifestEntry.Open(), Encoding.UTF8, true)) rows = ParseCsv(reader.ReadToEnd());
        if (rows.Count < 2 || rows[0].Length < 3)
        {
            items.Add(Invalid("manifest-sha256.csv", "Manifest non valido", "Intestazione o contenuto del manifest non riconosciuti."));
            return Complete(archivePath, items);
        }

        var expected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in rows.Skip(1))
        {
            if (row.Length < 3 || string.IsNullOrWhiteSpace(row[0])) continue;
            var name = Normalize(row[0]); expected.Add(name);
            var entry = archive.Entries.FirstOrDefault(x => string.Equals(Normalize(x.FullName), name, StringComparison.OrdinalIgnoreCase));
            if (entry is null) { items.Add(Invalid(name, "File mancante", "Il file dichiarato nel manifest non e presente.")); continue; }
            if (!long.TryParse(row[2], out var expectedSize)) { items.Add(Invalid(name, "Dimensione non valida", "Il manifest contiene una dimensione non numerica.")); continue; }
            string hash; using (var stream = entry.Open()) hash = Convert.ToHexString(SHA256.HashData(stream));
            if (entry.Length != expectedSize) items.Add(Invalid(name, "Dimensione differente", $"Attesa {expectedSize} byte, trovata {entry.Length} byte."));
            else if (!string.Equals(hash, row[1].Trim(), StringComparison.OrdinalIgnoreCase)) items.Add(Invalid(name, "Impronta differente", "Il contenuto e stato modificato dopo l'esportazione."));
            else items.Add(new() { FileName = name, Status = "Integro", Detail = $"SHA-256 verificato - {entry.Length} byte", IsValid = true });
        }

        foreach (var entry in archive.Entries.Where(x => !string.IsNullOrEmpty(x.Name)))
        {
            var name = Normalize(entry.FullName);
            if (!string.Equals(name, "manifest-sha256.csv", StringComparison.OrdinalIgnoreCase) && !expected.Contains(name)) items.Add(Invalid(name, "File non dichiarato", "Il file e presente nello ZIP ma non nel manifest."));
        }
        if (expected.Count == 0) items.Add(Invalid("manifest-sha256.csv", "Manifest vuoto", "Nessun file risulta dichiarato."));
        return Complete(archivePath, items);
    }

    private static SupplierRmaCapaDossierVerificationResult Complete(string archivePath, List<SupplierRmaCapaDossierVerificationItem> items)
    {
        var verifiedAt = DateTime.Now.ToString("s");
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Accyourate Enterprise X", "Verifiche fascicoli CAPA RMA"); Directory.CreateDirectory(folder);
        var reportPath = Path.Combine(folder, $"Verifica-{Safe(Path.GetFileNameWithoutExtension(archivePath))}-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
        var valid = items.Count > 0 && items.All(x => x.IsValid);
        var report = new StringBuilder().AppendLine("VERBALE VERIFICA INTEGRITA FASCICOLO CAPA RMA").AppendLine().AppendLine($"Fascicolo: {Path.GetFileName(archivePath)}").AppendLine($"Percorso: {archivePath}").AppendLine($"Verificato il: {DateTime.Now:dd/MM/yyyy HH:mm:ss}").AppendLine($"Esito: {(valid ? "INTEGRO" : "NON CONFORME")}").AppendLine();
        foreach (var item in items) report.AppendLine($"[{(item.IsValid ? "OK" : "KO")}] {item.FileName} - {item.Status} - {item.Detail}");
        File.WriteAllText(reportPath, report.ToString(), new UTF8Encoding(true));
        return new() { ArchivePath = archivePath, VerifiedAt = verifiedAt, Items = items, ReportPath = reportPath };
    }

    private static SupplierRmaCapaDossierVerificationItem Invalid(string file, string status, string detail) => new() { FileName = file, Status = status, Detail = detail, IsValid = false };
    private static string Normalize(string value) => value.Replace('\\', '/').TrimStart('/');
    private static string Safe(string value) { var invalid = Path.GetInvalidFileNameChars(); var result = new string(value.Select(x => invalid.Contains(x) ? '_' : x).ToArray()).Trim(); return string.IsNullOrWhiteSpace(result) ? "Fascicolo" : result; }
    private static List<string[]> ParseCsv(string value)
    {
        var rows = new List<string[]>(); var row = new List<string>(); var field = new StringBuilder(); var quoted = false;
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (quoted && c == '"' && i + 1 < value.Length && value[i + 1] == '"') { field.Append('"'); i++; }
            else if (c == '"') quoted = !quoted;
            else if (!quoted && c == ';') { row.Add(field.ToString().TrimStart('\uFEFF')); field.Clear(); }
            else if (!quoted && (c == '\r' || c == '\n')) { if (c == '\r' && i + 1 < value.Length && value[i + 1] == '\n') i++; row.Add(field.ToString().TrimStart('\uFEFF')); field.Clear(); if (row.Any(x => x.Length > 0)) rows.Add(row.ToArray()); row.Clear(); }
            else field.Append(c);
        }
        if (field.Length > 0 || row.Count > 0) { row.Add(field.ToString().TrimStart('\uFEFF')); if (row.Any(x => x.Length > 0)) rows.Add(row.ToArray()); }
        return rows;
    }
}
