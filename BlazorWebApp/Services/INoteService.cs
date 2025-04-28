using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorWebApp.Data;

namespace BlazorWebApp.Services
{
    public interface INoteService
    {
        Task<Guid>          CreateNoteAsync(string title, string content, bool isPublic, string? ownerId = null);
        Task<Note?>         GetNoteAsync(Guid id);
        Task<List<Note>>    GetPublicNotesAsync();
    }
}
