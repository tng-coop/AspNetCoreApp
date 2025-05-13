using Microsoft.EntityFrameworkCore;

namespace BlazorWebApp.Data.Seeders
{
    public static class ImageSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext dbContext)
        {
            // Exit if any images already exist
            if (await dbContext.Images.AnyAsync())
                return;

            // Path to your fractal PNGs
            var imagesFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot", "images", "fractals");

            var files = Directory
                .GetFiles(imagesFolder, "fractal_*.png");

            foreach (var path in files)
            {
                var bytes = await File.ReadAllBytesAsync(path);
                dbContext.Images.Add(new ImageAsset
                {
                    Content     = bytes,
                    ContentType = "image/png",
                    UploadedAt  = DateTimeOffset.UtcNow
                });
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
