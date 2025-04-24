using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorWebApp.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    // ← NEW: your retention table
    public DbSet<NameUuidRetention> NameUuidRetentions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // one-to-many: ApplicationUser → NameUuidRetention
        builder.Entity<NameUuidRetention>()
            .HasOne(r => r.Owner)
            .WithMany(u => u.NameUuidRetentions)
            .HasForeignKey(r => r.OwnerId)
            .IsRequired(false);

        // index for “latest per name” lookups
        builder.Entity<NameUuidRetention>()
            .HasIndex(r => new { r.Name, r.CreatedAt });
    }
}
