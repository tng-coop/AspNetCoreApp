using System.Globalization;
using BlazorWebApp.Models;
using BlazorWebApp.Data;

namespace BlazorWebApp.Utils;

public static class PublicationUtils
{
    public static string LocalizedTitle(PublicationReadDto dto)
    {
        return ShouldUseJapanese() && !string.IsNullOrWhiteSpace(dto.TitleJa)
            ? dto.TitleJa
            : dto.Title;
    }

    public static string LocalizedTitle(Publication p)
    {
        return ShouldUseJapanese() && !string.IsNullOrWhiteSpace(p.TitleJa)
            ? p.TitleJa
            : p.Title;
    }

    private static bool ShouldUseJapanese()
    {
        return CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ja";
    }
}
