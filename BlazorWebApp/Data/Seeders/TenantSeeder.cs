using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlazorWebApp.Data;

namespace BlazorWebApp.Data.Seeders
{
    public static class TenantSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            if (await db.Tenants.AnyAsync())
                return;

            // Seed your default tenants with distinct slugs
            db.Tenants.AddRange(
                new Tenant
                {
                    Id   = Guid.NewGuid(),
                    Name = "Acme Corporation",
                    Slug = "acme"
                },
                new Tenant
                {
                    Id   = Guid.NewGuid(),
                    Name = "Beta Industries",
                    Slug = "beta"
                });

            await db.SaveChangesAsync();
        }
    }
}
