using BlazorWebApp.Extensions;
using BlazorWebApp.Data;
using MudBlazor.Services;
using MudExtensions.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure services via extension methods
builder.Services
    .AddApplicationServices()
    .AddAuthenticationAndAuthorization(builder.Configuration)
    .AddPersistence(builder.Configuration)
    .AddLocalizationServices();
    builder.Services.AddScoped<BlazorWebApp.Services.ITreeMenuService, BlazorWebApp.Services.TreeMenuService>();
    builder.Services.AddMudServices();
    builder.Services.AddMudExtensions();

var app = builder.Build();

// Configure middleware and endpoints via extension methods
app
    .UseCustomMiddleware()
    .MapEndpoints();

// Seed data
await DataSeeder.SeedAsync(app);

app.Run();
