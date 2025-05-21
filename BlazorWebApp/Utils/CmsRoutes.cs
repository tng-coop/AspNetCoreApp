using System.Linq;

namespace BlazorWebApp.Utils;

public static class CmsRoutes
{
    public const string Base = "/_cms";

    public static string Combine(params string[] segments)
    {
        var path = string.Join("/", segments.Where(s => !string.IsNullOrWhiteSpace(s)));
        return string.IsNullOrEmpty(path) ? Base : $"{Base}/{path}";
    }

    /// <summary>
    /// Combine segments with a tenant prefix (e.g., "/_zzz").
    /// </summary>
    public static string CombinePrefixed(string prefix, params string[] segments)
    {
        prefix = prefix?.TrimEnd('/') ?? string.Empty;
        var basePath = string.IsNullOrEmpty(prefix) ? Base : $"{prefix}{Base}";
        var path = string.Join("/", segments.Where(s => !string.IsNullOrWhiteSpace(s)));
        return string.IsNullOrEmpty(path) ? basePath : $"{basePath}/{path}";
    }

    /// <summary>
    /// Extract tenant prefix from a path (e.g., "_zzz/_cms/foo" -> "/_zzz").
    /// </summary>
    public static string ExtractPrefix(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return string.Empty;

        var parts = relativePath.TrimStart('/')
                                 .Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length >= 2 && parts[0].StartsWith("_") && parts[1] == Base.TrimStart('/'))
        {
            return "/" + parts[0];
        }

        return string.Empty;
    }
}
