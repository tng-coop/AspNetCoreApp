using System.Globalization;
using BlazorWebApp.Models;
using BlazorWebApp.Data;

namespace BlazorWebApp.Utils;

public static class CategoryUtils
{
    public static string LocalizedName(CategoryDto dto)
    {
        return ShouldUseJapanese()
            && !string.IsNullOrWhiteSpace(dto.NameJa)
            ? dto.NameJa
            : dto.Name;
    }

    public static string LocalizedName(Category cat)
    {
        return ShouldUseJapanese()
            && !string.IsNullOrWhiteSpace(cat.NameJa)
            ? cat.NameJa
            : cat.Name;
    }

    private static bool ShouldUseJapanese()
    {
        return CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ja";
    }
}
