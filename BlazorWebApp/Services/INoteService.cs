// Services/INoteService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorWebApp.Data;

namespace BlazorWebApp.Services
{
    public interface INoteService
    {
        Task<Guid>               CreateNoteAsync(string title, string content, bool isPublic, string? ownerId = null);
        Task<Note?>               GetNoteAsync(Guid id, string? ownerId = null, bool isAdmin = false);
        Task<List<Note>>         GetPublicNotesAsync(string? ownerId = null);

        // ← NEW: update an existing note
        Task                     UpdateNoteAsync(Guid id, string title, string content, bool isPublic);
    }
}
