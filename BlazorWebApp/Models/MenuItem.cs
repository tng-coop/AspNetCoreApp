using System;
using System.Collections.Generic;

namespace BlazorWebApp.Data
{
    public class MenuItem
    {
        public Guid Id { get; set; }

        // Display title
        public string Title { get; set; } = string.Empty;

        // URL-friendly slug for routing
        public string Slug { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public Guid? ParentMenuItemId { get; set; }
        public MenuItem? Parent { get; set; }

        public ICollection<MenuItem> Children { get; set; } = new List<MenuItem>();

        // Optional link to a ContentItem
        public Guid? ContentItemId { get; set; }
    }
}
