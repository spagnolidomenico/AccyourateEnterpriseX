namespace Accyourate.App.ActionEngine;

public sealed class CapabilityRegistry
{
    private readonly List<IActionCapabilityHandler> _handlers = new();

    public void Register(IActionCapabilityHandler handler)
    {
        if (_handlers.Any(h => h.Descriptor.Id == handler.Descriptor.Id))
            return;

        _handlers.Add(handler);
    }

    public IReadOnlyList<CapabilityDescriptor> Capabilities => _handlers.Select(h => h.Descriptor).ToList();

    public IActionCapabilityHandler? FindHandler(string actionId)
    {
        return _handlers.FirstOrDefault(h => h.Descriptor.Id.Equals(actionId, StringComparison.OrdinalIgnoreCase));
    }
}
