// Auto-generated from Program.cs
using System.Security.Claims;
using BlazorWebApp.Components;
using BlazorWebApp.Models;
using BlazorWebApp.Modules.Cms;
using BlazorWebApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;

namespace BlazorWebApp.Extensions;

public static class EndpointMappingExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {

        app.MapGet("/api/hello", () => Results.Ok("Hello from API!"));

        // ⬇️ Ensure this is added:
        app.MapCmsEndpoints();
    
        // Blazor render
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
        
        app.MapAdditionalIdentityEndpoints();

        return app;
    }
}
