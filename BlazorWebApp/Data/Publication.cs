using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlazorWebApp.Data
{
    public enum PublicationStatus { Draft, Published, Scheduled }

public class Publication
{
    public Guid   Id        { get; set; }
    [Required]
    public string Title     { get; set; } = string.Empty;

    // New slug field (URL‐friendly, unique)
    [Required]
    public string Slug      { get; set; } = string.Empty;

    public string Html      { get; set; } = string.Empty;
    public int    FeaturedOrder { get; set; } = 0;

    public PublicationStatus Status { get; set; } = PublicationStatus.Draft;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }

        // Each publication belongs to exactly one category
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        // Revision history navigation
        public ICollection<PublicationRevision> PublicationRevisions { get; set; }
            = new List<PublicationRevision>();
}
}
