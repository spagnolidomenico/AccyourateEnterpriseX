using Accyourate.App.AssetManagement.Services;
using Accyourate.App.EnterpriseMasterData.Models;
using Accyourate.App.EnterpriseMasterData.Services;
using Accyourate.App.HumanResources.Models;

namespace Accyourate.App.HumanResources.Services;

public sealed class EmployeeAssetService
{
    private readonly MasterDataService _masterDataService;
    private readonly AssetAssignmentEngine _assignmentEngine;

    public EmployeeAssetService(MasterDataService? masterDataService = null, AssetAssignmentEngine? assignmentEngine = null)
    {
        _masterDataService = masterDataService ?? new MasterDataService();
        _assignmentEngine = assignmentEngine ?? new AssetAssignmentEngine(masterDataService: _masterDataService);
    }

    public EmployeeAssetProfile GetProfile(Employee employee)
    {
        var masterEmployee = FindMasterEmployee(employee);

        if (masterEmployee is null)
        {
            return new EmployeeAssetProfile
            {
                IsLinkedToMasterData = false,
                Message = "Dipendente non ancora collegato all'Anagrafica Aziendale. Per vedere gli asset assegnati, crea o sincronizza il dipendente anche in Anagrafica Aziendale."
            };
        }

        var assignments = _assignmentEngine.GetActiveAssignmentsForEmployee(masterEmployee.Id);

        return new EmployeeAssetProfile
        {
            IsLinkedToMasterData = true,
            MasterEmployeeId = masterEmployee.Id,
            MasterEmployeeName = masterEmployee.FullName,
            Assignments = assignments,
            Message = assignments.Count == 0
                ? "Nessun asset attualmente assegnato."
                : $"{assignments.Count} asset attualmente assegnati."
        };
    }

    private EmployeeMasterData? FindMasterEmployee(Employee employee)
    {
        var candidates = _masterDataService.GetEmployees();

        if (!string.IsNullOrWhiteSpace(employee.Email))
        {
            var byEmail = candidates.FirstOrDefault(x =>
                string.Equals(x.Email, employee.Email, StringComparison.OrdinalIgnoreCase));

            if (byEmail is not null)
                return byEmail;
        }

        return candidates.FirstOrDefault(x =>
            string.Equals(Normalize(x.FullName), Normalize(employee.FullName), StringComparison.OrdinalIgnoreCase));
    }

    private static string Normalize(string value)
    {
        return string.Join(" ", value
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Trim();
    }
}

public sealed class EmployeeAssetProfile
{
    public bool IsLinkedToMasterData { get; set; }
    public int MasterEmployeeId { get; set; }
    public string MasterEmployeeName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public IReadOnlyList<AssetAssignmentSummary> Assignments { get; set; } = Array.Empty<AssetAssignmentSummary>();
}
