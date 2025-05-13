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

            // grab category IDs by slug
            var cats = await db.Categories.ToDictionaryAsync(c => c.Slug, c => c.Id);

            // find the first fractal filename
            var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "fractals");
            var firstFractal = Directory.GetFiles(imagesFolder, "fractal_*.png")
                                       .Select(Path.GetFileName)
                                       .FirstOrDefault();

            // helper to wrap HTML with an <img>
            string WrapWithImage(string bodyHtml) =>
                firstFractal == null
                    ? bodyHtml
                    : $"<img src=\"/images/fractals/{firstFractal}\" alt=\"Fractal\" " +
                      "style=\"max-width:100%;margin-bottom:1rem;\" />" + bodyHtml;

            var now = DateTimeOffset.UtcNow;

            // define publications with their intended category slugs, including nested ones
            var pubs = new[]
            {
                (Pub: new Publication {
                    Id = Guid.NewGuid(),
                    Title = "Getting Started with Our CMS",
                    Html  = WrapWithImage("<h1>Welcome</h1><p>This is your first post. Edit me!</p>"),
                    Status = PublicationStatus.Published,
                    CreatedAt   = now.AddDays(-7),
                    PublishedAt = now.AddDays(-6)
                }, CategorySlug: "about"),

                (Pub: new Publication {
                    Id = Guid.NewGuid(),
                    Title = "Annual Ministries Kickoff",
                    Html  = WrapWithImage("<h2>Ministries Begin</h2><p>Our ministries launch event details...</p>"),
                    Status = PublicationStatus.Published,
                    CreatedAt   = now.AddDays(-5),
                    PublishedAt = now.AddDays(-4)
                }, CategorySlug: "ministries"),

                (Pub: new Publication {
                    Id = Guid.NewGuid(),
                    Title = "Service Opportunities Update",
                    Html  = WrapWithImage("<h2>Volunteer Service</h2><p>Latest opportunities in service...</p>"),
                    Status = PublicationStatus.Published,
                    CreatedAt   = now.AddDays(-4),
                    PublishedAt = now.AddDays(-3)
                }, CategorySlug: "service"),

                (Pub: new Publication {
                    Id = Guid.NewGuid(),
                    Title = "Outreach Team Training",
                    Html  = WrapWithImage("<h2>Training</h2><p>Upcoming outreach training session...</p>"),
                    Status = PublicationStatus.Published,
                    CreatedAt   = now.AddDays(-3),
                    PublishedAt = now.AddDays(-2)
                }, CategorySlug: "outreach"),

                (Pub: new Publication {
                    Id = Guid.NewGuid(),
                    Title = "Food Pantry Schedule",
                    Html  = WrapWithImage("<h2>Food Pantry</h2><p>Monthly schedule for food pantry...</p>"),
                    Status = PublicationStatus.Published,
                    CreatedAt   = now.AddDays(-2),
                    PublishedAt = now.AddDays(-1)
                }, CategorySlug: "food-pantry"),

                (Pub: new Publication {
                    Id = Guid.NewGuid(),
                    Title = "Clothing Drive Recap",
                    Html  = WrapWithImage("<h2>Clothing Drive</h2><p>Recap of our recent clothing drive...</p>"),
                    Status = PublicationStatus.Published,
                    CreatedAt   = now.AddDays(-1),
                    PublishedAt = now
                }, CategorySlug: "clothing-drive"),

                (Pub: new Publication {
                    Id = Guid.NewGuid(),
                    Title = "Mobile Pantry Route Announced",
                    Html  = WrapWithImage("<h2>Mobile Pantry</h2><p>Route details for this week...</p>"),
                    Status = PublicationStatus.Published,
                    CreatedAt   = now,
                    PublishedAt = now.AddHours(1)
                }, CategorySlug: "mobile-pantry"),

                // Additional outreach ones
                (Pub: new Publication {
                    Id = Guid.NewGuid(),
                    Title = "Community Outreach Recap",
                    Html  = WrapWithImage("<h2>Recap</h2><p>Here's what happened…</p>"),
                    Status = PublicationStatus.Published,
                    CreatedAt   = now.AddDays(-2),
                    PublishedAt = now.AddDays(-1)
                }, CategorySlug: "outreach"),

                (Pub: new Publication {
                    Id = Guid.NewGuid(),
                    Title = "Volunteer Spotlight",
                    Html  = WrapWithImage("<h2>Meet our volunteer</h2><p>Spotlight on Jane Doe…</p>"),
                    Status = PublicationStatus.Published,
                    CreatedAt   = now.AddDays(-1),
                    PublishedAt = now
                }, CategorySlug: "outreach"),

                (Pub: new Publication {
                    Id = Guid.NewGuid(),
                    Title = "Draft Post Example",
                    Html    = WrapWithImage("<p>This one is still a draft.</p>"),
                    Status  = PublicationStatus.Draft,
                    CreatedAt = now
                }, CategorySlug: (string?)null)
            };

            // add publications
            db.Publications.AddRange(pubs.Select(x => x.Pub));
            await db.SaveChangesAsync();

            // attach each publication to its category
            foreach (var (Pub, CategorySlug) in pubs)
            {
                if (!string.IsNullOrEmpty(CategorySlug)
                    && cats.TryGetValue(CategorySlug, out var catId))
                {
                    db.PublicationCategories.Add(new PublicationCategory {
                        PublicationId = Pub.Id,
                        CategoryId    = catId
                    });
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
