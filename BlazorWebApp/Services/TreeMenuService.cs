using System;
using BlazorWebApp.Data;
using BlazorWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorWebApp.Services
{

    public class TreeMenuService : ITreeMenuService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public TreeMenuService(IServiceScopeFactory scopeFactory) 
            => _scopeFactory = scopeFactory;

        public async Task<List<MenuItemDto>> GetMenuAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var allCats = await db.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();
            var catLookup = allCats.ToLookup(c => c.ParentCategoryId);

            var published = await db.Publications
                .Where(p => p.Status == PublicationStatus.Published)
                .Include(p => p.PublicationCategories)
                .ToListAsync();
            var pubsByCat = published
                .SelectMany(p => p.PublicationCategories.Select(pc => (pc.CategoryId, p)))
                .GroupBy(x => x.CategoryId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.p)
                          .OrderByDescending(p => p.PublishedAt)
                          .ToList()
                );

            List<MenuItemDto> MapCats(IEnumerable<Category> cats) =>
                cats.Select(cat =>
                {
                    var children = MapCats(catLookup[cat.Id]);

                    if (pubsByCat.TryGetValue(cat.Id, out var posts))
                    {
                        // The first (highest priority) article is reserved for
                        // the category page itself. Skip it in the tree menu
                        // so only articles starting from the second one are
                        // displayed under the category.
                        foreach (var pub in posts.Skip(1))
                        {
                            children.Add(new MenuItemDto
                            {
                                Id            = pub.Id,
                                Title         = pub.Title,
                                Slug          = $"{cat.Slug}/{pub.Slug}",
                                IconCss       = "bi-file-earmark-text",
                                SortOrder     = 0,
                                ContentItemId = pub.Id,
                                Children      = new List<MenuItemDto>()
                            });
                        }
                    }

                    return new MenuItemDto
                    {
                        Id            = cat.Id,
                        Title         = cat.Name,
                        Slug          = cat.Slug,
                        IconCss       = cat.Slug.Equals("home", StringComparison.OrdinalIgnoreCase)
                                       ? "house"
                                       : "list-nested",
                        SortOrder     = 0,
                        ContentItemId = null,
                        Children      = children
                    };
                })
                .ToList();

            return MapCats(catLookup[null]);
        }
    }
}
