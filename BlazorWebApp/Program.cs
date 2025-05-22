using BlazorWebApp.Extensions;
using BlazorWebApp.Data;
using BlazorWebApp.Services;
using MudBlazor.Services;
using BlazorWebApp.Modules.Cms;
using Microsoft.AspNetCore.Components.Web;
using BlazorWebApp.Components;


var builder = WebApplication.CreateBuilder(args);

// Configure services via extension methods
builder.Services
    .AddApplicationServices()
    .AddAuthenticationAndAuthorization(builder.Configuration)
    .AddPersistence(builder.Configuration)          // our factory-only + shim
    .AddLocalizationServices()
    .AddScoped<ITenantProvider, StubTenantProvider>()
    .AddMudServices()                              // MudBlazor
    .AddCmsModule();

builder.Services.AddControllers();

builder.Services.Configure<StubTenantProviderOptions>(
    builder.Configuration.GetSection("StubTenantProviderOptions"));

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        // Registers MyComponent as the <my-component> web component
        options.RootComponents.RegisterCustomElement<MyComponent>("my-component");
    });
 

var app = builder.Build();


// Configure middleware and endpoints via extension methods
app
  .UseCustomMiddleware()
  .MapEndpoints();

// Seed data
await DataSeeder.SeedAsync(app);

app.Run();
