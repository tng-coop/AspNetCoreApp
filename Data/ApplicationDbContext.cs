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
            new Member { Id = 1, FirstName = "Simon", LastName = "Peter", Email = "simon.peter@example.com" },
            new Member { Id = 2, FirstName = "Andrew", LastName = "", Email = "andrew@example.com" },
            new Member { Id = 3, FirstName = "James", LastName = "son of Zebedee", Email = "james.zebedee@example.com" },
            new Member { Id = 4, FirstName = "John", LastName = "son of Zebedee", Email = "john.zebedee@example.com" },
            new Member { Id = 5, FirstName = "Philip", LastName = "", Email = "philip@example.com" },
            new Member { Id = 6, FirstName = "Bartholomew", LastName = "", Email = "bartholomew@example.com" },
            new Member { Id = 7, FirstName = "Thomas", LastName = "", Email = "thomas@example.com" },
            new Member { Id = 8, FirstName = "Matthew", LastName = "Levi", Email = "matthew.levi@example.com" },
            new Member { Id = 9, FirstName = "James", LastName = "son of Alphaeus", Email = "james.alphaeus@example.com" },
            new Member { Id = 10, FirstName = "Thaddaeus", LastName = "", Email = "thaddaeus@example.com" },
            new Member { Id = 11, FirstName = "Simon", LastName = "the Zealot", Email = "simon.zealot@example.com" },
            new Member { Id = 12, FirstName = "Judas", LastName = "Iscariot", Email = "judas.iscariot@example.com" },
            new Member { Id = 13, FirstName = "Matthias", LastName = "", Email = "matthias@example.com" },
            new Member { Id = 14, FirstName = "Paul", LastName = "", Email = "paul@example.com" }
        );
    }
}
