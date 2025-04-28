using System;

namespace BlazorWebApp.Data
{
    public class Note
    {
        public Guid      Id        { get; set; }
        public string    Title     { get; set; } = null!;
        public string    Content   { get; set; } = null!;
        public bool      IsPublic  { get; set; } = true;
        public DateTime  CreatedAt { get; set; }

        // optional owner
        public string?           OwnerId { get; set; }
        public ApplicationUser?  Owner   { get; set; }
    }
}
