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
                DeltaJson = dto.DeltaJson,
                Html = dto.Html,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _db.Publications.Add(pub);
            await _db.SaveChangesAsync();

            // assign category if provided
            if (dto.CategoryId.HasValue)
            {
                _db.Set<PublicationCategory>().Add(new PublicationCategory
                {
                    PublicationId = pub.Id,
                    CategoryId = dto.CategoryId.Value
                });
                await _db.SaveChangesAsync();
            }

            return ToDto(pub);
        }

        public async Task<List<PublicationReadDto>> ListAsync()
        {
            return await _db.Publications
                .Include(p => p.PublicationCategories)
                    .ThenInclude(pc => pc.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => ToDto(p))
                .ToListAsync();
        }

        public async Task<PublicationReadDto?> GetAsync(Guid id)
        {
            var p = await _db.Publications
                .Include(pu => pu.PublicationCategories)
                    .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(pu => pu.Id == id);
            return p is null ? null : ToDto(p);
        }

        public async Task PublishAsync(Guid id)
        {
            var p = await _db.Publications.FindAsync(id);
            if (p == null) throw new KeyNotFoundException();
            p.Status = PublicationStatus.Published;
            p.PublishedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
        }

        private static PublicationReadDto ToDto(Publication p) => new()
        {
            Id = p.Id,
            Title = p.Title,
            DeltaJson = p.DeltaJson,
            Html = p.Html,
            Status = p.Status.ToString(),
            CreatedAt = p.CreatedAt,
            PublishedAt = p.PublishedAt,
            CategoryId = p.PublicationCategories.FirstOrDefault()?.CategoryId,
            CategoryName = p.PublicationCategories.FirstOrDefault()?.Category.Name
        };

        // ← NEW: allow updating an existing draft
        public async Task UpdateAsync(Guid id, PublicationWriteDto dto)
        {
            var pub = await _db.Publications.FindAsync(id);
            if (pub == null) throw new KeyNotFoundException($"Publication {id} not found");
            pub.Title = dto.Title;
            pub.DeltaJson = dto.DeltaJson;
            pub.Html = dto.Html;
            await _db.SaveChangesAsync();

            // update category mapping
            var existing = _db.Set<PublicationCategory>()
                              .Where(pc => pc.PublicationId == id)
                              .ToList();
            _db.Set<PublicationCategory>().RemoveRange(existing);
            if (dto.CategoryId.HasValue)
            {
                _db.Set<PublicationCategory>().Add(new PublicationCategory
                {
                    PublicationId = id,
                    CategoryId = dto.CategoryId.Value
                });
            }
            await _db.SaveChangesAsync();
        }
    }
}