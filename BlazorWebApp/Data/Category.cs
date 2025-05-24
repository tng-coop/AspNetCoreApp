using System;
using System.Collections.Generic;

namespace BlazorWebApp.Data
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // URL-friendly slug for routing
        public string Slug { get; set; } = string.Empty;

        // Optional ordering value for navigation
        public int? SortOrder { get; set; }

        // Self-referencing for hierarchy
        public Guid? ParentCategoryId { get; set; }
        public Category? Parent { get; set; }
        public ICollection<Category> Children { get; set; } = new List<Category>();

        // One-to-many relationship with publications
        public ICollection<Publication> Publications { get; set; }
            = new List<Publication>();
    }
}
