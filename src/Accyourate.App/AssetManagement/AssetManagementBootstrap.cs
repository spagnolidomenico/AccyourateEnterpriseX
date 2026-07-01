using Accyourate.App.AssetManagement.Services;

namespace Accyourate.App.AssetManagement;

public static class AssetManagementBootstrap
{
    public static int EnsureInitialized()
    {
        var service = new AssetService();
        return service.CountAssets();
    }
}
