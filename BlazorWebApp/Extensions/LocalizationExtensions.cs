using BlazorWebApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace BlazorWebApp.Extensions;

public static class LocalizationExtensions
{
    public static IServiceCollection AddLocalizationServices(this IServiceCollection services)
    {
        // Localization and HttpContextAccessor
        services.AddScoped<LocalizationService>();
        services.AddScoped<ProtectedLocalStorage>();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddHttpContextAccessor();
        
        // Configure custom AuthenticationOptions display names
        services.Configure<AuthenticationOptions>(opts =>
        {
            opts.Schemes.First(s => s.Name == "LINE").DisplayName = "LINE";
        });

        return services;
    }
}
