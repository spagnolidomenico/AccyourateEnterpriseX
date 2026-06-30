namespace Accyourate.App.ActionEngine;

public sealed class EnterpriseActionEngine
{
    private readonly CapabilityRegistry _registry;
    private readonly PermissionValidator _permissionValidator = new();

    public EnterpriseActionEngine(CapabilityRegistry registry)
    {
        _registry = registry;
    }

    public IReadOnlyList<CapabilityDescriptor> GetCapabilities()
    {
        return _registry.Capabilities;
    }

    public ActionResult Execute(ActionRequest request, ActionContext context)
    {
        var handler = _registry.FindHandler(request.ActionId);
        if (handler is null)
            return ActionResult.Fail($"Capability non trovata: {request.ActionId}");

        if (!_permissionValidator.CanExecute(context, handler.Descriptor))
            return ActionResult.Fail($"Permesso insufficiente per eseguire: {handler.Descriptor.DisplayName}");

        if (handler.Descriptor.RequiresConfirmation)
            return ActionResult.Fail("Questa azione richiede conferma utente. La conferma guidata sarà introdotta nelle prossime RC.");

        return handler.Execute(request, context);
    }
}
