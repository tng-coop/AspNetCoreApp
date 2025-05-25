using System;
using System.Collections.Generic;
using System.Linq;
using BlazorWebApp.Models;
using BlazorWebApp.Utils;

namespace BlazorWebApp.Services
{

    public class TreeMenuService : ITreeMenuService
    {
        private readonly IPublicationTreeService _treeService;

        public TreeMenuService(IPublicationTreeService treeService)
            => _treeService = treeService;

        public async Task<List<MenuItemDto>> GetMenuAsync()
        {
            var tree = await _treeService.GetTreeAsync();

            List<MenuItemDto> MapNodes(IEnumerable<CategoryTreeNode> nodes) =>
                nodes.Select(node =>
                {
                    var children = MapNodes(node.Children);

                    foreach (var pub in node.Publications.Skip(1))
                    {
                        children.Add(new MenuItemDto
                        {
                            Id            = pub.Id,
                            Title         = PublicationUtils.LocalizedTitle(pub),
                            Slug          = $"{node.Category.Slug}/{pub.Slug}",
                            IconFile       = "file-earmark-text",
                            SortOrder     = 0,
                            ContentItemId = pub.Id,
                            Children      = new List<MenuItemDto>()
                        });
                    }

                    return new MenuItemDto
                    {
                        Id            = node.Category.Id,
                        Title         = CategoryUtils.LocalizedName(node.Category),
                        Slug          = node.Category.Slug,
                        IconFile       = node.Category.Slug.Equals("home", StringComparison.OrdinalIgnoreCase)
                                       ? "house-door-fill"
                                       : "",
                        SortOrder     = node.Category.SortOrder ?? 0,
                        ContentItemId = null,
                        Children      = children
                    };
                })
                .ToList();
            return MapNodes(tree);
        }
    }
}
