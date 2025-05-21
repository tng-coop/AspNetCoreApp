using Bunit;
using BlazorWebApp.Components.Pages; // Ensure this matches your project's namespace
using BlazorWebApp.Models;
using BlazorWebApp.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class HomeComponentTests : TestContext
{
    private class StubTenantService : ITenantService
    {
        private readonly List<TenantDto> _tenants;
        public StubTenantService(List<TenantDto> tenants) => _tenants = tenants;
        public Task<List<TenantDto>> ListAsync() => Task.FromResult(_tenants);
    }
    [Fact]
    public void DefaultHomePageDisplaysExpectedContent()
    {
        // Arrange
        var tenants = new List<TenantDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Acme Corporation", Slug = "acme" }
        };
        Services.AddSingleton<ITenantService>(new StubTenantService(tenants));

        // Act
        var cut = RenderComponent<Home>();

        // Assert: Check that the rendered markup includes some expected text
        // Replace "Welcome" with a snippet of text that you know appears in Home.razor
        Assert.Contains("Welcome", cut.Markup);
        Assert.Contains("Acme Corporation", cut.Markup);
    }
}
