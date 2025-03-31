using System;
using Microsoft.AspNetCore.Identity;

namespace AspNetCoreApp.Models;

public class Member
{
    public int Id { get; set; }
    
    // Custom fields specific to Member
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

    // Foreign key linking to IdentityUser
    public string UserId { get; set; } = null!;
    public IdentityUser User { get; set; } = null!;
}
