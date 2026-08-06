using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SupplierRmaCapaDossierService
{
    public string Generate(SupplierRmaCorrectiveAction action,IReadOnlyList<SupplierRmaCorrectiveActionEvent> events,IReadOnlyList<SupplierRmaCorrectiveActionAttachment> attachments)
    {
        var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Accyourate Enterprise X","Fascicoli CAPA RMA");Directory.CreateDirectory(folder);
        var path=Path.Combine(folder,$"Fascicolo-CAPA-{Safe(action.CaseNumber)}-{action.Id}-{DateTime.Now:yyyyMMdd-HHmmss}.zip");var manifest=new List<string>{"File;SHA-256;Dimensione"};
        using var archive=ZipFile.Open(path,ZipArchiveMode.Create);
        AddText(archive,"riepilogo.txt",Summary(action),manifest);
        AddText(archive,"audit-trail.csv",AuditCsv(events),manifest);
        AddText(archive,"indice-evidenze.csv",AttachmentsCsv(attachments),manifest);
        foreach(var attachment in attachments.Where(x=>x.IsAvailable))
        {
            var entryName=$"Evidenze/{attachment.Id}-{Safe(attachment.FileName)}";var bytes=File.ReadAllBytes(attachment.StoredPath);AddBytes(archive,entryName,bytes,manifest);
        }
        AddText(archive,"manifest-sha256.csv",string.Join(Environment.NewLine,manifest),null);
        return path;
    }

    private static string Summary(SupplierRmaCorrectiveAction x)=>$"""
        FASCICOLO CAPA RMA

        Pratica: {x.CaseNumber}
        Azione: {x.Title}
        Descrizione: {x.Description}
        Responsabile: {x.Responsible}
        Priorita: {x.Priority}
        Stato: {x.Status}
        Scadenza: {Date(x.DueDate)}
        Note completamento: {x.VerificationNotes}
        Efficacia: {x.EffectivenessStatus}
        Evidenze efficacia: {x.EffectivenessNotes}
        Verificata il: {DateTimeValue(x.EffectivenessVerifiedAt)}
        Verificata da: {x.EffectivenessVerifiedBy}
        Generato il: {DateTime.Now:dd/MM/yyyy HH:mm}
        """;
    private static string AuditCsv(IEnumerable<SupplierRmaCorrectiveActionEvent> values){var lines=new List<string>{"Data;Evento;Valore precedente;Nuovo valore;Note;Operatore"};lines.AddRange(values.OrderBy(x=>x.CreatedAt).Select(x=>string.Join(";",new[]{DateTimeValue(x.CreatedAt),x.EventType,x.OldValue,x.NewValue,x.Notes,x.CreatedBy}.Select(Csv))));return string.Join(Environment.NewLine,lines);}
    private static string AttachmentsCsv(IEnumerable<SupplierRmaCorrectiveActionAttachment> values){var lines=new List<string>{"Data;Categoria;File;Dimensione;SHA-256;Note;Operatore;Disponibile"};lines.AddRange(values.Select(x=>string.Join(";",new[]{DateTimeValue(x.CreatedAt),x.Category,x.FileName,x.FileSize.ToString(),x.Sha256,x.Notes,x.CreatedBy,x.IsAvailable?"Si":"No"}.Select(Csv))));return string.Join(Environment.NewLine,lines);}
    private static void AddText(ZipArchive archive,string name,string content,List<string>? manifest)=>AddBytes(archive,name,new UTF8Encoding(true).GetBytes(content),manifest);
    private static void AddBytes(ZipArchive archive,string name,byte[] bytes,List<string>? manifest){var entry=archive.CreateEntry(name,CompressionLevel.Optimal);using(var stream=entry.Open())stream.Write(bytes);if(manifest is not null)manifest.Add($"{Csv(name)};{Convert.ToHexString(SHA256.HashData(bytes))};{bytes.Length}");}
    private static string Csv(string value)=>$"\"{(value??"").Replace("\"","\"\"")}\"";private static string Safe(string value){var invalid=Path.GetInvalidFileNameChars();var cleaned=new string(value.Select(x=>invalid.Contains(x)?'_':x).ToArray()).Trim();return string.IsNullOrWhiteSpace(cleaned)?"CAPA":cleaned;}private static string Date(string value)=>DateTime.TryParse(value,out var date)?date.ToString("dd/MM/yyyy"):"Non definita";private static string DateTimeValue(string value)=>DateTime.TryParse(value,out var date)?date.ToString("dd/MM/yyyy HH:mm"):"Non definita";
}
