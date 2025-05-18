using System;
using Microsoft.AspNetCore.Builder;
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
            // List all
            endpoints.MapGet("/api/publications", async (IPublicationService svc) =>
                Ok(await svc.ListAsync()));

            // Get by ID
            endpoints.MapGet("/api/publications/{id:guid}", async (Guid id, IPublicationService svc) =>
            {
                var pub = await svc.GetAsync(id);
                return pub != null ? Ok(pub) : NotFound();
            });

            // Create
            endpoints.MapPost("/api/publications", async (PublicationWriteDto dto, IPublicationService svc) =>
            {
                var created = await svc.CreateAsync(dto);
                return Created($"/api/publications/{created.Id}", created);
            });

            // Update
            endpoints.MapPut("/api/publications/{id:guid}", async (Guid id, PublicationWriteDto dto, IPublicationService svc) =>
            {
                await svc.UpdateAsync(id, dto);
                return NoContent();
            });

            // Publish
            endpoints.MapPost("/api/publications/{id:guid}/publish", async (Guid id, IPublicationService svc) =>
            {
                await svc.PublishAsync(id);
                return NoContent();
            });

            // Unpublish
            endpoints.MapPost("/api/publications/{id:guid}/unpublish", async (Guid id, IPublicationService svc) =>
            {
                await svc.UnpublishAsync(id);
                return NoContent();
            });

            // Delete
            endpoints.MapDelete("/api/publications/{id:guid}", async (Guid id, IPublicationService svc) =>
            {
                await svc.DeleteAsync(id);
                return NoContent();
            });

            // Revision history
            endpoints.MapGet("/api/publications/{id:guid}/revisions", async (Guid id, IPublicationService svc) =>
            {
                var revs = await svc.ListRevisionsAsync(id);
                return Ok(revs);
            });

            // Restore a revision
            endpoints.MapPost("/api/publications/revisions/{revisionId:guid}/restore", async (Guid revisionId, IPublicationService svc) =>
            {
                var restored = await svc.RestoreRevisionAsync(revisionId);
                return Ok(restored);
            });

            // Featured in category
            endpoints.MapGet("/api/categories/{categoryId:guid}/publications/featured", async (Guid categoryId, IPublicationService svc) =>
            {
                var featured = await svc.ListFeaturedInCategoryAsync(categoryId);
                return Ok(featured);
            });

            return endpoints;
        }
    }
}
