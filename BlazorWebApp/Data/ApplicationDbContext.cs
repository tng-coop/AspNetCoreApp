// Data/ApplicationDbContext.cs
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
        public DbSet<ContentType> ContentTypes { get; set; } = null!;
        public DbSet<MenuItem>    MenuItems    { get; set; } = null!;        

        public DbSet<Publication> Publications { get; set; } = null!;
        // ← Newly added DbSet for images
        public DbSet<ImageAsset> Images { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<PublicationCategory> PublicationCategories { get; set; } = null!;
        public DbSet<PublicationRevision> PublicationRevisions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // --- Category entity configuration --- 
            builder.Entity<Category>(e => { 
                e.HasKey(c => c.Id); 
                e.Property(c => c.Name).IsRequired(); 
                e.HasOne(c => c.Parent) 
                 .WithMany(c => c.Children) 
                 .HasForeignKey(c => c.ParentCategoryId) 
                 .OnDelete(DeleteBehavior.Restrict); 
            }); 
            
            // --- PublicationCategory join table configuration --- 
            builder.Entity<PublicationCategory>(e => { 
            // PublicationRevision configuration
            builder.Entity<PublicationRevision>(e =>{
                e.HasKey(r => r.Id);
                e.Property(r => r.CreatedAt).HasDefaultValueSql("NOW() AT TIME ZONE UTC");
                e.HasOne(r => r.Publication)
                 .WithMany()
                 .HasForeignKey(r => r.PublicationId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
                e.HasKey(pc => new { pc.PublicationId, pc.CategoryId }); 
                e.HasOne(pc => pc.Publication) 
                  .WithMany(p => p.PublicationCategories) 
                  .HasForeignKey(pc => pc.PublicationId); 
                e.HasOne(pc => pc.Category) 
                  .WithMany(c => c.PublicationCategories) 
                  .HasForeignKey(pc => pc.CategoryId); 
            });

        // Configure Publication entity
        builder.Entity<Publication>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Title).IsRequired();
            e.Property(p => p.Html).HasDefaultValue(string.Empty);
            e.Property(p => p.Status).HasDefaultValue(PublicationStatus.Draft);
            e.Property(p => p.CreatedAt)
             .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            e.Property(p => p.PublishedAt)
             .IsRequired(false);
            e.HasIndex(p => p.CreatedAt);
        });

        }
    }
}
