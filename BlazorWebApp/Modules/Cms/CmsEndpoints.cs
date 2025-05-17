// Modules/Cms/CmsEndpoints.cs
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using BlazorWebApp.Models;
using BlazorWebApp.Services;
using static Microsoft.AspNetCore.Http.Results;

namespace BlazorWebApp.Modules.Cms
{
    /// <summary>
    /// Registers CMS-related HTTP endpoints (e.g. publications API).
    /// </summary>
    public static class CmsEndpoints
    {
        public static IEndpointRouteBuilder MapCmsEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/api/publications", async (IPublicationService svc) => Ok(await svc.ListAsync()));

            endpoints.MapGet("/api/publications/{id:guid}", async (Guid id, IPublicationService svc) =>
            {
                var pub = await svc.GetAsync(id);
                return pub != null ? Ok(pub) : NotFound();
            });

            endpoints.MapPost("/api/publications", async (PublicationWriteDto dto, IPublicationService svc) =>
            {
                var created = await svc.CreateAsync(dto);
                return Created($"/api/publications/{created.Id}", created);
            });

            endpoints.MapPut("/api/publications/{id:guid}", async (Guid id, PublicationWriteDto dto, IPublicationService svc) =>
            {
                await svc.UpdateAsync(id, dto);
                return NoContent();
            });

            endpoints.MapPost("/api/publications/{id:guid}/publish", async (Guid id, IPublicationService svc) =>
            {
                await svc.PublishAsync(id);
                return NoContent();
            });

            return endpoints;
        }
    }
}
