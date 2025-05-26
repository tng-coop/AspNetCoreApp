using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlazorWebApp.Data;

namespace BlazorWebApp.Services
{
    public static class SlugService
    {
        public static async Task<string> GenerateUniqueSlugAsync(
            ApplicationDbContext db,
            string baseSlug,
            Guid? excludeEntityId = null,
            string? excludeEntityType = null)
        {
            var slug = baseSlug;
            var counter = 1;
            while (await db.Slugs.AnyAsync(s => s.Value == slug &&
                                               !(excludeEntityId.HasValue &&
                                                 s.EntityId == excludeEntityId.Value &&
                                                 s.EntityType == excludeEntityType)))
            {
                slug = $"{baseSlug}-{counter++}";
            }
            return slug;
        }

        public static async Task UpsertSlugRecordAsync(
            ApplicationDbContext db,
            Guid entityId,
            string entityType,
            string slug)
        {
            var record = await db.Slugs.FirstOrDefaultAsync(s => s.EntityId == entityId && s.EntityType == entityType);
            if (record == null)
            {
                db.Slugs.Add(new SlugRecord
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    EntityType = entityType,
                    Value = slug
                });
            }
            else
            {
                record.Value = slug;
            }
        }
    }
}
