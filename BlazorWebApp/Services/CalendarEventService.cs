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
    }
}
