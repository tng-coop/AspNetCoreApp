using Microsoft.EntityFrameworkCore;

namespace BlazorWebApp.Data.Seeders
{
    public static class CategorySeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            // ← now fully async
            if (await db.Categories.AnyAsync()) 
                return;

            // Top-level categories
            var home = new Category { Id = Guid.NewGuid(), Name = "Home", NameJa = "ホーム", Slug = "home", SortOrder = 0 };
            var about         = new Category { Id = Guid.NewGuid(), Name = "About",            NameJa = "概要",       Slug = "about", SortOrder = 10 };
            var ministries    = new Category { Id = Guid.NewGuid(), Name = "Ministries",       NameJa = "ミニストリー", Slug = "ministries", SortOrder = 20 };

            db.Categories.AddRange(home, about, ministries);

            // Second-level under Ministries
            var service     = new Category { Id = Guid.NewGuid(), Name = "Service",          NameJa = "奉仕",       Slug = "service", ParentCategoryId = ministries.Id, SortOrder = 0 };
            var outreach    = new Category { Id = Guid.NewGuid(), Name = "Outreach",         NameJa = "アウトリーチ", Slug = "outreach", ParentCategoryId = ministries.Id, SortOrder = 10 };
            db.Categories.AddRange(service, outreach);

            // Third-level under Service
            var foodPantry  = new Category { Id = Guid.NewGuid(), Name = "Food Pantry",      NameJa = "フードパントリー", Slug = "food-pantry", ParentCategoryId = service.Id, SortOrder = 0 };
            var clothingDr  = new Category { Id = Guid.NewGuid(), Name = "Clothing Drive",   NameJa = "衣料回収", Slug = "clothing-drive", ParentCategoryId = service.Id, SortOrder = 10 };
            db.Categories.AddRange(foodPantry, clothingDr);

            // Fourth-level under Food Pantry
            var mobilePantry = new Category { Id = Guid.NewGuid(), Name = "Mobile Pantry", NameJa = "移動パントリー", Slug = "mobile-pantry", ParentCategoryId = foodPantry.Id, SortOrder = 0 };
            db.Categories.Add(mobilePantry);

            await db.SaveChangesAsync();
        }
    }
}
