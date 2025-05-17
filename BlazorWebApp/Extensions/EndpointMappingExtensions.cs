using BlazorWebApp.Components;
using BlazorWebApp.Modules.Cms;

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
