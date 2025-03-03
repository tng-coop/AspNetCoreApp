using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AspNetCoreApp.Models;

namespace AspNetCoreApp.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext(options)
{
    public DbSet<Member> Members { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Member>().HasData(
            new Member
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                JoinedDate = new DateTime(2024, 2, 20, 0, 0, 0, DateTimeKind.Utc)
            },
            new Member
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                JoinedDate = new DateTime(2024, 2, 25, 0, 0, 0, DateTimeKind.Utc)
            },
            new Member
            {
                Id = 3,
                FirstName = "Alice",
                LastName = "Johnson",
                Email = "alice.johnson@example.com",
                JoinedDate = new DateTime(2024, 2, 28, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
