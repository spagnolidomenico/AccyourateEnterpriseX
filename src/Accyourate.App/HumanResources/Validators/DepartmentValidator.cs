using Accyourate.App.HumanResources.Models;
using Accyourate.App.Platform.Validation;

namespace Accyourate.App.HumanResources.Validators;

public sealed class DepartmentValidator
{
    public ValidationResult Validate(Department department)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(department.Code))
            result.AddError(nameof(department.Code), "DEPT_CODE_REQUIRED", "Il codice reparto è obbligatorio.");

        if (string.IsNullOrWhiteSpace(department.Name))
            result.AddError(nameof(department.Name), "DEPT_NAME_REQUIRED", "Il nome reparto è obbligatorio.");

        if (department.SiteId <= 0)
            result.AddError(nameof(department.SiteId), "DEPT_SITE_REQUIRED", "La sede del reparto è obbligatoria.");

        return result;
    }
}
