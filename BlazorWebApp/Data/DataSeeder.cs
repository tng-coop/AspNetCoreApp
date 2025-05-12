using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BlazorWebApp.Data.Seeders;

namespace BlazorWebApp.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db      = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var config  = app.Configuration;

            await db.Database.MigrateAsync();

            await RolesSeeder.SeedAsync(roleMgr);
            await UsersSeeder.SeedAsync(userMgr, config);
            await ContentTypeSeeder.SeedAsync(db);
            await CategorySeeder.SeedAsync(db);
            await MenuItemSeeder.SeedAsync(db);
        }
    }
}
