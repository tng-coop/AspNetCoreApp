using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorWebApp.Models
{
    public class CategoryWriteDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        // Manual slug entry; defaults to "default" when unspecified
        public string Slug { get; set; } = "default";

        public Guid? ParentCategoryId { get; set; }

        public int? SortOrder { get; set; }
    }
}
