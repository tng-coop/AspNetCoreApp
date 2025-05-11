using System.Collections.Generic;

namespace BlazorWebApp.Models
{
    public class MenuItem
    {
        public string Title { get; set; } = string.Empty;
        public string IconCss { get; set; } = string.Empty;
        public string? Url { get; set; }
        public List<MenuItem> Children { get; set; } = new List<MenuItem>();
    }
}
