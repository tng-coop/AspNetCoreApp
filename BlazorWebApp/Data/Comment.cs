using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorWebApp.Data
{
    public class Comment
    {
        public Guid Id { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Indicates whether the comment has been read/acknowledged.
        /// </summary>
        public bool IsRead { get; set; }
    }
}
