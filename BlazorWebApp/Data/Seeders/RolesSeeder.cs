using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace BlazorWebApp.Data.Seeders
{
    public static class RolesSeeder
    {
        private static readonly string[] DefaultRoles = { "Member", "Admin", "SuperAdmin" };

        public static async Task SeedAsync(RoleManager<IdentityRole> roleMgr)
        {
            foreach (var role in DefaultRoles)
                if (!await roleMgr.RoleExistsAsync(role))
                    await roleMgr.CreateAsync(new IdentityRole(role));
        }
    }
}
