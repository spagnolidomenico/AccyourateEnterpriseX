using System.Text.RegularExpressions;
using Accyourate.App.HumanResources.Models;
using Accyourate.App.Platform.Validation;

namespace Accyourate.App.HumanResources.Validators;

public sealed class EmployeeValidator
{
    public ValidationResult Validate(Employee employee)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(employee.FirstName))
            result.AddError(nameof(employee.FirstName), "EMP_FIRST_NAME_REQUIRED", "Il nome è obbligatorio.");

        if (string.IsNullOrWhiteSpace(employee.LastName))
            result.AddError(nameof(employee.LastName), "EMP_LAST_NAME_REQUIRED", "Il cognome è obbligatorio.");

        if (!string.IsNullOrWhiteSpace(employee.Email) && !IsValidEmail(employee.Email))
            result.AddError(nameof(employee.Email), "EMP_EMAIL_INVALID", "L'indirizzo email non è valido.");

        if (!string.IsNullOrWhiteSpace(employee.EmployeeCode) &&
            !Regex.IsMatch(employee.EmployeeCode, @"^[A-Z0-9\-]{3,30}$", RegexOptions.IgnoreCase))
            result.AddError(nameof(employee.EmployeeCode), "EMP_CODE_INVALID", "Il codice dipendente può contenere solo lettere, numeri e trattini.");

        if (employee.RoleId <= 0)
            result.AddError(nameof(employee.RoleId), "EMP_ROLE_REQUIRED", "Il ruolo è obbligatorio.");

        if (employee.DepartmentId <= 0)
            result.AddError(nameof(employee.DepartmentId), "EMP_DEPARTMENT_REQUIRED", "Il reparto è obbligatorio.");

        if (employee.SiteId <= 0)
            result.AddError(nameof(employee.SiteId), "EMP_SITE_REQUIRED", "La sede è obbligatoria.");

        if (!string.IsNullOrWhiteSpace(employee.TerminationDate) &&
            string.IsNullOrWhiteSpace(employee.HireDate))
            result.AddWarning(nameof(employee.TerminationDate), "EMP_TERMINATION_WITHOUT_HIRE_DATE", "È stata indicata una data cessazione senza data assunzione.");

        if (DateTime.TryParse(employee.HireDate, out var hireDate) &&
            DateTime.TryParse(employee.TerminationDate, out var terminationDate) &&
            terminationDate < hireDate)
            result.AddError(nameof(employee.TerminationDate), "EMP_TERMINATION_BEFORE_HIRE", "La data cessazione non può precedere la data assunzione.");

        return result;
    }

    private static bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
}
