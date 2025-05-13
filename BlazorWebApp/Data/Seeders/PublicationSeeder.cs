using System;
using System.Linq;
using System.Threading.Tasks;
using BlazorWebApp.Data;

namespace BlazorWebApp.Data.Seeders
{
    public static class PublicationSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            if (db.Publications.Any()) return;

            // grab your category IDs by slug
            var cats = db.Categories.ToDictionary(c => c.Slug, c => c.Id);

            var pubs = new[]
            {
                new Publication {
                    Id = Guid.NewGuid(),
                    Title = "Getting Started with Our CMS",
                    Html  = "<h1>Welcome</h1><p>This is your first post. Edit me!</p>",
                    Status = PublicationStatus.Published,
                    CreatedAt   = DateTimeOffset.UtcNow.AddDays(-7),
                    PublishedAt = DateTimeOffset.UtcNow.AddDays(-6)
                },
                new Publication {
                    Id = Guid.NewGuid(),
                    Title = "Upcoming Outreach Event",
                    Html  = "<h2>Join us</h2><p>Details coming soon…</p>",
                    Status      = PublicationStatus.Scheduled,
                    CreatedAt   = DateTimeOffset.UtcNow,
                    PublishedAt = DateTimeOffset.UtcNow.AddDays(3)
                },
                new Publication {
                    Id = Guid.NewGuid(),
                    Title = "Draft Post Example",
                    Html  = "<p>This one is still a draft.</p>",
                    Status    = PublicationStatus.Draft,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            };

            db.Publications.AddRange(pubs);
            await db.SaveChangesAsync();

            // attach each publication to a category
            // e.g. first two go under "about", last one no category
            foreach (var p in pubs)
            {
                if (cats.TryGetValue("about", out var catId))
                {
                    db.PublicationCategories.Add(new PublicationCategory {
                        PublicationId = p.Id,
                        CategoryId    = catId
                    });
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
