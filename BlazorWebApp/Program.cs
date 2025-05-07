using BlazorWebApp.Extensions;
using BlazorWebApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure services via extension methods
builder.Services
    .AddApplicationServices()
    .AddAuthenticationAndAuthorization(builder.Configuration)
    .AddPersistence(builder.Configuration)
    .AddLocalizationServices();

var app = builder.Build();

// Configure middleware and endpoints via extension methods
app
    .UseCustomMiddleware()
    .MapEndpoints();

// Seed data
await DataSeeder.SeedAsync(app);

app.Run();
