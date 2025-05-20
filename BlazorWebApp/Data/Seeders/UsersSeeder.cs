using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace BlazorWebApp.Data.Seeders
{
    public static class UsersSeeder
    {
        public static async Task SeedAsync(
            UserManager<ApplicationUser> userMgr,
            IConfiguration config)
        {
            var password = config["DefaultUser:Password"]
                ?? throw new InvalidOperationException("DefaultUser__Password not set");

            async Task Add(string email, string role)
            {
                if (await userMgr.FindByEmailAsync(email) is null)
                {
                    var u = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
                    if ((await userMgr.CreateAsync(u, password)).Succeeded)
                        await userMgr.AddToRoleAsync(u, role);
                }
            }

            await Add("superadmin@yourdomain.com", "SuperAdmin");
            await Add("admin@yourdomain.com", "Admin");
            await Add("member@yourdomain.com", "Member");
            await Add("member2@yourdomain.com", "Member");
        }
    }
}
