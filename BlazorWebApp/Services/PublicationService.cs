using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlazorWebApp.Data;
using BlazorWebApp.Models;
using BlazorWebApp.Utils;

namespace BlazorWebApp.Services
{
    public class PublicationService : IPublicationService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _factory;
        public PublicationService(IDbContextFactory<ApplicationDbContext> factory) => _factory = factory;

        private ApplicationDbContext CreateDb() => _factory.CreateDbContext();

        public async Task<PublicationReadDto> CreateAsync(PublicationWriteDto dto)
        {
            await using var db = CreateDb();
            var slugBase = string.IsNullOrWhiteSpace(dto.Slug)
                ? dto.Title
                : dto.Slug;
            var slug = await GenerateUniqueSlugAsync(db, SlugGenerator.Generate(slugBase));

            var pub = new Publication
            {
                Id           = Guid.NewGuid(),
                Title        = dto.Title,
                Slug         = slug,
                Html         = dto.Html,
                IsFeatured   = dto.IsFeatured,
                FeaturedOrder = dto.FeaturedOrder,
                CreatedAt    = DateTimeOffset.UtcNow
            };
            db.Publications.Add(pub);
            await db.SaveChangesAsync();

            if (dto.CategoryId.HasValue)
            {
                db.PublicationCategories.Add(new PublicationCategory
                {
                    PublicationId = pub.Id,
                    CategoryId    = dto.CategoryId.Value
                });
                await db.SaveChangesAsync();
            }

            return ToDto(pub);
        }

        public async Task<List<PublicationReadDto>> ListAsync()
        {
            await using var db = CreateDb();
            return await db.Publications
                .Include(p => p.PublicationCategories).ThenInclude(pc => pc.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => ToDto(p))
                .ToListAsync();
        }

        public async Task<PublicationReadDto?> GetAsync(Guid id)
        {
            await using var db = CreateDb();
            var p = await db.Publications
                .Include(pu => pu.PublicationCategories).ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(pu => pu.Id == id);
            return p == null ? null : ToDto(p);
        }

        public async Task<PublicationReadDto?> GetBySlugAsync(string slug)
        {
            await using var db = CreateDb();
            var p = await db.Publications
                .Include(pu => pu.PublicationCategories).ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(pu => pu.Slug == slug);
            return p == null ? null : ToDto(p);
        }

        public async Task PublishAsync(Guid id)
        {
            await using var db = CreateDb();
            var p = await db.Publications.FindAsync(id)
                    ?? throw new KeyNotFoundException();
            p.Status      = PublicationStatus.Published;
            p.PublishedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
        }

