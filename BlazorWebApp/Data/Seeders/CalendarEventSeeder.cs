using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace BlazorWebApp.Data.Seeders
{
    public static class CalendarEventSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            if (await db.CalendarEvents.AnyAsync())
                return;

            // Use UTC dates to satisfy PostgreSQL's
            // `timestamp with time zone` requirements
            var today = DateTime.UtcNow.Date;
            var pubs = await db.Publications
                .OrderBy(p => p.CreatedAt)
                .Include(p => p.Category)
                .ToListAsync();

            var events = new[]
            {
                new CalendarEvent
                {
                    Id = Guid.NewGuid(),
                    Start = today.AddDays(1).AddHours(9),
                    End = today.AddDays(1).AddHours(11)
                },
                new CalendarEvent
                {
                    Id = Guid.NewGuid(),
                    Start = today.AddDays(2).AddHours(10),
                    End = today.AddDays(2).AddHours(12)
                },
                new CalendarEvent
                {
                    Id = Guid.NewGuid(),
                    Start = today.AddDays(3).AddHours(12),
                    End = today.AddDays(3).AddHours(13)
                },
                new CalendarEvent
                {
                    Id = Guid.NewGuid(),
                    Start = today.AddDays(5),
                    AllDay = true
                },
                new CalendarEvent
                {
                    Id = Guid.NewGuid(),
                    Start = today.AddDays(7),
                    End = today.AddDays(9),
                    AllDay = true
                },
                new CalendarEvent
                {
                    Id = Guid.NewGuid(),
                    Start = today.AddDays(10).AddHours(15),
                    End = today.AddDays(10).AddHours(16)
                },
                new CalendarEvent
                {
                    Id = Guid.NewGuid(),
                    Start = today.AddDays(14).AddHours(11),
                    End = today.AddDays(14).AddHours(12)
                },
                new CalendarEvent
                {
                    Id = Guid.NewGuid(),
                    Start = today.AddDays(20),
                    End = today.AddDays(21),
                    AllDay = true
                }
            };

            for (int i = 0; i < events.Length; i++)
            {
                var pub = pubs[i % pubs.Count];
                events[i].PublicationId = pub.Id;
            }

            db.CalendarEvents.AddRange(events);
            await db.SaveChangesAsync();
        }
    }
}
