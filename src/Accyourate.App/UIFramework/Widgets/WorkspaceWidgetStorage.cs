using System.Text.Json;

namespace Accyourate.App.UIFramework.Widgets;

public static class WorkspaceWidgetStorage
{
    public static WorkspaceWidgetLayout Load(string userName)
    {
        try
        {
            var path = GetPath(userName);
            if (!File.Exists(path))
                return new WorkspaceWidgetLayout { UserName = userName };

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<WorkspaceWidgetLayout>(json) ?? new WorkspaceWidgetLayout { UserName = userName };
        }
        catch
        {
            return new WorkspaceWidgetLayout { UserName = userName };
        }
    }

    public static void Save(WorkspaceWidgetLayout layout)
    {
        var path = GetPath(layout.UserName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(layout, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static string GetPath(string userName)
    {
        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccyourateEnterpriseX",
            "WorkspaceLayouts");

        return Path.Combine(root, $"{Sanitize(userName)}.json");
    }

    private static string Sanitize(string value)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            value = value.Replace(c, '_');
        return value;
    }
}
