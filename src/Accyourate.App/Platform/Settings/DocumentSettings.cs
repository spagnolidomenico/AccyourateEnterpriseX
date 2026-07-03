namespace Accyourate.App.Platform.Settings;

public sealed class DocumentSettings
{
    public string DocumentRootPath { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "Accyourate Enterprise X");

    public string DeliveryReportsFolderName { get; set; } = "Verbali Consegna";
    public string HrDocumentsFolderName { get; set; } = "Documenti HR";
    public string AssetDocumentsFolderName { get; set; } = "Documenti Asset";
}
