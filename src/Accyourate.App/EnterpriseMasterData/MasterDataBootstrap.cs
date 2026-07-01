using Accyourate.App.EnterpriseMasterData.Services;

namespace Accyourate.App.EnterpriseMasterData;

public static class MasterDataBootstrap
{
    public static (int Companies, int Sites, int Departments, int Employees, int Suppliers) EnsureInitialized()
    {
        var service = new MasterDataService();

        return (
            service.CountCompanies(),
            service.CountSites(),
            service.CountDepartments(),
            service.CountEmployees(),
            service.CountSuppliers());
    }
}
