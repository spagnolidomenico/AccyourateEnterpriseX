namespace Accyourate.Infrastructure.Data;

public static class AccyourateDatabaseContextFactory
{
    public static AccyourateDatabaseContext CreateAssetManagementContext()
    {
        return new AccyourateDatabaseContext(new AccyourateDatabaseOptions
        {
            DatabaseName = AccyourateDatabaseNames.AssetManagement
        });
    }

    public static AccyourateDatabaseContext CreateMasterDataContext()
    {
        return new AccyourateDatabaseContext(new AccyourateDatabaseOptions
        {
            DatabaseName = AccyourateDatabaseNames.MasterData
        });
    }

    public static string GetAppDataFolder()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccyourateEnterpriseX");
    }
}
