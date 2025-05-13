using System;

namespace BlazorWebApp.Models
{
    public class RevisionDto
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}
