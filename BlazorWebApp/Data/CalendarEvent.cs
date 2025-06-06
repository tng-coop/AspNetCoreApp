using System;

namespace BlazorWebApp.Data
{
    public class CalendarEvent
    {
        public Guid Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public bool AllDay { get; set; }

        // Link this event to a specific article
        public Guid PublicationId { get; set; }
        public Publication Publication { get; set; } = null!;
    }
}
