using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BlazorWebApp.Data;

namespace BlazorWebApp.Services
{
    public class NoteService : INoteService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<NoteService> _logger;

        public NoteService(ApplicationDbContext db, ILogger<NoteService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Guid> CreateNoteAsync(string title, string content, bool isPublic, string? ownerId = null)
        {
            _logger.LogInformation("NoteService.CreateNoteAsync starting: Title='{Title}', contentLength={Len}, isPublic={Pub}",
                title, content?.Length, isPublic);

            var note = new Note
            {
                Id = Guid.NewGuid(),
                Title = title,
                Content = content!,    // suppress null-warning
                IsPublic = isPublic,
                CreatedAt = DateTime.UtcNow,
                OwnerId = ownerId
            };

            _db.Notes.Add(note);
            await _db.SaveChangesAsync();

            _logger.LogInformation("NoteService.CreateNoteAsync saved note with Id={Id}", note.Id);
            return note.Id;
        }

        public async Task<Note?> GetNoteAsync(Guid id, string? ownerId = null)
        {
            _logger.LogInformation("NoteService.GetNoteAsync loading Id={Id} for ownerId={OwnerId}", id, ownerId);
            return await _db.Notes
                .FirstOrDefaultAsync(n =>
                    n.Id == id
                    && (n.IsPublic || n.OwnerId == ownerId)
                );
        }

        public async Task<List<Note>> GetPublicNotesAsync(string? ownerId = null)
        {
            _logger.LogInformation("NoteService.GetPublicNotesAsync loading all notes for ownerId={OwnerId}", ownerId);

            return await _db.Notes
                .Where(n => n.IsPublic || n.OwnerId == ownerId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateNoteAsync(Guid id, string title, string content, bool isPublic)
        {
            _logger.LogInformation("NoteService.UpdateNoteAsync starting: Id={Id}, Title='{Title}', isPublic={Pub}",
                id, title, isPublic);

            var note = await _db.Notes.FindAsync(id);
            if (note == null)
                throw new KeyNotFoundException($"Note {id} not found");

            note.Title = title;
            note.Content = content;
            note.IsPublic = isPublic;

            await _db.SaveChangesAsync();

            _logger.LogInformation("NoteService.UpdateNoteAsync saved changes for Id={Id}", id);
        }
    }
}
