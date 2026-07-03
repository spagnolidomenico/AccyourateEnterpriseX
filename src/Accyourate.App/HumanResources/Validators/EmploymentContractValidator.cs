using Accyourate.App.HumanResources.Models;
using Accyourate.App.Platform.Validation;

namespace Accyourate.App.HumanResources.Validators;

public sealed class EmploymentContractValidator
{
    public ValidationResult Validate(EmploymentContract contract)
    {
        var result = new ValidationResult();

        if (contract.EmployeeId <= 0)
            result.AddError(nameof(contract.EmployeeId), "CONTRACT_EMPLOYEE_REQUIRED", "Il dipendente è obbligatorio.");

        if (string.IsNullOrWhiteSpace(contract.ContractType))
            result.AddError(nameof(contract.ContractType), "CONTRACT_TYPE_REQUIRED", "Il tipo contratto è obbligatorio.");

        if (string.IsNullOrWhiteSpace(contract.StartDate))
            result.AddError(nameof(contract.StartDate), "CONTRACT_START_DATE_REQUIRED", "La data inizio contratto è obbligatoria.");

        if (DateTime.TryParse(contract.StartDate, out var start) &&
            DateTime.TryParse(contract.EndDate, out var end) &&
            end < start)
            result.AddError(nameof(contract.EndDate), "CONTRACT_END_BEFORE_START", "La data fine contratto non può precedere la data inizio.");

        return result;
    }
}
