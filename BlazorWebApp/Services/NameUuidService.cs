using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlazorWebApp.Data;

namespace BlazorWebApp.Services
{
    public class NameUuidService : INameUuidService
    {
        private readonly ApplicationDbContext _db;
        public NameUuidService(ApplicationDbContext db) => _db = db;

        public async Task<Guid?> GetLatestUuidForNameAsync(string name, string? ownerId = null)
        {
            return await _db.NameUuidRetentions
                .Where(r => r.Name == name
                         && (ownerId == null || r.OwnerId == ownerId))
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => r.Uuid)
                .FirstOrDefaultAsync();
        }
    }
}
