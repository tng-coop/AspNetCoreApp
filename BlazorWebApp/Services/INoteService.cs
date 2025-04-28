using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorWebApp.Data;

namespace BlazorWebApp.Services
{
    public interface INoteService
    {
        Task<Guid> CreateNoteAsync(string title, string content, bool isPublic, string? ownerId = null);
        // change signature to accept ownerId
        Task<Note?> GetNoteAsync(Guid id, string? ownerId = null);

        Task<List<Note>> GetPublicNotesAsync();
    }
}
