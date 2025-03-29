// Integration test suite to verify the behavior of Members API endpoints
// This test suite checks authentication, authorization, and response correctness

// External libraries for HTTP requests, JSON handling, and Identity Management
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using AspNetCoreApp.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using AspNetCoreApp.Data;
using Microsoft.AspNetCore.Hosting;

// Indicates this test is part of a collection that uses shared database resources
[Collection("DatabaseCollection")]
public class MembersApiTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    // HTTP client for sending requests to the API endpoints
    private readonly HttpClient _client;

    // Database context for directly interacting with test database
    private readonly ApplicationDbContext _dbContext;

    // Test user object to simulate authenticated API requests
    private readonly IdentityUser _testUser;

    // Scope for resolving dependencies, such as services from ASP.NET Core
    private readonly IServiceScope _scope;

    // Constructor: sets up environment for tests (runs before each test)
    public MembersApiTests(WebApplicationFactory<Program> factory)
    {
        var appFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test"); // Explicitly set environment here
        });

        // Initialize HTTP client to simulate API calls
        _client = appFactory.CreateClient();

        // Setup dependency injection scope
        _scope = appFactory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Create unique test user with email/password for authentication testing
        var uuid = Guid.NewGuid().ToString(); // Unique identifier for user
        _testUser = new IdentityUser
        {
            UserName = $"test-{uuid}@example.com",
            Email = $"test-{uuid}@example.com",
            EmailConfirmed = true
        };

        // Use Identity framework to save the new test user to the database
        var userManager = _scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        userManager.CreateAsync(_testUser, "SecureP@ssword123!").Wait();
        userManager.AddToRoleAsync(_testUser, "Member").Wait(); // Assign user "Member" role

        // Add associated Member record into the Members table
        _dbContext.Members.Add(new Member
        {
            FirstName = "Test",
            LastName = "User",
            Email = _testUser.Email,
            UserId = _testUser.Id
        });

        _dbContext.SaveChanges(); // Persist changes in test database
    }

    // Test Case 1: Verify the API returns members when user is authenticated
    [Fact]
    public async Task MembersApi_WithValidJwt_ReturnsMembersList()
    {
        // Arrange: Authenticate the test user to obtain JWT token
        var loginResponse = await _client.PostAsJsonAsync("/api/login", new
        {
            email = _testUser.Email,
            password = "SecureP@ssword123!"
        });

        loginResponse.EnsureSuccessStatusCode(); // Assert login succeeded

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResult);
        Assert.False(string.IsNullOrEmpty(loginResult!.Token));

        // Attach JWT token to authorization header for subsequent requests
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.Token);

        // Act: Send authenticated request to fetch members
        var response = await _client.GetAsync("/api/members");

        // Assert: Check response success and validate returned data
        response.EnsureSuccessStatusCode();

        var members = await response.Content.ReadFromJsonAsync<List<Member>>();
        Assert.NotNull(members);
        Assert.Contains(members, m => m.Email == _testUser.Email); // Verify presence of test user
    }

    // Test Case 2: Verify unauthorized request fails
    [Fact]
    public async Task MembersApi_WithoutJwt_ReturnsUnauthorized()
    {
        // Arrange & Act: Make request without JWT token
        var response = await _client.GetAsync("/api/members");

        // Assert: Response should indicate unauthorized access (HTTP 401)
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // Cleanup method: executes after tests to remove test data and resources
    public void Dispose()
    {
        // Remove test user from Identity database
        var userManager = _scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        userManager.DeleteAsync(_testUser).Wait();

        // Remove associated member record
        var member = _dbContext.Members.FirstOrDefault(m => m.Email == _testUser.Email);
        if (member != null)
        {
            _dbContext.Members.Remove(member);
            _dbContext.SaveChanges();
        }

        // Dispose of resources to clean up
        _scope.Dispose();
        _dbContext.Dispose();
        _client.Dispose();
    }
}

// Data transfer object (DTO) to deserialize JWT token response
public record LoginResponse(string Token);
