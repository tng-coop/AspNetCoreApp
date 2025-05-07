// Auto-generated from Program.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using BlazorWebApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using BlazorWebApp.Services;

namespace BlazorWebApp.Extensions;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure PostgreSQL
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddDatabaseDeveloperPageExceptionFilter();

        // Identity setup with Default UI and Token Providers
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();
        services.AddTransient<IEmailSender, EmailSender>();

        return services;
    }
}
