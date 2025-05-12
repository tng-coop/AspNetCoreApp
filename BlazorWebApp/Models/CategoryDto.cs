using System;

namespace BlazorWebApp.Models
{
    public class CategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? ParentCategoryId { get; set; }

        // Added Slug so we can route and link correctly
        public string Slug { get; set; } = string.Empty;
    }
}
