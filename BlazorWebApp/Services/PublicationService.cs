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
                CreatedAt = DateTimeOffset.UtcNow
            };
            _db.Publications.Add(pub);
            await _db.SaveChangesAsync();
            return ToDto(pub);
        }

        public async Task<List<PublicationReadDto>> ListAsync()
        {
            return await _db.Publications
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => ToDto(p))
                .ToListAsync();
        }

        public async Task<PublicationReadDto?> GetAsync(Guid id)
        {
            var p = await _db.Publications.FindAsync(id);
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
            Html = p.Html,
            Status = p.Status.ToString(),
            CreatedAt = p.CreatedAt,
            PublishedAt = p.PublishedAt
        };
    }
}
