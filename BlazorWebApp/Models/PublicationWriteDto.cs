using System.ComponentModel.DataAnnotations;

namespace BlazorWebApp.Models
{
    public class PublicationWriteDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        // optional custom slug (if empty we generate from Title)
        public string Slug { get; set; } = string.Empty;

        // capture rendered HTML
        public string Html { get; set; } = string.Empty;
        public int    FeaturedOrder { get; set; }

        // Category mapping 
        public Guid? CategoryId { get; set; }
    }
}
