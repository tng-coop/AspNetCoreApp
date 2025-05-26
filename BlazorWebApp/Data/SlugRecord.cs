using System;

namespace BlazorWebApp.Data
{
    public class SlugRecord
    {
        public Guid Id { get; set; }
        public string Value { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string EntityType { get; set; } = string.Empty;
    }
}
