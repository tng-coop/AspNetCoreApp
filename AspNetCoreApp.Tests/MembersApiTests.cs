using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using AspNetCoreApp.Models;
using System.Diagnostics; // Import the models namespace for EF Member

[Collection("DatabaseCollection")]
public class MembersApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MembersApiTests(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.Test.json");
            });
        }).CreateClient();
    }

    [Fact]
    public async Task MembersApi_WithValidJwt_ReturnsMembersList()
    {
        // Arrange
        var loginResponse = await _client.PostAsJsonAsync("/api/login", new
        {
            email = "admin@example.com",
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
        var rc = await response.Content.ReadAsStringAsync();

        // Debug: Log the raw JSON response
        Debug.WriteLine("Raw JSON Response:");
        Debug.WriteLine(rc);

        // Deserialize the response into the EF Member class
        var members = await response.Content.ReadFromJsonAsync<List<Member>>();
        Assert.NotNull(members);
        Assert.NotEmpty(members);
    }

    [Fact]
    public async Task MembersApi_WithoutJwt_ReturnsUnauthorized()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/members");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

// DTO examples:
public record LoginResponse(string Token);
