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
                    NameJa = c.NameJa,
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
                    NameJa = c.NameJa,
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
            if (!SlugUtils.IsAsciiSlug(slugBase))
                throw new ArgumentException("Slug may contain only ASCII letters, digits and hyphens.", nameof(dto.Slug));
            var slug = await SlugService.GenerateUniqueSlugAsync(db, slugBase);

            var cat = new Category
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                NameJa = dto.NameJa,
                Slug = slug,
                ParentCategoryId = dto.ParentCategoryId,
                SortOrder = dto.SortOrder
            };

            db.Categories.Add(cat);
            await SlugService.UpsertSlugRecordAsync(db, cat.Id, nameof(Category), slug);
            await db.SaveChangesAsync();

            return new CategoryDto
            {
                Id = cat.Id,
                Name = cat.Name,
                NameJa = cat.NameJa,
                ParentCategoryId = cat.ParentCategoryId,
                Slug = cat.Slug,
                SortOrder = cat.SortOrder
            };
        }

        public async Task<CategoryDto> UpdateAsync(Guid id, CategoryWriteDto dto)
        {
            await using var db = CreateDb();
            var cat = await db.Categories.FindAsync(id)
                       ?? throw new KeyNotFoundException();

            var slugBase = SlugUtils.Normalize(dto.Slug);
            if (!SlugUtils.IsAsciiSlug(slugBase))
                throw new ArgumentException("Slug may contain only ASCII letters, digits and hyphens.", nameof(dto.Slug));
            cat.Slug = await SlugService.GenerateUniqueSlugAsync(db, slugBase, id, nameof(Category));
            cat.Name = dto.Name;
            cat.NameJa = dto.NameJa;
            cat.ParentCategoryId = dto.ParentCategoryId;
            cat.SortOrder = dto.SortOrder;
            await SlugService.UpsertSlugRecordAsync(db, cat.Id, nameof(Category), cat.Slug);

            await db.SaveChangesAsync();

            return new CategoryDto
            {
                Id = cat.Id,
                Name = cat.Name,
                NameJa = cat.NameJa,
                ParentCategoryId = cat.ParentCategoryId,
                Slug = cat.Slug,
                SortOrder = cat.SortOrder
            };
        }

    }
}
