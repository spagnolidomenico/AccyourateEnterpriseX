using System.Security.Cryptography;
using System.Text;
using Accyourate.App.Platform.Pdf;
using Accyourate.App.Platform.Settings;
using Microsoft.Data.Sqlite;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SupplierRmaCapaAttestationExportService
{
    private readonly string _connectionString;
    private readonly SettingsService _settings = new();

    public SupplierRmaCapaAttestationExportService(string? databasePath = null)
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AccyourateEnterpriseX");
        Directory.CreateDirectory(folder);
        _connectionString = $"Data Source={databasePath ?? Path.Combine(folder, "accyourate-assets.db")}";
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS SupplierRmaCapaAttestationExports(Id INTEGER PRIMARY KEY AUTOINCREMENT,Format TEXT NOT NULL,FilterDescription TEXT NOT NULL,RecordCount INTEGER NOT NULL,ValidCount INTEGER NOT NULL,InvalidCount INTEGER NOT NULL,MissingCount INTEGER NOT NULL,FilePath TEXT NOT NULL,FileHash TEXT NOT NULL,ExportedBy TEXT NOT NULL,ExportedAt TEXT NOT NULL);";
        command.ExecuteNonQuery();
    }

    public string ExportCsv(IReadOnlyList<SupplierRmaCapaAttestation> rows, string filters)
    {
        var path = Path.Combine(ExportFolder(), $"Registro-attestazioni-CAPA-{DateTime.Now:yyyyMMdd-HHmmss}.csv");
        var text = new StringBuilder("Pratica;Revisione;Approvatore;Ruolo;Data attestazione;Stato;SHA-256;Archivio;Verbale\r\n");
        foreach (var row in rows)
            text.AppendLine(string.Join(";", new[] { Csv(row.CaseNumber), Csv(row.Revision), Csv(row.Approver), Csv(row.Role), Csv(Date(row.AttestedAt)), Csv(row.ValidationStatus), Csv(row.ArchiveHash), Csv(row.ArchivePath), Csv(row.ReportPath) }));
        File.WriteAllText(path, text.ToString(), new UTF8Encoding(true));
        Register("CSV", rows, filters, path);
        return path;
    }

    public string ExportPdf(IReadOnlyList<SupplierRmaCapaAttestation> rows, string filters)
    {
        var settings = _settings.Load();
        var template = settings.DocumentTemplate ?? new DocumentTemplateSettings();
        var document = new SimplePdfDocument { Title = "Registro attestazioni CAPA" };
        ApplyBranding(document, settings, template, $"CAPA-ATT-{DateTime.Now:yyyyMMdd-HHmm}");
        var valid = rows.Count(x => x.IsValid);
        var missing = rows.Count(x => !x.ArchiveAvailable);
        var invalid = rows.Count - valid - missing;
        document.AddTitle("Registro attestazioni fascicoli CAPA");
        document.AddKeyValue("Data elaborazione", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        document.AddKeyValue("Operatore", Environment.UserName);
        document.AddKeyValue("Filtri applicati", filters);
        document.AddHeading("Riepilogo esiti");
        document.AddKeyValue("Attestazioni esportate", rows.Count.ToString());
        document.AddKeyValue("Valide", valid.ToString());
        document.AddKeyValue("Non valide", invalid.ToString());
        document.AddKeyValue("Archivio mancante", missing.ToString());
        document.AddStatus("Esito del registro", invalid == 0 && missing == 0 ? "Conforme" : "Richiede attenzione");
        document.AddHeading("Dettaglio attestazioni");
        if (rows.Count == 0) document.AddText("Nessuna attestazione corrisponde ai filtri selezionati.");
        foreach (var row in rows)
        {
            document.AddText($"{row.CaseNumber} - revisione {Dash(row.Revision)}", 11);
            document.AddText($"Stato: {row.ValidationStatus} | Approvatore: {Dash(row.Approver)} | Ruolo: {Dash(row.Role)}", 9);
            document.AddText($"Data attestazione: {Date(row.AttestedAt)}", 9);
            document.AddText($"SHA-256: {row.ArchiveHash}", 8);
        }
        document.AddHeading("Tracciabilita del rapporto");
        document.AddText("Il rapporto rappresenta lo stato del registro al momento dell'esportazione e rispetta i filtri indicati nei metadati.", 9);
        document.AddSignaturePair("Responsabile qualita", "Responsabile processo");
        var path = new PdfExportService().Export(document, ExportFolder(), $"Registro-attestazioni-CAPA-{DateTime.Now:yyyyMMdd-HHmmss}");
        Register("PDF", rows, filters, path);
        return path;
    }

    private void Register(string format, IReadOnlyList<SupplierRmaCapaAttestation> rows, string filters, string path)
    {
        var valid = rows.Count(x => x.IsValid); var missing = rows.Count(x => !x.ArchiveAvailable); var invalid = rows.Count - valid - missing;
        var hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
        File.WriteAllText(path + ".sha256", hash, Encoding.ASCII);
        using var connection = Open(); using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO SupplierRmaCapaAttestationExports(Format,FilterDescription,RecordCount,ValidCount,InvalidCount,MissingCount,FilePath,FileHash,ExportedBy,ExportedAt) VALUES($f,$d,$n,$v,$i,$m,$p,$h,$u,$a);";
        command.Parameters.AddWithValue("$f", format); command.Parameters.AddWithValue("$d", filters); command.Parameters.AddWithValue("$n", rows.Count); command.Parameters.AddWithValue("$v", valid); command.Parameters.AddWithValue("$i", invalid); command.Parameters.AddWithValue("$m", missing); command.Parameters.AddWithValue("$p", path); command.Parameters.AddWithValue("$h", hash); command.Parameters.AddWithValue("$u", Environment.UserName); command.Parameters.AddWithValue("$a", DateTime.Now.ToString("s")); command.ExecuteNonQuery();
    }

    private static void ApplyBranding(SimplePdfDocument d, ApplicationSettings s, DocumentTemplateSettings t, string code)
    {
        d.Branding.CompanyName = string.IsNullOrWhiteSpace(s.Company.LegalName) ? s.Company.CompanyName : s.Company.LegalName;
        d.Branding.CompanyDetailLines.AddRange(new[] { s.Company.Address, string.Join(" ", new[] { s.Company.City, s.Company.Province }.Where(x => !string.IsNullOrWhiteSpace(x))), string.Join(" - ", new[] { s.Company.Phone, s.Company.Email }.Where(x => !string.IsNullOrWhiteSpace(x))), string.Join(" - ", new[] { s.Company.VatNumber, s.Company.FiscalCode }.Where(x => !string.IsNullOrWhiteSpace(x))) }.Where(x => !string.IsNullOrWhiteSpace(x)));
        d.Branding.HeaderLayout=t.HeaderLayout; d.Branding.LogoPath=s.Company.LogoPath; d.Branding.LogoSize=t.LogoSize; d.Branding.LogoPosition=t.LogoPosition; d.Branding.PrimaryColor=t.PrimaryColor; d.Branding.DocumentLabel="REGISTRO ATTESTAZIONI CAPA"; d.Branding.DocumentCode=code; d.Branding.DocumentVersion=t.DocumentVersion; d.Branding.FooterText=t.FooterText; d.Branding.ConfidentialityText=t.ConfidentialityText; d.Branding.ShowLogo=t.ShowLogo; d.Branding.ShowCompanyDetails=t.ShowCompanyDetails; d.Branding.ShowDocumentMetadata=t.ShowDocumentMetadata; d.Branding.ShowFooter=t.ShowFooter; d.Branding.ShowPageNumber=t.ShowPageNumber; d.Branding.ShowPrintTimestamp=t.ShowPrintTimestamp;
    }

    private static string ExportFolder(){var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Accyourate Enterprise X","Registro attestazioni CAPA");Directory.CreateDirectory(folder);return folder;}
    private static string Csv(string value)=>$"\"{(value??string.Empty).Replace("\"","\"\"")}\"";
    private static string Date(string value)=>DateTime.TryParse(value,out var date)?date.ToString("dd/MM/yyyy HH:mm"):value;
    private static string Dash(string value)=>string.IsNullOrWhiteSpace(value)?"Non specificato":value;
    private SqliteConnection Open(){var connection=new SqliteConnection(_connectionString);connection.Open();return connection;}
}
