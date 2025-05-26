using System;

namespace BlazorWebApp.Models
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// True if the comment has been marked as read.
        /// </summary>
        public bool IsRead { get; set; }
    }
}
