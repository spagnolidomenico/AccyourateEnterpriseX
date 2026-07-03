namespace Accyourate.App.Platform.Settings;

public sealed class NumberingSettings
{
    public string EmployeePrefix { get; set; } = "EMP";
    public string AssetPrefix { get; set; } = "AST";
    public string DeliveryReportPrefix { get; set; } = "VRB";
    public string DocumentPrefix { get; set; } = "DOC";
    public int Padding { get; set; } = 6;
    public bool IncludeYearInDeliveryReports { get; set; } = true;
}
