using System.Globalization;
using BlazorWebApp.Models;
using BlazorWebApp.Data;

namespace BlazorWebApp.Utils;

public static class CategoryUtils
{
    public static string LocalizedName(CategoryDto dto)
    {
        if (ShouldUseJapanese())
        {
            if (!string.IsNullOrWhiteSpace(dto.NameJa))
                return dto.NameJa;
            if (!string.IsNullOrWhiteSpace(dto.Name))
                return dto.Name;
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(dto.Name))
                return dto.Name;
            if (!string.IsNullOrWhiteSpace(dto.NameJa))
                return dto.NameJa;
        }
        return dto.Name;
    }

    public static string LocalizedName(Category cat)
    {
        if (ShouldUseJapanese())
        {
            if (!string.IsNullOrWhiteSpace(cat.NameJa))
                return cat.NameJa;
            if (!string.IsNullOrWhiteSpace(cat.Name))
                return cat.Name;
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(cat.Name))
                return cat.Name;
            if (!string.IsNullOrWhiteSpace(cat.NameJa))
                return cat.NameJa;
        }
        return cat.Name;
    }

    private static bool ShouldUseJapanese()
    {
        return CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ja";
    }
}
