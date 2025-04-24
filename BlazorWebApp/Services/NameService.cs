using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlazorWebApp.Data;

namespace BlazorWebApp.Services
{
    public class NameService : INameService
    {
        private readonly ApplicationDbContext _db;
        public NameService(ApplicationDbContext db) => _db = db;

        public async Task<String?> GetLatestForNameAsync(string name, string? ownerId = null)
        {
            return await _db.NameRetentions
                .Where(r => r.Name == name
                         && (ownerId == null || r.OwnerId == ownerId))
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => r.Value)
                .FirstOrDefaultAsync();
        }
    }
}
