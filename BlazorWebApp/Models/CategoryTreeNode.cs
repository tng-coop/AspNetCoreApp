using System.Collections.Generic;

namespace BlazorWebApp.Models
{
    /// <summary>
    /// Represents a category with its child categories and published articles.
    /// </summary>
    public class CategoryTreeNode
    {
        public CategoryDto Category { get; set; } = new();
        public List<CategoryTreeNode> Children { get; set; } = new();
        public List<PublicationReadDto> Publications { get; set; } = new();
    }
}
