using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AspNetCoreApp.Models;

namespace AspNetCoreApp.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<IdentityUser, IdentityRole, string>(options)
{
    public DbSet<Member> Members { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Member>().HasData(
            new Member { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", JoinedDate = DateTime.UtcNow }
        );
    }
}

