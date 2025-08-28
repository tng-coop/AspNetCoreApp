using BlazorWebApp.Components.Account;
using Microsoft.AspNetCore.Components.Authorization;
using BlazorWebApp.Services;

namespace BlazorWebApp.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Add HTTP and application services
        services.AddHttpClient();
        services.AddScoped<IHostModeService, HostModeService>();
        
        // Add Razor Components and Authentication State
        services.AddRazorComponents()
            .AddInteractiveServerComponents();
        services.AddCascadingAuthenticationState();
        services.AddScoped<IdentityUserAccessor>();
        services.AddScoped<IdentityRedirectManager>();
        services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        return services;
    }
}
