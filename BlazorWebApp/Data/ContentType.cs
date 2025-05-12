using System;

namespace BlazorWebApp.Data
{
    /// <summary>
    /// Defines a type of content (Page, News, Event, Bulletin, etc.)
    /// </summary>
    public class ContentType
    {
        public Guid   Id   { get; set; }
        public string Name { get; set; } = string.Empty;

        // URL-friendly slug used when generating routes
        public string Slug { get; set; } = string.Empty;
    }
}
