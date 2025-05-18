using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorWebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<MenuItem>    MenuItems     { get; set; } = null!;
        public DbSet<Publication> Publications   { get; set; } = null!;
        public DbSet<ImageAsset>  Images         { get; set; } = null!;
        public DbSet<Category>    Categories     { get; set; } = null!;
        public DbSet<PublicationCategory> PublicationCategories { get; set; } = null!;
        public DbSet<PublicationRevision> PublicationRevisions { get; set; } = null!;
        public DbSet<Tenant> Tenants { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // --- Category entity configuration ---
            builder.Entity<Category>(e =>
            {
                e.HasKey(c => c.Id);
                e.Property(c => c.Name).IsRequired();
                e.Property(c => c.Slug).IsRequired();
                e.HasIndex(c => c.Slug)
                 .IsUnique();
                e.HasOne(c => c.Parent)
                 .WithMany(c => c.Children)
                 .HasForeignKey(c => c.ParentCategoryId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // --- MenuItem entity configuration ---
            builder.Entity<MenuItem>(e =>
            {
                e.HasKey(mi => mi.Id);
                e.Property(mi => mi.Slug).IsRequired();
                e.HasIndex(mi => mi.Slug)
                 .IsUnique();
                // Optional: configure parent-child relationship if not already annotated
                e.HasOne(mi => mi.Parent)
                 .WithMany(mi => mi.Children)
                 .HasForeignKey(mi => mi.ParentMenuItemId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // --- PublicationRevision configuration ---
            builder.Entity<PublicationRevision>(e =>
            {
                e.HasKey(r => r.Id);
                e.Property(r => r.CreatedAt)
                 .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                e.HasOne(r => r.Publication)
                 .WithMany(p => p.PublicationRevisions)
                 .HasForeignKey(r => r.PublicationId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // --- PublicationCategory join table configuration ---
            builder.Entity<PublicationCategory>(e =>
            {
                e.HasKey(pc => new { pc.PublicationId, pc.CategoryId });
                e.HasOne(pc => pc.Publication)
                 .WithMany(p => p.PublicationCategories)
                 .HasForeignKey(pc => pc.PublicationId);
                e.HasOne(pc => pc.Category)
                 .WithMany(c => c.PublicationCategories)
                 .HasForeignKey(pc => pc.CategoryId);
            });

            // --- Publication entity configuration ---
            builder.Entity<Publication>(e =>
            {
                e.HasKey(p => p.Id);
                e.Property(p => p.Title).IsRequired();
                e.Property(p => p.Html)
                 .HasDefaultValue(string.Empty);
                e.Property(p => p.Status)
                 .HasDefaultValue(PublicationStatus.Draft);
                e.Property(p => p.CreatedAt)
                 .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                e.Property(p => p.PublishedAt)
                 .IsRequired(false);
                e.HasIndex(p => p.CreatedAt);
                    e.Property(p => p.IsFeatured)
     .HasDefaultValue(false);
    e.Property(p => p.FeaturedOrder)
     .HasDefaultValue(0);
    e.HasIndex(p => new { p.IsFeatured, p.FeaturedOrder });
            });
        }
    }
}
