using System;

namespace BlazorWebApp.Data
{
    public class PublicationCategory
    {
        public Guid PublicationId { get; set; }
        public Publication Publication { get; set; } = null!;

        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;
    }
}
