using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BlazorWebApp.Data.Seeders;

namespace BlazorWebApp.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(WebApplication app)
        {
            // 1) Create a scope for resolving services
            using var scope = app.Services.CreateScope();
            var sp      = scope.ServiceProvider;

            // 2) Resolve dependencies
            var factory = sp.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
            var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var config  = sp.GetRequiredService<IConfiguration>();

            // 3) Migrate database
            await using (var db = factory.CreateDbContext())
            {
                await db.Database.MigrateAsync();
            }

            // 4) Seed Identity (roles + users)
            await RolesSeeder.SeedAsync(roleMgr);
            await UsersSeeder.SeedAsync(userMgr, config);

            // 5) Seed application data
            await RunSeederAsync(factory, ContentTypeSeeder.SeedAsync);
            await RunSeederAsync(factory, CategorySeeder.SeedAsync);
            await RunSeederAsync(factory, PublicationSeeder.SeedAsync);
            await RunSeederAsync(factory, MenuItemSeeder.SeedAsync);
            await RunSeederAsync(factory, ImageSeeder.SeedAsync);
        }

        /// <summary>
        /// Helper to create a fresh DbContext, run the seeder, then dispose.
        /// </summary>
        private static async Task RunSeederAsync(
            IDbContextFactory<ApplicationDbContext> factory,
            Func<ApplicationDbContext, Task> seeder)
        {
            await using var db = factory.CreateDbContext();
            await seeder(db);
        }
    }
}