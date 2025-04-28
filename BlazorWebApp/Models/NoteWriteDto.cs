using System.ComponentModel.DataAnnotations;

namespace BlazorWebApp.Models
{
    public class NoteWriteDto
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title    { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required")]
        public string Content  { get; set; } = string.Empty;

        public bool   IsPublic { get; set; } = true;
    }
}
