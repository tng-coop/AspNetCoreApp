using System.ComponentModel.DataAnnotations;

namespace BlazorWebApp.Models
{
    public class PublicationWriteDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string DeltaJson { get; set; } = string.Empty;

        // capture rendered HTML
        public string Html { get; set; } = string.Empty;
        // Category mapping 
        public Guid? CategoryId { get; set; }
    }
}
