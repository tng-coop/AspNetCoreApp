using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorWebApp.Models
{
    public class CategoryWriteDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        // Optional custom slug; if empty we generate from Name
        public string Slug { get; set; } = string.Empty;

        public Guid? ParentCategoryId { get; set; }
    }
}
