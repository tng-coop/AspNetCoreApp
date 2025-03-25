using System.Net;
using System.Net.Http.Json;
using AspNetCoreApp;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Microsoft.Extensions.Configuration;

namespace AspNetCoreApp.Tests;

[Collection("DatabaseCollection")]
public class WeatherForecastTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public WeatherForecastTests(WebApplicationFactory<Program> factory)
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
    public async Task GetWeatherForecast_ReturnsSuccessAndCorrectData()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/weatherforecast");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var forecasts = await response.Content.ReadFromJsonAsync<WeatherForecast[]>();

        Assert.NotNull(forecasts);
        Assert.NotEmpty(forecasts);
        Assert.Equal(5, forecasts.Length);

        foreach (var forecast in forecasts)
        {
            Assert.True(forecast.TemperatureC >= -20 && forecast.TemperatureC <= 55);
            Assert.False(string.IsNullOrEmpty(forecast.Summary));
        }
    }

    private record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary);
}
