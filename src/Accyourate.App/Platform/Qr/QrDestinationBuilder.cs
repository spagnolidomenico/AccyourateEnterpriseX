using Accyourate.App.Platform.Settings;

namespace Accyourate.App.Platform.Qr;

public static class QrDestinationBuilder
{
    public static string Build(
        DocumentTemplateSettings template,
        string resource,
        string identifier,
        IEnumerable<string> fallbackLines)
    {
        if (TryBuildWebUrl(template.QrBaseUrl, resource, identifier, out var url))
            return url;

        return string.Join("\n", fallbackLines.Where(line => !string.IsNullOrWhiteSpace(line)));
    }

    private static bool TryBuildWebUrl(string? baseUrl, string resource, string identifier, out string url)
    {
        url = string.Empty;
        if (!Uri.TryCreate(baseUrl?.Trim(), UriKind.Absolute, out var root) ||
            (root.Scheme != Uri.UriSchemeHttps && root.Scheme != Uri.UriSchemeHttp))
            return false;

        var normalizedRoot = root.AbsoluteUri.TrimEnd('/') + "/";
        var relativePath = $"{resource.Trim('/')}/{Uri.EscapeDataString(identifier.Trim())}";
        url = new Uri(new Uri(normalizedRoot), relativePath).AbsoluteUri;
        return true;
    }
}
