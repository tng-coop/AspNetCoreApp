using System;

namespace BlazorWebApp.Utils
{
    public static class SlugUtils
    {
        /// <summary>
        /// Normalize slug values according to project rules.
        /// If the slug is null/whitespace or starts with an underscore,
        /// it is replaced with the literal "default".
        /// </summary>
        public static string Normalize(string? slug)
        {
            if (string.IsNullOrWhiteSpace(slug) || slug.StartsWith("_"))
                return "default";
            return slug;
        }

        /// <summary>
        /// Validate that a slug contains only ASCII letters, digits and hyphens.
        /// </summary>
        public static bool IsAsciiSlug(string slug)
        {
            foreach (var ch in slug)
            {
                bool letterOrDigit =
                    (ch >= 'a' && ch <= 'z') ||
                    (ch >= 'A' && ch <= 'Z') ||
                    (ch >= '0' && ch <= '9');
                if (!(letterOrDigit || ch == '-'))
                    return false;
            }
            return true;
        }
    }
}
