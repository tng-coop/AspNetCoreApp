using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlazorWebApp.Data;

namespace BlazorWebApp.Data.Seeders
{
    public static class PublicationSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            if (await db.Publications.AnyAsync())
                return;

            // grab your category IDs by slug
            var cats = await db.Categories.ToDictionaryAsync(c => c.Slug, c => c.Id);

            // find the first fractal filename
            var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "fractals");
            var firstFractal = Directory.GetFiles(imagesFolder, "fractal_*.png")
                                       .Select(Path.GetFileName)
                                       .FirstOrDefault();

            // helper to wrap HTML with an <img>
            string WrapWithImage(string bodyHtml) =>
                firstFractal is null
                    ? bodyHtml
                    : $"<img src=\"/images/fractals/{firstFractal}\" alt=\"Fractal\" " +
                      "style=\"max-width:100%;margin-bottom:1rem;\" />" + bodyHtml;

            var now = DateTimeOffset.UtcNow;
            var pubs = new[]
            {
                new Publication {
                    Id = Guid.NewGuid(),
                    Title = "Getting Started with Our CMS",
                    Html  = WrapWithImage("<h1>Welcome</h1><p>This is your first post. Edit me!</p>"),
                    Status = PublicationStatus.Published,
                    CreatedAt   = now.AddDays(-7),
                    PublishedAt = now.AddDays(-6)
                },
                new Publication {
                    Id = Guid.NewGuid(),
                    Title = "Upcoming Outreach Event",
                    Html  = WrapWithImage("<h2>Join us</h2><p>Details coming soon…</p>"),
                    Status      = PublicationStatus.Scheduled,
                    CreatedAt   = now,
                    PublishedAt = now.AddDays(3)
                },
                new Publication {
                    Id = Guid.NewGuid(),
                    Title = "Draft Post Example",
                    Html  = WrapWithImage("<p>This one is still a draft.</p>"),
                    Status    = PublicationStatus.Draft,
                    CreatedAt = now
                }
            };

            db.Publications.AddRange(pubs);
            await db.SaveChangesAsync();

            // attach each publication to the "about" category
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
