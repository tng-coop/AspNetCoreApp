using Microsoft.AspNetCore.Identity;

namespace BlazorWebApp.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string PreferredLanguage { get; set; } = "en";

    // ← NEW: one-to-many nav-prop for NameRetention
    public ICollection<NameRetention> NameRetentions { get; set; }
        = new List<NameRetention>();
}
