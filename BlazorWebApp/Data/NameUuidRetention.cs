using System;

namespace BlazorWebApp.Data
{
    public class NameUuidRetention
    {
        public long     Id         { get; set; }      // surrogate PK
        public string   Name       { get; set; } = null!;  // stable key
        public Guid     Uuid       { get; set; }          // generated GUID
        public DateTime CreatedAt  { get; set; }          // insertion timestamp

        // optional ownership
        public string?            OwnerId { get; set; }
        public ApplicationUser?   Owner   { get; set; }
    }
}
