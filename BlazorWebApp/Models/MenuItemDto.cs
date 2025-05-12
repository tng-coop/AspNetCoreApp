using System;
using System.Collections.Generic;

namespace BlazorWebApp.Models
{
    public class MenuItemDto
    {
        public Guid Id            { get; set; }
        public string Title       { get; set; } = string.Empty;
        public string Slug        { get; set; } = string.Empty;
        public string IconCss     { get; set; } = string.Empty;
        public int    SortOrder   { get; set; }
        public Guid?  ContentItemId { get; set; }
        public List<MenuItemDto> Children { get; set; } = new();
    }
}
