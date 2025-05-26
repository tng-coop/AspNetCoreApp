using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace BlazorWebApp.Data.Seeders
{
    public static class FileSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext dbContext)
        {
            // Only seed sample fractal images when the table is empty
            var hasAny = await dbContext.Files.AnyAsync();
            if (!hasAny)
            {
                // Path to your fractal PNGs
                var imagesFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot", "images", "fractals");

                var files = Directory
                    .GetFiles(imagesFolder, "fractal_*.png");

                foreach (var path in files)
                {
                    var bytes = await File.ReadAllBytesAsync(path);
                    dbContext.Files.Add(new FileAsset
                    {
                        Content     = bytes,
                        ContentType = "image/png",
                        FileName    = Path.GetFileName(path),
                        UploadedAt  = DateTimeOffset.UtcNow
                    });
                }
            }

            // Add a seeded PDF example
            var pdfPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot", "_pdf", "simple.pdf");
            var seededId = Guid.Parse("5d46f464-efa8-4c15-b1ea-cd3c6c3a77f0");
            if (File.Exists(pdfPath) &&
                !await dbContext.Files.AnyAsync(f => f.Id == seededId))
            {
                var pdfBytes = await File.ReadAllBytesAsync(pdfPath);
                dbContext.Files.Add(new FileAsset
                {
                    Id = seededId,
                    Content     = pdfBytes,
                    ContentType = "application/pdf",
                    FileName    = "seeded.pdf",
                    UploadedAt  = DateTimeOffset.UtcNow
                });
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
