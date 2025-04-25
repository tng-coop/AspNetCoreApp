using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlazorWebApp.Data;         // for ApplicationDbContext, NameRetention
using BlazorWebApp.Services;     // for INameService

namespace BlazorWebApp.Services
{
    public class NameService : INameService
    {
        private readonly ApplicationDbContext _db;
        public NameService(ApplicationDbContext db) => _db = db;

        public async Task<string?> GetLatestForNameAsync(string name, string? ownerId = null)
        {
            return await _db.NameRetentions
                .Where(r => r.Name == name
                         && (ownerId == null || r.OwnerId == ownerId))
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => r.Value)
                .FirstOrDefaultAsync();
        }

        public async Task SetNameAsync(string name, string value, string? ownerId = null)
        {
            var entry = new NameRetention
            {
                Name      = name,
                Value     = value,
                CreatedAt = DateTime.UtcNow,
                OwnerId   = ownerId
            };
            _db.NameRetentions.Add(entry);
            await _db.SaveChangesAsync();
        }
    }
}
