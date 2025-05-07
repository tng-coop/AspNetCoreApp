// Auto-generated from Program.cs
using System.Security.Claims;
using BlazorWebApp.Components;
using BlazorWebApp.Models;
using BlazorWebApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;

namespace BlazorWebApp.Extensions;

public static class EndpointMappingExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        // Name and Hello endpoints
        app.MapGet("/api/name/{key}", async (
                string key,
                ClaimsPrincipal user,
                INameService svc) =>
        {
            var value = await svc.GetLatestForNameAsync(key);
            return value is not null
                ? Results.Ok(value)
                : Results.NotFound();
        })
        .RequireAuthorization(JwtBearerDefaults.AuthenticationScheme);
        
        app.MapPut("/api/name/{key}", async (
                string key,
                NameWriteDto dto,
                ClaimsPrincipal user,
                INameService svc) =>
        {
            await svc.SetNameAsync(key, dto.Value, null);
            return Results.NoContent();
        })
        .RequireAuthorization(JwtBearerDefaults.AuthenticationScheme);
        
        app.MapGet("/api/hello", () => Results.Ok("Hello from API!"))
        .RequireAuthorization(JwtBearerDefaults.AuthenticationScheme);

        // Publication endpoints
        app.MapGet("/api/publications", async (IPublicationService svc) =>
            Results.Ok(await svc.ListAsync()))
            .RequireAuthorization(JwtBearerDefaults.AuthenticationScheme);
        
        app.MapGet("/api/publications/{id:guid}", async (Guid id, IPublicationService svc) =>
        {
            var pub = await svc.GetAsync(id);
            return pub is not null ? Results.Ok(pub) : Results.NotFound();
        }).RequireAuthorization(JwtBearerDefaults.AuthenticationScheme);
        
        app.MapPost("/api/publications", async (PublicationWriteDto dto, IPublicationService svc) =>
        {
            var created = await svc.CreateAsync(dto);
            return Results.Created($"/api/publications/{created.Id}", created);
        }).RequireAuthorization(JwtBearerDefaults.AuthenticationScheme);
        
        app.MapPut("/api/publications/{id:guid}", async (Guid id, PublicationWriteDto dto, IPublicationService svc) =>
        {
            await svc.UpdateAsync(id, dto);
            return Results.NoContent();
        }).RequireAuthorization(JwtBearerDefaults.AuthenticationScheme);
        
        app.MapPost("/api/publications/{id:guid}/publish", async (Guid id, IPublicationService svc) =>
        {
            await svc.PublishAsync(id);
            return Results.NoContent();
        }).RequireAuthorization(JwtBearerDefaults.AuthenticationScheme);

        // Blazor render
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
        
        app.MapAdditionalIdentityEndpoints();

        return app;
    }
}
