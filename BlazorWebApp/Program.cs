using BlazorWebApp.Extensions;
using BlazorWebApp.Data;
using BlazorWebApp.Services;
using MudBlazor.Services;
using Microsoft.AspNetCore.Rewrite;
using System.Net;


var builder = WebApplication.CreateBuilder(args);

// Configure services via extension methods
builder.Services
    .AddApplicationServices()
    .AddAuthenticationAndAuthorization(builder.Configuration)
    .AddPersistence(builder.Configuration)          // our factory-only + shim
    .AddLocalizationServices()
    .AddScoped<ITreeMenuService, TreeMenuService>() // page-menu service
    .AddScoped<ITenantProvider, StubTenantProvider>()
    .AddMudServices();                               // MudBlazor

builder.Services.Configure<StubTenantProviderOptions>(
    builder.Configuration.GetSection("StubTenantProviderOptions"));

var app = builder.Build();


// Configure middleware and endpoints via extension methods
app
    .UseCustomMiddleware()
    .MapEndpoints();

// Seed data
await DataSeeder.SeedAsync(app);

app.Run();
