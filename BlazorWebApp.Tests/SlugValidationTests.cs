using System;
using System.Threading.Tasks;
using BlazorWebApp.Data;
using BlazorWebApp.Models;
using BlazorWebApp.Services;
using BlazorWebApp.Utils;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class SlugValidationTests
{
    private class TestFactory : IDbContextFactory<ApplicationDbContext>
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        public TestFactory(DbContextOptions<ApplicationDbContext> options) => _options = options;
        public ApplicationDbContext CreateDbContext() => new ApplicationDbContext(_options);
    }

    [Fact]
    public async Task CategoryService_Rejects_Invalid_Slug()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var service = new CategoryService(new TestFactory(options));
        var dto = new CategoryWriteDto { Name = "Test", Slug = "invalid slug" };
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task PublicationService_Rejects_Invalid_Slug()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var service = new PublicationService(new TestFactory(options));
        var dto = new PublicationWriteDto { Title = "T", Html = "", Slug = "bad slug" };
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
    }
}

