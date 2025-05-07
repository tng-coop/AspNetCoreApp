// Auto-generated from Program.cs
using BlazorWebApp.Components.Account;
using BlazorWebApp.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorWebApp.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Add HTTP and application services
        services.AddHttpClient();
        services.AddScoped<INameService, NameService>();
        services.AddScoped<INoteService, NoteService>();
        services.AddScoped<IPublicationService, PublicationService>();
        // register category service
        services.AddScoped<ICategoryService, CategoryService>();

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
