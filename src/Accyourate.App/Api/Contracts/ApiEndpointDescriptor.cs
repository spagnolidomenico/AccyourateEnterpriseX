namespace Accyourate.App.Api.Contracts;

public sealed class ApiEndpointDescriptor
{
    public string Method { get; set; } = "";
    public string Route { get; set; } = "";
    public string Description { get; set; } = "";
    public string Status { get; set; } = "Planned";
}
