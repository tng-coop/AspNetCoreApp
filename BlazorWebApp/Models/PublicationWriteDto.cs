using System.ComponentModel.DataAnnotations;

namespace BlazorWebApp.Models
{
    public class PublicationWriteDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        // Manual slug entry; defaults to "default" when unspecified
        public string Slug { get; set; } = "default";

        // capture rendered HTML
        public string Html { get; set; } = string.Empty;
        public int    FeaturedOrder { get; set; }

        // Category mapping 
        public Guid? CategoryId { get; set; }
    }
}
