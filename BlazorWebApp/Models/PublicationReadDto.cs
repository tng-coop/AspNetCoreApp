using System;

namespace BlazorWebApp.Models
{
    public class PublicationReadDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Html { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? PublishedAt { get; set; }
    }
}
