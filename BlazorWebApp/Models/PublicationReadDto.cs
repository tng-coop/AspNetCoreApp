using System;
using BlazorWebApp.Data;

namespace BlazorWebApp.Models
{
    public class PublicationReadDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string TitleJa { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Html { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int    FeaturedOrder { get; set; }

        public PublicationContentMode Mode { get; set; }
        public Guid? PdfFileId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? PublishedAt { get; set; }
        // Category info 
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? CategorySlug { get; set; }
    }
}
