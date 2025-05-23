using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlazorWebApp.Data;
using System.Collections.Generic;

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

            // collect fractal image filenames (expected 10: fractal_0.png through fractal_9.png)
            var imagesFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot", "images", "fractals");

            var fractalFiles = Directory.GetFiles(imagesFolder, "fractal_*.png")
                                   .Select(Path.GetFileName)
                                   .OrderBy(name => name)
                                   .ToList();

            // helper to wrap HTML with a specific <img>
            string WrapWithImage(string bodyHtml, string filename) =>
                $"<img src=\"/images/fractals/{filename}\" alt=\"Fractal\" " +
                "style=\"max-width:25%;height:auto;margin-bottom:1rem;\" />" + bodyHtml;

            var now = DateTimeOffset.UtcNow;

            // load raw publication seed data
            var rawPubs = PublicationSeedData.Entries;

            // build Publication entities with optional fractal images per entry
            var pubs = rawPubs.Select((entry, i) =>
            {
                string html = entry.Html;
                if (entry.IncludeFractalImage)
                {
                    if (fractalFiles.Count == 0)
                        throw new InvalidOperationException("Fractal file name cannot be null or empty.");

                    var file = fractalFiles.Count > i ? fractalFiles[i] : fractalFiles[0];
                    if (string.IsNullOrEmpty(file))
                        throw new InvalidOperationException("Fractal file name cannot be null or empty.");

                    html = WrapWithImage(html, file);
                }

                var pub = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = entry.Title,
                    Slug = entry.Slug,
                    Html = html,
                    Status = entry.Status,
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
        }
    }
}
