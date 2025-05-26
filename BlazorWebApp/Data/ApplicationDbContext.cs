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

        public DbSet<Publication> Publications   { get; set; } = null!;
        public DbSet<FileAsset>   Files         { get; set; } = null!;
        public DbSet<Category>    Categories    { get; set; } = null!;
        public DbSet<PublicationRevision> PublicationRevisions { get; set; } = null!;
        public DbSet<Tenant>      Tenants       { get; set; } = null!;
        public DbSet<SlugRecord>  Slugs         { get; set; } = null!;
        public DbSet<CalendarEvent> CalendarEvents { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // --- Category entity configuration ---
            builder.Entity<Category>(e =>
            {
                e.HasKey(c => c.Id);
                e.Property(c => c.Name).IsRequired();
                e.Property(c => c.NameJa).IsRequired(false);
                e.Property(c => c.Slug).IsRequired();
                e.Property(c => c.SortOrder).IsRequired(false);
                e.HasIndex(c => c.Slug)
                 .IsUnique();
                e.HasIndex(c => c.SortOrder);
                e.HasOne(c => c.Parent)
                 .WithMany(c => c.Children)
                 .HasForeignKey(c => c.ParentCategoryId)
                 .OnDelete(DeleteBehavior.Restrict);
            });


            // --- PublicationRevision configuration ---
            builder.Entity<PublicationRevision>(e =>
            {
                e.HasKey(r => r.Id);
                e.Property(r => r.CreatedAt)
                 .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                e.Property(r => r.TitleJa).IsRequired(false);
                e.HasOne(r => r.Publication)
                 .WithMany(p => p.PublicationRevisions)
                 .HasForeignKey(r => r.PublicationId)
                 .OnDelete(DeleteBehavior.Cascade);
            });


            // --- Publication entity configuration ---
            builder.Entity<Publication>(e =>
            {
                e.HasKey(p => p.Id);
                e.Property(p => p.Title).IsRequired();
                e.Property(p => p.TitleJa).IsRequired(false);
                e.Property(p => p.Slug).IsRequired();
                e.HasIndex(p => p.Slug).IsUnique();
                e.Property(p => p.Html)
                 .HasDefaultValue(string.Empty);
                e.Property(p => p.Status)
                 .HasDefaultValue(PublicationStatus.Draft);
                e.Property(p => p.CreatedAt)
                 .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                e.Property(p => p.PublishedAt)
                 .IsRequired(false);
                e.HasIndex(p => p.CreatedAt);
                e.Property(p => p.FeaturedOrder)
                    .HasDefaultValue(0);
                e.HasIndex(p => p.FeaturedOrder);
                e.HasOne(p => p.Category)
                 .WithMany(c => c.Publications)
                 .HasForeignKey(p => p.CategoryId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Tenant entity configuration ---
            builder.Entity<Tenant>(e =>
            {
                e.HasKey(t => t.Id);
                e.Property(t => t.Name).IsRequired();
                e.Property(t => t.Slug).IsRequired();
                e.HasIndex(t => t.Slug).IsUnique();
            });

            // --- SlugRecord configuration ---
            builder.Entity<SlugRecord>(e =>
            {
                e.HasKey(s => s.Id);
                e.Property(s => s.Value).IsRequired();
                e.Property(s => s.EntityType).IsRequired();
                e.HasIndex(s => s.Value).IsUnique();
            });

            // --- CalendarEvent configuration ---
            builder.Entity<CalendarEvent>(e =>
            {
                e.HasKey(c => c.Id);
                e.Property(c => c.Title).IsRequired();
                e.Property(c => c.Start).IsRequired();
                e.Property(c => c.End).IsRequired(false);
                e.Property(c => c.AllDay).HasDefaultValue(false);
                e.Property(c => c.Url).IsRequired(false);

                e.HasOne(c => c.Publication)
                 .WithMany(p => p.CalendarEvents)
                 .HasForeignKey(c => c.PublicationId)
                .OnDelete(DeleteBehavior.Cascade);
            });

            // --- Comment configuration ---
            builder.Entity<Comment>(e =>
            {
                e.HasKey(c => c.Id);
                e.Property(c => c.Text).IsRequired();
                e.Property(c => c.CreatedAt)
                    .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            });
        }
    }
}
