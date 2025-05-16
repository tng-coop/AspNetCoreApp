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

            // Seed your default tenant(s) here
            db.Tenants.Add(new Tenant {
                Id   = Guid.NewGuid(),
                Name = "Acme Corporation",
                Slug = "acme"
            });

            await db.SaveChangesAsync();
        }
    }
}
