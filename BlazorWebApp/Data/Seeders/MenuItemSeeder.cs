using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorWebApp.Data.Seeders
{
    public static class MenuItemSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            if (db.MenuItems.Any()) return;

            var items = new[]
            {
                ("Home","home",  0),
                ("About","about",10),
                ("Ministries","ministries",20)
            }
            .Select(t => new MenuItem { Id = Guid.NewGuid(), Title = t.Item1, Slug = t.Item2, SortOrder = t.Item3 });

            db.MenuItems.AddRange(items);
            await db.SaveChangesAsync();
        }
    }
}
