using Microsoft.AspNetCore.Identity;
using AspNetCoreApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreApp.Data;

public static class DbInitializer
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Administrator", "Moderator", "Member" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    public static async Task SeedUsersAndMembersAsync(UserManager<IdentityUser> userManager, ApplicationDbContext context)
    {
        var usersWithRoles = new (string Email, string Role)[]
        {
            ("admin@example.com", "Administrator"),
            ("simon.peter@example.com", "Member"),
            ("andrew@example.com", "Member"),
            ("james.zebedee@example.com", "Member"),
            ("john.zebedee@example.com", "Member"),
            ("philip@example.com", "Member"),
            ("bartholomew@example.com", "Member"),
            ("thomas@example.com", "Member"),
            ("matthew.levi@example.com", "Member"),
            ("james.alphaeus@example.com", "Member"),
            ("thaddaeus@example.com", "Member"),
            ("simon.zealot@example.com", "Member"),
            ("judas.iscariot@example.com", "Member"),
            ("matthias@example.com", "Member"),
            ("paul@example.com", "Member")
        };

        foreach (var (email, role) in usersWithRoles)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "SecureP@ssword123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);

                    // ✅ Also add a corresponding Member record
                    var names = email.Split('@')[0].Split('.');
                    var firstName = names.FirstOrDefault() ?? "First";
                    var lastName = names.Skip(1).FirstOrDefault() ?? "Last";

                    var memberExists = await context.Members.AnyAsync(m => m.Email == email);
                    if (!memberExists)
                    {
                        var member = new Member
                        {
                            FirstName = Capitalize(firstName),
                            LastName = Capitalize(lastName),
                            Email = email,
                            JoinedDate = DateTime.UtcNow,
                            UserId = user.Id  // link the IdentityUser here
                        };

                        context.Members.Add(member);
                    }
                }
                else
                {
                    throw new Exception($"Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        await context.SaveChangesAsync();
    }

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        await SeedRolesAsync(roleManager);
        await SeedUsersAndMembersAsync(userManager, context);
    }

    private static string Capitalize(string input)
    {
        return string.IsNullOrWhiteSpace(input)
            ? input
            : char.ToUpper(input[0]) + input.Substring(1).ToLower();
    }
}
