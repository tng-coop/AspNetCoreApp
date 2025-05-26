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
            var events = new[]
            {
                new CalendarEvent
                {
                    Id = Guid.NewGuid(),
                    Title = "Board meeting",
                    Start = today.AddDays(1).AddHours(9),
                    End = today.AddDays(1).AddHours(11)
                },
                new CalendarEvent
                {
                    Id = Guid.NewGuid(),
                    Title = "Sprint planning",
                    Start = today.AddDays(2).AddHours(10),
                    End = today.AddDays(2).AddHours(12)
                },
                new CalendarEvent
                {
                    Id = Guid.NewGuid(),
                    Title = "Team lunch",
                    Start = today.AddDays(3).AddHours(12),
                    End = today.AddDays(3).AddHours(13)
                },
                new CalendarEvent
                {
                    Id = Guid.NewGuid(),
                    Title = "All hands",
                    Start = today.AddDays(5),
                    AllDay = true
                },
                new CalendarEvent
                {
                    Id = Guid.NewGuid(),
                    Title = "Conference",
                    Start = today.AddDays(7),
                    End = today.AddDays(9),
                    AllDay = true
                },
                new CalendarEvent
                {
                    Id = Guid.NewGuid(),
                    Title = "Customer meeting",
                    Start = today.AddDays(10).AddHours(15),
                    End = today.AddDays(10).AddHours(16),
                    Url = "https://example.com"
                },
                new CalendarEvent
                {
                    Id = Guid.NewGuid(),
                    Title = "Sprint demo",
                    Start = today.AddDays(14).AddHours(11),
                    End = today.AddDays(14).AddHours(12)
                },
                new CalendarEvent
                {
                    Id = Guid.NewGuid(),
                    Title = "Hackathon",
                    Start = today.AddDays(20),
                    End = today.AddDays(21),
                    AllDay = true
                }
            };

            db.CalendarEvents.AddRange(events);
            await db.SaveChangesAsync();
        }
    }
}
