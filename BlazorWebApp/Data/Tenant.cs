using System;

namespace BlazorWebApp.Data
{
    /// <summary>
    /// Represents a logical customer (church, co-op, business, etc.)
    /// </summary>
    public class Tenant
    {
        public Guid   Id   { get; set; }
        public string Name { get; set; } = string.Empty;

        // URL-friendly identifier used for subdomain or path segment
        public string Slug { get; set; } = string.Empty;
    }
}
