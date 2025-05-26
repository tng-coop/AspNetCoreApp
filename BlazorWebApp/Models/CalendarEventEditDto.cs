using System;

namespace BlazorWebApp.Models
{
    public class CalendarEventEditDto
    {
        public Guid Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public bool AllDay { get; set; }
        public string? Url { get; set; }
    }
}
