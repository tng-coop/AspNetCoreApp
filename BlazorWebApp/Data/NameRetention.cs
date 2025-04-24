using System;

namespace BlazorWebApp.Data
{
    public class NameRetention
    {
        public long     Id         { get; set; }      // surrogate PK
        public string   Name       { get; set; } = null!;  // stable key
        public string Value { get; set; } = null!;  // stable value
        public DateTime CreatedAt  { get; set; }          // insertion timestamp

        // optional ownership
        public string?            OwnerId { get; set; }
        public ApplicationUser?   Owner   { get; set; }
    }
}
