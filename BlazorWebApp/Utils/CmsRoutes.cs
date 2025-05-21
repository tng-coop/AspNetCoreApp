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
}
