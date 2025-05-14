using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlazorWebApp.Data;
using BlazorWebApp.Models;

namespace BlazorWebApp.Services
{
    public class PublicationService : IPublicationService
    {
        private readonly ApplicationDbContext _db;
        public PublicationService(ApplicationDbContext db) => _db = db;

        public async Task<PublicationReadDto> CreateAsync(PublicationWriteDto dto)
        {
            var pub = new Publication
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Html = dto.Html,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _db.Publications.Add(pub);
            await _db.SaveChangesAsync();

            if (dto.CategoryId.HasValue)
            {
                _db.PublicationCategories.Add(new PublicationCategory
                {
                    PublicationId = pub.Id,
                    CategoryId    = dto.CategoryId.Value
                });
                await _db.SaveChangesAsync();
            }

            return ToDto(pub);
        }

        public async Task<List<PublicationReadDto>> ListAsync()
        {
            return await _db.Publications
                .Include(p => p.PublicationCategories).ThenInclude(pc => pc.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => ToDto(p))
                .ToListAsync();
        }

        public async Task<PublicationReadDto?> GetAsync(Guid id)
        {
            var p = await _db.Publications
                .Include(pu => pu.PublicationCategories).ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(pu => pu.Id == id);
            return p == null ? null : ToDto(p);
        }

        public async Task PublishAsync(Guid id)
        {
            var p = await _db.Publications.FindAsync(id)
                    ?? throw new KeyNotFoundException();
            p.Status      = PublicationStatus.Published;
            p.PublishedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task UnpublishAsync(Guid id)
        {
            var p = await _db.Publications.FindAsync(id)
                    ?? throw new KeyNotFoundException();
            p.Status      = PublicationStatus.Draft;
            p.PublishedAt = null;
            await _db.SaveChangesAsync();
        }

public async Task UpdateAsync(Guid id, PublicationWriteDto dto)
{
    var pub = await _db.Publications
        .Include(p => p.PublicationCategories)
        .FirstOrDefaultAsync(p => p.Id == id)
        ?? throw new KeyNotFoundException($"Publication {id} not found");

    // 1) snapshot current state:
    var oldCategoryId = pub.PublicationCategories.FirstOrDefault()?.CategoryId;
    _db.PublicationRevisions.Add(new PublicationRevision {
        Id            = Guid.NewGuid(),
        PublicationId = id,
        Title         = pub.Title,
        Html          = pub.Html,
        CategoryId    = oldCategoryId,
        CreatedAt     = DateTimeOffset.UtcNow
    });

    // 2) apply the update
    pub.Title = dto.Title;
    pub.Html  = dto.Html;

    // 3) reassign category
    var existing = _db.PublicationCategories.Where(pc => pc.PublicationId == id);
    _db.PublicationCategories.RemoveRange(existing);
    if (dto.CategoryId.HasValue)
        _db.PublicationCategories.Add(new PublicationCategory {
            PublicationId = id,
            CategoryId    = dto.CategoryId.Value
        });

    await _db.SaveChangesAsync();
}


        // ——— Revision history ———

        public async Task<List<RevisionDto>> ListRevisionsAsync(Guid publicationId)
        {
            return await _db.PublicationRevisions
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
            var rev = await _db.PublicationRevisions.FindAsync(revisionId)
                      ?? throw new KeyNotFoundException($"Revision {revisionId} not found");

            var pub = await _db.Publications
                .Include(p => p.PublicationCategories).ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Id == rev.PublicationId)
                ?? throw new KeyNotFoundException($"Publication {rev.PublicationId} not found");

            // snapshot current state
            _db.PublicationRevisions.Add(new PublicationRevision
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
            var existing = _db.PublicationCategories.Where(pc => pc.PublicationId == pub.Id);
            _db.PublicationCategories.RemoveRange(existing);
            if (rev.CategoryId.HasValue)
            {
                _db.PublicationCategories.Add(new PublicationCategory
                {
                    PublicationId = pub.Id,
                    CategoryId    = rev.CategoryId.Value
                });
            }

            await _db.SaveChangesAsync();
            return ToDto(pub);
        }

        private static PublicationReadDto ToDto(Publication p) => new()
        {
            Id           = p.Id,
            Title        = p.Title,
            Html         = p.Html,
            Status       = p.Status.ToString(),
            CreatedAt    = p.CreatedAt,
            PublishedAt  = p.PublishedAt,
            CategoryId   = p.PublicationCategories.FirstOrDefault()?.CategoryId,
            CategoryName = p.PublicationCategories.FirstOrDefault()?.Category.Name
        };
    }
}
