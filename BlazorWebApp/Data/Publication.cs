using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlazorWebApp.Data
{
    public enum PublicationStatus { Draft, Published, Scheduled }

    public class Publication
    {
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string DeltaJson { get; set; } = string.Empty;

        public string Html { get; set; } = string.Empty;
        public PublicationStatus Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? PublishedAt { get; set; }

        // navigation for many-to-many with Category
        public ICollection<PublicationCategory> PublicationCategories { get; set; }
            = new List<PublicationCategory>();
    }
}
