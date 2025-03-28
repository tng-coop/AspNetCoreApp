using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using AspNetCoreApp.Models;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using AspNetCoreApp.Data;


[Collection("DatabaseCollection")]
public class MembersApiTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly ApplicationDbContext _dbContext;
    private readonly IdentityUser _testUser;
    private readonly IServiceScope _scope;

    public MembersApiTests(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");

        var appFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.Test.json");
            });
        });

        _client = appFactory.CreateClient();

        _scope = appFactory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Create unique test user
        var uuid = Guid.NewGuid().ToString();
        _testUser = new IdentityUser
        {
            UserName = $"test-{uuid}@example.com",
            Email = $"test-{uuid}@example.com",
            EmailConfirmed = true
        };

        var userManager = _scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        userManager.CreateAsync(_testUser, "SecureP@ssword123!").Wait();
        userManager.AddToRoleAsync(_testUser, "Member").Wait();

        _dbContext.Members.Add(new Member
        {
            FirstName = "Test",
            LastName = "User",
            Email = _testUser.Email,
            UserId = _testUser.Id
        });

        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task MembersApi_WithValidJwt_ReturnsMembersList()
    {
        // Arrange
        var loginResponse = await _client.PostAsJsonAsync("/api/login", new
        {
            email = _testUser.Email,
            password = "SecureP@ssword123!"
        });

        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResult);
        Assert.False(string.IsNullOrEmpty(loginResult!.Token));

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.Token);

        // Act
        var response = await _client.GetAsync("/api/members");

        // Assert
        response.EnsureSuccessStatusCode();

        var members = await response.Content.ReadFromJsonAsync<List<Member>>();
        Assert.NotNull(members);
        Assert.Contains(members, m => m.Email == _testUser.Email);
    }

    [Fact]
    public async Task MembersApi_WithoutJwt_ReturnsUnauthorized()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/members");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public void Dispose()
    {
        var userManager = _scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        userManager.DeleteAsync(_testUser).Wait();

        var member = _dbContext.Members.FirstOrDefault(m => m.Email == _testUser.Email);
        if (member != null)
        {
            _dbContext.Members.Remove(member);
            _dbContext.SaveChanges();
        }

        _scope.Dispose();
        _dbContext.Dispose();
        _client.Dispose();
    }
}

// DTO examples:
public record LoginResponse(string Token);