using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlazorWebApp.Data;
using BlazorWebApp.Models;
using BlazorWebApp.Utils;

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
                .OrderBy(c => c.SortOrder ?? int.MaxValue)
                .ThenBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentCategoryId = c.ParentCategoryId,
                    Slug = c.Slug,
                    SortOrder = c.SortOrder
                })
                .ToListAsync();
        }

        public async Task<List<CategoryDto>> GetAncestryAsync(Guid categoryId)
        {
            await using var db = CreateDb();
            // Load all categories into memory
            var all = await db.Categories
                .OrderBy(c => c.SortOrder ?? int.MaxValue)
                .ThenBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentCategoryId = c.ParentCategoryId,
                    Slug = c.Slug,
                    SortOrder = c.SortOrder
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

        public async Task<CategoryDto> CreateAsync(CategoryWriteDto dto)
        {
            await using var db = CreateDb();

            // Manual slug entry; ensure a slug value exists and does not start with '_'
            var slugBase = SlugUtils.Normalize(dto.Slug);
            var slug = await GenerateUniqueSlugAsync(db, slugBase);

            var cat = new Category
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Slug = slug,
                ParentCategoryId = dto.ParentCategoryId,
                SortOrder = dto.SortOrder
            };

            db.Categories.Add(cat);
            await db.SaveChangesAsync();

            return new CategoryDto
            {
                Id = cat.Id,
                Name = cat.Name,
                ParentCategoryId = cat.ParentCategoryId,
                Slug = cat.Slug,
                SortOrder = cat.SortOrder
            };
        }

        private static async Task<string> GenerateUniqueSlugAsync(
            ApplicationDbContext db,
            string baseSlug,
            Guid? excludeId = null)
        {
            var slug = baseSlug;
            var counter = 1;
            while (await db.Categories.AnyAsync(c => c.Slug == slug &&
                                                    (excludeId == null || c.Id != excludeId)))
            {
                slug = $"{baseSlug}-{counter++}";
            }
            return slug;
        }
    }
}
