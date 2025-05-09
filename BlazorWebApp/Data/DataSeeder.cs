// Auto-generated from Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorWebApp.Services;

namespace BlazorWebApp.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(WebApplication app)
        {
            // Data seeding
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            
                // Apply migrations
                db.Database.Migrate();
            
                // Seed roles
                foreach (var role in new[] { "Member", "Admin" })
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
                }
            
                // Pull default password from env-var
                var defaultPassword = app.Configuration["DefaultUser:Password"]
                                      ?? throw new InvalidOperationException(
                                            "DefaultUser__Password environment variable is not set");
            
                // Seed default Admin user
                const string adminEmail = "admin@yourdomain.com";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true
                    };
                    if ((await userManager.CreateAsync(adminUser, defaultPassword)).Succeeded)
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            
                // Seed default Member user
                const string memberEmail = "member@yourdomain.com";
                var memberUser = await userManager.FindByEmailAsync(memberEmail);
                if (memberUser == null)
                {
                    memberUser = new ApplicationUser
                    {
                        UserName = memberEmail,
                        Email = memberEmail,
                        EmailConfirmed = true
                    };
                    if ((await userManager.CreateAsync(memberUser, defaultPassword)).Succeeded)
                        await userManager.AddToRoleAsync(memberUser, "Member");
                }
            
                // Seed second default Member user
                const string secondMemberEmail = "member2@yourdomain.com";
                var secondMemberUser = await userManager.FindByEmailAsync(secondMemberEmail);
                if (secondMemberUser == null)
                {
                    secondMemberUser = new ApplicationUser
                    {
                        UserName = secondMemberEmail,
                        Email = secondMemberEmail,
                        EmailConfirmed = true
                    };
                    if ((await userManager.CreateAsync(secondMemberUser, defaultPassword)).Succeeded)
                        await userManager.AddToRoleAsync(secondMemberUser, "Member");
                }
            
            }
        }
    }
}
