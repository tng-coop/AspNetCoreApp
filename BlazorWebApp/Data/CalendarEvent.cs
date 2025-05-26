using System;

namespace BlazorWebApp.Data
{
    public class CalendarEvent
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public bool AllDay { get; set; }
        public string? Url { get; set; }

        // Link this event to a specific article
        public Guid PublicationId { get; set; }
        public Publication Publication { get; set; } = null!;
    }
}
