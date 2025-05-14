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

        // Rendered HTML content
        public string Html { get; set; } = string.Empty;

        // Current publication status (draft, published, scheduled)
        public PublicationStatus Status { get; set; } = PublicationStatus.Draft;

        // Timestamps
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? PublishedAt { get; set; }

        // Many-to-many join with categories
        public ICollection<PublicationCategory> PublicationCategories { get; set; }
            = new List<PublicationCategory>();

        // Revision history navigation
        public ICollection<PublicationRevision> PublicationRevisions { get; set; }
            = new List<PublicationRevision>();
    }
}