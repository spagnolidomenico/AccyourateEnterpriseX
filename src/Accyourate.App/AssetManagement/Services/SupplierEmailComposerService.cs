using System.Text;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SupplierEmailComposerService
{
    public string CreateDraft(string to,string subject,string body,IEnumerable<string> attachmentPaths)
    {
        if(string.IsNullOrWhiteSpace(to))throw new InvalidOperationException("Il contatto selezionato non dispone di un indirizzo e-mail.");
        var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"AccyourateEnterpriseX","SupplierPortal","EmailDrafts");
        Directory.CreateDirectory(folder);var boundary="----Accyourate-"+Guid.NewGuid().ToString("N");var files=attachmentPaths.Where(File.Exists).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var sb=new StringBuilder();sb.AppendLine($"To: {Header(to)}");sb.AppendLine($"Subject: {Header(subject)}");sb.AppendLine("MIME-Version: 1.0");sb.AppendLine($"Content-Type: multipart/mixed; boundary=\"{boundary}\"");sb.AppendLine();
        sb.AppendLine($"--{boundary}");sb.AppendLine("Content-Type: text/plain; charset=utf-8");sb.AppendLine("Content-Transfer-Encoding: base64");sb.AppendLine();AppendBase64(sb,Encoding.UTF8.GetBytes(body));
        foreach(var path in files){var name=Path.GetFileName(path);sb.AppendLine($"--{boundary}");sb.AppendLine($"Content-Type: application/octet-stream; name=\"{Header(name)}\"");sb.AppendLine("Content-Transfer-Encoding: base64");sb.AppendLine($"Content-Disposition: attachment; filename=\"{Header(name)}\"");sb.AppendLine();AppendBase64(sb,File.ReadAllBytes(path));}
        sb.AppendLine($"--{boundary}--");var safe=string.Concat(subject.Select(c=>Path.GetInvalidFileNameChars().Contains(c)?'_':c));if(safe.Length>60)safe=safe[..60];var destination=Path.Combine(folder,$"{DateTime.Now:yyyyMMdd-HHmmss}-{safe}.eml");File.WriteAllText(destination,sb.ToString(),new UTF8Encoding(false));return destination;
    }
    public static string ApplyTemplate(string template,string supplier,string contact,string rma,string dueDate)=>NormalizeLines(template).Replace("{FORNITORE}",supplier,StringComparison.OrdinalIgnoreCase).Replace("{CONTATTO}",contact,StringComparison.OrdinalIgnoreCase).Replace("{RMA}",rma,StringComparison.OrdinalIgnoreCase).Replace("{SCADENZA}",dueDate,StringComparison.OrdinalIgnoreCase);
    public static string NormalizeLines(string value)=>value.Replace("\\r\\n","\n",StringComparison.Ordinal).Replace("\\n","\n",StringComparison.Ordinal).Replace("\r\n","\n",StringComparison.Ordinal).Replace("\n",Environment.NewLine,StringComparison.Ordinal);
    private static string Header(string value)=>value.Replace("\r"," ").Replace("\n"," ").Replace("\"","'");
    private static void AppendBase64(StringBuilder sb,byte[] bytes){var text=Convert.ToBase64String(bytes);for(var i=0;i<text.Length;i+=76)sb.AppendLine(text.Substring(i,Math.Min(76,text.Length-i)));}
}
