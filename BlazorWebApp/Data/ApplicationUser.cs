using Microsoft.AspNetCore.Identity;

namespace BlazorWebApp.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string PreferredLanguage { get; set; } = "en"; // match exactly
}
