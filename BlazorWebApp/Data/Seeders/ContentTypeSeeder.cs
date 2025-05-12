using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorWebApp.Data.Seeders
{
    public static class ContentTypeSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            if (db.ContentTypes.Any()) return;

            db.ContentTypes.AddRange(
                new ContentType { Id = Guid.NewGuid(), Name = "Page",     Slug = "page"     },
                new ContentType { Id = Guid.NewGuid(), Name = "News",     Slug = "news"     },
                new ContentType { Id = Guid.NewGuid(), Name = "Event",    Slug = "event"    },
                new ContentType { Id = Guid.NewGuid(), Name = "Bulletin", Slug = "bulletin" }
            );
            await db.SaveChangesAsync();
        }
    }
}
