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
                var nameSvc = scope.ServiceProvider.GetRequiredService<INameService>();
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
            
                // Seed video links
                var videoSeeds = new Dictionary<string, string>
                {
                    ["video1"] = "https://vimeo.com/1078878884",
                    ["video2"] = "https://vimeo.com/1078878886",
                    ["video3"] = "https://vimeo.com/1078878889",
                    ["video4"] = "https://vimeo.com/1078878893",
                    ["video5"] = "https://vimeo.com/1078878899",
                    ["video6"] = "https://vimeo.com/1078878852",
                    ["video7"] = "https://vimeo.com/1078878865",
                    ["video8"] = "https://vimeo.com/1078878869",
                    ["video9"] = "https://vimeo.com/1078878875",
                    ["video10"] = "https://vimeo.com/1078878880",
                };
            
                foreach (var (key, url) in videoSeeds)
                {
                    if (await nameSvc.GetLatestForNameAsync(key) is null)
                        await nameSvc.SetNameAsync(key, url, ownerId: null);
                }
            }
        }
    }
}
