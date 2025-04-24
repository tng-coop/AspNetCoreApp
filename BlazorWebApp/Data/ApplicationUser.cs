using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace BlazorWebApp.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string PreferredLanguage { get; set; } = "en";

    // ← NEW: one-to-many nav-prop for NameUuidRetention
    public ICollection<NameUuidRetention> NameUuidRetentions { get; set; }
        = new List<NameUuidRetention>();
}
