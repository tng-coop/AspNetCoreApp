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

        modelBuilder.Entity<IdentityUser>().HasData(
            new IdentityUser { Id = Guid.NewGuid().ToString(), UserName = "simon.peter@example.com", NormalizedUserName = "SIMON.PETER@EXAMPLE.COM", Email = "simon.peter@example.com", NormalizedEmail = "SIMON.PETER@EXAMPLE.COM", EmailConfirmed = true },
            new IdentityUser { Id = Guid.NewGuid().ToString(), UserName = "andrew@example.com", NormalizedUserName = "ANDREW@EXAMPLE.COM", Email = "andrew@example.com", NormalizedEmail = "ANDREW@EXAMPLE.COM", EmailConfirmed = true },
            new IdentityUser { Id = Guid.NewGuid().ToString(), UserName = "james.zebedee@example.com", NormalizedUserName = "JAMES.ZEBEDEE@EXAMPLE.COM", Email = "james.zebedee@example.com", NormalizedEmail = "JAMES.ZEBEDEE@EXAMPLE.COM", EmailConfirmed = true },
            new IdentityUser { Id = Guid.NewGuid().ToString(), UserName = "john.zebedee@example.com", NormalizedUserName = "JOHN.ZEBEDEE@EXAMPLE.COM", Email = "john.zebedee@example.com", NormalizedEmail = "JOHN.ZEBEDEE@EXAMPLE.COM", EmailConfirmed = true },
            new IdentityUser { Id = Guid.NewGuid().ToString(), UserName = "philip@example.com", NormalizedUserName = "PHILIP@EXAMPLE.COM", Email = "philip@example.com", NormalizedEmail = "PHILIP@EXAMPLE.COM", EmailConfirmed = true },
            new IdentityUser { Id = Guid.NewGuid().ToString(), UserName = "bartholomew@example.com", NormalizedUserName = "BARTHOLOMEW@EXAMPLE.COM", Email = "bartholomew@example.com", NormalizedEmail = "BARTHOLOMEW@EXAMPLE.COM", EmailConfirmed = true },
            new IdentityUser { Id = Guid.NewGuid().ToString(), UserName = "thomas@example.com", NormalizedUserName = "THOMAS@EXAMPLE.COM", Email = "thomas@example.com", NormalizedEmail = "THOMAS@EXAMPLE.COM", EmailConfirmed = true },
            new IdentityUser { Id = Guid.NewGuid().ToString(), UserName = "matthew.levi@example.com", NormalizedUserName = "MATTHEW.LEVI@EXAMPLE.COM", Email = "matthew.levi@example.com", NormalizedEmail = "MATTHEW.LEVI@EXAMPLE.COM", EmailConfirmed = true },
            new IdentityUser { Id = Guid.NewGuid().ToString(), UserName = "james.alphaeus@example.com", NormalizedUserName = "JAMES.ALPHAEUS@EXAMPLE.COM", Email = "james.alphaeus@example.com", NormalizedEmail = "JAMES.ALPHAEUS@EXAMPLE.COM", EmailConfirmed = true },
            new IdentityUser { Id = Guid.NewGuid().ToString(), UserName = "thaddaeus@example.com", NormalizedUserName = "THADDAEUS@EXAMPLE.COM", Email = "thaddaeus@example.com", NormalizedEmail = "THADDAEUS@EXAMPLE.COM", EmailConfirmed = true },
            new IdentityUser { Id = Guid.NewGuid().ToString(), UserName = "simon.zealot@example.com", NormalizedUserName = "SIMON.ZEALOT@EXAMPLE.COM", Email = "simon.zealot@example.com", NormalizedEmail = "SIMON.ZEALOT@EXAMPLE.COM", EmailConfirmed = true },
            new IdentityUser { Id = Guid.NewGuid().ToString(), UserName = "judas.iscariot@example.com", NormalizedUserName = "JUDAS.ISCARIOT@EXAMPLE.COM", Email = "judas.iscariot@example.com", NormalizedEmail = "JUDAS.ISCARIOT@EXAMPLE.COM", EmailConfirmed = true },
            new IdentityUser { Id = Guid.NewGuid().ToString(), UserName = "matthias@example.com", NormalizedUserName = "MATTHIAS@EXAMPLE.COM", Email = "matthias@example.com", NormalizedEmail = "MATTHIAS@EXAMPLE.COM", EmailConfirmed = true },
            new IdentityUser { Id = Guid.NewGuid().ToString(), UserName = "paul@example.com", NormalizedUserName = "PAUL@EXAMPLE.COM", Email = "paul@example.com", NormalizedEmail = "PAUL@EXAMPLE.COM", EmailConfirmed = true }
        );
    }
}
