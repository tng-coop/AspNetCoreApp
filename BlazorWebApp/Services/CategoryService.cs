using System;
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
        private readonly IDbContextFactory<ApplicationDbContext> _factory;
        public CategoryService(IDbContextFactory<ApplicationDbContext> factory) => _factory = factory;

        /// <summary>
        /// Create a new DbContext for each operation to avoid concurrent usage issues.
        /// </summary>
        private ApplicationDbContext CreateDb() => _factory.CreateDbContext();

        public async Task<List<CategoryDto>> ListAsync()
        {
            await using var db = CreateDb();
            return await db.Categories
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentCategoryId = c.ParentCategoryId,
                    Slug = c.Slug
                })
                .ToListAsync();
        }

        public async Task<List<CategoryDto>> GetAncestryAsync(Guid categoryId)
        {
            await using var db = CreateDb();
            // Load all categories into memory
            var all = await db.Categories
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentCategoryId = c.ParentCategoryId,
                    Slug = c.Slug
                })
                .ToListAsync();

            // Build lookup for parent traversal
            var lookup = all.ToDictionary(c => c.Id);

            var result = new List<CategoryDto>();
            if (!lookup.TryGetValue(categoryId, out var current))
                return result;

            // Walk up the tree
            while (current.ParentCategoryId.HasValue)
            {
                var parent = lookup[current.ParentCategoryId.Value];
                result.Add(parent);
                current = parent;
            }

            // Reverse to get from root to immediate parent
            result.Reverse();
            return result;
        }
    }
}
