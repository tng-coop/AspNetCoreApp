using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorWebApp.Data;
using BlazorWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorWebApp.Services
{
    public interface ITreeMenuService
    {
        Task<List<MenuItemDto>> GetMenuAsync();
    }

    public class TreeMenuService : ITreeMenuService
    {
        private readonly ApplicationDbContext _db;
        public TreeMenuService(ApplicationDbContext db) => _db = db;

        public async Task<List<MenuItemDto>> GetMenuAsync()
        {
            // load all categories for menu
            var categories = await _db.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            // build lookup by ParentCategoryId
            var lookup = categories.ToLookup(c => c.ParentCategoryId);

            List<MenuItemDto> Map(IEnumerable<Category> cats)
                => cats.Select(c => new MenuItemDto
                   {
                       Id = c.Id,
                       Title = c.Name,
                       Slug = c.Slug,
                       IconCss = string.Empty,  // or map from Category if you add one
                       SortOrder = 0,           // categories can be sorted by Name
                       ContentItemId = null,
                       Children = Map(lookup[c.Id])
                   })
                   .ToList();

            // root categories have null parent
            return Map(lookup[null]);
        }
    }
}