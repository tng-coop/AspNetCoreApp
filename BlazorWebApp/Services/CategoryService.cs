using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlazorWebApp.Data;
using BlazorWebApp.Models;

namespace BlazorWebApp.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _db;
        public CategoryService(ApplicationDbContext db) => _db = db;

        public async Task<List<CategoryDto>> ListAsync()
        {
            return await _db.Categories
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentCategoryId = c.ParentCategoryId,
                    // Map the Slug from the entity
                    Slug = c.Slug
                })
                .ToListAsync();
        }

        public async Task<List<CategoryDto>> GetAncestryAsync(Guid categoryId)
        {
            // 1) get the flat list of all categories
            var all = await _db.Categories
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentCategoryId = c.ParentCategoryId,
                    Slug = c.Slug
                })
                .ToListAsync();

            // 2) build a lookup for fast parent-lookup
            var lookup = all.ToDictionary(c => c.Id);

            // 3) walk upward from the target category
            var result = new List<CategoryDto>();
            if (!lookup.TryGetValue(categoryId, out var current))
                return result;

            // collect parents until root
            while (current.ParentCategoryId.HasValue)
            {
                var parent = lookup[current.ParentCategoryId.Value];
                result.Add(parent);
                current = parent;
            }

            // now result = [ immediateParent, grandParent, … ], so reverse
            result.Reverse();
            return result;
        }

    }
}
