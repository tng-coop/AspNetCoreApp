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
            var entities = await _db.MenuItems
                .AsNoTracking()
                .OrderBy(mi => mi.SortOrder)
                .ToListAsync();

            var lookup = entities.ToLookup(mi => mi.ParentMenuItemId);

            List<MenuItemDto> Map(IEnumerable<MenuItem> items)
                => items.Select(e => new MenuItemDto
                   {
                       Id = e.Id,
                       Title = e.Title,
                       Slug = e.Slug,
                       IconCss = e.IconCss,
                       SortOrder = e.SortOrder,
                       ContentItemId = e.ContentItemId,
                       Children = Map(lookup[e.Id])
                   })
                   .OrderBy(d => d.SortOrder)
                   .ToList();

            return Map(lookup[null]);
        }
    }
}
