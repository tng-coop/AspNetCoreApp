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

            // collect all fractal image filenames (expected 10: fractal_0.png through fractal_9.png)
            var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "fractals");
            var fractalFiles = Directory.GetFiles(imagesFolder, "fractal_*.png")
                                       .Select(Path.GetFileName)
                                       .OrderBy(name => name)
                                       .ToList();

            // helper to wrap HTML with a specific <img>
            string WrapWithImage(string bodyHtml, string filename) =>
                $"<img src=\"/images/fractals/{filename}\" alt=\"Fractal\" " +
                "style=\"max-width:25%;height:auto;margin-bottom:1rem;\" />" + bodyHtml;

            var now = DateTimeOffset.UtcNow;

            // define raw publications: (categorySlug, html, status, createdOffsetDays, publishedOffsetDays?)
            var rawPubs = new (string? CategorySlug, string Html, PublicationStatus Status, int CreatedOffset, int? PublishedOffset)[]
            {
                ("about",         "<h1>Welcome</h1><p>This is your first post. Edit me!</p>",          PublicationStatus.Published,   -7, -6),
                ("ministries",    "<h2>Ministries Begin</h2><p>Ministries launch details...</p>",   PublicationStatus.Published,   -5, -4),
                ("service",       "<h2>Volunteer Service</h2><p>Latest service opportunities...</p>", PublicationStatus.Published,   -4, -3),
                ("outreach",      "<h2>Training</h2><p>Upcoming outreach training...</p>",         PublicationStatus.Published,   -3, -2),
                ("food-pantry",   "<h2>Food Pantry</h2><p>Monthly schedule for food pantry...</p>", PublicationStatus.Published,   -2, -1),
                ("clothing-drive", "<h2>Clothing Drive</h2><p>Recap of the clothing drive...</p>",     PublicationStatus.Published,   -1,  0),
                ("mobile-pantry", "<h2>Mobile Pantry</h2><p>Route details for this week...</p>",      PublicationStatus.Published,    0, +1),
                ("outreach",      "<h2>Recap</h2><p>Here's what happened…</p>",                    PublicationStatus.Published,   -2, -1),
                ("outreach",      "<h2>Meet our volunteer</h2><p>Spotlight on Jane Doe…</p>",          PublicationStatus.Published,   -1,  0),
                (null,             "<p>This post is still a draft.</p>",                               PublicationStatus.Draft,         0, null)
            };

            // build Publication entities with one-to-one image mapping
            var pubs = rawPubs.Select((entry, i) =>
            {
                var file = fractalFiles.Count > i ? fractalFiles[i] : fractalFiles[0];
                if (string.IsNullOrEmpty(file))
                {
                    throw new InvalidOperationException("Fractal file name cannot be null or empty.");
                }

                var pub = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = GenerateTitle(entry.CategorySlug, i),
                    Slug = Utils.SlugGenerator.Generate(GenerateTitle(entry.CategorySlug, i)),
                    Html = WrapWithImage(entry.Html, file),
                    Status = entry.Status,
                    IsFeatured    = entry.CategorySlug == "about",
                    FeaturedOrder = entry.CategorySlug == "about" ? 1 : 0,
                    CreatedAt = now.AddDays(entry.CreatedOffset),
                    PublishedAt = entry.PublishedOffset.HasValue
                                  ? now.AddDays(entry.PublishedOffset.Value)
                                  : (DateTimeOffset?)null
                };
                return (Pub: pub, CategorySlug: entry.CategorySlug);
            }).ToList();

            // save publications
            db.Publications.AddRange(pubs.Select(p => p.Pub));
            await db.SaveChangesAsync();

            // attach each publication to its category
            foreach (var pair in pubs)
            {
                var slug = pair.CategorySlug;
                if (slug != null && cats.TryGetValue(slug, out var catId))
                {
                    db.PublicationCategories.Add(new PublicationCategory
                    {
                        PublicationId = pair.Pub.Id,
                        CategoryId = catId
                    });
                }
            }
            await db.SaveChangesAsync();

            // local helper to generate a title
            static string GenerateTitle(string? slug, int index) => slug switch
            {
                null => "Draft Post Example",
                _ when index == 0 => "Getting Started with Our CMS",
                _ when slug == "ministries" => "Annual Ministries Kickoff",
                _ when slug == "service" => "Service Opportunities Update",
                _ when slug == "outreach" && index == 3 => "Outreach Team Training",
                _ when slug == "food-pantry" => "Food Pantry Schedule",
                _ when slug == "clothing-drive" => "Clothing Drive Recap",
                _ when slug == "mobile-pantry" => "Mobile Pantry Route Announced",
                _ when slug == "outreach" && index == 7 => "Community Outreach Recap",
                _ when slug == "outreach" && index == 8 => "Volunteer Spotlight",
                _ => $"Post {index + 1}"
            };
        }
    }
}
