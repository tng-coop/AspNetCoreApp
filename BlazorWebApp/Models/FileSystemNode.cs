using System;
using System.Collections.Generic;

namespace BlazorWebApp.Models;

public enum NodeKind
{
    Directory,
    Publication
}

public class FileSystemNode
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public NodeKind Kind { get; set; }
    public List<FileSystemNode> Children { get; set; } = new();
}

