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
    .AddMudServices();                               // MudBlazor

var app = builder.Build();

// —— strip trailing slash ONLY under /articles/** —————————
var rewriteOptions = new RewriteOptions()
    // matches:  /articles/{anything}/   (but not /foo/...)
    .AddRedirect(
        @"^articles/(.+)/$",           // incoming-path regex (no leading slash)
        "/articles/$1",                // target
        (int)HttpStatusCode.MovedPermanently
    );

app.UseRewriter(rewriteOptions);

// Configure middleware and endpoints via extension methods
app
    .UseCustomMiddleware()
    .MapEndpoints();

// Seed data
await DataSeeder.SeedAsync(app);

app.Run();
