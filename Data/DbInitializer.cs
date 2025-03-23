using Microsoft.AspNetCore.Identity;

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

    public static async Task SeedUsersAsync(UserManager<IdentityUser> userManager)
    {
        var usersWithPasswords = new (string Email, string Password, string Role)[]
        {
    ("admin@example.com", "SecureP@ssword123!", "Administrator"),
    ("simon.peter@example.com", "MemberP@ssword123!", "Member"),
    ("andrew@example.com", "MemberP@ssword123!", "Member"),
    ("james.zebedee@example.com", "MemberP@ssword123!", "Member"),
    ("john.zebedee@example.com", "MemberP@ssword123!", "Member"),
    ("philip@example.com", "MemberP@ssword123!", "Member"),
    ("bartholomew@example.com", "MemberP@ssword123!", "Member"),
    ("thomas@example.com", "MemberP@ssword123!", "Member"),
    ("matthew.levi@example.com", "MemberP@ssword123!", "Member"),
    ("james.alphaeus@example.com", "MemberP@ssword123!", "Member"),
    ("thaddaeus@example.com", "MemberP@ssword123!", "Member"),
    ("simon.zealot@example.com", "MemberP@ssword123!", "Member"),
    ("judas.iscariot@example.com", "MemberP@ssword123!", "Member"),
    ("matthias@example.com", "MemberP@ssword123!", "Member"),
    ("paul@example.com", "MemberP@ssword123!", "Member")
        };


        foreach (var (email, password, role) in usersWithPasswords)
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

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
                else
                {
                    throw new Exception($"Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager);
    }
}
