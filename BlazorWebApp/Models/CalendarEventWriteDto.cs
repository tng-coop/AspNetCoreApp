using System;

namespace BlazorWebApp.Models
{
    public class CalendarEventWriteDto
    {
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public bool AllDay { get; set; }
    }
}
