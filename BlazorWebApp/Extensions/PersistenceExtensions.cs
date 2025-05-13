using BlazorWebApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using BlazorWebApp.Services;

namespace BlazorWebApp.Extensions
{
    public static class PersistenceExtensions
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            var conn = configuration
                .GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // 1) ONLY register the factory
            services.AddDbContextFactory<ApplicationDbContext>(opts =>
                opts.UseNpgsql(conn)
            );

            // 2) Shim so that ANY direct ApplicationDbContext injection (Identity, DataSeeder, etc.)
            //    will resolve via factory.CreateDbContext()
            services.AddScoped(sp =>
                sp.GetRequiredService<IDbContextFactory<ApplicationDbContext>>()
                  .CreateDbContext()
            );

            services.AddDatabaseDeveloperPageExceptionFilter();

            // 3) Identity wiring (unchanged)
            services.AddIdentity<ApplicationUser, IdentityRole>(opts =>
                opts.SignIn.RequireConfirmedAccount = true
            )
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

            services.AddTransient<IEmailSender, EmailSender>();

            return services;
        }
    }
}
