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

        // Self-referencing for hierarchy
        public Guid? ParentCategoryId { get; set; }
        public Category? Parent { get; set; }
        public ICollection<Category> Children { get; set; } = new List<Category>();

        // Join table for many-to-many with ContentItem (formerly Publication)
        public ICollection<PublicationCategory> PublicationCategories { get; set; }
            = new List<PublicationCategory>();
    }
}
