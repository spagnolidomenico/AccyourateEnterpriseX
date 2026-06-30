namespace Accyourate.App.Data;

public static class AppPaths
{
    public static string DataDirectory
    {
        get
        {
            var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var dir = Path.Combine(programData, "Accyourate Enterprise X", "data");
            Directory.CreateDirectory(dir);
            return dir;
        }
    }

    public static string DatabasePath => Path.Combine(DataDirectory, "accyourate_x.db");
}
