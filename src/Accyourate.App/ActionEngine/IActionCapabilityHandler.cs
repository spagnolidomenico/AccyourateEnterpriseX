namespace Accyourate.App.ActionEngine;

public interface IActionCapabilityHandler
{
    CapabilityDescriptor Descriptor { get; }
    ActionResult Execute(ActionRequest request, ActionContext context);
}
