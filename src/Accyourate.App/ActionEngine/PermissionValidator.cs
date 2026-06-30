namespace Accyourate.App.ActionEngine;

public sealed class PermissionValidator
{
    public bool CanExecute(ActionContext context, CapabilityDescriptor descriptor)
    {
        // Foundation RC1:
        // in questa fase lasciamo passare le capability in sola lettura.
        // Le azioni distruttive o di modifica verranno vincolate ai permessi granulari nelle prossime RC.
        if (string.IsNullOrWhiteSpace(descriptor.RequiredPermission))
            return true;

        if (descriptor.RequiredPermission.Equals("read", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
}
