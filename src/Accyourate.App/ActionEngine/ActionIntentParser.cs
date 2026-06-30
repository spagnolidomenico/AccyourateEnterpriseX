using System.Text.RegularExpressions;

namespace Accyourate.App.ActionEngine;

public sealed class ActionIntentParser
{
    public ActionRequest Parse(string? query)
    {
        var q = query ?? string.Empty;
        var lower = q.ToLowerInvariant();

        if (lower.Contains("batteria") &&
    (
        lower.Contains("20") ||
        lower.Contains("bassa") ||
        lower.Contains("sotto")
    ))
        {
            return new ActionRequest
            {
                ActionId = "digital-twin.filter-low-battery",
                ModuleId = "digital-twin",
                Query = q,
                Parameters = ExtractThreshold(q)
            };
        }

        if (lower.Contains("offline"))
        {
            return new ActionRequest
            {
                ActionId = "digital-twin.filter-offline",
                ModuleId = "digital-twin",
                Query = q
            };
        }

        if (lower.Contains("telemetria") || lower.Contains("telemetry"))
        {
            return new ActionRequest
            {
                ActionId = "digital-twin.show-telemetry",
                ModuleId = "digital-twin",
                Query = q,
                Parameters = ExtractDeviceCode(q)
            };
        }

        if (lower.Contains("ecg") || lower.Contains("elettrocardiogramma"))
        {
            return new ActionRequest
            {
                ActionId = "digital-twin.show-ecg",
                ModuleId = "digital-twin",
                Query = q,
                Parameters = ExtractDeviceCode(q)
            };
        }

        if (lower.Contains("apri") && (lower.Contains("digital twin") || lower.Contains("dispositivo") || lower.Contains("capo")))
        {
            return new ActionRequest
            {
                ActionId = "digital-twin.open-device",
                ModuleId = "digital-twin",
                Query = q,
                Parameters = ExtractDeviceCode(q)
            };
        }

        if (lower.Contains("cerca") || lower.Contains("trova") || lower.Contains("mostra"))
        {
            return new ActionRequest
            {
                ActionId = "digital-twin.search-device",
                ModuleId = "digital-twin",
                Query = q,
                Parameters = ExtractDeviceCode(q)
            };
        }

        return new ActionRequest
        {
            ActionId = "workspace.open-module",
            ModuleId = "workspace",
            Query = q,
            Parameters = new Dictionary<string, string> { ["module"] = "digital-twin" }
        };
    }

    private static Dictionary<string, string> ExtractDeviceCode(string query)
    {
        var result = new Dictionary<string, string>();
        var match = Regex.Match(query, @"\b[A-Z]{2,5}\d{2,5}\b", RegexOptions.IgnoreCase);
        if (match.Success)
            result["deviceCode"] = match.Value.ToUpperInvariant();

        return result;
    }

    private static Dictionary<string, string> ExtractThreshold(string query)
    {
        var result = new Dictionary<string, string>();
        var match = Regex.Match(query, @"(\d{1,3})\s*%?");
        result["threshold"] = match.Success ? match.Groups[1].Value : "20";
        return result;
    }
}
