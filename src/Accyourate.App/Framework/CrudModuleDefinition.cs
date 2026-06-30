namespace Accyourate.App.Framework;

public sealed class CrudModuleDefinition
{
    public string Code { get; init; } = "";
    public string Title { get; init; } = "";
    public string Permission { get; init; } = "";
    public List<ModuleAction> Actions { get; init; } = new();

    public static CrudModuleDefinition CreateStandard(string code, string title, string permission)
    {
        return new CrudModuleDefinition
        {
            Code = code,
            Title = title,
            Permission = permission,
            Actions =
            {
                new ModuleAction { Code = "new", Title = "Nuovo", Permission = permission },
                new ModuleAction { Code = "edit", Title = "Modifica", Permission = permission },
                new ModuleAction { Code = "archive", Title = "Archivia", Permission = permission },
                new ModuleAction { Code = "qr", Title = "QR Code", Permission = permission },
                new ModuleAction { Code = "label", Title = "Etichetta", Permission = permission },
                new ModuleAction { Code = "excel", Title = "Excel", Permission = permission },
                new ModuleAction { Code = "history", Title = "Storico", Permission = permission }
            }
        };
    }
}
