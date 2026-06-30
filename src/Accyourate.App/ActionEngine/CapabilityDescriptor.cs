namespace Accyourate.App.ActionEngine;

public sealed class CapabilityDescriptor
{
    public string Id { get; init; } = string.Empty;
    public string ModuleId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool RequiresConfirmation { get; init; }
    public string RequiredPermission { get; init; } = string.Empty;
}
