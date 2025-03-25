﻿using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using AspNetCoreApp; // Adjust this namespace to match your actual app's namespace

[Collection("DatabaseCollection")]
public class MembersApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MembersApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
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
public record Member(int Id, string Name);
