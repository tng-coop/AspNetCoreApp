using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorWebApp.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<NameRetention> NameRetentions { get; set; }
    public DbSet<Note> Notes { get; set; }

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
        // default UTC timestamp for CreatedAt
        builder.Entity<NameRetention>()
            .Property(r => r.CreatedAt)
            .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

        // model config for Note
        builder.Entity<Note>()
            .HasKey(n => n.Id);
        builder.Entity<Note>()
            .HasOne(n => n.Owner)
            .WithMany(u => u.Notes)
            .HasForeignKey(n => n.OwnerId)
            .IsRequired(false);
        // index on CreatedAt
        builder.Entity<Note>()
            .HasIndex(n => n.CreatedAt);
        // default UTC timestamp for CreatedAt
        builder.Entity<Note>()
            .Property(n => n.CreatedAt)
            .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
    }
}
