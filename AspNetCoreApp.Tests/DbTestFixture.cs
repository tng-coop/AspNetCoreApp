using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using AspNetCoreApp;
using AspNetCoreApp.Data;  // Adjust to your actual namespace for DbContext

namespace AspNetCoreApp.Tests
{
    public class DbTestFixture : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public DbTestFixture(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        public ApplicationDbContext GetDbContext()
        {
            var scope = _factory.Services.CreateScope();  // Correct usage of CreateScope
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return dbContext;
        }
    }
}


public class DbTestFixture : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ApplicationDbContext _dbContext;

    public DbTestFixture()
    {
        // Set up the application factory and database context
        _factory = new WebApplicationFactory<Program>();
        var scope = _factory.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Apply migrations and seed the database
        _dbContext.Database.Migrate();  // Ensure migrations are applied
        DbInitializer.InitializeAsync(scope.ServiceProvider).Wait();  // Seed data
    }

    public HttpClient CreateClient() => _factory.CreateClient();

    // Dispose of resources when done
    public void Dispose()
    {
        _dbContext.Dispose();
        _factory.Dispose();
    }
}
