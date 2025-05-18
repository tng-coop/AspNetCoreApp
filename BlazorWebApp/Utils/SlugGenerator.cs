using System.Text;

namespace BlazorWebApp.Utils;

public static class SlugGenerator
{
    public static string Generate(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var slug = text.ToLowerInvariant();
        slug = slug.Replace(" ", "-")
                   .Replace(":", "")
                   .Replace("’", "")
                   .Replace("'", "")
                   .Replace("…", "")
                   .Replace("--", "-")
                   .Trim('-');
        return slug;
    }

    public static bool ContainsOnlyAscii(string text)
    {
        return text.All(c => c <= sbyte.MaxValue);
    }
}
