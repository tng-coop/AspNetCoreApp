using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using AspNetCoreApp.Data;
using Microsoft.EntityFrameworkCore;
using AspNetCoreApp;

public class DatabaseFixture : WebApplicationFactory<Program>, IDisposable
{
    public ApplicationDbContext DbContext { get; }

    public DatabaseFixture()
    {
        // Set up test environment
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
        var scope = Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        DbContext.Database.EnsureDeleted();
        DbContext.Database.Migrate();

        // Seed initial data here if needed
        SeedDatabase();
    }

    private void SeedDatabase()
    {
        // Add common seed data here
    }

    public new void Dispose()
    {
        DbContext.Dispose();
        base.Dispose();
    }
}
