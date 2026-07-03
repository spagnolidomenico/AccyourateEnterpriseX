namespace Accyourate.App.Platform.Events;

public sealed class PlatformEvent
{
    public string Name { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = DateTime.Now.ToString("s");
    public string Payload { get; set; } = string.Empty;
}