        public async Task UnpublishAsync(Guid id)
        {
            await using var db = CreateDb();
            var p = await db.Publications.FindAsync(id)
                    ?? throw new KeyNotFoundException();
            p.Status      = PublicationStatus.Draft;
            p.PublishedAt = null;
            await db.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(Guid id)
        {
            await using var db = CreateDb();
            var p = await db.Publications.FindAsync(id)
                      ?? throw new KeyNotFoundException();
            db.Publications.Remove(p);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid id, PublicationWriteDto dto)
        {
            await using var db = CreateDb();
            var pub = await db.Publications
                .Include(p => p.PublicationCategories)
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new KeyNotFoundException($"Publication {id} not found");

            // 1) snapshot current state:
            var oldCategoryId = pub.PublicationCategories.FirstOrDefault()?.CategoryId;
            db.PublicationRevisions.Add(new PublicationRevision
            {
                Id            = Guid.NewGuid(),
                PublicationId = id,
                Title         = pub.Title,
                Html          = pub.Html,
                CategoryId    = oldCategoryId,
                CreatedAt     = DateTimeOffset.UtcNow
            });

            // 2) apply the update
            pub.Title        = dto.Title;
            pub.Html         = dto.Html;
            pub.IsFeatured   = dto.IsFeatured;
            pub.FeaturedOrder = dto.FeaturedOrder;

            var slugBase = string.IsNullOrWhiteSpace(dto.Slug)
                ? dto.Title
                : dto.Slug;
            pub.Slug = await GenerateUniqueSlugAsync(db,
                SlugGenerator.Generate(slugBase), pub.Id);

            // 3) reassign category
            var existing = db.PublicationCategories.Where(pc => pc.PublicationId == id);
            db.PublicationCategories.RemoveRange(existing);
            if (dto.CategoryId.HasValue)
                db.PublicationCategories.Add(new PublicationCategory
                {
                    PublicationId = id,
                    CategoryId    = dto.CategoryId.Value
                });

            await db.SaveChangesAsync();
        }

        public async Task<List<RevisionDto>> ListRevisionsAsync(Guid publicationId)
        {
            await using var db = CreateDb();
            return await db.PublicationRevisions
                .Where(r => r.PublicationId == publicationId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new RevisionDto
                {
                    Id        = r.Id,
                    CreatedAt = r.CreatedAt,
                    Title     = r.Title
                })
                .ToListAsync();
        }

        public async Task<PublicationReadDto> RestoreRevisionAsync(Guid revisionId)
        {
            await using var db = CreateDb();
            var rev = await db.PublicationRevisions.FindAsync(revisionId)
                      ?? throw new KeyNotFoundException($"Revision {revisionId} not found");

            var pub = await db.Publications
                .Include(p => p.PublicationCategories).ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Id == rev.PublicationId)
                ?? throw new KeyNotFoundException($"Publication {rev.PublicationId} not found");

            // snapshot current state
            db.PublicationRevisions.Add(new PublicationRevision
            {
                Id            = Guid.NewGuid(),
                PublicationId = pub.Id,
                Title         = pub.Title,
                Html          = pub.Html,
                CategoryId    = pub.PublicationCategories.FirstOrDefault()?.CategoryId,
                CreatedAt     = DateTimeOffset.UtcNow
            });

            // apply revision
            pub.Title = rev.Title;
            pub.Html  = rev.Html;

            // reassign category
            var existing = db.PublicationCategories.Where(pc => pc.PublicationId == pub.Id);
            db.PublicationCategories.RemoveRange(existing);
            if (rev.CategoryId.HasValue)
            {
                db.PublicationCategories.Add(new PublicationCategory
                {
                    PublicationId = pub.Id,
                    CategoryId    = rev.CategoryId.Value
                });
            }

            await db.SaveChangesAsync();
            return ToDto(pub);
        }

        private static PublicationReadDto ToDto(Publication p) => new()
        {
            Id           = p.Id,
            Title        = p.Title,
            Slug         = p.Slug,
            Html         = p.Html,
            Status       = p.Status.ToString(),
            IsFeatured   = p.IsFeatured,
            FeaturedOrder = p.FeaturedOrder,
            CreatedAt    = p.CreatedAt,
            PublishedAt  = p.PublishedAt,
           CategoryId   = p.PublicationCategories.FirstOrDefault()?.CategoryId,
            CategoryName = p.PublicationCategories.FirstOrDefault()?.Category.Name,
            CategorySlug = p.PublicationCategories.FirstOrDefault()?.Category.Slug
        };

        private static async Task<string> GenerateUniqueSlugAsync(
            ApplicationDbContext db,
            string baseSlug,
            Guid? excludeId = null)
        {
            var slug = baseSlug;
            var counter = 1;
            while (await db.Publications.AnyAsync(p => p.Slug == slug &&
                                                    (excludeId == null || p.Id != excludeId)))
            {
                slug = $"{baseSlug}-{counter++}";
            }
            return slug;
        }
           public async Task<List<PublicationReadDto>> ListFeaturedInCategoryAsync(Guid categoryId)
    {
        await using var db = CreateDb();
        return await db.Publications
            .Where(p => p.Status == PublicationStatus.Published
                     && p.PublicationCategories.Any(pc => pc.CategoryId == categoryId)
                     && p.IsFeatured)
            .OrderBy(p => p.FeaturedOrder)
            .ThenByDescending(p => p.PublishedAt)
            .Select(p => ToDto(p))
            .ToListAsync();
    }
    }
}
