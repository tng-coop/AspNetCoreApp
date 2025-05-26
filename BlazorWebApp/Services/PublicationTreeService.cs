using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorWebApp.Data;
using BlazorWebApp.Models;
using BlazorWebApp.Utils;
using Microsoft.EntityFrameworkCore;

namespace BlazorWebApp.Services
{
    public class PublicationTreeService : IPublicationTreeService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _factory;

        public PublicationTreeService(IDbContextFactory<ApplicationDbContext> factory)
            => _factory = factory;

        private ApplicationDbContext CreateDb() => _factory.CreateDbContext();

        public async Task<List<CategoryTreeNode>> GetTreeAsync()
        {
            await using var db = CreateDb();

            var categories = await db.Categories
                .AsNoTracking()
                .OrderBy(c => c.SortOrder ?? int.MaxValue)
                .ThenBy(c => c.Name)
                .ToListAsync();

            var catDtos = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                NameJa = c.NameJa,
                ParentCategoryId = c.ParentCategoryId,
                Slug = c.Slug,
                SortOrder = c.SortOrder
            }).ToList();

            var nodes = catDtos.ToDictionary(c => c.Id, c => new CategoryTreeNode
            {
                Category = c,
                Children = new List<CategoryTreeNode>(),
                Publications = new List<PublicationReadDto>()
            });

            foreach (var node in nodes.Values)
            {
                if (node.Category.ParentCategoryId.HasValue &&
                    nodes.TryGetValue(node.Category.ParentCategoryId.Value, out var parent))
                {
                    parent.Children.Add(node);
                }
            }

            var publications = await db.Publications
                .AsNoTracking()
                .Where(p => p.Status == PublicationStatus.Published)
                .Include(p => p.Category)
                .ToListAsync();

            foreach (var p in publications)
            {
                if (!nodes.TryGetValue(p.CategoryId, out var node))
                    continue;

                var dto = new PublicationReadDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    TitleJa = p.TitleJa,
                    Slug = p.Slug,
                    Html = p.Html,
                    Status = p.Status.ToString(),
                    FeaturedOrder = p.FeaturedOrder,
                    Mode = p.Mode,
                    PdfFileId = p.PdfFileId,
                    CreatedAt = p.CreatedAt,
                    PublishedAt = p.PublishedAt,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category == null ? null : CategoryUtils.LocalizedName(p.Category),
                    CategorySlug = p.Category?.Slug
                };

                node.Publications.Add(dto);
            }

            void SortNode(CategoryTreeNode n)
            {
                n.Children = n.Children
                    .OrderBy(c => c.Category.SortOrder ?? int.MaxValue)
                    .ThenBy(c => CategoryUtils.LocalizedName(c.Category))
                    .ToList();

                n.Publications = n.Publications
                    .OrderBy(p => p.FeaturedOrder == 0 ? int.MaxValue : p.FeaturedOrder)
                    .ThenByDescending(p => p.PublishedAt ?? DateTimeOffset.MinValue)
                    .ToList();

                foreach (var child in n.Children)
                    SortNode(child);
            }

            var roots = nodes.Values
                .Where(n => n.Category.ParentCategoryId == null)
                .ToList();

            foreach (var r in roots)
                SortNode(r);

            roots = roots
                .OrderBy(c => c.Category.SortOrder ?? int.MaxValue)
                .ThenBy(c => CategoryUtils.LocalizedName(c.Category))
                .ToList();

            return roots;
        }
    }
}
