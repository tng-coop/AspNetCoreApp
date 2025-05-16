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
    }
}
