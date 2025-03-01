using Microsoft.EntityFrameworkCore;

namespace AspNetCoreApp.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : DbContext(options)
{
    // Define entities here. Example:
    // public DbSet<Member> Members { get; set; }
}
