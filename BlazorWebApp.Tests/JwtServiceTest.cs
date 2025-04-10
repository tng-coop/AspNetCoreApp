using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using BlazorWebApp.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BlazorWebApp.Tests.Services;

public class JwtTokenServiceTests
{
    private IConfiguration GetTestConfiguration()
    {
        var config = new Dictionary<string, string>
        {
            {"JwtSettings:Issuer", "TestIssuer"},
            {"JwtSettings:Audience", "TestAudience"}
        };
        return new ConfigurationBuilder()
            .AddInMemoryCollection(config!)
            .AddEnvironmentVariables()
            .Build();
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidJwt()
    {
        // Arrange
        var configuration = GetTestConfiguration();
        configuration["JwtSettings:PrivateKeyBase64"].Should().NotBeNullOrEmpty("JWT private key configuration must be set for testing.");

        var service = new JwtTokenService(configuration);

        // Act
        var token = service.GenerateToken("user123", "user@example.com");

        // Assert
        token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Should().NotBeNull();
        jwtToken.Issuer.Should().Be("TestIssuer");
        jwtToken.Audiences.Should().Contain("TestAudience");

        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "user123");
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "user@example.com");
    }
}
