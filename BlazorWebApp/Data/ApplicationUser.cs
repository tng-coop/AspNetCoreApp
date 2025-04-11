using Microsoft.AspNetCore.Identity;

namespace BlazorWebApp.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string PreferredCulture { get; set; } = "en-US"; // default to English
}

