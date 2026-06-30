using System.Text.Json;

namespace Accyourate.App.UIFramework.AI;

public static class AiIntentCatalogStorage
{
    public static IReadOnlyList<AiIntentDefinition> Load()
    {
        try
        {
            var path = GetPath();
            if (!File.Exists(path))
            {
                Save(AiIntentCatalog.Intents);
                return AiIntentCatalog.Intents;
            }

            var json = File.ReadAllText(path);
            var result = JsonSerializer.Deserialize<List<AiIntentDefinition>>(json);

            if (result is null || result.Count == 0)
                return AiIntentCatalog.Intents;

            return result;
        }
        catch
        {
            return AiIntentCatalog.Intents;
        }
    }

    public static void Save(IEnumerable<AiIntentDefinition> intents)
    {
        var path = GetPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(intents, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static void Reset()
    {
        Save(AiIntentCatalog.Intents);
    }

    public static string GetPath()
    {
        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccyourateEnterpriseX",
            "AI");

        return Path.Combine(root, "intent-catalog.json");
    }
}
