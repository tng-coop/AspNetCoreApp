using BlazorWebApp.Components;

namespace BlazorWebApp.Extensions;

public static class EndpointMappingExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {

        app.MapGet("/api/hello", () => Results.Ok("Hello from API!"));

        // Additional API endpoints can be added here
    
        // Blazor render
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
        
        app.MapAdditionalIdentityEndpoints();

        return app;
    }
}
