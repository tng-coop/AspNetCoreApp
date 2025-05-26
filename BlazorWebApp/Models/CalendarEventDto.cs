using System.Text.Json.Serialization;

namespace BlazorWebApp.Models
{
    public class CalendarEventDto
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("start")]
        public string Start { get; set; } = string.Empty;

        [JsonPropertyName("end")]
        public string? End { get; set; }

        [JsonPropertyName("allDay")]
        public bool? AllDay { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}
