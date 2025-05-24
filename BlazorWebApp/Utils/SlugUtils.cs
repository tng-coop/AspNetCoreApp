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
    }
}
