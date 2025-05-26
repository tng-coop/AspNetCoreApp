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
    public class CalendarEventService : ICalendarEventService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _factory;
        public CalendarEventService(IDbContextFactory<ApplicationDbContext> factory)
        {
            _factory = factory;
        }

        private ApplicationDbContext CreateDb() => _factory.CreateDbContext();

        public async Task<List<CalendarEventDto>> ListAsync()
        {
            await using var db = CreateDb();
            var entities = await db.CalendarEvents
                .Include(e => e.Publication)
                    .ThenInclude(p => p.Category)
                .OrderBy(e => e.Start)
                .ToListAsync();

            return entities.Select(e => new CalendarEventDto
            {
                Title = e.Publication != null
                    ? PublicationUtils.LocalizedTitle(e.Publication)
                    : string.Empty,
                Start = e.AllDay
                    ? e.Start.ToString("yyyy-MM-dd")
                    : e.Start.ToString("yyyy-MM-dd'T'HH:mm:ss"),
                End = e.End.HasValue
                    ? e.AllDay
                        ? e.End.Value.ToString("yyyy-MM-dd")
                        : e.End.Value.ToString("yyyy-MM-dd'T'HH:mm:ss")
                    : null,
                AllDay = e.AllDay ? true : (bool?)null,
                Url = e.Publication != null
                    ? Utils.CmsRoutes.Combine(e.Publication.Category.Slug, e.Publication.Slug)
                    : e.Url
            }).ToList();
        }

        public async Task<List<CalendarEventDto>> ListByPublicationAsync(Guid publicationId)
        {
            await using var db = CreateDb();
            var entities = await db.CalendarEvents
                .Where(e => e.PublicationId == publicationId)
                .Include(e => e.Publication)
                    .ThenInclude(p => p.Category)
                .OrderBy(e => e.Start)
                .ToListAsync();

            return entities.Select(e => new CalendarEventDto
            {
                Title = e.Publication != null
                    ? PublicationUtils.LocalizedTitle(e.Publication)
                    : string.Empty,
                Start = e.AllDay
                    ? e.Start.ToString("yyyy-MM-dd")
                    : e.Start.ToString("yyyy-MM-dd'T'HH:mm:ss"),
                End = e.End.HasValue
                    ? e.AllDay
                        ? e.End.Value.ToString("yyyy-MM-dd")
                        : e.End.Value.ToString("yyyy-MM-dd'T'HH:mm:ss")
                    : null,
                AllDay = e.AllDay ? true : (bool?)null,
                Url = e.Publication != null
                    ? Utils.CmsRoutes.Combine(e.Publication.Category.Slug, e.Publication.Slug)
                    : e.Url
            }).ToList();
        }

        private static CalendarEventEditDto ToEditDto(CalendarEvent e) => new()
        {
            Id = e.Id,
            Start = e.Start,
            End = e.End,
            AllDay = e.AllDay,
            Url = e.Url
        };

        public async Task<List<CalendarEventEditDto>> ListForPublicationAsync(Guid publicationId)
        {
            await using var db = CreateDb();
            var entities = await db.CalendarEvents
                .Where(e => e.PublicationId == publicationId)
                .OrderBy(e => e.Start)
                .ToListAsync();

            return entities.Select(ToEditDto).ToList();
        }

        public async Task<CalendarEventEditDto> CreateAsync(Guid publicationId, CalendarEventWriteDto dto)
        {
            await using var db = CreateDb();
            var entity = new CalendarEvent
            {
                Id = Guid.NewGuid(),
                PublicationId = publicationId,
                Start = dto.Start,
                End = dto.End,
                AllDay = dto.AllDay,
                Url = dto.Url
            };
            db.CalendarEvents.Add(entity);
            await db.SaveChangesAsync();
            return ToEditDto(entity);
        }

        public async Task<CalendarEventEditDto> UpdateAsync(Guid id, CalendarEventWriteDto dto)
        {
            await using var db = CreateDb();
            var entity = await db.CalendarEvents.FindAsync(id) ?? throw new KeyNotFoundException();
            entity.Start = dto.Start;
            entity.End = dto.End;
            entity.AllDay = dto.AllDay;
            entity.Url = dto.Url;
            await db.SaveChangesAsync();
            return ToEditDto(entity);
        }

        public async Task DeleteAsync(Guid id)
        {
            await using var db = CreateDb();
            var entity = await db.CalendarEvents.FindAsync(id);
            if (entity != null)
            {
                db.CalendarEvents.Remove(entity);
                await db.SaveChangesAsync();
            }
        }
    }
}
