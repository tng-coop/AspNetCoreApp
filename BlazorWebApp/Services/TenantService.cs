using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlazorWebApp.Data;
using BlazorWebApp.Models;

namespace BlazorWebApp.Services
{
    public class TenantService : ITenantService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _factory;
        public TenantService(IDbContextFactory<ApplicationDbContext> factory) => _factory = factory;

        private ApplicationDbContext CreateDb() => _factory.CreateDbContext();

        public async Task<List<TenantDto>> ListAsync()
        {
            await using var db = CreateDb();
            return await db.Tenants
                .OrderBy(t => t.Name)
                .Select(t => new TenantDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Slug = t.Slug
                })
                .ToListAsync();
        }
    }
}
