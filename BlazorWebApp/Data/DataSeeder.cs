// Data/DataSeeder.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using BlazorWebApp.Data.Seeders;

namespace BlazorWebApp.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(WebApplication app)
        {
            // 1) Spin up a scope for resolving services
            using var scope = app.Services.CreateScope();
            var sp = scope.ServiceProvider;

            // 2) Grab the factory + identity managers + config
            var factory = sp.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
            var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var config = sp.GetRequiredService<IConfiguration>();

            // 3) Migrate with a fresh context
            await using (var db = factory.CreateDbContext())
                await db.Database.MigrateAsync();

            // 4) Seed Identity (roles + users)
            await RolesSeeder.SeedAsync(roleMgr);
            await UsersSeeder.SeedAsync(userMgr, config);

            // 5) Seed each area on its own fresh context
            await using (var db = factory.CreateDbContext())
                await ContentTypeSeeder.SeedAsync(db);

            await using (var db = factory.CreateDbContext())
                await CategorySeeder.SeedAsync(db);

            await using (var db = factory.CreateDbContext())
                await PublicationSeeder.SeedAsync(db);

            await using (var db = factory.CreateDbContext())
                await MenuItemSeeder.SeedAsync(db);
        }
    }
}
