using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorWebApp.Models;
using BlazorWebApp.Utils;

namespace BlazorWebApp.Services;

public class FileSystemService : IFileSystemService
{
    private readonly IPublicationTreeService _treeService;

    public FileSystemService(IPublicationTreeService treeService)
    {
        _treeService = treeService;
    }

    public async Task<List<FileSystemNode>> GetRootAsync()
    {
        var tree = await _treeService.GetTreeAsync();
        return tree.Select(n => ToNode(n, string.Empty)).ToList();
    }

    public async Task<FileSystemNode?> GetNodeAsync(string path)
    {
        var segments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var roots = await GetRootAsync();
        List<FileSystemNode> current = roots;
        FileSystemNode? node = null;
        foreach (var seg in segments)
        {
            node = current.FirstOrDefault(n => n.Slug == seg);
            if (node == null)
                return null;
            current = node.Children;
        }
        return node;
    }

    private FileSystemNode ToNode(CategoryTreeNode node, string prefix)
    {
        var path = string.IsNullOrEmpty(prefix) ? node.Category.Slug : $"{prefix}/{node.Category.Slug}";
        var fsNode = new FileSystemNode
        {
            Id = node.Category.Id,
            Name = CategoryUtils.LocalizedName(node.Category),
            Slug = node.Category.Slug,
            Path = path,
            Kind = NodeKind.Directory
        };
        fsNode.Children.AddRange(node.Children.Select(c => ToNode(c, path)));
        fsNode.Children.AddRange(node.Publications.Select(p => new FileSystemNode
        {
            Id = p.Id,
            Name = PublicationUtils.LocalizedTitle(p),
            Slug = p.Slug,
            Path = $"{path}/{p.Slug}",
            Kind = NodeKind.Publication,
            Children = new List<FileSystemNode>()
        }));
        return fsNode;
    }
}


