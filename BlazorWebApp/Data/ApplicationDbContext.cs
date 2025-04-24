using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorWebApp.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    // ← NEW: your retention table
    public DbSet<NameRetention> NameRetentions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // one-to-many: ApplicationUser → NameRetention
        builder.Entity<NameRetention>()
            .HasOne(r => r.Owner)
            .WithMany(u => u.NameRetentions)
            .HasForeignKey(r => r.OwnerId)
            .IsRequired(false);

        // index for “latest per name” lookups
        builder.Entity<NameRetention>()
            .HasIndex(r => new { r.Name, r.CreatedAt });
    }
}
