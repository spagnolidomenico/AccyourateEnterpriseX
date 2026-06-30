namespace Accyourate.App.Data;

public sealed class AnalyticsKpiRecord
{
    public string Code { get; set; } = "";
    public string Title { get; set; } = "";
    public string Value { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string Area { get; set; } = "";
    public string Severity { get; set; } = "Info";
}
