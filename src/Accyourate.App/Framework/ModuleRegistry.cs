using Accyourate.App.Security;

namespace Accyourate.App.Framework;

public static class ModuleRegistry
{
    public static IReadOnlyList<CrudModuleDefinition> Modules { get; } =
    [
        CrudModuleDefinition.CreateStandard("people", "Persone", PermissionCodes.PeopleView),
        CrudModuleDefinition.CreateStandard("assets", "Asset IT", PermissionCodes.AssetsView),
        CrudModuleDefinition.CreateStandard("medical", "Dispositivi Medici", PermissionCodes.MedicalView),
        CrudModuleDefinition.CreateStandard("documents", "Documenti", PermissionCodes.DocumentsView)
    ];
}
