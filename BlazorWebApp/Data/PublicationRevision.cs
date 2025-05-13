using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorWebApp.Data
{
    public class PublicationRevision
    {
        public Guid Id { get; set; }
        public Guid PublicationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Html { get; set; } = string.Empty;
        public Guid? CategoryId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        [ForeignKey("PublicationId")]
        public Publication Publication { get; set; } = null!;
    }
}
