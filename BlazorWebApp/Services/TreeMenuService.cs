using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorWebApp.Data;
using BlazorWebApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorWebApp.Services
{
    public interface ITreeMenuService
    {
        Task<List<MenuItemDto>> GetMenuAsync();
    }

    public class TreeMenuService : ITreeMenuService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        // ← Inject the scope factory instead of DbContext directly
        public TreeMenuService(IServiceScopeFactory scopeFactory) 
            => _scopeFactory = scopeFactory;

        public async Task<List<MenuItemDto>> GetMenuAsync()
        {
            // ← Create a new scope (and with it, a fresh ApplicationDbContext) each call
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var categories = await db.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            var lookup = categories.ToLookup(c => c.ParentCategoryId);

            List<MenuItemDto> Map(IEnumerable<Category> cats)
                => cats.Select(c => new MenuItemDto
                   {
                       Id = c.Id,
                       Title = c.Name,
                       Slug = c.Slug,
                       IconCss = string.Empty,
                       SortOrder = 0,
                       ContentItemId = null,
                       Children = Map(lookup[c.Id])
                   })
                   .ToList();

            return Map(lookup[null]);
        }
    }
}
